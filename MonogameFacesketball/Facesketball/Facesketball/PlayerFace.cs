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
    public class PlayerFace : DrawableSprite
    {
        List<Vector2> partPoints;
        
        public PlayerFace(Game game)
            : base(game)
        {
            // TODO: Construct any child components here
            
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here
            partPoints = new List<Vector2>();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            this.spriteTexture = Game.Content.Load<Texture2D>("RedGhost");
            //this.spriteTexture = new Texture2D(Game.GraphicsDevice, 100, 100);

            // Extract collision data
            this.SpriteTextureData =
                new Color[this.spriteTexture.Width * this.spriteTexture.Height];
            this.spriteTexture.GetData(this.SpriteTextureData);

            base.LoadContent();
            //this.Orgin = new Vector2(this.spriteTexture.Width / 2, this.spriteTexture.Height / 2);
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here
            //ParticleManager.Instance().ParticleSystems["motionparticles"].AddParticles(this.Location);
            
            base.Update(gameTime);
        }

        public void GetPointsFromRect(Rectangle r, ref List<Vector2> points)
        {
            points.Clear();
            points.Add(new Vector2(r.Top, r.Left));
            points.Add(new Vector2(r.Top, r.Right));
            points.Add(new Vector2(r.Bottom, r.Left));
            points.Add(new Vector2(r.Bottom, r.Right));
        }
    }
}