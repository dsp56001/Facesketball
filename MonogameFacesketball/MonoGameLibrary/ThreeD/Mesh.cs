using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;

namespace MonoGameLibrary.ThreeD
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class Mesh : Microsoft.Xna.Framework.DrawableGameComponent
    {
        protected Model model;

        protected ContentManager content;
        protected GraphicsDeviceManager graphics;
        Camera camera;

        protected Vector3 location, direction, rotation;
        protected float scale;
        protected float yaw, pitch, roll;

        //Bounding Sphere for collision
        BoundingSphere meshBoundingSphere;
        public BoundingSphere MeshBoundingSphere
        {
            get { return meshBoundingSphere; }
        }


        protected string modelName;

        public string ModelName
        {
            get { return this.modelName; }
            set { this.modelName = value; }
        }

        public Vector3 Location
        {
            get { return this.location; }
            set { this.location = value; }
        }

        public Vector3 Direction
        {
            get { return this.direction; }
            set { this.direction = value; }
        }

        public Vector3 Rotation
        {
            get { return this.rotation; }
            set { this.rotation = value; }
        }
        public float Scale
        {
            get { return this.scale; }
            set { this.scale = value; }
        }
        public float Yaw
        {
            get { return this.yaw; }
            set { this.yaw = value; }
        }
        public float Pitch
        {
            get { return this.pitch; }
            set { this.pitch = value; }
        }
        public float Roll
        {
            get { return this.roll; }
            set { this.roll = value; }
        }

        //Draw with BasicEffects default lighting
        public bool DefaultLighting { get; set; }


        public Mesh(Game game)
            : this(game, "monkey")
        {
        }

        public Mesh(Game game, string modelName)
            : base(game)
        {
            
            graphics = (GraphicsDeviceManager)Game.Services.GetService(typeof(IGraphicsDeviceManager));
            camera = (Camera)Game.Services.GetService(typeof(ICamera));
            this.modelName = modelName;
            this.scale = 1.0f;
            content = game.Content;
            this.meshBoundingSphere = new BoundingSphere();
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
            
            //Elapsed time since last update
            float time = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            
            this.yaw += Rotation.Y * (time/600);
            this.pitch += Rotation.X * (time / 1000);
            this.Roll += Rotation.Z * (time / 1000);

            this.location += this.direction * (time / 1000);
            base.Update(gameTime);
        }

        protected override void LoadContent()
        {
            model = content.Load<Model>(this.modelName);
            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            this.DrawModel(camera);
            base.Draw(gameTime);
        }

        private void DrawModel(Camera camera)
        {
            Matrix[] transforms = new Matrix[model.Bones.Count];
            float aspectRatio = this.Game.GraphicsDevice.Viewport.Width /
                                    this.Game.GraphicsDevice.Viewport.Height;
            model.CopyAbsoluteBoneTransformsTo(transforms);

            Matrix projection = camera.Projection;
            Matrix view = camera.View;

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    if (DefaultLighting)
                    {
                        effect.EnableDefaultLighting();
                    }
                    else
                    {
                        effect.Alpha = 1.0f;
                        effect.DiffuseColor = new Vector3(.33f, 0.0f, .33f);
                        effect.SpecularColor = new Vector3(0.25f, 0.25f, 0.25f);
                        effect.SpecularPower = 5.0f;
                        effect.AmbientLightColor = new Vector3(0.75f, 0.75f, 0.75f);

                        effect.DirectionalLight0.Enabled = true;
                        effect.DirectionalLight0.DiffuseColor = Vector3.One;
                        effect.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(1.0f, -1.0f, -1.0f));
                        effect.DirectionalLight0.SpecularColor = Vector3.One;

                        effect.DirectionalLight1.Enabled = true;
                        effect.DirectionalLight1.DiffuseColor = new Vector3(0.5f, 0.5f, 0.5f);
                        effect.DirectionalLight1.Direction = Vector3.Normalize(new Vector3(-1.0f, -1.0f, 1.0f));
                        effect.DirectionalLight1.SpecularColor = new Vector3(0.5f, 0.5f, 0.5f);

                        effect.LightingEnabled = true;
                    }

                    effect.View = view;
                    effect.Projection = projection;

                    Matrix world = mesh.ParentBone.Transform;   //i

                    world *= Matrix.CreateScale(this.scale);   //s
                    //r
                    world *= Matrix.CreateFromYawPitchRoll(MathHelper.ToRadians(Yaw),
                        MathHelper.ToRadians(Pitch),
                        MathHelper.ToRadians(Roll));
                    //o
                    //t
                    world *= Matrix.CreateTranslation(this.location);

                    effect.World = world;

                }
                mesh.Draw();
            }
        }

        private void DrawModel()
        {
            Matrix[] transforms = new Matrix[model.Bones.Count];
            //float aspectRatio = 640.0f / 480.0f;
            float aspectRatio = this.Game.GraphicsDevice.Viewport.Width /
                                    this.Game.GraphicsDevice.Viewport.Height;
            model.CopyAbsoluteBoneTransformsTo(transforms);
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(
                               MathHelper.ToRadians(45.0f), aspectRatio, 1.0f, 100.0f);
            Matrix view = Matrix.CreateLookAt(
                                new Vector3(0.0f, 5.0f, 5.0f), Vector3.Zero, Vector3.Up);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();

                    effect.View = view;
                    effect.Projection = projection;
                    effect.World = mesh.ParentBone.Transform;
                }
                mesh.Draw();
            }
        }
    }


    
}