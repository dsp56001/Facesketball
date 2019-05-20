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


namespace Facesketball
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class FoatingBallManager : Microsoft.Xna.Framework.DrawableGameComponent
    {

        List<FloatingBall> floatingBalls;
        public List<FloatingBall> FloatingBalls { get { return floatingBalls; } }
        Random r;
        
        public FoatingBallManager(Game game)
            : base(game)
        {
            // TODO: Construct any child components here
            this.floatingBalls = new List<FloatingBall>();
            r = new Random();
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
            for (int i = 0; i < 4; i++)
            {
                AddBall();

                //Make a particle system for each FloatingBall
                ParticleManager.Instance().ParticleSystems.Add("motionparticles" + i,
                new ParticleSystem(10, 100,
                    Game.Content.Load<Texture2D>("GhostHit"), 
                    2, 7, //Speed
                    1, 3, //Accel
                    1, 10, //Rot
                    2.5f, 4.0f, //Life
                    0.1f, 0.7f, //Scale
                    10));
            }
            base.LoadContent();
        }

        private void AddBall()
        {
            FloatingBall fb = new FloatingBall(Game);
            fb.Initialize();
            fb.Location = GetLocation();
            fb.SetTranformAndRect();
            //no overlapping
            foreach (FloatingBall f in floatingBalls)
            {
                while (fb.Intersects(f))
                {
                    fb.Location = GetLocation();
                    fb.SetTranformAndRect();
                }
            }
            fb.Scale = 1.0f;
            fb.Enabled = true;
            fb.Visible = true;
            floatingBalls.Add(fb);
        }

        private Vector2 GetLocation()
        {
            
            Vector2 loc;
            loc.X = r.Next(Game.Window.ClientBounds.Width);
            loc.Y = r.Next(Game.Window.ClientBounds.Height);
            return loc;
            
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            //Elapsed time since last update
            float time = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            List<FloatingBall> colList = floatingBalls;
            foreach (FloatingBall fb in floatingBalls)
            {
                if (fb.Enabled)
                {
                    if (fb.Direction.Length() > 0)
                    {
                        if (fb.particlesEnabled)
                        {
                            ParticleManager.Instance().ParticleSystems["motionparticles" + floatingBalls.IndexOf(fb)].AddParticles(
                                new Vector2(fb.Location.X + fb.LocationRect.Width / 2,
                                    fb.Location.Y + fb.LocationRect.Height / 2),
                                Vector2.Negate(fb.GravityDir));

                        }
                        ParticleManager.Instance().ParticleSystems["motionparticles" + floatingBalls.IndexOf(fb)].Update(0.1f);
                        //FloatingBall Collision
                        foreach (FloatingBall f in colList)
                        {
                            //No collideing with self
                            if (!(fb == f))
                            {
                                if (fb.Intersects(f))
                                {
                                    fb.particlesEnabled = false;
                                    while (fb.Intersects(f))
                                    {
                                        fb.Location = GetLocation();
                                        fb.SetTranformAndRect();
                                    }


                                }
                            }
                        }
                    }
                    fb.Update(gameTime);
                }
 
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            
            foreach (FloatingBall fb in floatingBalls)
            {
                if(fb.Visible)
                {
                fb.Draw(gameTime);
                }
            }
            base.Draw(gameTime);
        }
    }
}