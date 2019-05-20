#define _CLIENT

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary.Particle;
using MonoGameLibrary.Util;
using MotionDetection;
using Facesketball_Server;

#if _CLIENT
using MonoGameLibrary.Network;
using MonoGameLibrary.Network.Client;
#endif

namespace Facesketball
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
#if _CLIENT
        GameClient client;
#endif

        GraphicsDeviceManager graphics;
        FoodManager fMan;
        public SpriteBatch spriteBatch;

        SpriteFont font;

        //GameLibrary Components
        FPS fps;
        InputHandler input;
        GameConsole gameConsole;

        public PlayerFace FaceTracker;
        //public FloatingBall Fball;
        public BasketBall Bball;
        public FoatingBallManager FBM;

        CameraManager cm;
        public FaceChaser faceChaser;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            fMan = new FoodManager(this, "20px_1trans", new Point(10, 10));
            //graphics.ToggleFullScreen();
            //graphics.PreferredBackBufferWidth = 1024;
            //graphics.PreferredBackBufferHeight = 768;
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            Content.RootDirectory = "Content";

            #region Library Components
            input = new InputHandler(this);
            this.Components.Add(input);

            gameConsole = new GameConsole(this);
            //gameConsole.ToggleState();
            this.Components.Add(gameConsole);

            fps = new FPS(this);
            this.Components.Add(fps);
            
            
            #endregion

            FaceTracker = new PlayerFace(this);
            FaceTracker.ShowMarkers = false;
            FaceTracker.Scale = 1.0f;
            FaceTracker.Visible = false;
            this.Components.Add(FaceTracker);

            FBM = new FoatingBallManager(this);
            this.Components.Add(FBM);

            faceChaser = new FaceChaser(this);
            faceChaser.Location = new Vector2(10, 10);
            faceChaser.ChaseSpeed = new Vector2(1.4f, 1.4f);
            faceChaser.ShowMarkers = false;
            this.Components.Add(faceChaser);
            faceChaser.Enabled = true;
            faceChaser.Visible = true;

            Bball = new BasketBall(this);
            this.Components.Add(Bball);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
#if _CLIENT
            client = new GameClient();
            //client.Connect("172.31.31.67");
            client.Connect("127.0.0.1");
            if (client.Connected)
                client.BeginWrite(onWrite, new NetworkWindowInformation(0, 0, this.graphics.GraphicsDevice.Viewport.Width,
                    this.GraphicsDevice.Viewport.Height));
#endif
            base.Initialize();
        }

#if _CLIENT
        private void onWrite(object sender, OnWriteEventArgs e)
        {

        }

        private void onRead(object sender, OnReadEventArgs e)
        {
            string type = e.Obj.GetType().ToString();
            switch (type)
            {
                case "Facesketball.Ball":
                    Ball serverBall = ((Ball)e.Obj);
                    Bball.Location = serverBall.Position;
                    Bball.Direction = serverBall.Direction;
                    Bball.Speed = serverBall.Speed;
                    Bball.IsOffScreen = false;
                    Bball.GravityDir = serverBall.GravityDirection;
                    break;
                default:
                    break;
            }
        }
