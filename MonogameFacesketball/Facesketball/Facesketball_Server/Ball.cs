using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoGameLibrary.Network;
using System.Runtime.Serialization;

namespace Facesketball_Server
{
    [Serializable]
    public class Ball : INetworkObject
    {
        public string Type { get { return m_Type; } set { m_Type = value; } }

        public Vector2 Position;
        public Vector2 Direction;
        public Vector2 GravityDirection;
        public float Speed;
        public bool FindNewScreen;
        public Entrance Enter;

        private string m_Type;

        public Ball(Vector2 nPosition, Vector2 nDirection, Vector2 gravityDir, float nSpeed, bool ExitRight, bool nFindNewScreen)
        {
            this.Position = nPosition;
            this.Direction = nDirection;
            this.Speed = nSpeed;
            this.FindNewScreen = nFindNewScreen;
            this.GravityDirection = gravityDir;
            if (ExitRight)
            {
                this.Enter = Entrance.Left;
            }
            else
            {
                this.Enter = Entrance.Right;
            }
        }
        public Ball()
            : this(Vector2.Zero, Vector2.Zero, Vector2.Zero, 0.0f, false, true)
        {

        }

    }
}
