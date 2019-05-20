using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;

namespace IntroGameLibrary.Sprite
{
    /// <summary>
    /// This is and Extension of Drawable sprite. Each Sprite has
    /// an animation adapter that can manage animations
    /// </summary>
    public class DrawableAnimatableSprite : DrawableSprite
    {

        protected SpriteAnimationAdapter spriteAnimationAdapter;
        
        public DrawableAnimatableSprite(Game game)
            : base(game)
        {
            // TODO: Construct any child components here
            //content = game.Content;
            spriteAnimationAdapter = new SpriteAnimationAdapter(game);
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here
            //graphics = (GraphicsDeviceManager)Game.Services.GetService(typeof(IGraphicsDeviceManager));
            base.Initialize();
        }

        protected override void LoadContent()
        {

            //spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
            
            base.LoadContent();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here
            //Elapsed time since last update
            lastUpdateTime = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            //GamePad1
            SpriteEffects = SpriteEffects.None;       //Default Sprite Effects
            this.spriteTexture = this.spriteAnimationAdapter.CurrentTexture;        //update texture for collision
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();

            Rectangle currentTextureRect = spriteAnimationAdapter.GetCurrentDrawRect(lastUpdateTime);

            this.locationRect = new Rectangle((int)Location.X - (int)this.Orgin.X,
                (int)Location.Y - (int)this.Orgin.Y,
                currentTextureRect.Width,
                currentTextureRect.Height);

            
            spriteBatch.Draw(spriteAnimationAdapter.CurrentTexture,
                new Rectangle((int)Location.X, (int)Location.Y,
                currentTextureRect.Width,
                currentTextureRect.Height),
                currentTextureRect,
                Color.White,
                MathHelper.ToRadians(Rotate),
                this.Orgin,
                SpriteEffects,
                0);
            

            base.DrawMarkers(spriteBatch);

            spriteBatch.End();
            //base.Draw(gameTime);
        }
    }


    public class SpriteAnimationAdapter
    {
        List<SpriteAnimation> spriteAnimations;
        protected SpriteAnimation currentAnimation;
        protected CelAnimationManager celAnimationManger;

        public Rectangle CurrentLocationRect
        {
            get
            {
                return this.GetCurrentDrawRect();
            }
        }

        public CelAnimationManager CelAnimationManager { get { return celAnimationManger;}}
        public SpriteAnimation CurrentAnimation {
            get { return currentAnimation; }
            set {
                    if(!(spriteAnimations.Contains(value)))
                    {
                        this.spriteAnimations.Add(value);
                    }
                        this.currentAnimation = value;
            }
        }
              
        public SpriteAnimationAdapter(Game game)
        {
            spriteAnimations = new List<SpriteAnimation>();
            
            celAnimationManger = (CelAnimationManager)game.Services.GetService(typeof(ICelAnimationManager));
            if (celAnimationManger == null)
            {
                throw new Exception("To use a DrawableAnimatedSprite you must a CelAnimationManager to the game as a service!");
            }
            
        }

        public Texture2D CurrentTexture
        {
            get { return celAnimationManger.GetTexture(currentAnimation.TextureName); }
        }

        public void AddAnimation(SpriteAnimation s)
        {
            this.spriteAnimations.Add(s);
            this.celAnimationManger.AddAnimation(s.AnimationName, s.TextureName, s.CellCount, s.FPS);
            this.celAnimationManger.ToggleAnimation(s.AnimationName, false);
            if (spriteAnimations.Count == 1)
            {
                currentAnimation = s;
            }
            
        }

        public void ResetAnimation(SpriteAnimation s)
        {

            this.celAnimationManger.ResetAnimation(s.AnimationName);
        }

        public void RemoveAnimation(SpriteAnimation s)
        {
            this.spriteAnimations.Remove(s);
            this.celAnimationManger.Animations.Remove(s.AnimationName);
        }

        public void PauseAnimation(SpriteAnimation s)
        {
            this.celAnimationManger.ToggleAnimation(s.AnimationName, true);
        }

        public void GotToFrame(SpriteAnimation s, int frame)
        {
            //TODO
        }

        public void ResumeAmination(SpriteAnimation s)
        {
            this.celAnimationManger.ToggleAnimation(s.AnimationName, false);
        }

        public Rectangle GetCurrentDrawRect(float elapsedTime)
        {
            return this.CelAnimationManager.GetCurrentDrawRect(elapsedTime, currentAnimation.AnimationName);
        }

        public Rectangle GetCurrentDrawRect()
        {
            return GetCurrentDrawRect(0.0f);
        }

        public int GetLoopCount()
        {
            return this.celAnimationManger.Animations[currentAnimation.AnimationName].LoopCount;
        }


    }

    public class SpriteAnimation 
    {

        public string AnimationName;
        public int FPS;
        public string TextureName;
        public CelCount CellCount;

        protected bool isPaused;
        public bool IsPaused { get { return isPaused;} set { isPaused = value;} }

        public SpriteAnimation(string animationName, string textureName,
            int fps,  int numberOfCols, int numberOfRows  )
        {
            this.AnimationName = animationName;
            this.FPS = fps;
            this.TextureName = textureName;
            this.CellCount = new CelCount(numberOfCols,numberOfRows);
            isPaused = true;
        }

    }
}