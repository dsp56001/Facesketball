using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;

namespace MonoGameLibrary.Sprite
{
    /// <summary>
    /// This is and Extension of Drawable sprite. Each Sprite has
    /// an animation adapter that can manage animations
    /// </summary>
    public class DrawableAnimatableSprite : DrawableSprite
    {

        protected SpriteAnimationAdapter spriteAnimationAdapter;
        Rectangle currentTextureRect;

        public DrawableAnimatableSprite(Game game)
            : base(game)
        {
            
            spriteAnimationAdapter = new SpriteAnimationAdapter(game, this);
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            spriteAnimationAdapter.Initialize();
            
        }

        protected override void LoadContent()
        {
            spriteAnimationAdapter.LoadContent();
            base.LoadContent();
        }

        
        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            //Elapsed time since last update
            lastUpdateTime = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            
            SpriteEffects = SpriteEffects.None;       //Default Sprite Effects
            if(this.spriteAnimationAdapter.HasAnimations)
                this.spriteTexture = this.spriteAnimationAdapter.CurrentTexture;        //update texture for collision
               
            base.Update(gameTime);

            currentTextureRect = spriteAnimationAdapter.GetCurrentDrawRect(lastUpdateTime, this.scale);
            SetTranformAndRect();
            //HACK
            this.SpriteTextureData = new Color[this.spriteAnimationAdapter.CurrentTexture.Width * this.spriteAnimationAdapter.CurrentTexture.Height];
            this.spriteAnimationAdapter.CurrentTexture.GetData(this.SpriteTextureData);
            //this.locationRect = new Rectangle((int)Location.X - (int)this.Orgin.X,
            //    (int)Location.Y - (int)this.Orgin.Y,
            //    currentTextureRect.Width,
            //    currentTextureRect.Height);
        }

        public override void SetTranformAndRect()
        {
            try
            {
                // Build the block's transform
                spriteTransform =
                    Matrix.CreateTranslation(new Vector3(this.Origin * -1, 0.0f)) *
                    Matrix.CreateScale(this.Scale) *
                    Matrix.CreateRotationZ(0.0f) *
                    Matrix.CreateTranslation(new Vector3(this.Location, 0.0f));

                // Calculate the bounding rectangle of this block in world space
                this.locationRect = CalculateBoundingRectangle(
                         new Rectangle(0, 0, this.currentTextureRect.Width,
                             this.currentTextureRect.Height),
                         spriteTransform);
            }
            catch (NullReferenceException nu)
            {
                //nothing
                if (this.spriteTexture == null)
                {
                    //first time this will fail because load content hasn't been called yet
                }
                else
                {
                    throw nu;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();

            spriteBatch.Draw(spriteAnimationAdapter.CurrentTexture,
                new Rectangle((int)Location.X, (int)Location.Y,
                (int)(currentTextureRect.Width * this.scale),
                (int)(currentTextureRect.Height* this.scale)),
                currentTextureRect,
                Color.White,
                MathHelper.ToRadians(Rotate),
                this.Origin,
                SpriteEffects,
                0);
            

            base.DrawMarkers(spriteBatch);

            spriteBatch.End();
            //base.Draw(gameTime);
        }

        /// <summary>
        /// Checks if this sprites pixels intersect with another sprite
        /// This is more painful than checking rectangles
        /// </summary>
        /// <param name="OtherSprite"></param>
        /// <returns></returns>
        public override bool PerPixelCollision(Sprite OtherSprite)
        {
            return IntersectPixels(this.spriteTransform, 
                this.currentTextureRect.Width,
                this.currentTextureRect.Height, 
                this.SpriteTextureData,
                OtherSprite.spriteTransform,
                OtherSprite.SpriteTexture.Width,
                OtherSprite.SpriteTexture.Height,
                OtherSprite.SpriteTextureData);
        }
    }

    public class SpriteAnimationAdapter
    {
        List<SpriteAnimation> spriteAnimations;
        protected SpriteAnimation currentAnimation;
        protected CelAnimationManager celAnimationManger;

        protected Texture2D defaultTexture; //only used if no sprite animations are set
        protected Sprite parent;
        public bool HasAnimations
        {
            get
            {
                if (currentAnimation != null)
                    return true;
                return false;
            }
        }

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
              
        public SpriteAnimationAdapter(Game game, Sprite sprite)
        {
            this.parent = sprite;
            spriteAnimations = new List<SpriteAnimation>();
            
            celAnimationManger = (CelAnimationManager)game.Services.GetService(typeof(ICelAnimationManager));
            if (celAnimationManger == null)
            {
                //throw new Exception("To use a DrawableAnimatedSprite you must a CelAnimationManager to the game as a service!");
                celAnimationManger = new CelAnimationManager(game);
                game.Components.Add(celAnimationManger);
            }   
        }

        public void LoadContent()
        {
            this.defaultTexture = parent.SpriteTexture;
            if (parent.SpriteTexture == null)
            {
                parent.Initialize();
                this.defaultTexture = parent.SpriteTexture;
            }
        }

        public Texture2D CurrentTexture
        {
            get {
                if(currentAnimation == null)
                {
                    return this.defaultTexture;
                }
                return celAnimationManger.GetTexture(currentAnimation.TextureName); }
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

        public Rectangle GetCurrentDrawRect(float elapsedTime, float scale)
        {
            Rectangle drawRect;
            if (this.HasAnimations)
                drawRect = this.CelAnimationManager.GetCurrentDrawRect(elapsedTime, currentAnimation.AnimationName, scale);
            else
                drawRect = defaultTexture.Bounds;
            return drawRect;
        }

        public Rectangle GetCurrentDrawRect(float elapsedTime)
        {
            return GetCurrentDrawRect(0.0f, 0.0f);
        }

        public Rectangle GetCurrentDrawRect()
        {
            return GetCurrentDrawRect(0.0f);
        }

        public int GetLoopCount()
        {
            return this.celAnimationManger.Animations[currentAnimation.AnimationName].LoopCount;
        }

        internal void Initialize()
        {
            //throw new NotImplementedException();
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