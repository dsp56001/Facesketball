using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.VideoSurveillance;
using Emgu.Util;
using Microsoft.Xna.Framework;
using System.Drawing;
using Microsoft.Xna.Framework.Graphics;
using Facesketball;
using Microsoft.Xna.Framework.Input;
using System.Runtime.InteropServices;
using MonoGameLibrary.Util;

namespace MotionDetection
{
    class CameraManager : GameComponent
    {
        private Capture _capture; //Frame from Camera

        public Texture2D CameraBackGround;

        public bool SetBitmap;   //draws captured frame as bitmap
        public bool FindFaces;
        protected bool findMotion;

        
        
        
        public bool FindMotion { 
            get {   return findMotion;  }
            set {   
                findMotion = value;
                if (findMotion == false)
                {
                    md.MotionSum = Vector2.Zero;
                    md.TotalMotionsFound = 0;
                    md.OverallMotionPixelCount = 0;
                }
            }
        }
        
        Bitmap bimage;              
        public Bitmap Bimage { get { return bimage; } }
        
        public int UpdateCameraFrameInterval, UpdateFaceFrameInterval, UpdateMotionDetectorFrameInterval;

        public FaceDetector fd;
        public MotionDetector md;

        public double CameraWidth { get { return _capture.Width; } }
        public double CameraHeight { get { return _capture.Height; } }

        Double ReadFrameTime, ReadMotionTime, ReadFaceTime;

        InputHandler input;
        GameConsole gameConsole;

        //HACK 
        //FloatingBall fball;
        FoatingBallManager fbm;
        PlayerFace PlayerFace;
        public bool DrawPlayerFace;
        List<PlayerFace> LFaces;
        
