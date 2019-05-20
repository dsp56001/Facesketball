using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace MonoGameLibrary.Network
{
    //Network Delegates
    public delegate void onConnectEventHandler(object sender, OnConnectEventArgs e);
    public delegate void onReadEventHandler(object sender, OnReadEventArgs e);
    public delegate void onWriteEventHandler(object sender, OnWriteEventArgs e);


    //EventArgs
    public class OnConnectEventArgs
    {
        public int ClientID;

        public OnConnectEventArgs(int id)
        {
            ClientID = id;
        }
    }

    public class OnReadEventArgs
    {
        public int ClientID;
        public object Obj;

        public OnReadEventArgs()
        {
        }
    }

    public class OnWriteEventArgs
    {
        public int ClientID;

        public OnWriteEventArgs()
        {

        }
    }

    //I don't know if this is really necessary.  Gonna try labeling this as
    //Serialiable, then  see if I can get away with just inheriting from this
    //instead of marking every object that I want to send across the network
    //as serializable...not that it's so bad.
    public interface INetworkObject
    {
        string Type { get; set; }
    }


    //Guess I'll leave this here as an example of how to create a network object
    [Serializable]
    public class TestObject : INetworkObject
    {
        public string Type { get { return m_type; } set { m_type = value; } }

        public float x, y, z;

        private string m_type;

        public TestObject()
        {
            Type = this.GetType().ToString();
            x = 0;
            y = 1;
            z = 2;
        }
    }
}
