
#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

#endregion

namespace MonoGameLibrary
{
    /// <summary>
    /// Interface for CelAnimationManager
    /// </summary>
    public interface ICelAnimationManager { }

    /// <summary>
    /// A game service that manages animations.
    /// </summary>
    public sealed partial class CelAnimationManager : Microsoft.Xna.Framework.GameComponent, ICelAnimationManager
    {
        //Dictionary that contains a string key name for each animation
        private Dictionary<string, CelAnimation> animations = new Dictionary<string, CelAnimation>();
        //Dictionary that contains a string key name for each Texture
        private Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
        
        public Dictionary<string, CelAnimation> Animations { get { return animations; } }
        
        public CelAnimationManager(Game game)
            : base(game)
        {
            //Add CellAnimationManager to game as a service (singleton) using the Interface ICelAnimationManager as the type
            game.Services.AddService(typeof(ICelAnimationManager), this);
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// Adds an animation to the CellAnimation Manager
        /// </summary>
        /// <param name="animationKey">Key for Animation Namr</param>
        /// <param name="textureName">Texture2D Name from Pipeline tool</param>
        /// <param name="celCount">Number of Cells in Animation. CelCount has Number of Rows and Number of Columns</param>
        /// <param name="framesPerSecond">Frame Rate to play back animation in frames per secong</param>
        public void AddAnimation(string animationKey, string textureName,
            CelCount celCount, int framesPerSecond)
        {
            //Make sure texture is unique
            if (!textures.ContainsKey(textureName))
            {
                //Add texture to Dictionary using Texture Name
                textures.Add(textureName, this.Game.Content.Load<Texture2D>(textureName));
            }

            int celWidth = (int)(textures[textureName].Width / celCount.NumberOfColumns); //find cell width by diving Texture Width by number Columns
            int celHeight = (int)(textures[textureName].Height / celCount.NumberOfRows);  // find cell height by diving Texture Width by number of Rows

            int numberOfCels = celCount.NumberOfColumns * celCount.NumberOfRows;

            //we create a cel range by passing in start location of 1,1
            //and end with number of column and rows
            //2,1  =   1,1,2,1  ;    4,2  =  1,1,4,2

            AddAnimation(animationKey, textureName,
                new CelRange(1, 1, celCount.NumberOfColumns, celCount.NumberOfRows),
                celWidth, celHeight, numberOfCels,
                framesPerSecond);
        }

        /// <summary>
        /// Adds an animation to the CellAnimation Manager using a different Texture Key and Texture2D
        /// </summary>
        /// <param name="animationKey">Key for Animation Namr</param>
        /// <param name="textureName">Texture2D Name from Pipeline tool</param>
        /// <param name="celCount">Number of Cells in Animation. CelCount has Number of Rows and Number of Columns</param>
        /// <param name="framesPerSecond">Frame Rate to play back animation in frames per secong</param>
        /// <param name="texture">Texture 2D to load</param>
        
        public void AddAnimation(string animationKey, string textureName,
            Texture2D texture, CelCount celCount, int framesPerSecond)
        {
            if (texture == null) this.Game.Content.Load<Texture2D>(textureName);
            if (!textures.ContainsKey(textureName))
            {
                
                textures.Add(textureName, texture);
            }

            int celWidth = (int)(textures[textureName].Width / celCount.NumberOfColumns);
            int celHeight = (int)(textures[textureName].Height / celCount.NumberOfRows);

            int numberOfCels = celCount.NumberOfColumns * celCount.NumberOfRows;

            //we create a cel range by passing in start location of 1,1
            //and end with number of column and rows
            //2,1  =   1,1,2,1  ;    4,2  =  1,1,4,2

            AddAnimation(animationKey, textureName,
                new CelRange(1, 1, celCount.NumberOfColumns, celCount.NumberOfRows),
                celWidth, celHeight, numberOfCels,
                framesPerSecond);
        }

        public void AddAnimation(string animationKey, string textureName,
            CelRange celRange, int celWidth, int celHeight,
            int numberOfCels, int framesPerSecond)
        {
            CelAnimation ca = new CelAnimation(textureName, celRange, framesPerSecond);

            if (!textures.ContainsKey(textureName))
            {
                textures.Add(textureName, this.Game.Content.Load<Texture2D>(textureName));
            }

            ca.CelWidth = celWidth;
            ca.CelHeight = celHeight;

            ca.NumberOfCels = numberOfCels;

            ca.CelsPerRow = textures[textureName].Width / celWidth;

            if (animations.ContainsKey(animationKey))
                animations[animationKey] = ca;
            else
                animations.Add(animationKey, ca);
        }

        /// <summary>
        /// Pauses and starts and animation
        /// </summary>
        /// <param name="animationKey">Name of animation to pause</param>
        /// <param name="paused"></param>
        public void ToggleAnimation(string animationKey, bool paused)
        {
            if (animations.ContainsKey(animationKey))
                animations[animationKey].Paused = paused;
        }

        /// <summary>
        /// Rewinds and resets loop count on an animation
        /// </summary>
        /// <param name="animationKey">Name of animation to Reset</param>
        public void ResetAnimation(string animationKey)
        {
            if(animations.ContainsKey(animationKey))
            {
               animations[animationKey].Frame = animations[animationKey].StillFrame;
               animations[animationKey].Paused = true;
               animations[animationKey].LoopCount = 0;
            }
        }
        /// <summary>
        /// Pauses and starts and animation
        /// </summary>
        /// <param name="animationKey">Name of animation to pause</param>
        public void ToggleAnimation(string animationKey)
        {
            if (animations.ContainsKey(animationKey))
                animations[animationKey].Paused = !animations[animationKey].Paused;
        }
        
        public override void Update(GameTime gameTime)
        {
            foreach (KeyValuePair<string, CelAnimation> animation in animations)
            {
                CelAnimation ca = animation.Value;

                if (ca.Paused)
                    continue; //no need to update this animation, check next one

                ca.TotalElapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (ca.TotalElapsedTime > ca.TimePerFrame)
                {                   
                    ca.Frame++;
                   //min: 0, max: total cels
                    if (ca.Frame >= ca.NumberOfCels)
                    {
                        ca.LoopCount++;
                    }
                    ca.Frame = ca.Frame % (ca.NumberOfCels);

                    //reset our timer
                    ca.TotalElapsedTime -= ca.TimePerFrame;
                }
            }

            base.Update(gameTime);
        }

        public void Draw(float elapsedTime, string animationKey, SpriteBatch batch, Vector2 position)
        {
            Draw(elapsedTime, animationKey, batch, animations[animationKey].Frame, position);
        }

        public void Draw(float elapsedTime, string animationKey, SpriteBatch batch, int frame, Vector2 position)
        {
            Draw(elapsedTime, animationKey, batch, animations[animationKey].Frame, position, Color.White);
        }

        public Rectangle GetCurrentDrawRect(float elapsedTime, string animationKey)
        {
            return GetCurrentDrawRect(elapsedTime, animationKey, 0.0f);
        }

        public Rectangle GetCurrentDrawRect(float elapsedTime, string animationKey, float scale)
        {
            if (!animations.ContainsKey(animationKey))
            {
                throw new Exception("animationKey not found");
            }

            CelAnimation ca = animations[animationKey];

            //first get our x increase amount (add our offset-1 to our current frame)
            int xincrease = (ca.Frame + ca.CelRange.FirstCelX - 1);
            //now we need to wrap the value so it will loop to the next row
            int xwrapped = xincrease % ca.CelsPerRow;
            //finally we need to take the product of our wrapped value and a cel's width
            int x = xwrapped * ca.CelWidth;

            //to determine how much we should increase y, we need to look at how much we
            //increased x and do an integer divide
            int yincrease = xincrease / ca.CelsPerRow;
            //now we can take this increase and add it to our Y offset-1 and multiply the sum by
            //our cel height
            int y = (yincrease + ca.CelRange.FirstCelY - 1) * ca.CelHeight;

            Rectangle cel = new Rectangle(x, y, 
                (int)(ca.CelWidth),
                (int)(ca.CelHeight));
            return cel;
        }


        public Texture2D GetTexture(string textureName)
        {
            if (!textures.ContainsKey(textureName))
            {
                throw new Exception("textureName not found");
            }

            return textures[textureName];
        }

        /// <summary>
        /// Get the current loop count for and aniamtion
        /// </summary>
        /// <param name="animationKey">Name of animation</param>
        /// <returns></returns>
        public int GetLoopCount(string animationKey)
        {
            CelAnimation ca = animations[animationKey];
            return ca.LoopCount;
        }


        public void Draw(float elapsedTime, string animationKey, 
            SpriteBatch batch, int frame, Vector2 position, Color color)
        {
            if (!animations.ContainsKey(animationKey))
                return;

            CelAnimation ca = animations[animationKey];

            Rectangle cel = this.GetCurrentDrawRect(elapsedTime, animationKey);

            //Vector2 orgin = new Vector2((ca.CelWidth / 2), (ca.CelHeight / 2));
            //batch.Draw(textures[ca.TextureName], position, cel, color, 0.0f, orgin, 1.0f, SpriteEffects.None, 0f);

            batch.Draw(textures[ca.TextureName], position, cel, color);
        }

        public void DrawBottomCenter(float elapsedTime, string animationKey, SpriteBatch batch, Vector2 position)
        {
            DrawBottomCenter(elapsedTime, animationKey, batch, animations[animationKey].Frame, position);
        }

        public void DrawBottomCenter(float elapsedTime, string animationKey, SpriteBatch batch, int frame, Vector2 position)
        {
            DrawBottomCenter(elapsedTime, animationKey, batch, animations[animationKey].Frame, position, Color.White);
        }

        public void DrawBottomCenter(float elapsedTime, string animationKey, SpriteBatch batch, int frame, Vector2 position, Color color)
        {
            if (!animations.ContainsKey(animationKey))
                return;

            CelAnimation ca = animations[animationKey];

            //first get our x increase amount (add our offset-1 to our current frame)
            int xincrease = (ca.Frame + ca.CelRange.FirstCelX - 1);
            //now we need to wrap the value so it will loop to the next row
            int xwrapped = xincrease % ca.CelsPerRow;
            //finally we need to take the product of our wrapped value and a cel's width
            int x = xwrapped * ca.CelWidth;

            //to determine how much we should increase y, we need to look at how much we
            //increased x and do an integer divide
            int yincrease = xincrease / ca.CelsPerRow;
            //now we can take this increase and add it to our Y offset-1 and multiply the sum by
            //our cel height
            int y = (yincrease + ca.CelRange.FirstCelY - 1) * ca.CelHeight;
            
            Rectangle cel = new Rectangle(x, y, ca.CelWidth, ca.CelHeight);

            Vector2 orgin = new Vector2((ca.CelWidth / 2),  (ca.CelHeight));
            batch.Draw(textures[ca.TextureName], position, cel, color, 0.0f, orgin, 1.0f, SpriteEffects.None, 0f);
           
            //batch.Draw(textures[ca.TextureName], position, cel, color);
        }   
    }
    public class CelAnimation
    {
        private string textureName;
        private CelRange celRange;
        private int framesPerSecond;
        private float timePerFrame;

