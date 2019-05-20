using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using MonoGameLibrary.Network.Serializer;

namespace MonoGameLibrary.Network.Server
{
    public class GameServer
    {
        //static singleton server
        private static GameServer m_server;

        //Accessors
        public List<ClientInfo> Clients { get { return m_clients; } }

        //Private members
        private TcpListener m_listener;
        private List<ClientInfo> m_clients;

        //Local Callbacks - Recieve AsyncResuls and process them before invoking
        //user callbacks.  Essentially wraps the process of reading object sizes
        //and deserializing objects.
        private AsyncCallback onConnectLocalCallback;
        private AsyncCallback onReadHeaderLocalCallback;
        private AsyncCallback onReadLocalCallback;
        private AsyncCallback onWriteHeaderLocalCallback;
        private AsyncCallback onWriteLocalCallback;

        //User Callbacks
        public event onConnectEventHandler onConnectUserCallback;
        public event onReadEventHandler onReadUserCallback;
        public event onWriteEventHandler onWriteUserCallback;


        //Server instance accessor
        public static GameServer Server
        {
            get
            {
                if (m_server == null)
                {
                    m_server = new GameServer();
                }
                return m_server;
            }
        }


        private GameServer()
        {
            m_clients = new List<ClientInfo>();

            onConnectLocalCallback = new AsyncCallback(onConnect);
            onReadHeaderLocalCallback = new AsyncCallback(onReadHeader);
            onReadLocalCallback = new AsyncCallback(onRead);
            onWriteHeaderLocalCallback = new AsyncCallback(onWriteHeader);
            onWriteLocalCallback = new AsyncCallback(onWrite);
        }

        public void Initialize(int port)
        {
            //Host listener for specified port
            m_listener = new TcpListener(IPAddress.Any, port);  
        }

        public void Initialize()
        {
            this.Initialize(19737); //HARDCODED - just picked a random port someone mentioned online
        }

        public void BeginAcceptingClients(onConnectEventHandler callback)
        {
            if (m_listener != null)
            {
                //Set a callback to notify the user of a successful client connection
                onConnectUserCallback = callback;
                m_listener.Start();
                m_listener.BeginAcceptTcpClient(onConnectLocalCallback, null);
            }
            else
            {
                throw new Exception("Use GameServer.Initialize to assign the server a default port (or use the overload to assign a specific port).  Once the server is initialized, it can begin accepting clients.");
            }
        }


