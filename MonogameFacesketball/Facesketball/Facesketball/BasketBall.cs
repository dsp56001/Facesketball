#define _CLIENT

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Storage;
using MonoGameLibrary.Sprite;

namespace Facesketball
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class BasketBall : DrawableSprite
    {
#if _CLIENT
        public const int arbitrarymodifier = 20;
        public bool IsOffScreen;
        public bool ExitRight;
        public bool FindNewScreen;
#endif

        public Vector2 GravityDir;
        float GravityAccel;
        float SpeedMax;

        Texture2D NormalTexture;
        

        //BAD
        DrawableSprite ghostFaceTracker;
        DrawableSprite ghostFaceChaser;
        //DrawableSprite gostFball;
        List<FloatingBall> ghostFballList;
        
        
        public BasketBall(Game game)
            : base(game)
        {
            // TODO: Construct any child components here
            this.ghostFaceTracker = ((Game1)game).FaceTracker;
            this.ghostFaceChaser = ((Game1)game).faceChaser;
            //this.gostFball = ((Game1)game).Fball;
            this.ghostFballList = ((Game1)game).FBM.FloatingBalls;
#if _CLIENT
            this.ExitRight = false;
#endif
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here

            base.Initialize();
        }

        protected override void LoadContent()
        {
#if _CLIENT
            IsOffScreen = true;
            FindNewScreen = false; 
#endif

            GravityDir = new Vector2(0, 1);
            GravityAccel = 1.5f;
            NormalTexture = Game.Content.Load<Texture2D>("pacManSingle");
            this.Speed = 2;
            this.SpeedMax = 9;
            this.Direction = new Vector2(-1, 0);
            this.Location = new Vector2(500, 100);
            this.spriteTexture = NormalTexture;
            base.LoadContent();

            // Extract collision data
            this.SpriteTextureData =
                new Color[this.spriteTexture.Width * this.spriteTexture.Height];
            this.spriteTexture.GetData(this.SpriteTextureData);

        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
#if _CLIENT
            if(IsOffScreen)
                return;
#endif

            // TODO: Add your update code here
            //Elapsed time since last update
            float time = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            //Collision Face Tracker
            if ((this.ghostFaceTracker.Enabled) && (this.ghostFaceTracker.Visible))
            {
                if (this.Intersects(this.ghostFaceTracker))
                {
                    if (this.PerPixelCollision(this.ghostFaceTracker))
                    {
                        this.Direction = Vector2.Negate(this.Direction);
                        this.Location += this.Direction;
                    }
                }
            }
            //Collision face chaser
            if ((this.ghostFaceChaser.Enabled) && (this.ghostFaceChaser.Visible))
            {
                if (this.Intersects(this.ghostFaceChaser))
                {
                    if (this.PerPixelCollision(this.ghostFaceChaser))
                    {
                        if (ghostFaceChaser.Direction == Vector2.Zero)
                        {
                            this.Direction = Vector2.Negate(this.Direction);
                            //this.Direction = Vector2.Reflect(ghostFaceChaser.Direction, Vector2.Zero);
                        }
                        else
                        {
                            this.Direction = Vector2.Reflect(ghostFaceChaser.Direction, Vector2.Zero);
                            while(this.Intersects(this.ghostFaceChaser))
                            {
                               //not time corrected move
                                this.Location = this.Location + (this.Direction * this.Speed);
                                this.SetTranformAndRect();
                            }
                        }
                    }
                }
            }
            
            //Collision with floatingball
            foreach (DrawableSprite d in ghostFballList)
            {
                if (d.Enabled && d.Visible)
                {
                    if (this.Intersects(d))
                    {
                        if (this.PerPixelCollision(d))
                        {
                            //enable particle
                            if (d is FloatingBall)
                            {
                                ((FloatingBall)d).particlesEnabled = true;
                            }
                            if (d.Direction == Vector2.Zero)
                            {
                                this.Direction = Vector2.Negate(this.Direction);
                            }
                            else
                            {
                                this.Direction = Vector2.Reflect(d.Direction, Vector2.Zero);
                            }
                            //this.Direction = Vector2.Negate(this.Direction);
                            //this.Location += this.Direction;
                        }
                    }
                }
            }
      
            //Comment out Gravity stuff if not working on network...
            
            //Gravity
            this.Direction += GravityDir;
            
            //PacManDir = PacManDir + GravityDir;
            if ((this.Speed < this.SpeedMax))
                this.Speed = this.Speed + GravityAccel;
            else
            {
                this.Speed = this.SpeedMax;
            }

            

            //Time corrected move. 
            this.Location = this.Location + ((this.Direction * (time / 1000)) * this.Speed);  //Simple Move 

#if !_CLIENT

            //Keep PacMan On Screen
            //X right
            if (this.Location.X >
                    graphics.GraphicsDevice.Viewport.Width - this.spriteTexture.Width)
            {
                //Negate X
                this.Direction = this.Direction * new Vector2(-1, 1);
                this.Location.X = graphics.GraphicsDevice.Viewport.Width - this.spriteTexture.Width;
            }

            //X left
            if (this.Location.X < 0)
            {
                //Negate X
                this.Direction = this.Direction * new Vector2(-1, 1);
                this.Location.X = 0;
            }

            //Y bottom
            if (this.Location.Y >=
                    graphics.GraphicsDevice.Viewport.Height - this.spriteTexture.Height)
            {
                //Negate Y
                this.Direction = this.Direction * new Vector2(1, -1);
                this.Location.Y = graphics.GraphicsDevice.Viewport.Height - this.spriteTexture.Height;
                //this.GravityDir = new Vector2(this.GravityDir.X, this.GravityDir.Y * -1);
                if (this.Direction.Y == 0)
                {
                    this.GravityDir = new Vector2(this.GravityDir.X, this.GravityDir.Y * -1);
                }
            }

            //Y top
            if (this.Location.Y <= 0)
            {
                //Negate Y
                this.Direction = this.Direction * new Vector2(1, -1);
                this.Location.Y = 0;
                if (this.Direction.Y == 0)
                {
                    this.GravityDir = new Vector2(this.GravityDir.X, this.GravityDir.Y * -1);
                }
            }
  
#else
            //Keep PacMan On Screen
            //X right
            if (this.Location.X >
                    this.Game.GraphicsDevice.Viewport.Width)//- this.spriteTexture.Width)
            {
                IsOffScreen = FindNewScreen = ExitRight = true;
                //Negate X
                //this.Direction = this.Direction * new Vector2(-1, 1);
                //this.Location.X = graphics.GraphicsDevice.Viewport.Width - this.spriteTexture.Width;
            }

            //X left
            if (this.Location.X < -spriteTexture.Width)
            {
                IsOffScreen = FindNewScreen = true;
                ExitRight = false;
                //Negate X
                //this.Direction = this.Direction * new Vector2(-1, 1);
                //this.Location.X = 0;
            }

            //Y bottom
            if (this.Location.Y >=
                    this.Game.GraphicsDevice.Viewport.Height - this.spriteTexture.Height)
            {
                //Negate Y
                this.Direction = this.Direction * new Vector2(1, -1);
                this.Location.Y = this.Game.GraphicsDevice.Viewport.Height - this.spriteTexture.Height;
                //this.GravityDir = new Vector2(this.GravityDir.X, this.GravityDir.Y * -1);
                if (this.Direction.Y == 0)
                {
                    this.GravityDir = new Vector2(this.GravityDir.X, this.GravityDir.Y * -1);
                }
            }

            //Y top
            if (this.Location.Y < 0)
            {
                //Negate Y
                this.Direction = this.Direction * new Vector2(1, -1);
                this.Location.Y = 0;
                if (this.Direction.Y == 0)
                {
                    this.GravityDir = new Vector2(this.GravityDir.X, this.GravityDir.Y * -1);
                }
            }
#endif

            base.Update(gameTime);
        }

        #region IMultiScreenObject Members

        public Vector2 CurrentScreen {
            get { return Vector2.Zero; }
            set { }
        }

        public Vector2 ScreenLocation
        {
            get { throw new NotImplementedException(); }
        }

        public Vector2 ScreenDirection
        {
            get { throw new NotImplementedException(); }
        }

        public Vector2 GameScreen
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        #endregion
    }
}