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
    /// This is a game component that implements DrawableGameComponent.
    /// The Basic Sprite Class Cannot Draw itself it doens't have a spriteBatch
    /// </summary>
    public class Sprite : Microsoft.Xna.Framework.DrawableGameComponent
    {
        public Vector2 Location, Direction, Orgin;  //Origin atsrts at top left
        public float Speed, Rotate;
        public SpriteEffects SpriteEffects;
        public Rectangle LocationRect { get { return locationRect; } set { locationRect = value; } }    //current location

        public Texture2D spriteTexture;  //current Texture
        protected Color[] spriteTextureData;
        protected Matrix spriteTransform;
        protected ContentManager content;
        protected GraphicsDeviceManager graphics;
        protected float lastUpdateTime;   
        protected Rectangle locationRect; //current location
        protected float scale;
        public float Scale
        {
            get { return this.scale; }
            set {
                if (value != this.scale)
                {
                    //reset spriteTransform and locationRect
                    SetTranformAndRect();
                }
                this.scale = value;
            }
        }

        //SpriteMarkers are small dots that mark the locationRect and Orgin of a sprite
        //showMarkers turns them on and off needs to be initally set in constructor
        protected bool showMarkers;    
        public bool ShowMarkers
        {
            get { return this.showMarkers; }
            set { this.showMarkers = value; }
        }
        protected Texture2D SpriteMarkersTexture;


        public Sprite(Game game)
            : base(game)
        {
            // TODO: Construct any child components here
            content = game.Content;
            this.Scale = 1;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here
            graphics = (GraphicsDeviceManager)Game.Services.GetService(typeof(IGraphicsDeviceManager));
            base.Initialize();
            SpriteEffects = SpriteEffects.None;  
        }

        protected override void LoadContent()
        {

            if (showMarkers)
            {
                this.SpriteMarkersTexture = content.Load<Texture2D>("SpriteMarker");
            }
            //top left orgin
            this.Orgin = Vector2.Zero;
            //center orgin
            //this.Orgin = new Vector2(this.spriteTexture.Width / 2, this.spriteTexture.Height / 2);
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
            //SpriteEffects = SpriteEffects.None;       //Default Sprite Effects
            SetTranformAndRect();

            base.Update(gameTime);
        }

        public void SetTranformAndRect()
        {

            try
            {
                // Build the block's transform
                spriteTransform =
                    Matrix.CreateTranslation(new Vector3(this.Orgin * -1, 0.0f)) *
                    Matrix.CreateScale(this.Scale) *
                    Matrix.CreateRotationZ(0.0f) *
                    //Matrix.CreateTranslation(Vector3.Negate( new Vector3(this.Orgin * 2, 0.0f))) *
                    Matrix.CreateTranslation(new Vector3(this.Location, 0.0f));

                // Calculate the bounding rectangle of this block in world space
                this.locationRect = CalculateBoundingRectangle(
                         new Rectangle(0, 0, this.spriteTexture.Width,
                             this.spriteTexture.Height),
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
            catch( Exception ex)
            {
                throw ex;
            }
        }

        public void Draw(SpriteBatch sb)
        {
            //Don't nee this the spriteTransform handles the restangle now
            //this.locationRect = new Rectangle(
            //        (int)Location.X - (int)this.Orgin.X,
            //        (int)Location.Y - (int)this.Orgin.Y,
            //        (int)(spriteTexture.Width * this.Scale),
            //        (int)(spriteTexture.Height * this.Scale));

            
              sb.Draw(spriteTexture,
                new Rectangle(
                    (int)Location.X,
                    (int)Location.Y,
                    (int)(spriteTexture.Width * this.Scale),
                    (int)(spriteTexture.Height * this.Scale)),
                null,
                Color.White,
                MathHelper.ToRadians(Rotate),
                this.Orgin,
                SpriteEffects,
                0);
            
            
            DrawMarkers(sb);
    
        }

        protected void DrawMarkers(SpriteBatch sb)
        {
            //Show markers on the location and rect of a sprite
            if (showMarkers)
            {
                
                //Rect Top Left
                sb.Draw(this.SpriteMarkersTexture,
                    new Rectangle(this.locationRect.Left - this.SpriteMarkersTexture.Width/2,
                        this.locationRect.Top - this.SpriteMarkersTexture.Height / 2, 
                        SpriteMarkersTexture.Width, SpriteMarkersTexture.Height),
                    Color.Red);
                //Rect Top Right
                sb.Draw(this.SpriteMarkersTexture,
                   new Rectangle(this.locationRect.Right - this.SpriteMarkersTexture.Width / 2,
                       this.locationRect.Top, SpriteMarkersTexture.Width, SpriteMarkersTexture.Height),
                   Color.Red);
                //Rect Bottom Left
                sb.Draw(this.SpriteMarkersTexture,
                   new Rectangle(this.locationRect.Left - this.SpriteMarkersTexture.Width / 2,
                       this.locationRect.Bottom - this.SpriteMarkersTexture.Height / 2,
                       SpriteMarkersTexture.Width, SpriteMarkersTexture.Height),
                   Color.Red);
                //Rect Bottom Right
                sb.Draw(this.SpriteMarkersTexture,
                   new Rectangle(this.locationRect.Right - this.SpriteMarkersTexture.Width / 2,
                       this.locationRect.Bottom - this.SpriteMarkersTexture.Height / 2,
                       SpriteMarkersTexture.Width, SpriteMarkersTexture.Height),
                   Color.Red);

                //location Marker
                sb.Draw(this.SpriteMarkersTexture,
                    new Rectangle((int)this.Location.X - this.SpriteMarkersTexture.Width / 2,
                        (int)this.Location.Y - this.SpriteMarkersTexture.Height / 2,
                        SpriteMarkersTexture.Width, SpriteMarkersTexture.Height),
                    Color.Yellow);

                
            }
        }

        /// <summary>
        /// This function takes a Vector2 as input, and returns that vector "clamped"
        /// to the current graphics viewport. We use this function to make sure that 
        /// no one can go off of the screen.
        /// </summary>
        /// <param name="vector">an input vector</param>
        /// <returns>the input vector, clamped between the minimum and maximum of the
        /// viewport.</returns>
        protected Vector2 clampToViewport(Vector2 vector)
        {
            Viewport vp = graphics.GraphicsDevice.Viewport;
            vector.X = MathHelper.Clamp(vector.X, vp.X, vp.X + vp.Width);
            vector.Y = MathHelper.Clamp(vector.Y, vp.Y, vp.Y + vp.Height);
            return vector;
        }

        #region Sprite Collision
        
        /// <summary>
        /// Checks for intersection of this sprite and another sprite
        /// </summary>
        /// <param name="OtherSprite">Other Sprite</param>
        /// <returns>true if the two sprites intersect otherwise returns false</returns>
        public bool Intersects(Sprite OtherSprite)
        {
            return Sprite.Intersects(this.locationRect, OtherSprite.locationRect);
        }

        /// <summary>
        /// Checks if this sprites pixels intersect with another sprite
        /// This is more painfull than checking rectangles
        /// </summary>
        /// <param name="OtherSprite"></param>
        /// <returns></returns>
        public bool PerPixelCollision(Sprite OtherSprite)
        {
            
            Color[] OtherSpriteColors;
            Color[] SpriteColors;

            //GraphicsDevice.Textures[0] = null;          //Bug 
            /*
             * Exception thrown
             * The operation was aborted. You may not modify a resource that has been set on a 
             * device, or after it has been used within a tiling bracket.
             */

            OtherSpriteColors = new Color[OtherSprite.spriteTexture.Width * 
                OtherSprite.spriteTexture.Height];
            SpriteColors = new Color[this.spriteTexture.Width * this.spriteTexture.Height];

            this.spriteTexture.GetData<Color>(SpriteColors);

            OtherSprite.spriteTexture.GetData<Color>(OtherSpriteColors);

            return IntersectPixels(this.locationRect, SpriteColors, 
                OtherSprite.locationRect, OtherSpriteColors);
        }

        /// <summary>
        /// Checks if this sprites pixels intersect with another sprite
        /// This is more painfull than checking rectangles
        /// </summary>
        /// <param name="OtherSprite"></param>
        /// <returns></returns>
        public bool PerPixelCollision2(Sprite OtherSprite)
        {


            return IntersectPixels(this.spriteTransform, this.spriteTexture.Width,
                this.spriteTexture.Height, this.spriteTextureData,
                OtherSprite.spriteTransform, OtherSprite.spriteTexture.Width,
                OtherSprite.spriteTexture.Height,
                OtherSprite.spriteTextureData);
                
        }
        
        /// <summary>
        /// Checks if two rectanges intersect
        /// </summary>
        /// <param name="a">Rectangle A</param>
        /// <param name="b">Rectangle B</param>
        /// <returns>bool</returns>
        public static bool Intersects(Rectangle a, Rectangle b)
        {
            // check if two Rectangles intersect
            return (a.Right > b.Left && a.Left < b.Right &&
                    a.Bottom > b.Top && a.Top < b.Bottom);
        }

        /// <summary>
        /// Checks if two rectanges intersect
        /// </summary>
        /// <param name="rectangle1"></param>
        /// <param name="rectangle2"></param>
        /// <returns>Rectangle of intersection of rectangle1 and rectangle2</returns>
        public static Rectangle Intersection(Rectangle rectangle1, Rectangle rectangle2)
        {
            int x1 = Math.Max(rectangle1.Left, rectangle2.Left);
            int y1 = Math.Max(rectangle1.Top, rectangle2.Top);
            int x2 = Math.Min(rectangle1.Right, rectangle2.Right);
            int y2 = Math.Min(rectangle1.Bottom, rectangle2.Bottom);

            if ((x2 >= x1) && (y2 >= y1))
            {
                return new Rectangle(x1, y1, x2 - x1, y2 - y1);
            }
            return Rectangle.Empty;
        }

        public static Rectangle Normalize(Rectangle reference, Rectangle rectangle)
        {
            return new Rectangle(
              rectangle.X - reference.X,
              rectangle.Y - reference.Y,
              rectangle.Width,
              rectangle.Height);
        }

        /// <summary>
        /// Determines if there is overlap of the non-transparent pixels
        /// between two sprites.
        /// </summary>
        /// <param name="rectangleA">Bounding rectangle of the first sprite</param>
        /// <param name="dataA">Pixel data of the first sprite</param>
        /// <param name="rectangleB">Bouding rectangle of the second sprite</param>
        /// <param name="dataB">Pixel data of the second sprite</param>
        /// <returns>True if non-transparent pixels overlap; false otherwise</returns>
        public static bool IntersectPixels(Rectangle rectangleA, Color[] dataA,
                                    Rectangle rectangleB, Color[] dataB)
        {
            // Find the bounds of the rectangle intersection
            int top = Math.Max(rectangleA.Top, rectangleB.Top);
            int bottom = Math.Min(rectangleA.Bottom, rectangleB.Bottom);
            int left = Math.Max(rectangleA.Left, rectangleB.Left);
            int right = Math.Min(rectangleA.Right, rectangleB.Right);

            // Check every point within the intersection bounds
            for (int y = top; y < bottom; y++)
            {
                for (int x = left; x < right; x++)
                {
                    // Get the color of both pixels at this point
                    Color colorA = dataA[(x - rectangleA.Left) +
                                         (y - rectangleA.Top) * rectangleA.Width];
                    Color colorB = dataB[(x - rectangleB.Left) +
                                         (y - rectangleB.Top) * rectangleB.Width];

                    // If both pixels are not completely transparent,
                    if (colorA.A != 0 && colorB.A != 0)
                    {
                        // then an intersection has been found
                        return true;
                    }
                }
            }

            // No intersection found
            return false;
        }

        /// <summary>
        /// Determines if there is overlap of the non-transparent pixels between two
        /// sprites.
        /// </summary>
        /// <param name="transformA">World transform of the first sprite.</param>
        /// <param name="widthA">Width of the first sprite's texture.</param>
        /// <param name="heightA">Height of the first sprite's texture.</param>
        /// <param name="dataA">Pixel color data of the first sprite.</param>
        /// <param name="transformB">World transform of the second sprite.</param>
        /// <param name="widthB">Width of the second sprite's texture.</param>
        /// <param name="heightB">Height of the second sprite's texture.</param>
        /// <param name="dataB">Pixel color data of the second sprite.</param>
        /// <returns>True if non-transparent pixels overlap; false otherwise</returns>
        public static bool IntersectPixels(
                            Matrix transformA, int widthA, int heightA, Color[] dataA,
                            Matrix transformB, int widthB, int heightB, Color[] dataB)
        {
            // Calculate a matrix which transforms from A's local space into
            // world space and then into B's local space
            Matrix transformAToB = transformA * Matrix.Invert(transformB);

            // When a point moves in A's local space, it moves in B's local space with a
            // fixed direction and distance proportional to the movement in A.
            // This algorithm steps through A one pixel at a time along A's X and Y axes
            // Calculate the analogous steps in B:
            Vector2 stepX = Vector2.TransformNormal(Vector2.UnitX, transformAToB);
            Vector2 stepY = Vector2.TransformNormal(Vector2.UnitY, transformAToB);

            // Calculate the top left corner of A in B's local space
            // This variable will be reused to keep track of the start of each row
            Vector2 yPosInB = Vector2.Transform(Vector2.Zero, transformAToB);

            // For each row of pixels in A
            for (int yA = 0; yA < heightA; yA++)
            {
                // Start at the beginning of the row
                Vector2 posInB = yPosInB;

                // For each pixel in this row
                for (int xA = 0; xA < widthA; xA++)
                {
                    // Round to the nearest pixel
                    int xB = (int)Math.Round(posInB.X);
                    int yB = (int)Math.Round(posInB.Y);

                    // If the pixel lies within the bounds of B
                    if (0 <= xB && xB < widthB &&
                        0 <= yB && yB < heightB)
                    {
                        try
                        {

                            // Get the colors of the overlapping pixels
                            Color colorA = dataA[xA + yA * widthA];
                            Color colorB = dataB[xB + yB * widthB];
                            // If both pixels are not completely transparent,
                            if (colorA.A != 0 && colorB.A != 0)
                            {
                                // then an intersection has been found
                                return true;
                            }
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                            //return false;
                        }
                        
                    }

                    // Move to the next pixel in the row
                    posInB += stepX;
                }

                // Move to the next row
                yPosInB += stepY;
            }

            // No intersection found
            return false;
        }

        /// <summary>
        /// Calculates an axis aligned rectangle which fully contains an arbitrarily
        /// transformed axis aligned rectangle.
        /// </summary>
        /// <param name="rectangle">Original bounding rectangle.</param>
        /// <param name="transform">World transform of the rectangle.</param>
        /// <returns>A new rectangle which contains the trasnformed rectangle.</returns>
        public static Rectangle CalculateBoundingRectangle(Rectangle rectangle,
                                                           Matrix transform)
        {
            // Get all four corners in local space
            Vector2 leftTop = new Vector2(rectangle.Left, rectangle.Top);
            Vector2 rightTop = new Vector2(rectangle.Right, rectangle.Top);
            Vector2 leftBottom = new Vector2(rectangle.Left, rectangle.Bottom);
            Vector2 rightBottom = new Vector2(rectangle.Right, rectangle.Bottom);

            // Transform all four corners into work space
            Vector2.Transform(ref leftTop, ref transform, out leftTop);
            Vector2.Transform(ref rightTop, ref transform, out rightTop);
            Vector2.Transform(ref leftBottom, ref transform, out leftBottom);
            Vector2.Transform(ref rightBottom, ref transform, out rightBottom);

            // Find the minimum and maximum extents of the rectangle in world space
            Vector2 min = Vector2.Min(Vector2.Min(leftTop, rightTop),
                                      Vector2.Min(leftBottom, rightBottom));
            Vector2 max = Vector2.Max(Vector2.Max(leftTop, rightTop),
                                      Vector2.Max(leftBottom, rightBottom));

            // Return that as a rectangle
            return new Rectangle((int)min.X, (int)min.Y,
                                 (int)(max.X - min.X), (int)(max.Y - min.Y));
        }
        #endregion
    }

}