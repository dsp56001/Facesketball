using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace MonoGameLibrary.Util
{

    public interface IScoreService
    {
        //not the right way to do interfaces.
    }
    
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class ScoreService : Microsoft.Xna.Framework.DrawableGameComponent, IScoreService
    {

        public int CurrentScore { get; set; }
        public Vector2 ScoreLoc { get; set; } 
        SpriteFont font;
        SpriteBatch sb;
        
        public ScoreService(Game game)
            : base(game)
        {
            
            game.Services.AddService(typeof(IScoreService), this);
        }

        protected override void LoadContent()
        {
            CurrentScore = 0;
            sb = new SpriteBatch(Game.GraphicsDevice);
            font = Game.Content.Load<SpriteFont>("Arial");
            ScoreLoc = new Vector2(100, 100);
            base.LoadContent();
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            

            base.Initialize();
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
            sb.Begin();
            sb.DrawString(font, "Score: " + this.CurrentScore, ScoreLoc, Color.White);
            sb.End();
            base.Draw(gameTime);
        }
    }
}
