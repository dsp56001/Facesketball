using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace MonoGameLibrary.Particle
{
    public class Particle
    {
        public Vector2 position;
        public Vector2 velocity;
        public Vector2 acceleration;
       
        private float lifeTime;
        public float LifeTime { get { return this.lifeTime; } set { this.lifeTime = value; } }

        private float elapsedTime;
        public float ElapsedTime { get { return elapsedTime; } set { elapsedTime = value; } }

        private float scale;
        public float Scale { get { return scale; } set { scale = value; } }

        private float rotation;
        public float Rotation { get { return rotation; } set { rotation = value; } }

        private float rotationSpeed;
        public float RotationSpeed { get { return rotationSpeed; } set { rotationSpeed = value; } }

        public bool IsActive { get { return this.elapsedTime < this.lifeTime; } }   //Used as a Pooled object in a particle system

        public Particle()
        {

        }

        //used for delaying the initialization of the particles
        //So we can add them, then diffuse them
        public void Initialize(Vector2 position, Vector2 velocity, Vector2 acceleration, float lifetime, float scale, float rotationSpeed, float rotation)
        {
            this.position = position;
            this.velocity = velocity;
            this.acceleration= acceleration;
            this.lifeTime = lifetime;
            this.scale = scale;
            this.rotationSpeed = rotationSpeed;
            this.elapsedTime = 0.0f;
            this.Rotation = rotation;
        }


        //The update for the function which will be called by a manager
        public void Update(float time)
        {
            
            this.velocity += this.acceleration * time;
            this.position += this.velocity * time;
            this.rotation += this.rotationSpeed * time;

            this.elapsedTime += time;
        }
    }
}
