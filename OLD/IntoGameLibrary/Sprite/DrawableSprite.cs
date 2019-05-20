using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;

namespace IntroGameLibrary.Sprite
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// Each sprite has it's own spritebatch this is not efficient but it's easy to use
    /// </summary>
    public class DrawableSprite : Sprite
    {
        
        protected SpriteBatch spriteBatch;
        
        public DrawableSprite(Game game)
            : base(game)
        {
            // TODO: Construct any child components here
            //content = game.Content;
            
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here
            //graphics = (GraphicsDeviceManager)Game.Services.GetService(typeof(IGraphicsDeviceManager));
            base.Initialize();
        }

        protected override void LoadContent()
        {

            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
            base.LoadContent();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

       

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            this.Draw(spriteBatch);
            spriteBatch.End();
            //base.Draw(gameTime);
        }
    }
}