        //Begins an Asynchronous read from a specific client's network stream.  This function handles
        //reading in header information that defines the size of the object, and then invokes a callback
        //which will handle reading and deserializing the actual object.
        public void BeginRead(onReadEventHandler callback, int clientNumber)
        {
            if (m_listener != null)
            {
                try
                {
                    //Set a callback to notify the user of a succsesful read, once we've finished with
                    //our part.
                    onReadUserCallback = callback;
                    
                    //ServerReadInfo will hold the id of the client who we're reading from, and an empty 
                    //array to read into.
                    ServerReadInfo header = new ServerReadInfo();
                    header.Id = clientNumber;
                    header.Data = new Byte[4];

                    //Set reading to true.  The caller has to use an if to check if we're reading before
                    //calling this function. Note:  Maybe the if can be done in this class?  I wasn't sure
                    //it as appropriate to take away that control.
                    m_clients[clientNumber].Reading = true;

                    //Read the data into our header object and send the instance of that object to the
                    //onRead header local callback.
                    m_clients[clientNumber].Stream.BeginRead(header.Data, 0, 4, onReadHeaderLocalCallback, header);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }


        //Begins an Asynchronous write to a specific client's network stream.  This function handles
        //writing of header information that defines the size of the object, and then invokes a callback
        //which will handle serializing and writing the actual object.
        public void BeginWrite(onWriteEventHandler callback, int clientNumber, object obj)
        {
            if (m_listener != null)
            {
                try
                {
                    //I resused the ServerReadInfo class which contains the client's id and a byte array
                    //containing data to be written or read to or from the server.  Here we will de-
                    //serialize the object that we want to write.
                    ServerReadInfo writeInfo = new ServerReadInfo();
                    writeInfo.Id = clientNumber;
                    writeInfo.Data = Serializer.Serializer.SerializeObject(obj);

                    //Store the size of our object in a Byte array.
                    Byte[] array = BitConverter.GetBytes(writeInfo.Data.Length);

                    onWriteUserCallback = callback;

                    //Set writing to true.  The caller has to use an if to check if we're writing before
                    //calling this function. 
                    m_clients[clientNumber].Writing = true;

                    //Write out the size of the object.  Send the actual object to the onWrite header local callback
                    m_clients[clientNumber].Stream.BeginWrite(array, 0, array.Length, onWriteHeaderLocalCallback, writeInfo);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        private void onConnect(IAsyncResult ar)
        {
            try
            {
                //Finalize accepting client
                TcpClient temp = m_listener.EndAcceptTcpClient(ar);

                //Add the client to our list.
                m_clients.Add(new ClientInfo(temp));

                //Create eventArgs to send to the user (just contains the id for us).
                OnConnectEventArgs eA = new OnConnectEventArgs(m_clients.Count - 1);

                //Invoke the user callback.
                onConnectUserCallback.Invoke(m_server, eA);

                //Be ready to accept another client
                m_listener.BeginAcceptTcpClient(onConnect, null);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        //This function is triggered by an event when a header is successfully read.
        private void onReadHeader(IAsyncResult ar)
        {
            try
            {
                //Cast the object which we send from "BeginRead" (which is stored in the
                //IAsyncResult as "AsyncState", which I think is a stupid name because
                //the object is not a "state" at all)...whatever
                ServerReadInfo header = (ServerReadInfo)ar.AsyncState;
                
                int objectSize = BitConverter.ToInt32(header.Data, 0);

                //Got the objectSize stored in a new int, and the id is obviously the same,
                //so we set the byte array in "header" to a new Byte array with the size of
                //the object so we can basically reuse this object.
                header.Data = new Byte[objectSize];

                //End the current read
                m_clients[header.Id].Stream.EndRead(ar);

                //Begin a new Async read now that we know the size of the object.  Store it
                //again in the data field of "header" and send the "header" object to the
                //final callback.
                m_clients[header.Id].Stream.BeginRead(header.Data, 0, objectSize, onReadLocalCallback, header);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void onRead(IAsyncResult ar)
        {
            //WTF: Okay, so this try and catch was falling into the catch and throwing an index
            //out of bounds exception WHICH IT'S CLEARLY NOT because the function works fine
            //without the try and catch, and it didn't even after I added the "if" statement
            //below (labeled by a comment saying "//TEST"), which clearly prevents ANY sort of
            //out of bounds id.


            //try
            //{
                //Cast ar.StupidName as our ReadInfo so we can get the idea and object data
                OnReadEventArgs eA = new OnReadEventArgs();
                ServerReadInfo readObject = (ServerReadInfo)ar.AsyncState;

                int id = readObject.Id;

                //TEST
                if (id > m_clients.Count - 1 || id < 0)
                {
                    return;
                }

                //Deserialize the byte array to create an object.  We're gonna store the object
                //in our OnReadEventArgs along with the id to send back to the original caller.
                eA.Obj = Serializer.Serializer.DeserializeByteArray(readObject.Data);
                eA.ClientID = id;

                //Done reading.  The position of this assignment is actually very important in 
                //the async world, apparently.
                m_clients[id].Reading = false;
                m_clients[id].Stream.EndRead(ar);

                //Invoke the User's callback
                onReadUserCallback.Invoke(this, eA);
            //}
            /*catch (Exception e)
            {
                throw e;
            }*/
        }

        private void onWriteHeader(IAsyncResult ar)
        {
            try
            {
                //We've successfully written the size of the object.  Now simply write the object
                //which was passed to us in the AsyncState.
                ServerReadInfo writeInfo = (ServerReadInfo)ar.AsyncState;
                m_clients[writeInfo.Id].Stream.EndWrite(ar);
                m_clients[writeInfo.Id].Stream.BeginWrite(writeInfo.Data, 0, writeInfo.Data.Length, onWriteLocalCallback, writeInfo.Id);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void onWrite(IAsyncResult ar)
        {
            try
            {
                //I didn't like writing e.ClientID all over, so I made a new int to hold the id
                //which was passed to us as the AsyncNotAState.
                int id = (int)ar.AsyncState;

                //Create Args to send back to user...Just sending them the clientID anyway.
                OnWriteEventArgs e = new OnWriteEventArgs();
                e.ClientID = id;

                //Done writing
                m_clients[id].Stream.EndWrite(ar);
                m_clients[id].Writing = false;

                //Invoke user callback
                onWriteUserCallback.Invoke(this, e);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }

    public class ServerReadInfo
    {
        public int Id;          //ClientID
        public Byte[] Data;     //Data to read/write

        public ServerReadInfo()
        {
            Id = 0;
        }
    }
}