#endif

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //Cache textures
            Content.Load<Texture2D>("20px_1trans");
            Content.Load<Texture2D>("Food");
            Content.Load<Texture2D>("PurpleGhost");
            Content.Load<Texture2D>("Smoke");
            Content.Load<Texture2D>("SmokeLight");
            Content.Load<Texture2D>("GhostHit");

            // TODO: use this.Content to load your game content here
            font = Content.Load<SpriteFont>("SpriteFont1");

            //ParticleManager.Instance().ParticleSystems.Add("motionparticles", 
            //    new ParticleSystem(100, 200, 
            //        this.Content.Load<Texture2D>("GhostHit"),
            //        2, 10,
            //        1, 5,
            //        1, 10,
            //        2.5f, 5.0f,
            //        0.1f, 0.5f,
            //        50));
            ParticleManager.Instance().ParticleSystems.Add("motionparticles",
                new ParticleSystem(10, 50, this.Content.Load<Texture2D>("SmokeLight"),
                    1, 5, //speed
                    .5f, 2.5f, //accel
                    1, 10, //rot
                    2.5f, 3.0f, //life
                    0.5f, 1.5f, //scale
                    5));
            cm = new CameraManager(this);
            this.Components.Add(cm);

            FaceController.GameSize = new Vector2(this.GraphicsDevice.Viewport.Width, this.GraphicsDevice.Viewport.Height);
            FaceController.VideoSize = new Vector2((float)cm.CameraWidth, (float)cm.CameraHeight);
            //FaceController.VideoSize = new Vector2(320, 240);
            //FaceController.VideoSize = new Vector2(640, 480);
            FaceController.ScaleLocation = true;
            cm.fd.AccurateAndSlow = false;
            cm.fd.FindEyes = false;
            cm.SetBitmap = true;
            cm.DrawPlayerFace = false;
            cm.FindFaces = true;
            cm.EnableMotion();

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

#if _CLIENT
            if (client.Connected)
            {
                if (!client.Reading)
                {
                    client.BeginRead(onRead);
                }
            }

            if (!Bball.IsOffScreen)
            {
                if (!client.Writing)
                {
                    client.BeginWrite(onWrite, new Ball(Bball.Location, Bball.Direction, Bball.GravityDir, Bball.Speed, Bball.ExitRight, false));
                }
            }
            else if(Bball.FindNewScreen)
            {
                if (!client.Writing)
                {
                    client.BeginWrite(onWrite, new Ball(Bball.Location, Bball.Direction, Bball.GravityDir, Bball.Speed, Bball.ExitRight, true));
                    Bball.FindNewScreen = false;
                }
            }
#endif

            if (input.KeyboardState.WasKeyPressed(Keys.I))
            {

                Bball.Direction = Vector2.Negate(Bball.Direction);
            }

            if (input.KeyboardState.WasKeyPressed(Keys.P))
            {
                if (ParticleManager.Instance().Enabled)
                {
                    ParticleManager.Instance().Enabled = false;
                }
                else
                {
                    ParticleManager.Instance().Enabled = true;
                }

            }
            fMan.UpdatePoints(Bball.LocationRect);


            //faceChaser.Update(FaceTracker, gameTime);

            ParticleManager.Instance().ParticleSystems["motionparticles"].Update(gameTime);


            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);
            spriteBatch.Begin();

            // TODO: Add your drawing code here
            if (cm.SetBitmap && (cm.CameraBackGround != null))
            {
                if (FaceController.ScaleLocation)
                {
                    //Scale video to gamesize
                    spriteBatch.Draw(cm.CameraBackGround,
                        new Rectangle(0, 0, this.Window.ClientBounds.Width,
                            this.Window.ClientBounds.Height), Color.White);
                }
                else
                {
                    //Draw video
                    spriteBatch.Draw(cm.CameraBackGround, Vector2.Zero, Color.White);
                }
            }

            //if (cm.fd.Faces.Count > 0)
            //{
            //    foreach(FaceController f in cm.fd.Faces)
            //    {
            //        spriteBatch.DrawString(font, f.About(), f.Location - new Vector2(0,-50), Color.AliceBlue);
            //    }
            //    PacMan.Location = cm.fd.Faces[0].Location;
            //    //2.5 is a HACK bese on the PacMan texture size
            //    PacMan.Scale = cm.fd.Faces[0].Scale / PacMan.spriteTexture.Width * 2.5f;   
            //}
            ParticleManager.Instance().Draw(spriteBatch);
            spriteBatch.End();

            //ParticleManager.Instance().ParticleSystems["motionparticles0"].Draw(spriteBatch);
            
            base.Draw(gameTime);
        }
    }
}