        public float TotalElapsedTime = 0.0f;
        public int CelWidth;
        public int CelHeight;
        public int NumberOfCels;
        public int CelsPerRow;
        public int Frame;
        public int StillFrame;
        public bool Paused = false;

        private int loopCount;
        public int LoopCount { get { return loopCount; } set { loopCount = value; } }


        public CelAnimation(string textureName, CelRange celRange, int framesPerSecond)
        {
            this.textureName = textureName;
            this.celRange = celRange;
            this.framesPerSecond = framesPerSecond;
            this.timePerFrame = 1.0f / (float)framesPerSecond;
            this.Frame = 0;
            this.StillFrame = 0;
            this.loopCount = 0;
        }

        public string TextureName
        {
            get { return (textureName); }
        }

        public CelRange CelRange
        {
            get { return (celRange); }
        }

        public int FramesPerSecond
        {
            get { return (framesPerSecond); }
        }

        public float TimePerFrame
        {
            get { return (timePerFrame); }
        }
    }

    public struct CelCount
    {
        public int NumberOfColumns;
        public int NumberOfRows;

        public CelCount(int numberOfColumns, int numberOfRows)
        {
            NumberOfColumns = numberOfColumns;
            NumberOfRows = numberOfRows;
        }
    }

    public struct CelRange
    {
        public int FirstCelX;
        public int FirstCelY;
        public int LastCelX;
        public int LastCelY;

        public CelRange(int firstCelX, int firstCelY, int lastCelX, int lastCelY)
        {
            FirstCelX = firstCelX;
            FirstCelY = firstCelY;
            LastCelX = lastCelX;
            LastCelY = lastCelY;
        }
    }
}


