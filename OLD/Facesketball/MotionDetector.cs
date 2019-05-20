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
using IntroGameLibrary.Util;
using Facesketball;
namespace MotionDetection
{
    class MotionDetector
    {
        private MotionHistory _motionHistory;
        private IBGFGDetector<Bgr> _forgroundDetector;

        public Vector2 MotionSum;
        public int OverallMotionPixelCount;
        public int TotalMotionsFound;

        private GameConsole gameConsole;
        

        public MotionDetector(Game game)
        {
            InitalizeMotion();

            gameConsole = (GameConsole)game.Services.GetService(typeof(IGameConsole));
        }

        public void InitalizeMotion()
        {
            //_motionHistory = new MotionHistory(
            //    1.0, //mhiDuration In second, the duration of motion history you wants to keep
            //    0.05, //maxTimeDelta In second. Any change happens between a time interval greater than this will not be considerred
            //    0.5); //minTimeDelta In second. Any change happens between a time interval smaller than this will not be considerred.

            _motionHistory = new MotionHistory(
                2.0, //mhiDuration In second, the duration of motion history you wants to keep
                0.08, //maxTimeDelta In second. Any change happens between a time interval greater than this will not be considerred
                0.5); //minTimeDelta In second. Any change happens between a time interval smaller than this will not be considerred.

            
            MotionSum = Vector2.One;
            
        }

        public void ProcessFrame(ref Image<Bgr, Byte> image, bool SetBitmap, GameTime gameTime)
        {
            
            using (MemStorage storage = new MemStorage()) //create storage for motion components
            {
                if (_forgroundDetector == null)
                {
                    //Whats the differnce between these?
                    //_forgroundDetector = new BGCodeBookModel<Bgr>();
                    //_forgroundDetector = new FGDetector<Bgr>(Emgu.CV.CvEnum.FORGROUND_DETECTOR_TYPE.FGD);
                    //_forgroundDetector = new FGDetector<Bgr>(Emgu.CV.CvEnum.FORGROUND_DETECTOR_TYPE.FGD_SIMPLE);
                    //_forgroundDetector = new FGDetector<Bgr>(Emgu.CV.CvEnum.FORGROUND_DETECTOR_TYPE.MOG);
                    //_forgroundDetector = new BGStatModel<Bgr>(image, Emgu.CV.CvEnum.BG_STAT_TYPE.FGD_STAT_MODEL);
                    _forgroundDetector = new BGStatModel<Bgr>(image, Emgu.CV.CvEnum.BG_STAT_TYPE.GAUSSIAN_BG_MODEL);
                }

                _forgroundDetector.Update(image);

                //update the motion history
                _motionHistory.Update(_forgroundDetector.ForgroundMask);

                #region get a copy of the motion mask and enhance its color
                double[] minValues, maxValues;
                System.Drawing.Point[] minLoc, maxLoc;
                _motionHistory.Mask.MinMax(out minValues, out maxValues, out minLoc, out maxLoc);
                Image<Gray, Byte> motionMask = _motionHistory.Mask.Mul(255.0 / maxValues[0]);
                #endregion

                //create the motion image 
                Image<Bgr, Byte> motionImage = new Image<Bgr, byte>(motionMask.Size);
                //display the motion pixels one of the colors
                motionImage[gameTime.TotalGameTime.Milliseconds % 3] = motionMask;

                //Threshold to define a motion area, reduce the value to detect smaller motion
                //default 100;
                double minArea = 100;

                storage.Clear(); //clear the storage
                Seq<MCvConnectedComp> motionComponents = _motionHistory.GetMotionComponents(storage);

                Vector2 partVect, partDir;
                float partRadius, partXDirection, partYDirection;

                //iterate through each of the motion component
                foreach (MCvConnectedComp comp in motionComponents)
                {
                    //reject the components that have small area;
                    if (comp.area < minArea) continue;

                    // find the angle and motion pixel count of the specific area
                    double angle, motionPixelCount;
                    _motionHistory.MotionInfo(comp.rect, out angle, out motionPixelCount);

                    //Motion Particles
                    if (ParticleManager.Instance().Enabled)
                    {
                        partVect = new Vector2(comp.rect.X, comp.rect.Y);
                        partVect = FaceController.ScaleFromVideoResolution(partVect);

                        //Get the overall motion and set it to a vector2
                        partRadius = (motionMask.ROI.Width + motionMask.ROI.Height) >> 2;
                        partXDirection = (float)(Math.Cos(angle * (Math.PI / 180.0)) * partRadius);
                        partYDirection = (float)(Math.Sin(angle * (Math.PI / 180.0)) * partRadius);
                        partDir = new Vector2(partXDirection, partYDirection);
                        ParticleManager.Instance().ParticleSystems["motionparticles"].AddParticles(partVect, Vector2.Normalize(partDir));
                    }
                    //reject the area that contains too few motion
                    if (motionPixelCount < comp.area * 0.05) continue;

                    //Draw each individual motion in red
                    if (SetBitmap) DrawMotion(motionImage, comp.rect, angle, new Bgr(Color.Red));
                }

                // find and draw the overall motion angle
                double overallAngle, overallMotionPixelCount;
                _motionHistory.MotionInfo(motionMask.ROI, out overallAngle, out overallMotionPixelCount);

                //Get the overall motion and set it to a vector2
                float circleRadius = (motionMask.ROI.Width + motionMask.ROI.Height) >> 2;
                float xDirection = (float)(Math.Cos(overallAngle * (Math.PI / 180.0)) * circleRadius);
                float yDirection = (float)(Math.Sin(overallAngle * (Math.PI / 180.0)) * circleRadius);
                
                MotionSum = new Vector2(xDirection, yDirection);
                OverallMotionPixelCount = (int)overallMotionPixelCount;
                TotalMotionsFound = (int)motionComponents.Total;
                if (SetBitmap)
                {
                    DrawMotion(motionImage, motionMask.ROI, overallAngle, new Bgr(Color.Green));
                    image = image.Add(motionImage);
                    gameConsole.DebugText = String.Format("Total Motions found: {0};\n Motion Pixel count: {1}\nMotionSum:\n{2}"
                        , motionComponents.Total, overallMotionPixelCount, MotionSum);
                    gameConsole.DebugText += String.Format("\nOverallAngle: {0};",
                         overallAngle);
                    gameConsole.DebugText += String.Format("\nMotionSumLength(): {0};",
                         MotionSum.Length());
                }

                
            }
        }
        
        private static void DrawMotion(Image<Bgr, Byte> image, System.Drawing.Rectangle motionRegion, double angle, Bgr color)
        {
            float circleRadius = (motionRegion.Width + motionRegion.Height) >> 2;
            System.Drawing.Point center = new System.Drawing.Point(motionRegion.X + motionRegion.Width >> 1, motionRegion.Y + motionRegion.Height >> 1);

            CircleF circle = new CircleF(
               center,
               circleRadius);

            int xDirection = (int)(Math.Cos(angle * (Math.PI / 180.0)) * circleRadius);
            int yDirection = (int)(Math.Sin(angle * (Math.PI / 180.0)) * circleRadius);
            System.Drawing.Point pointOnCircle = new System.Drawing.Point(
                center.X + xDirection,
                center.Y - yDirection);
            LineSegment2D line = new LineSegment2D(center, pointOnCircle);

            image.Draw(circle, color, 1);
            image.Draw(line, color, 2);
        }
    }
}
