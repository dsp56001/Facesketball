
#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
#endregion

namespace IntroGameLibrary.Util
{
    /// <summary>
    /// This is a game component that implements DrawableGameComponent.
    /// It's to profile performance
    /// </summary>
     

    public sealed class FPS : Microsoft.Xna.Framework.DrawableGameComponent
    {
        private float fps;
        private float updateInterval = 1.0f;
        private float timeSinceLastUpdate = 0.0f;
        private float framecount = 0;

        private bool updateTimeFixed;
        private bool synchronizeWithVerticalRetrace;

        public FPS(Game game, bool synchWithVerticalRetrace, bool isFixedTimeStep)
            : this(game, synchWithVerticalRetrace, isFixedTimeStep,
                   game.TargetElapsedTime) { }
        
        public FPS(Game game) : this(game, false, true) { }

        public FPS(Game game, bool synchWithVerticalRetrace,
                   bool isFixedTimeStep, TimeSpan targetElapsedTime)
            : base(game)
        {
            GraphicsDeviceManager graphics =
                (GraphicsDeviceManager)Game.Services.GetService(
                typeof(IGraphicsDeviceManager));

            graphics.SynchronizeWithVerticalRetrace = synchWithVerticalRetrace;
            Game.IsFixedTimeStep = isFixedTimeStep;
            Game.TargetElapsedTime = targetElapsedTime;

            updateTimeFixed = Game.IsFixedTimeStep;
        }

        public void ToggleTimeFixed()
        {
            if (updateTimeFixed)
            {
                updateTimeFixed = false;
                
            }
            else
            {
                updateTimeFixed = true;

            }
            GraphicsDeviceManager graphics =
            (GraphicsDeviceManager)Game.Services.GetService(
            typeof(IGraphicsDeviceManager));

            Game.IsFixedTimeStep = updateTimeFixed;
        }

        public void ToggleSynchronizeWithVerticalRetrace()
        {
            if (synchronizeWithVerticalRetrace)
            {
                synchronizeWithVerticalRetrace = false;
            }
            else
            {
                synchronizeWithVerticalRetrace = true;
            }

            GraphicsDeviceManager graphics =
                (GraphicsDeviceManager)Game.Services.GetService(
                typeof(IGraphicsDeviceManager));

            graphics.SynchronizeWithVerticalRetrace = synchronizeWithVerticalRetrace;
        }

        /// <summary>
        /// Allows the game component to perform any initialization 
        /// it needs to before starting to run.  This is where it can query for
        /// any required services and load content.
        /// </summary>
        public sealed override void Initialize()
        {
            // TODO: Add your initialization code here
            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides snapshot of timing values.</param>
        public sealed override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here

            base.Update(gameTime);
        }

        public sealed override void Draw(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedRealTime.TotalSeconds;
            framecount++;
            timeSinceLastUpdate += elapsed;
            if (timeSinceLastUpdate > updateInterval)
            {
                fps = framecount / timeSinceLastUpdate;

#if XBOX360
                System.Diagnostics.Debug.WriteLine("FPS: " + fps.ToString());
#else
                Game.Window.Title = "FPS: " + fps.ToString();
#endif
                framecount = 0;
                timeSinceLastUpdate -= updateInterval;
            }
            base.Draw(gameTime);
        }

        
    }
}
