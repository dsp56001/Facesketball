using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameLibrary.Particle
{
    public class ParticleSystem
    {
        private Texture2D texture;
        private Vector2 origin;
        private int maxEffectSpawns;

        public int MaxEffectSpawns { get { return maxEffectSpawns; } }  //Spawns per frame

        private Particle[] particles;

        public Particle[] Particles {  get { return particles; } }

        private Queue<Particle> particleQueue;  //particles to be reused

        public Queue<Particle> ParticleQueue {  get { return particleQueue; } }

        private Random random;

        private int minNumParticles;
        private int maxNumParticles;
        public int MaxNumParticles {  get { return maxNumParticles; } }     //Total max particles
        public int MinNumParticles { get { return minNumParticles; } }  //min particles

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

        private float time;

        /// <summary>
        /// Initialize a particle system
        /// </summary>
        /// <param name="minNumParticles">min number of particles</param>
        /// <param name="maxNumParticles">max numver of particles</param>
        /// <param name="texture">texture for particles</param>
        /// <param name="minInitialSpeed"></param>
        /// <param name="maxInitialSpeed"></param>
        /// <param name="minAcceleration"></param>
        /// <param name="maxAcceleration"></param>
        /// <param name="minRotationSpeed"></param>
        /// <param name="maxRotationSpeed"></param>
        /// <param name="minLifetime"></param>
        /// <param name="maxLifetime"></param>
        /// <param name="minScale"></param>
        /// <param name="maxScale"></param>
        /// <param name="maxEffectSpawns">max number of particles to spawn per update</param>
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

        /// <summary>
        /// Fills Array and sets up the the Queue with particles the same size as the array
        /// </summary>
        public void PopulateQueue()
        {
            this.particles = new Particle[this.maxEffectSpawns * this.maxNumParticles];  //Array to hold max number of particles
            this.particleQueue = new Queue<Particle>(this.maxEffectSpawns * this.maxNumParticles);
            for (int i = 0; i < this.particles.Length; i++)
            {
                this.particles[i] = new Particle();
                this.particleQueue.Enqueue(particles[i]);
            }
        }

 
        public void Update(GameTime gameTime)
        {
            //if running @ 60 fps will be around .16 if not runing slowly
            time = (float)gameTime.ElapsedGameTime.Milliseconds / 100;

            if (enabled)
            {
                for (int i = 0; i < this.particles.Length; i++)
                {
                    //Update the particle if it is active
                    if (this.particles[i].IsActive == true)
                    {
                        
                        this.particles[i].Update(time);

                        //If that particle died on that update, put it back into the queue
                        //reuse the particles from the list
                        if (this.particles[i].IsActive == false)
                        {
                            this.particleQueue.Enqueue(this.particles[i]);
                        }
                    }
                }
            }
        }

        //Privates for draw used to fade out and scale up
        float normalizedLifetime; 
        float alpha, scale;
        Color color;
        /// <summary>
        /// Draws particles in particle systems
        /// </summary>
        /// <param name="spriteBatch">Expects an sptiteBatch that is begun</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            if (enabled)
            {
                
                for (int i = 0; i < this.particles.Length; i++)
                {
                    if (this.particles[i].IsActive == true)
                    {
                        normalizedLifetime = this.particles[i].ElapsedTime / this.particles[i].LifeTime;

                        alpha = 4 * normalizedLifetime * (1 - normalizedLifetime);
                        color = new Color(new Vector4(1, 1, 1, alpha));
                        scale = this.particles[i].Scale * (.75f + .25f * normalizedLifetime);

                        spriteBatch.Draw(texture, this.particles[i].position, null, color,
                            this.particles[i].Rotation, this.origin, scale, SpriteEffects.None, 0.0f);
                    }
                }
            }
        }

        int numParticles;
        Particle particle;
        /// <summary>
        /// Adds particles from queue
        /// </summary>
        /// <param name="position">starting position for particle</param>
        public void AddParticles(Vector2 position)
        {
            numParticles = this.random.Next(minNumParticles, maxNumParticles);
            //if there are particles in the queue add the chosen number of particles
            for (int i = 0; i < numParticles && this.particleQueue.Count > 0; i++)
            {
                particle = this.particleQueue.Dequeue();
                this.InitializeParticle(particle, position);
            }
        }

        /// <summary>
        /// Adds particles from queue
        /// </summary>
        /// <param name="position">starting position for particleparam>
        /// <param name="direction">starting direction for particle</param>
        public void AddParticles(Vector2 position, Vector2 direction)
        {
            numParticles = this.random.Next(minNumParticles, maxNumParticles);
            for (int i = 0; i < numParticles && this.particleQueue.Count > 0; i++)
            {
                particle = this.particleQueue.Dequeue();
                this.InitializeParticle(particle, position, direction);
            }
        }

        private void InitializeParticle(Particle particle, Vector2 position) 
        {
            InitializeParticle(particle, position, PickRandomDirection());
        }

        float velocity, acceleration, lifetime, rotationSpeed;
        Vector2 rdirection;
        private void InitializeParticle(Particle particle, Vector2 position, Vector2 direction)
        {

            rdirection = PickRandomDirection();
            direction = Vector2.Add(direction, Vector2.Multiply( rdirection, 1.5f));
            Vector2.Normalize(direction);
            
            velocity = this.RNext(minInitialSpeed, maxInitialSpeed);
            acceleration = this.RNext(minAcceleration, maxAcceleration);
            lifetime = this.RNext(minLifetime, maxLifetime);
            scale = this.RNext(minScale, maxScale);
            rotationSpeed = this.RNext(minRotationSpeed, maxRotationSpeed);

            particle.Initialize(position, velocity * direction, acceleration * direction, lifetime, scale, rotationSpeed, this.RNext(0, MathHelper.TwoPi));
        }

        float angle;
        protected virtual Vector2 PickRandomDirection()
        {
            angle = this.RNext(0, MathHelper.TwoPi);
            return new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
        }

        private float RNext(float min, float max)
        {
            return min + (float)this.random.NextDouble() * (max - min);
        }
    }
}
