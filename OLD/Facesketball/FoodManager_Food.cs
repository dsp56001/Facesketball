using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IntroGameLibrary.Sprite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Facesketball
{
    sealed partial class FoodManager
    {
        sealed private class Food : Sprite
        {
            internal Rectangle Bounds { get { return new Rectangle((int)Location.X, (int)Location.Y, spriteTexture.Width, spriteTexture.Height); } }
            internal Food(Game game, Point size, Color tint)
                : base(game)
            {
                //this.spriteTexture = new Microsoft.Xna.Framework.Graphics.Texture2D(game.GraphicsDevice, size.X, size.Y, 0, TextureUsage.None, SurfaceFormat.Color);
                //Color[] colors = new Color[size.X * size.Y];
                //for (int i = 0; i < colors.Length; i++) colors[i] = tint;
                //this.spriteTexture.SetData<Color>(colors);
            }

            protected override void LoadContent()
            {
                //this.spriteTexture = Game.Content.Load<Texture2D>("20px_1trans");
                //base.LoadContent();
            }

            internal Food(Game game, Texture2D image, Color tint)
                : base(game)
            {
                this.spriteTexture = image;
            }
            internal void Update()
            {
                locationRect = new Rectangle((int)Location.X, (int)Location.Y, spriteTexture.Width, spriteTexture.Height);
            }
        }
    }
}
