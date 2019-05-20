using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Storage;


namespace Facesketball
{
    sealed partial class FoodManager : DrawableGameComponent
    {
        Food[,] FoodList;
        Point FoodSize, Offset;
        SpriteBatch spriteBatch;
        string Filepath;
        Texture2D FoodImage;

        uint Score; //Try to keep score
        Vector2 scoreLocation;
        SpriteFont font;


        internal FoodManager(Game game, Point foodsize, Point offset) : base(game)
        {
            FoodSize = foodsize;
            game.Components.Add(this);
            game.Services.AddService(typeof(FoodManager), this);
            Offset = offset;
            this.scoreLocation = new Vector2(10, 10);
            Score = 0;
        }
        internal FoodManager(Game game, string foodImagePath, Point offset)
            : this(game, new Point(0, 0), offset)
        {
            Filepath = foodImagePath;
        }

        public override void Initialize()
        {

            base.Initialize();
        }

        protected override void LoadContent()
        {
            font = Game.Content.Load<SpriteFont>("SpriteFontScore");
            spriteBatch = new SpriteBatch(Game.GraphicsDevice);
            FoodImage = Game.Content.Load<Texture2D>(Filepath);
            FoodSize = new Point(FoodImage.Width, FoodImage.Height);
            float width = spriteBatch.GraphicsDevice.Viewport.Width / (FoodSize.X + Offset.X), height = spriteBatch.GraphicsDevice.Viewport.Height / (FoodSize.Y + Offset.Y);
            FoodList = new Food[(int)width, (int)height];
            SetGrid();
            base.LoadContent();
        }

        internal void SetGrid()
        {
            float width = spriteBatch.GraphicsDevice.Viewport.Width / (FoodSize.X + Offset.X), height = spriteBatch.GraphicsDevice.Viewport.Height / (FoodSize.Y + Offset.Y);
            for (int row = 0; row < height; row++)
                for (int column = 0; column < width; column++)
                {
                    Food tempFood = new Food(Game, FoodImage, Color.GhostWhite);
                    tempFood.Location = new Vector2(column * FoodSize.X + (Offset.X * column), row * FoodSize.Y + (Offset.Y * row));
                    FoodList[column, row] = tempFood;
                }
        }

        public override void Update(GameTime gameTime)
        {
            if (FoodList.Length == 0) SetGrid();
            base.Update(gameTime);
            for (int i = 0; i < FoodList.GetLength(0); i++)
            {
                for (int j = 0; j < FoodList.GetLength(1); j++)
                {
                    if (FoodList[i, j].Visible) return;
                }
            }
            RefreshGrid();
        }
        internal void RefreshGrid()
        {
            for (int i = 0; i < FoodList.GetLength(0); i++)
            {
                for (int j = 0; j < FoodList.GetLength(1); j++)
                {
                    FoodList[i, j].Visible = true;
                }
            }
        }
        internal void UpdatePoints(Rectangle location)
        {
            for (int i = 0; i < FoodList.GetLength(0); i++)
            {
                for (int j = 0; j < FoodList.GetLength(1); j++)
                {
                    FoodList[i, j].Update();
                    if (FoodList[i, j].LocationRect.Intersects(location) && FoodList[i,j].Visible)
                    {
                        Score++;
                        FoodList[i, j].Visible = false;
                    }
                }
            }
        }
        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();

            foreach (Food food in FoodList) 
                if(food.Visible) food.Draw(spriteBatch);
            spriteBatch.DrawString(font, Score.ToString(), scoreLocation, Color.Violet);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        public void Addscore()
        {
            this.Score++;
        }
    }
}