using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Facesketball
{
    public class ParticleSystem
    {
        private Texture2D texture;
        private Vector2 origin;
        private int maxEffectSpawns;
        private Particle[] particles;
        private Queue<Particle> particleQueue;

        private Random random;

        private int minNumParticles;
        private int maxNumParticles;

        private float minInitialSpeed;
        private float maxInitialSpeed;

        private float minAcceleration;
        private float maxAcceleration;

        private float minRotationSpeed;
        private float maxRotationSpeed;

        private float minLifetime;
        private float maxLifetime;

        private float minScale;
        private float maxScale;

        private bool enabled;
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        public ParticleSystem(
            int minNumParticles, 
            int maxNumParticles, 
            Texture2D texture,
            float minInitialSpeed,
            float maxInitialSpeed,
            float minAcceleration,
            float maxAcceleration,
            float minRotationSpeed,
            float maxRotationSpeed,
            float minLifetime,
            float maxLifetime,
            float minScale,
            float maxScale,
            int maxEffectSpawns)
        {
            this.minNumParticles = minNumParticles;
            this.maxNumParticles = maxNumParticles;
            this.texture = texture;
            this.minInitialSpeed = minInitialSpeed;
            this.maxInitialSpeed = maxInitialSpeed;
            this.minAcceleration = minAcceleration;
            this.maxAcceleration = maxAcceleration;
            this.minRotationSpeed = minRotationSpeed;
            this.maxRotationSpeed = maxRotationSpeed;
            this.minLifetime = minLifetime;
            this.maxLifetime = maxLifetime;
            this.minScale = minScale;
            this.maxScale = maxScale;
            this.maxEffectSpawns = maxEffectSpawns;
            this.origin = new Vector2(this.texture.Width *.5f, this.texture.Height * .5f);
            this.random = new Random();
            this.enabled = true;
            this.PopulateQueue();
        }


        public void PopulateQueue()
        {
            this.particles = new Particle[this.maxEffectSpawns * this.maxNumParticles];
            this.particleQueue = new Queue<Particle>(this.maxEffectSpawns * this.maxNumParticles);
            for (int i = 0; i < this.particles.Length; i++)
            {
                this.particles[i] = new Particle();
                this.particleQueue.Enqueue(particles[i]);
            }
        }

 
        public void Update(float time)
        {
            if (enabled)
            {
                for (int i = 0; i < this.particles.Length; i++)
                {
                    //Update the particle if it is active
                    if (this.particles[i].IsActive == true)
                    {
                        this.particles[i].Update(time);

                        //If that particle died on that update, put it back into the queue
                        if (this.particles[i].IsActive == false)
                        {
                            this.particleQueue.Enqueue(this.particles[i]);
                        }
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (enabled)
            {
                spriteBatch.Begin(SpriteBlendMode.AlphaBlend);

                for (int i = 0; i < this.particles.Length; i++)
                {
                    if (this.particles[i].IsActive == true)
                    {
                        float normalizedLifetime = this.particles[i].ElapsedTime / this.particles[i].LifeTime;

                        float alpha = 4 * normalizedLifetime * (1 - normalizedLifetime);
                        Color color = new Color(new Vector4(1, 1, 1, alpha));

                        float scale = this.particles[i].Scale * (.75f + .25f * normalizedLifetime);

                        spriteBatch.Draw(texture, this.particles[i].position, null, color,
                            this.particles[i].Rotation, this.origin, scale, SpriteEffects.None, 0.0f);
                    }
                }

                spriteBatch.End();
            }
        }


        public void AddParticles(Vector2 position)
        {
            int numParticles = this.random.Next(minNumParticles, maxNumParticles);

          
            for (int i = 0; i < numParticles && this.particleQueue.Count > 0; i++)
            {
                Particle particle = this.particleQueue.Dequeue();
                this.InitializeParticle(particle, position);
            }
        }

        public void AddParticles(Vector2 position, Vector2 direction)
        {
            int numParticles = this.random.Next(minNumParticles, maxNumParticles);


            for (int i = 0; i < numParticles && this.particleQueue.Count > 0; i++)
            {
                Particle particle = this.particleQueue.Dequeue();
                this.InitializeParticle(particle, position, direction);
            }
        }


        private void InitializeParticle(Particle particle, Vector2 position) 
        {


            InitializeParticle(particle, position, PickRandomDirection());
          
        }

        private void InitializeParticle(Particle particle, Vector2 position, Vector2 direction)
        {

            Vector2 rdirection = PickRandomDirection();
            direction = Vector2.Add(direction, Vector2.Multiply( rdirection, 1.5f));
            Vector2.Normalize(direction);
            //direction = PickRandomDirection();

            float velocity = this.RNext(minInitialSpeed, maxInitialSpeed);
            float acceleration = this.RNext(minAcceleration, maxAcceleration);
            float lifetime = this.RNext(minLifetime, maxLifetime);
            float scale = this.RNext(minScale, maxScale);
            float rotationSpeed = this.RNext(minRotationSpeed, maxRotationSpeed);


            particle.Initialize(position, velocity * direction, acceleration * direction, lifetime, scale, rotationSpeed, this.RNext(0, MathHelper.TwoPi));
        }


        protected virtual Vector2 PickRandomDirection()
        {
            float angle = this.RNext(0, MathHelper.TwoPi);
            return new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
        }

        private float RNext(float min, float max)
        {
            return min + (float)this.random.NextDouble() * (max - min);
        }
    }
}
