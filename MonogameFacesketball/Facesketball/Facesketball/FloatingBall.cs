using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using MonoGameLibrary.Sprite;

namespace Facesketball
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class FloatingBall : DrawableSprite
    {
        public Vector2 GravityDir; //BAD?
        public float GravityAccel;
        float SpeedMax;

        Texture2D NormalTexture, InfluencedTexture;

        public SpriteBatch Sb { get { return this.spriteBatch; } }
        public bool particlesEnabled;

        public FloatingBall(Game game)
            : base(game)
        {
            // TODO: Construct any child components here
            particlesEnabled = false;
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
            GravityDir = new Vector2(0, 0);
            GravityAccel = .007f;
            NormalTexture = Game.Content.Load<Texture2D>("TealGhost");
            InfluencedTexture = Game.Content.Load<Texture2D>("GhostHit");
            this.Speed = 2;
            this.SpeedMax = 5;
            this.Direction = new Vector2(0, 0);
            this.Location = new Vector2(200, 200);
            this.spriteTexture = NormalTexture;

            // Extract collision data
            this.SpriteTextureData =
                new Color[this.spriteTexture.Width * this.spriteTexture.Height];
            this.spriteTexture.GetData(this.SpriteTextureData);


            base.LoadContent();
        }

        public void SetTexture(bool influenced)
        {
            if (influenced)
            {
                this.spriteTexture = InfluencedTexture;
                // Extract collision data
                this.SpriteTextureData =
                    new Color[this.spriteTexture.Width * this.spriteTexture.Height];
                this.spriteTexture.GetData(this.SpriteTextureData);
            }
            else
            {
                this.spriteTexture = NormalTexture;
                // Extract collision data
                this.SpriteTextureData =
                    new Color[this.spriteTexture.Width * this.spriteTexture.Height];
                this.spriteTexture.GetData(this.SpriteTextureData);
            }
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            
            // TODO: Add your update code here
            //Elapsed time since last update
            float time = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            //Gravity and influence
            this.Direction += GravityDir;
            //PacManDir = PacManDir + GravityDir;

            //Slow down id not influence
            if (this.GravityDir == Vector2.Zero)
            {
                //slow down
                this.Speed = MathHelper.Clamp(this.Speed -GravityAccel, 0.0f, this.SpeedMax);
                this.SetTexture(false);
            }
            else
            {
                //Speed up
                this.Speed = MathHelper.Clamp(this.Speed + GravityAccel, 0.0f, this.SpeedMax);
                this.SetTexture(true);
            }



            //Time corrected move. 
            this.Location = this.Location + ((this.Direction * (time / 1000)) * this.Speed);  //Simple Move 

            //Keep PacMan On Screen
            //X right
            if (this.Location.X >
                    this.Game.GraphicsDevice.Viewport.Width - this.locationRect.Width)
            {
                //Negate X
                this.Direction = this.Direction * new Vector2(0, 1);
                this.Location.X = this.Game.GraphicsDevice.Viewport.Width - this.locationRect.Width;
            }

            //X left
            if (this.Location.X < 0)
            {
                //Negate X
                this.Direction = this.Direction * new Vector2(0, 1);
                this.Location.X = 0;
            }

            //Y top
            if (this.Location.Y >
                    this.Game.GraphicsDevice.Viewport.Height - this.locationRect.Height)
            {
                //Negate Y
                this.Direction = this.Direction * new Vector2(1, 0);
                this.Location.Y = this.Game.GraphicsDevice.Viewport.Height - this.locationRect.Height;
            }

            //Y bottom
            if (this.Location.Y < 0)
            {
                //Negate Y
                this.Direction = this.Direction * new Vector2(1, 0);
                this.Location.Y = 0;
            }

            //Disable Ghpst if it hasn't moved in a while
            if (this.Speed < .05f)
            {
                this.Visible = false;
                this.particlesEnabled = false;

            }
            else
            {
                this.Visible = true;
            }

            base.Update(gameTime);
        }
    }
}