        public CameraManager(Game game) : base (game)
        {
            //Hack
            //fball = ((Game1)game).Fball;
            PlayerFace = ((Game1)game).FaceTracker;
            fbm = ((Game1)game).FBM;

            gameConsole = (GameConsole)game.Services.GetService(typeof(IGameConsole));

            this.LFaces = new List<PlayerFace>();

            //try to create the capture
            if (_capture == null)
            {
                try
                {
                    //gameConsole.GameConsoleWrite("Camera intialized:" + _capture.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FORMAT).ToString());
                    _capture = new Capture();

                    //240 X 90

                    //_capture.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_WIDTH, 160);
                    //_capture.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_HEIGHT, 120);

                    //_capture.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_WIDTH, 240);
                    //_capture.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_HEIGHT, 90);

                    //_capture.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_WIDTH, 320);
                    //_capture.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_HEIGHT, 240);
                    
                    _capture.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_WIDTH, 720);
                    _capture.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_HEIGHT, 405);
                    //_capture.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FPS, 15);

                    gameConsole.GameConsoleWrite("Camera FPS:" + _capture.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FPS).ToString());
                    gameConsole.GameConsoleWrite("Camera WIDTH:" + _capture.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_WIDTH).ToString());
                    gameConsole.GameConsoleWrite("Camera HEIGHT:" + _capture.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_HEIGHT).ToString());
                }
                catch (NullReferenceException excpt)
                {   //show errors if there is any
                    throw excpt;
                } 
            }
            this.UpdateCameraFrameInterval = 400;
            this.UpdateFaceFrameInterval = this.UpdateCameraFrameInterval + 0;
            this.UpdateMotionDetectorFrameInterval = this.UpdateCameraFrameInterval + 0;
            fd = new FaceDetector();
            md = new MotionDetector(game);

            input = (InputHandler)game.Services.GetService(typeof(IInputHandler));
            gameConsole = (GameConsole)game.Services.GetService(typeof(IGameConsole));
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public void ChangeRes(float ResMultiplier)
        {
            _capture.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_WIDTH,
                _capture.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_WIDTH) * ResMultiplier);
            _capture.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_HEIGHT,
                _capture.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_HEIGHT) * ResMultiplier);
                    

        }

        public override void Update(GameTime gameTime)
        {
            // The time since Update was called last
            // Time elapsed since the last call to update.
            double elapsedTime = gameTime.TotalGameTime.TotalMilliseconds;

            //only Check camera less than 60 times a second
            if (elapsedTime > ReadFrameTime)
            {
                this.ProcessFrame(gameTime, elapsedTime);
                if (this.SetBitmap)
                {
                    CameraBackGround = TextureFromBitmap(this.Bimage);
                    //CameraBackGround = CreateTextureFromBitmap(this.Bimage);
                }
                ReadFrameTime = elapsedTime + this.UpdateCameraFrameInterval;    //milliseconds to check camera

            }

            //floatingball Manager
            if (this.FindMotion)
            {
                if (this.md.OverallMotionPixelCount > 1000 && this.md.TotalMotionsFound > 10) //500 is a filter to find big motions
                {
                    //gameConsole.DebugText = Vector2.Normalize(cm.md.MotionSum).ToString();
                    Vector2 fixedVector = Vector2.Normalize(this.md.MotionSum) * new Vector2(1, -1);
                    //fball.GravityDir = fixedVector;
                    //fball.SetTexture(true);
                    foreach (FloatingBall f in fbm.FloatingBalls)
                    {
                        f.GravityDir = fixedVector;
                        f.SetTexture(true);
                    }
                    
                }
                else
                {
                    //fball.GravityDir = Vector2.Zero;
                    //fball.SetTexture(false);

                    foreach (FloatingBall f in fbm.FloatingBalls)
                    {
                        f.GravityDir = Vector2.Zero; ;
                        f.SetTexture(false);
                    }
                }
            }
            //PacMan Test
            if (this.fd.Faces.Count > 0)
            {
                LFaces = new List<PlayerFace>();
                foreach (FaceController f in this.fd.Faces)
                {
                    //spriteBatch.DrawString(font, f.About(), f.Location - new Vector2(0, -50), Color.AliceBlue);
                    PlayerFace foundFace = PlayerFace;
                    foundFace.Location = f.Location;
                    foundFace.Scale = f.Scale;
                    LFaces.Add(foundFace);
                }

                PlayerFace.Enabled = true;
                if(DrawPlayerFace)
                    PlayerFace.Visible = true;
                //PlayerFace.Location = this.fd.Faces[0].Location;
                PlayerFace.Location = new Vector2(this.fd.Faces[0].Rect.Center.X,
                    this.fd.Faces[0].Rect.Center.Y);
                //2.5 is a HACK bese on the PacMan texture size
                PlayerFace.Scale = this.fd.Faces[0].Scale / PlayerFace.spriteTexture.Width * 4.5f;
            }
            else
            {
                PlayerFace.Enabled = false;
                if(DrawPlayerFace)
                    PlayerFace.Visible = false;
            }
            


            //KeyBoard input
            #region Keyboard Input
            //V toggles Video
            if (input.KeyboardState.WasKeyPressed(Keys.V))
            {
                if (this.SetBitmap)
                {
                    this.SetBitmap = false;
                }
                else
                {
                    this.SetBitmap = true;
                }
                gameConsole.GameConsoleWrite("SetBitmap " + this.SetBitmap);
            }

            //A toggles AccurateAndSlow
            if (input.KeyboardState.WasKeyPressed(Keys.A))
            {
                if (this.fd.AccurateAndSlow)
                {
                    this.fd.AccurateAndSlow = false;
                }
                else
                {
                    this.fd.AccurateAndSlow = true;
                }
                gameConsole.GameConsoleWrite("AccurateAndSlow " + this.fd.AccurateAndSlow);
            }

            //F toggles Find Eyers
            if (input.KeyboardState.WasKeyPressed(Keys.F))
            {
                if (this.fd.FindEyes)
                {
                    this.fd.FindEyes = false;
                }
                else
                {
                    this.fd.FindEyes = true;
                }
                gameConsole.GameConsoleWrite("FindEyes " + this.fd.FindEyes);
            }

            //S toggles Scale location
            if (input.KeyboardState.WasKeyPressed(Keys.S))
            {
                if (FaceController.ScaleLocation)
                {
                    FaceController.ScaleLocation = false;
                }
                else
                {
                    FaceController.ScaleLocation = true;
                }
                gameConsole.GameConsoleWrite("FaceController.ScaleLocation " + FaceController.ScaleLocation);
                gameConsole.GameConsoleWrite("Video Size:" + this.CameraWidth + " " + this.CameraHeight);
            }

            //Up and down UpdateCameraFrameInterval Interval Interval
            if (input.KeyboardState.WasKeyPressed(Keys.OemOpenBrackets))
            {
                this.UpdateCameraFrameInterval -= 50;
                if (this.UpdateCameraFrameInterval < 50)
                    this.UpdateCameraFrameInterval = 50;
                gameConsole.GameConsoleWrite("UpdateFrameInterval: " + this.UpdateCameraFrameInterval);
                this.UpdateFaceFrameInterval = this.UpdateCameraFrameInterval;
                this.UpdateMotionDetectorFrameInterval = this.UpdateCameraFrameInterval;
            }
            if (input.KeyboardState.WasKeyPressed(Keys.OemCloseBrackets))
            {
                this.UpdateCameraFrameInterval += 50;
                gameConsole.GameConsoleWrite("UpdateFrameInterval: " + this.UpdateCameraFrameInterval);
                this.UpdateFaceFrameInterval = this.UpdateCameraFrameInterval;
                this.UpdateMotionDetectorFrameInterval = this.UpdateCameraFrameInterval;
            }

            //Up and down UpdateMotionDetectorFrameInterval Interval Interval
            if (input.KeyboardState.WasKeyPressed(Keys.OemSemicolon))
            {
                this.UpdateMotionDetectorFrameInterval -= 10;
                if (this.UpdateMotionDetectorFrameInterval < 10)
                    this.UpdateMotionDetectorFrameInterval = 10;
                gameConsole.GameConsoleWrite("UpdateMotionDetectorFrameInterval: " + this.UpdateMotionDetectorFrameInterval);
            }
            if (input.KeyboardState.WasKeyPressed(Keys.OemQuotes))
            {
                this.UpdateMotionDetectorFrameInterval += 10;
                gameConsole.GameConsoleWrite("UpdateMotionDetectorFrameInterval: " + this.UpdateMotionDetectorFrameInterval);
            }

            //Up and down UpdateFaceFrameInterval Interval Interval
            if (input.KeyboardState.WasKeyPressed(Keys.OemComma))
            {
                this.UpdateFaceFrameInterval -= 10;
                if (this.UpdateFaceFrameInterval < 10)
                    this.UpdateFaceFrameInterval = 10;
                gameConsole.GameConsoleWrite("UpdateFaceFrameInterval: " + this.UpdateFaceFrameInterval);
            }
            if (input.KeyboardState.WasKeyPressed(Keys.OemPeriod))
            {
                this.UpdateFaceFrameInterval += 10;
                gameConsole.GameConsoleWrite("UpdateFaceFrameInterval: " + this.UpdateFaceFrameInterval);
            }

            

            //Find Faces
            if (input.KeyboardState.WasKeyPressed(Keys.NumPad1))
            {
                if (this.FindFaces)
                {
                    this.FindFaces = false;
                    //Disable all face objects
                    ((Game1)Game).FaceTracker.Enabled = false;
                    ((Game1)Game).FaceTracker.Visible = false;
                    ((Game1)Game).faceChaser.Enabled = false;
                    ((Game1)Game).faceChaser.Visible = false;
                }
                else
                {
                    this.FindFaces = true;
                    //Enable all face objects
                    ((Game1)Game).FaceTracker.Enabled = true;
                    //((Game1)Game).FaceTracker.Visible = true;
                    ((Game1)Game).faceChaser.Enabled = true;
                    ((Game1)Game).faceChaser.Visible = true;
                }
                gameConsole.GameConsoleWrite("FindFaces: " + this.FindFaces);
            }

            //Find Motion
            if (input.KeyboardState.WasKeyPressed(Keys.NumPad2))
            {
                EnableMotion();
                gameConsole.GameConsoleWrite("FindMotion: " + this.FindMotion);
            }


            #endregion

            
            base.Update(gameTime);

            if (input.KeyboardState.WasKeyPressed(Keys.M))
            {
                this.ChangeRes(2);
                gameConsole.GameConsoleWrite("Camera FPS:" + _capture.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FPS).ToString());
                gameConsole.GameConsoleWrite("Camera wIDTH:" + _capture.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_WIDTH).ToString());
                gameConsole.GameConsoleWrite("Camera height:" + _capture.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_HEIGHT).ToString());
            }

            if (input.KeyboardState.WasKeyPressed(Keys.N))
            {
                this.ChangeRes(-.5f);
                gameConsole.GameConsoleWrite("Camera FPS:" + _capture.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FPS).ToString());
                gameConsole.GameConsoleWrite("Camera wIDTH:" + _capture.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_WIDTH).ToString());
                gameConsole.GameConsoleWrite("Camera height:" + _capture.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_HEIGHT).ToString());
            }
        }

        public void EnableMotion()
        {
            if (this.FindMotion)
            {
                this.FindMotion = false;
                //fball.GravityDir = Vector2.Zero;
                foreach (FloatingBall f in fbm.FloatingBalls)
                {
                    f.GravityDir = Vector2.Zero;
                    f.Enabled = false;
                    f.Visible = false;

                }
            }
            else
            {
                this.FindMotion = true;
                foreach (FloatingBall f in fbm.FloatingBalls)
                {
                    f.GravityDir = Vector2.Zero;
                    f.Enabled = true;
                    f.Visible = true;

                }
                this.md.InitalizeMotion();
            }
        }

        public void ProcessFrame(GameTime gameTime, Double elapsedTime)
        {
            Image<Bgr, Byte> image = _capture.QueryFrame().Flip(Emgu.CV.CvEnum.FLIP.HORIZONTAL);
            
                Image<Gray, Byte> gray = image.Convert<Gray, Byte>(); //Convert it to Grayscale

                //normalizes brightness and increases contrast of the image
                gray._EqualizeHist();
                
                //If find faces is set and the Correct amount of time has passed
                if(this.FindFaces &&
                    (ReadFaceTime < elapsedTime + UpdateFaceFrameInterval))
                {
                    fd.ProcessFrame(ref image, ref gray, this.SetBitmap);
                    ReadFaceTime = elapsedTime + UpdateFaceFrameInterval;
                }

                //If find motion is set and the correct amount of time has passed
                if (this.FindMotion &&
                    (ReadMotionTime < elapsedTime + UpdateMotionDetectorFrameInterval))
                {
                    md.ProcessFrame(ref image, this.SetBitmap, gameTime);
                    ReadMotionTime = elapsedTime + UpdateMotionDetectorFrameInterval;
                } 
                if (SetBitmap)
                {
                    bimage = image.Bitmap;
                }  
        }

        /// <summary>
        /// Trys to read a texture from a System,Drawing.Bitmap
        /// can fail if the bitmap is incomplete
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        private Texture2D TextureFromBitmap(System.Drawing.Bitmap bmp)
        {
            Texture2D tx = null;
            using (System.IO.MemoryStream s = new System.IO.MemoryStream())
            {
                try
                {
                    bmp.Save(s, System.Drawing.Imaging.ImageFormat.Png);
                    s.Seek(0, System.IO.SeekOrigin.Begin);
                    tx = Texture2D.FromStream(this.Game.GraphicsDevice, s);
                }
                catch
                {
                    tx = null;
                }
            }
            return tx;
        }

        //NOT USED
        //TODO Try this
        //Returns a Texture2D containing the same image as the bitmap parameter, resized if necessary.  Returns null if an error occurs. 
        public Texture2D CreateTextureFromBitmap(GraphicsDevice graphics_device, System.Drawing.Bitmap bitmap)
        {
            Texture2D texture = null;
            bool dispose_bitmap = false;
            try
            {
                if (bitmap != null)
                {
                    //Resize the bitmap if necessary, then capture its final size 
                    
                    //if (graphics_device.GraphicsDeviceCapabilities.TextureCapabilities.RequiresPower2)
                    //{

                    //    System.Drawing.Size new_size; //New size will be next largest power of two, so bitmap will always be scaled up, never down
                    //    new_size = new Size((int)Math.Pow(2.0, Math.Ceiling(Math.Log((double)bitmap.Width) / Math.Log(2.0))), (int)Math.Pow(2.0, Math.Ceiling(Math.Log((double)bitmap.Height) / Math.Log(2.0))));
                    //    bitmap = new Bitmap(bitmap, new_size);
                    //    dispose_bitmap = true;
                    //}
                    System.Drawing.Size bitmap_size = bitmap.Size;

                    //Create a texture with an appropriate format 
                    texture = new Texture2D(graphics_device, bitmap_size.Width, bitmap_size.Height, false, SurfaceFormat.Color);

                    //Lock the bitmap data and copy it out to a byte array 
                    System.Drawing.Imaging.BitmapData bmpdata = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap_size.Width, bitmap_size.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    byte[] pixel_bytes = null;
                    try
                    {
                        pixel_bytes = new byte[bmpdata.Stride * bmpdata.Height];
                        Marshal.Copy(bmpdata.Scan0, pixel_bytes, 0, pixel_bytes.Length);
                    }
                    catch { } //If error occurs allocating memory, bitmap will still be unlocked properly
                    bitmap.UnlockBits(bmpdata);

                    //Set the texture's data to the byte array containing the bitmap data that was just copied 
                    if (pixel_bytes != null) texture.SetData<byte>(pixel_bytes);
                }
            }
            catch
            {
                //Error occured; existing texture must be considered invalid 
                if (texture != null)
                {
                    texture.Dispose();
                    texture = null;
                }
            }
            finally
            {
                if (dispose_bitmap)
                    bitmap.Dispose();
            }
            return texture;
        } 


    }
}
