#region File Description
//-----------------------------------------------------------------------------
// SpriteEntity.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

#endregion

namespace MonoGameLibrary.ThreeD
{
    /// <summary>
    /// Base class for game entities that are displayed as billboard sprites,
    /// and which can emit 3D sounds. 
    /// </summary>
    public abstract class QuadSprite : DrawableGameComponent
    {
        #region Properties


        protected ContentManager content;
        float scale;
        Vector3 position;
        Vector3 forward;
        Vector3 up;
        Vector3 velocity;
        Texture2D texture;

        public float Scale { get { return scale; } set { scale = value; } }

        /// <summary>
        /// Gets or sets the 3D position of the entity.
        /// </summary>
        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        /// <summary>
        /// Gets or sets which way the entity is facing.
        /// </summary>
        public Vector3 Forward
        {
            get { return forward; }
            set { forward = value; }
        }

        /// <summary>
        /// Gets or sets the orientation of this entity.
        /// </summary>
        public Vector3 Up
        {
            get { return up; }
            set { up = value; }
        }
        
        /// <summary>
        /// Gets or sets how fast this entity is moving.
        /// </summary>
        public Vector3 Velocity
        {
            get { return velocity; }
            protected set { velocity = value; }
        }

        /// <summary>
        /// Gets or sets the texture used to display this entity.
        /// </summary>
        public Texture2D Texture
        {
            get { return texture; }
            set { texture = value; }
        }

        #endregion

        public QuadSprite(Game game)
            : base(game)
        {
            this.content = game.Content;            
        }

        protected override void LoadContent()
        {
            
            base.LoadContent();
        }


        

        /// <summary>
        /// Updates the position of the entity, and allows it to play sounds.
        /// </summary>
        /// 
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        /// <summary>
        /// Draws the entity as a billboard sprite.
        /// </summary>
        public void Draw(QuadDrawer quadDrawer, Vector3 cameraPosition,
                         Matrix view, Matrix projection)
        {
            Matrix world = Matrix.CreateTranslation(0, 1, 0) *
                           Matrix.CreateScale(scale) *
                           Matrix.CreateConstrainedBillboard(Position, cameraPosition,
                                                             Up, null, null);

            quadDrawer.DrawQuad(Texture, 1, world, view, projection);
        }
    }
}
