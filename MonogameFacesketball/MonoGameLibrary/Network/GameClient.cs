using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonoGameLibrary.Network.Client
{
    public class GameClient
    {
        //Get Connection status
        public bool Connected { get { return m_client.Connected; } }

        //Get Reading/Writing status
        public bool Reading { get { return m_reading; } set { m_reading = value; } }
        public bool Writing { get { return m_writing; } set { m_writing = value; } }


        private bool m_reading;
        private bool m_writing;

        private TcpClient m_client;
        private NetworkStream m_stream;

        //Local Callbacks
        private AsyncCallback onReadHeaderLocalCallback;
        private AsyncCallback onReadLocalCallback;
        private AsyncCallback onWriteHeaderLocalCallback;
        private AsyncCallback onWriteLocalCallback;

        //User Callbacks
        public event onReadEventHandler onReadUserCallback;
        public event onWriteEventHandler onWriteUserCallback;

        public GameClient()
        {
            m_client = new TcpClient();

            onReadHeaderLocalCallback = new AsyncCallback(onReadHeader);
            onReadLocalCallback = new AsyncCallback(onRead);
            onWriteHeaderLocalCallback = new AsyncCallback(onWriteHeader);
            onWriteLocalCallback = new AsyncCallback(onWrite);
        }

        public bool Connect(string hostName, int port)
        {
            m_client.Connect(hostName, port);
            if (Connected)
            {
                m_stream = m_client.GetStream();
                return true;
            }
            return false;
        }

        public bool Connect(string hostName)
        {
            if (this.Connect(hostName, 19737))
                return true;
            return false;
        }

        public bool Connect()
        {
            if (this.Connect(IPAddress.Loopback.ToString()))
                return true;
            return false;
        }

        public void BeginRead(onReadEventHandler callback)
        {
            if (m_client.Connected)
            {
                try
                {
                    onReadUserCallback = callback;

                    ClientReadInfo header = new ClientReadInfo();
                    header.Data = new Byte[4];

                    m_reading = true;
                    m_stream.BeginRead(header.Data, 0, 4, onReadHeaderLocalCallback, header);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        public void BeginWrite(onWriteEventHandler callback, object obj)
        {
            if (m_client.Connected)
            {
                try
                {
                    ClientReadInfo writeInfo = new ClientReadInfo();
                    writeInfo.Data = Serializer.Serializer.SerializeObject(obj);

                    Byte[] array = BitConverter.GetBytes(writeInfo.Data.Length);

                    onWriteUserCallback = callback;

                    m_writing = true;
                    m_stream.BeginWrite(array, 0, array.Length, onWriteHeaderLocalCallback, writeInfo);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        private void onReadHeader(IAsyncResult ar)
        {
            try
            {
                ClientReadInfo header = (ClientReadInfo)ar.AsyncState;
                int objectSize = BitConverter.ToInt32(header.Data, 0);

                header.Data = new Byte[objectSize];

                m_stream.BeginRead(header.Data, 0, objectSize, onReadLocalCallback, header);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void onRead(IAsyncResult ar)
        {
            try
            {
                OnReadEventArgs eA = new OnReadEventArgs();
                ClientReadInfo readObject = (ClientReadInfo)ar.AsyncState;

                eA.Obj = Serializer.Serializer.DeserializeByteArray(readObject.Data);

                m_reading = false;

                m_stream.EndRead(ar);
                onReadUserCallback.Invoke(this, eA);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void onWriteHeader(IAsyncResult ar)
        {
            try
            {
                ClientReadInfo writeInfo = (ClientReadInfo)ar.AsyncState;
                m_stream.EndWrite(ar);
                m_stream.BeginWrite(writeInfo.Data, 0, writeInfo.Data.Length, onWriteLocalCallback, null);
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
                OnWriteEventArgs e = new OnWriteEventArgs();

                m_stream.EndWrite(ar);

                m_writing = false;

                onWriteUserCallback.Invoke(this, e);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }


    public class ClientReadInfo
    {
        public Byte[] Data;

        public ClientReadInfo()
        {

        }
    }
}
