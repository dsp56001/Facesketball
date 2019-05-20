using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameLibrary.Particle
{
    /// <summary>
    /// Singleton manager that can mamage multiple particle systems
    /// </summary>
    public class ParticleManager
    {
        private static ParticleManager instance;        //static instnace for singleton
        private Dictionary<string, ParticleSystem> particleSystems; //Dictionary of particle systems managed by this class
        public Dictionary<string, ParticleSystem> ParticleSystems { get { return this.particleSystems; } }

        protected bool enabled;

        public bool Enabled
        {
            get { return enabled; }
            set
            {
                enabled = value;
                foreach (KeyValuePair<string, ParticleSystem> pair in particleSystems)
                {
                    pair.Value.Enabled = value;
                }
            }
        }

        private ParticleManager()
        {
            this.particleSystems = new Dictionary<string, ParticleSystem>();
            this.Enabled = true;
            
        }

        public static ParticleManager Instance()
        {
            if (instance != null)
            {
                return instance;
            }
            else
            {
                return (instance = new ParticleManager());
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (enabled)
            {
                foreach (KeyValuePair<string, ParticleSystem> pair in particleSystems)
                {
                    pair.Value.Draw(spriteBatch);
                }
            }
        }

        public static int GetActiveCount(string Key)
        {
            return instance.ParticleSystems[Key].Particles.Count(p => p.IsActive);
            
        }

        public static int GetMaxCount(string Key)
        {
            return instance.ParticleSystems[Key].MaxNumParticles;
        }
    }
}
