using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace MonoGameLibrary.Network
{
    //While this looks a lot like the top of the GameClient class, this is just a lightish
    //class to instantiate on the server to keep track of which clients it is reading/writing
    //from/to.
    public class ClientInfo
    {
        public TcpClient Client { get { return m_client; } set { m_client = value; } }
        public NetworkStream Stream { get { return m_stream; } set { m_stream = value; } }
        public bool Reading { get { return m_reading; } set { m_reading = value; } }
        public bool Writing { get { return m_writing; } set { m_writing = value; } }

        
        private TcpClient m_client;
        private NetworkStream m_stream;
        private bool m_reading;
        private bool m_writing;

        public ClientInfo(TcpClient client)
        {
            m_client = client;
            m_stream = m_client.GetStream();
            m_reading = false;
            m_writing = false;
        }
    }
}
