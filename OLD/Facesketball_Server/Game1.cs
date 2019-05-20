using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using IntroGameLibrary.Network;
using IntroGameLibrary.Network.Server;
using IntroGameLibrary.Network.Client;
using IntroGameLibrary.Network.Serializer;
using IntroGameLibrary.Util;

namespace Facesketball
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        GameServer server;

        List<NetworkWindowInformation> clientWindows;
        int DraggingWindow;
        int ClientControllingBall;

        InputHandler input;

        Ball serverBall;
        bool controllingBall;

        Texture2D blackPix;
        Texture2D redPix;

        public Game1()
        {
            
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            input = new InputHandler(this);
            this.Components.Add(input);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            clientWindows = new List<NetworkWindowInformation>();
            controllingBall = true;
            serverBall = new Ball(new Vector2(0, 0), new Vector2(.5f, .5f), 1f);

            server = GameServer.Server;
            server.Initialize();
            server.BeginAcceptingClients(onConnect);

            base.Initialize();
        }

        private void onConnect(object sender, OnConnectEventArgs e)
        {
            if (!server.Clients[e.ClientID].Reading)
            {
                server.BeginRead(onRead, e.ClientID);
            }
        }

        private void onRead(object sender, OnReadEventArgs e)
        {
            string type = e.Obj.GetType().ToString();

            switch (type)
            {
                case "Facesketball.NetworkWindowInformation":
                    NetworkWindowInformation tempNetInfo = (NetworkWindowInformation)e.Obj;
                    tempNetInfo.windowRect.Width /= 10;
                    tempNetInfo.windowRect.Height /= 10;
                    clientWindows.Add(tempNetInfo);

                    for(int i = 0; i < clientWindows.Count - 1; i++)
                    {
                        //Scoot newest window over by the cumulative width of the rest of the client windows
                        clientWindows[clientWindows.Count - 1].windowRect.X += clientWindows[i].windowRect.Width;
                    }
                    break;

                case "Facesketball.Ball":
                    serverBall = (Ball)e.Obj;
                    serverBall.Position *= .1f;
                    serverBall.Speed *= .1f;
                    serverBall.Position.X += clientWindows[e.ClientID].windowRect.Left;
                    serverBall.Position.Y += clientWindows[e.ClientID].windowRect.Top;
                    controllingBall = true;
                    break;

                default:
                    break;
            }
        }

        private void onWrite(object sender, OnWriteEventArgs e)
        {
            server.BeginRead(onRead, e.ClientID);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            blackPix = Content.Load<Texture2D>("black_pixel");
            //redPix = Content.Load<Texture2D>("red_pixel");
            redPix = blackPix;
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (input.MouseState.LeftButton == ButtonState.Pressed && input.PreviousMouseState.LeftButton == ButtonState.Released)
            {
                for (int i = 0; i < clientWindows.Count; i++)
                {
                    if (input.MouseState.X > clientWindows[i].windowRect.Left &&
                       input.MouseState.X < clientWindows[i].windowRect.Right &&
                       input.MouseState.Y > clientWindows[i].windowRect.Top &&
                       input.MouseState.Y < clientWindows[i].windowRect.Bottom)
                    {
                        DraggingWindow = i;
                    }
                }
            }
            else if (input.MouseState.LeftButton == ButtonState.Released && input.PreviousMouseState.LeftButton == ButtonState.Pressed)
            {
                DraggingWindow = -1;
            }

            if (DraggingWindow >= clientWindows.Count)
            {
                DraggingWindow = -1;
            }
            else if (DraggingWindow >= 0)
            {
                clientWindows[DraggingWindow].windowRect.X = input.MouseState.X - (clientWindows[DraggingWindow].windowRect.Width / 2);
                clientWindows[DraggingWindow].windowRect.Y = input.MouseState.Y - (clientWindows[DraggingWindow].windowRect.Height / 2);
            }

            if (controllingBall)
            {
                serverBall.Position += serverBall.Direction * serverBall.Speed;

                if (serverBall.Position.Y > 150)//graphics.GraphicsDevice.Viewport.Height)
                {
                    serverBall.Direction.Y = -1;
                }
                else if (serverBall.Position.Y < 0)
                {
                    serverBall.Direction.Y = 1;
                }

                if (serverBall.Position.X > 400)//graphics.GraphicsDevice.Viewport.Width)
                {
                    serverBall.Direction.X = -1;
                }
                else if (serverBall.Position.X < 0)
                {
                    serverBall.Direction.X = 1;
                }

                if(serverBall.Direction != Vector2.Zero)
                    serverBall.Direction.Normalize();

                Rectangle clientRect;
                Rectangle ballRect = new Rectangle((int)serverBall.Position.X, (int)serverBall.Position.Y, 4, 4); //HARDCODED BAAAAAAAAADDDDD -AB
                for (int i = 0; i < clientWindows.Count; i++)
                {
                    clientRect = clientWindows[i].windowRect;
                    if (clientRect.Intersects(ballRect))
                    {
                        if (!server.Clients[i].Writing)
                        {
                            controllingBall = false;
                            ClientControllingBall = i;
                            serverBall.Position.Y -= clientRect.Top;
                            serverBall.Position.X -= clientRect.Left;
                            serverBall.Position *= 10;
                            serverBall.Speed *= 10;
                            serverBall.Direction *= 10;

                            server.BeginWrite(onWrite, i, serverBall);
                        }
                    }
                }
            }
            else
            {
                if (!server.Clients[ClientControllingBall].Reading)
                {
                    server.BeginRead(onRead, ClientControllingBall);
                }
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            for(int i = 0; i < clientWindows.Count; i++)
            {
                spriteBatch.Draw(blackPix, clientWindows[i].windowRect, Color.White);
            }

            Rectangle ballRect = new Rectangle((int)serverBall.Position.X, (int)serverBall.Position.Y, 4, 4);
            spriteBatch.Draw(redPix, ballRect, Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }

    [Serializable]
    public class Ball : INetworkObject
    {
        public string Type { get { return m_Type; } set { m_Type = value; } }

        public Vector2 Position;
        public Vector2 Direction;
        public float Speed;

        private string m_Type;

        public Ball(Vector2 nPosition, Vector2 nDirection, float nSpeed)
        {
            this.Position = nPosition;
            this.Direction = nDirection;
            this.Speed = nSpeed;
        }
        public Ball()
            : this(Vector2.Zero, Vector2.Zero, 0.0f)
        {

        }

    }

    [Serializable]
    public class NetworkWindowInformation : INetworkObject
    {
        public string Type { get { return m_Type; } set { m_Type = value; } }

        public Rectangle windowRect;

        private string m_Type;

        public NetworkWindowInformation(int nWidth, int nHeight)
        {
            this.windowRect = new Rectangle(200, 200, nWidth, nHeight);
        }

        public NetworkWindowInformation()
            : this(128, 80)
        {

        }

    }
}


// Allows the game to exit
            /*for (int i = 0; i < server.Clients.Count; i++)
            {
                if (!server.Clients[i].Reading)
                {
                    server.BeginRead(onRead, i);
                }
                
                if (!server.Clients[i].Writing)
                {
                    server.BeginWrite(onWrite, i, serverBall);
                }
            }*/