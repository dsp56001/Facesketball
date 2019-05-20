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

namespace MotionDetection
{

    class FaceController
    {
        Vector2 location;

        public static Vector2 VideoSize, GameSize;
        public static bool ScaleLocation;
        public float Scale;
        
        public ConfidenceAmount Confidence;
        
        public Vector2 Location
        {
            get {
                if (FaceController.ScaleLocation)
                {
                    return FaceController.ScaleFromVideoResolution(location);
                }   
                else
                {
                    return location;
                }
            }
            set { location = value; }
        }

        public Microsoft.Xna.Framework.Rectangle Rect
        {
            get;
            set;
        }

        public FaceController(float x, float y)
        {
            this.location.X = x;
            this.location.Y = y;
            this.Scale = 1.0f;
        }

        public FaceController(MCvAvgComp f) : this (f, 0)
        {
           
        }

        public FaceController(MCvAvgComp f, int confidence)
        {
            //Center
            //this.location.X = f.rect.Location.X + (f.rect.Width / 2);
            //this.location.Y = f.rect.Location.Y + (f.rect.Height / 2);
            this.location.X = f.rect.Location.X;
            this.location.Y = f.rect.Location.Y;
            this.Rect = new Microsoft.Xna.Framework.Rectangle(
                (int)this.Location.X,
               (int)this.Location.Y, f.rect.Width, f.rect.Height);
            
            this.Confidence = (ConfidenceAmount)confidence;
            this.Scale = f.rect.Right - f.rect.Left;
        }

        public static Vector2 ScaleFromVideoResolution(Vector2 input)
        {
            Vector2 OutVector = input;
            
            if (FaceController.GameSize.X > FaceController.VideoSize.X)
            {
                OutVector.X = (input.X / FaceController.VideoSize.X) * FaceController.GameSize.X;
                OutVector.Y = (input.Y / FaceController.VideoSize.Y) * FaceController.GameSize.Y;
            }
            return OutVector;
        }

        public string About()
        {
            return Location.ToString() + "\n" + this.Confidence + " " + this.Scale;
        }

        public enum ConfidenceAmount
        {
            noFace, Face, OneEye, TwoEyes
        }
    }
    
    class FaceDetector 
    {
        
        private HaarCascade face;
        private HaarCascade eye;

        public List<FaceController> Faces;

        public bool AccurateAndSlow;
        public bool FindEyes;

        //public Emgu.CV.UI.ImageBox capturedImageBox;

        public FaceDetector()
        {
            // adjust path to find your xml
            //Read the HaarCascade objects
            face = new HaarCascade("haarcascade_frontalface_alt_tree.xml");
            eye = new HaarCascade("haarcascade_eye.xml");

            Faces = new List<FaceController>();
            this.AccurateAndSlow = false;
            this.FindEyes = false;
            
        }

        public void ProcessFrame(ref Image<Bgr, Byte> image, ref Image<Gray, Byte> gray, bool SetBitmap)
        {
            
                //Detect the faces  from the gray scale image and store the locations as rectangle
                //The first dimensional is the channel
                //The second dimension is the index of the rectangle in the specific channel
                //        The default
                //     parameters (scale_factor=1.1, min_neighbors=3, flags=0) are tuned for accurate
                //     yet slow object detection. For a faster operation on real video images the
                //     settings are: scale_factor=1.2, min_neighbors=2, flags=CV_HAAR_DO_CANNY_PRUNING,
                //     min_size=<minimum possible face size> (for example, ~1/4 to 1/16 of the image
                //     area in case of video conferencing).
                MCvAvgComp[][] facesDetected;
                if (AccurateAndSlow)
                {
                    facesDetected = gray.DetectHaarCascade(face, 1.1, 3,
                    Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(20, 20));

                }
                else
                {
                    //facesDetected = gray.DetectHaarCascade(face, 1.2, 2,
                    //Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(20, 20));

                    facesDetected = gray.DetectHaarCascade(face, 1.2, 2,
                    Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(gray.Width / 8, gray.Height / 8));
                }
                
                //Clear perviuos list
                Faces.Clear();

                MCvAvgComp[][] eyesDetected = null;
                int eyes = 0;
                
                foreach (MCvAvgComp f in facesDetected[0])
                {
                    //Set the region of interest on the faces
                    gray.ROI = f.rect;

                    //draw the face detected in the 0th (gray) channel with blue color
                    if (SetBitmap)
                        image.Draw(f.rect, new Bgr(System.Drawing.Color.Blue), 2);
                    
                    if (FindEyes)
                    {
                        eyesDetected = gray.DetectHaarCascade(eye, 1.1, 1, Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(20, 20));
                        gray.ROI = System.Drawing.Rectangle.Empty;

                        //if there is no eye in the specific region, the region shouldn't contains a face
                        //note that we might not be able to recoginize a person who ware glass in this case 
                        if (eyesDetected[0].Length == 0) continue;

                       

                        foreach (MCvAvgComp ey in eyesDetected[0])
                        {
                            if (ey.neighbors > 100)
                            {
                                if (SetBitmap)
                                {
                                    System.Drawing.Rectangle eyeRect = ey.rect;
                                    eyeRect.Offset(f.rect.X, f.rect.Y);
                                    image.Draw(eyeRect, new Bgr(System.Drawing.Color.Red), 2);
                                }
                            }
                        }
                    }
                     if(!(eyesDetected == null))
                         eyes =  eyesDetected[0].Length;
                    Faces.Add(new FaceController(f, eyes));

                }   
        }
    }

    
}
