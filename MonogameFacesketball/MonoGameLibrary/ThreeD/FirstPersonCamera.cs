
#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
#endregion

namespace MonoGameLibrary.ThreeD
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public partial class FirstPersonCamera : Camera
    {
        public FirstPersonCamera(Game game)
            : base(game)
        {
            
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            
            //reset movement vector
            movement = Vector3.Zero;

            if (input.KeyboardState.IsKeyDown(Keys.A) ||
                (input.GamePads[0].ThumbSticks.Left.X < 0))
            {
                movement.X--;
            }
            if (input.KeyboardState.IsKeyDown(Keys.D) ||
                (input.GamePads[0].ThumbSticks.Left.X > 0))
            {
                movement.X++;
            }

            if (input.KeyboardState.IsKeyDown(Keys.S) ||
                (input.GamePads[0].ThumbSticks.Left.Y < 0))
            {
                movement.Z++;
            }
            if (input.KeyboardState.IsKeyDown(Keys.W) ||
                (input.GamePads[0].ThumbSticks.Left.Y > 0))
            {
                movement.Z--;
            }

            //make sure we don't increase speed if pushing up and over (diagonal)
            if (movement.LengthSquared() != 0)
                movement.Normalize();

            base.Update(gameTime);
        }
    }
}


