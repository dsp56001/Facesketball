
using MonoGameLibrary.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Facesketball_Server
{
    [Serializable]
    public enum Entrance
    {
        Left,
        Right
    }

    [Serializable]
    public class NetworkWindowInformation : INetworkObject
    {
        public string Type { get { return m_Type; } set { m_Type = value; } }

        
        public Rectangle windowRect;

        private string m_Type;

        public NetworkWindowInformation(int nWidth, int nHeight) : this(0, 0, nWidth, nHeight)
        {

        }
        public NetworkWindowInformation(int startX, int startY, int nWidth, int nHeight)
        {
            this.windowRect = new Rectangle(startX, startY, nWidth, nHeight);
        }

        public NetworkWindowInformation()
            : this(128, 80)
        {

        }

    }
}
