using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MonoGameLibrary.Sprite;

namespace Facesketball
{
    public class FaceChaser : DrawableSprite
    {
        //Texture2D Image;
        Vector2 Target;
        //float Scale;
        //public Vector2 Position { get; set; }
        public Vector2 ChaseSpeed { get; set; }
        float targetScale { get; set; }
        float scaleSpeed, scaleMin;

        PlayerFace playerFace;

        public FaceChaser(Game game)
            : base(game)
        {
            playerFace = ((Game1)game).FaceTracker;
            this.scaleSpeed = .02f;
            this.scaleMin = .2f;
        }


        //public FaceChaser(Game game, string filepath)
        //{
        //    Image = game.Content.Load<Texture2D>(filepath);
        //    Scale = 1f;
        //}

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
            this.spriteTexture = Game.Content.Load<Texture2D>("PurpleGhost");
            //this.spriteTexture = new Texture2D(Game.GraphicsDevice, 100, 100);

            this.Origin = new Vector2(this.spriteTexture.Width / 2, this.spriteTexture.Height / 2);
            // Extract collision data
            this.SpriteTextureData =
                new Color[this.spriteTexture.Width * this.spriteTexture.Height];
            this.spriteTexture.GetData(this.SpriteTextureData);
            base.LoadContent();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here
            
            Target = playerFace.Location;

            if (playerFace.Enabled)
            {
                targetScale = playerFace.Scale;
                if (targetScale < Scale) Scale -= this.scaleSpeed;
                if (targetScale > Scale) Scale += this.scaleSpeed;
            }
            else
            {
                targetScale = this.scaleMin;
                if (targetScale < Scale) Scale -= this.scaleSpeed;
                if (targetScale > Scale) Scale += this.scaleSpeed;
            }

            if (this.Scale <= this.scaleMin + .01f)
            {
                this.Visible = false;
            }
            else
            {
                this.Visible = true;
            }
            
            
            if (Target.X < this.Location.X) this.Location -= new Vector2(ChaseSpeed.X, 0f);
            else if (Target.X > this.Location.X) Location += new Vector2(ChaseSpeed.X, 0f);

            if (Target.Y < this.Location.Y) Location -= new Vector2(0f, ChaseSpeed.Y);
            else if (Target.Y > this.Location.Y) Location += new Vector2(0f, ChaseSpeed.Y);

            base.Update(gameTime);
            
        }
        
        
    }
}
