
#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary.Util;
#endregion

namespace MonoGameLibrary.ThreeD
{

    public interface ICamera
    {
    }
    
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    /// 
public partial class Camera : Microsoft.Xna.Framework.GameComponent, ICamera
    {
        bool WriteDebug;
    
        protected IInputHandler input;          //input 
        protected GameConsole console;
        private GraphicsDeviceManager graphics;

        private Matrix projection;
        private Matrix view;

        protected Vector3 cameraPosition = new Vector3(0.0f, 10.0f, 0.0f);
        private Vector3 cameraTarget = Vector3.Zero;
        private Vector3 cameraUpVector = Vector3.Up;
        public Vector3 CameraUp { get { return cameraUpVector;} }
        Vector3 transformedReference;
        public Vector3 TransformedReference
        {
            get { return transformedReference; }
        }

        //camera Forward
        private Vector3 cameraReference = Vector3.Forward;
        public Vector3 CameraForward { get { return cameraReference; } }
        
        private float cameraYaw = 0.0f;         //Y rotation
        private float cameraPitch = 7.0f;       //X Rotation

        protected Vector3 movement = Vector3.Zero;
        
        //Spin and Move rates will eventually be our 'look sensitivity' levels.
        private const float spinRate = 70.0f;
        private const float moveRate = 70.0f;

        private int playerIndex = 0;
        private Viewport viewport;

        

        public Camera(Game game)
            : base(game)
        {
            graphics = (GraphicsDeviceManager)Game.Services.GetService(typeof(IGraphicsDeviceManager));
            input = (IInputHandler)game.Services.GetService(typeof(IInputHandler));
            console = (GameConsole)game.Services.GetService(typeof(IGameConsole));

            
        }

        public void ResetCamera()
        {
            this.Position = new Vector3(0.0f, 20.0f, 30.0f);
            this.cameraTarget = Vector3.Zero;
        }

        public override void Initialize()
        {
            base.Initialize();
            InitializeCamera();
            WriteDebug = true;
        }

        private void InitializeCamera()
        {
            float aspectRatio = (float)this.Game.GraphicsDevice.Viewport.Width /
                (float)this.Game.GraphicsDevice.Viewport.Height;
            Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio,
                1.0f, 10000.0f, out projection);

            Matrix.CreateLookAt(ref cameraPosition, ref cameraTarget,
                ref cameraUpVector, out view);
        }
        
        public Matrix View
        {
            get { return view; }
        }

        public Matrix Projection
        {
            get { return projection; }
        }

        public PlayerIndex PlayerIndex
        {
            get { return ((PlayerIndex)playerIndex); }
            set { playerIndex = (int)value; }
        }

        public Vector3 Position
        {
            get { return (cameraPosition); }
            set { cameraPosition = value; }
        }

        public Vector3 Orientation
        {
            get { return (cameraReference); }
            set { cameraReference = value; }
        }

        public Vector3 Target
        {
            get { return (cameraTarget); }
            set { cameraTarget = value; }
        }

        public Viewport Viewport
        {
            get
            {
                return ((Viewport)viewport);
            }
            set
            {
                viewport = value;
                InitializeCamera();
            }
        }

        public override void Update(GameTime gameTime)
        {
            float timeDelta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (input.KeyboardState.IsKeyDown(Keys.Left) ||
                (input.GamePads[playerIndex].ThumbSticks.Right.X < 0))
            {
                cameraYaw += (spinRate * timeDelta);
            }
            if (input.KeyboardState.IsKeyDown(Keys.Right) ||
                (input.GamePads[playerIndex].ThumbSticks.Right.X > 0))
            {
                cameraYaw -= (spinRate * timeDelta);
            }

            if (input.KeyboardState.IsKeyDown(Keys.Down) ||
                (input.GamePads[playerIndex].ThumbSticks.Right.Y < 0))
            {
                cameraPitch -= (spinRate * timeDelta);
            }
            if (input.KeyboardState.IsKeyDown(Keys.Up) ||
                (input.GamePads[playerIndex].ThumbSticks.Right.Y > 0))
            {
                cameraPitch += (spinRate * timeDelta);
            }

#if !XBOX360
            if ((input.MouseState.X > input.PreviousMouseState.X))
            {
                cameraYaw -= (spinRate * timeDelta);
            }

            if ((input.MouseState.X < input.PreviousMouseState.X))
            {
                cameraYaw += (spinRate * timeDelta);
            }
            
            if ((input.PreviousMouseState.X > input.MouseState.X) &&
                (input.MouseState.LeftButton == ButtonState.Pressed))
            {
                cameraYaw += ((spinRate * (input.PreviousMouseState.X - input.MouseState.X))* timeDelta);
            }
            else if ((input.PreviousMouseState.X < input.MouseState.X) &&
                (input.MouseState.LeftButton == ButtonState.Pressed))
            {
                cameraYaw -= ((spinRate * (input.MouseState.X - input.PreviousMouseState.X)) * timeDelta);
            }

            if ((input.PreviousMouseState.Y > input.MouseState.Y) &&
                (input.MouseState.LeftButton == ButtonState.Pressed))
            {
                cameraPitch += ((spinRate * (input.PreviousMouseState.Y - input.MouseState.Y)) * timeDelta);
            }
            else if ((input.PreviousMouseState.Y < input.MouseState.Y) &&
                (input.MouseState.LeftButton == ButtonState.Pressed))
            {
                cameraPitch -= ((spinRate * (input.MouseState.Y - input.PreviousMouseState.Y)) * timeDelta);
            }

            //Keep mouse from getting stuck on edges
            //Should be moved to inputClass ???
            if (input.MouseState.X <= 0)
            {
                Mouse.SetPosition(Game.Window.ClientBounds.Width, input.MouseState.Y);
            
            }
            if (input.MouseState.X >= Game.Window.ClientBounds.Width)
            {
                Mouse.SetPosition(0, input.MouseState.Y);
            }
            if (input.MouseState.Y <= 0)
            {
                Mouse.SetPosition(input.MouseState.X, Game.Window.ClientBounds.Height);

            }
            if (input.MouseState.Y >= Game.Window.ClientBounds.Height)
            {
                Mouse.SetPosition(input.MouseState.X, 0 );
            }
#endif

            //reset camera angle if needed
            if (cameraYaw > 360)
                cameraYaw -= 360;
            else if (cameraYaw < 0)
                cameraYaw += 360;

            //keep camera from rotating a full 90 degrees in either direction
            if (cameraPitch > 89)
                cameraPitch = 89;
            if (cameraPitch < -89)
                cameraPitch = -89;

            //update movement (none for this base class)
            movement *= (moveRate * timeDelta);

            Matrix rotationMatrix;
            

            Matrix.CreateRotationY(MathHelper.ToRadians(cameraYaw), out rotationMatrix);

            if (movement != Vector3.Zero)
            {
                Vector3.Transform(ref movement, ref rotationMatrix, out movement);
                cameraPosition += movement;
            }

            //add in pitch to the rotation
            rotationMatrix = Matrix.CreateRotationX(MathHelper.ToRadians(cameraPitch)) 
                * rotationMatrix;

            // Create a vector pointing the direction the camera is facing.
            Vector3.Transform(ref cameraReference, ref rotationMatrix,
                out transformedReference);
            // Calculate the position the camera is looking at.
            Vector3.Add(ref cameraPosition, ref transformedReference, out cameraTarget);
            //Vector3 cameraTarget = transformedReference + cameraPosition;


            Matrix.CreateLookAt(ref cameraPosition, ref cameraTarget, ref cameraUpVector,
                out view);

            if (WriteDebug)
            {
                console.DebugText = string.Format("Camera cameraYaw: {0}", cameraYaw);
                console.DebugText += string.Format("\nCamera cameraPitch: {0}", cameraPitch);
                console.DebugText += string.Format("\nCamera cameraPosition: \n{0}", cameraPosition);
                console.DebugText += string.Format("\nCamera cameraTarget: \n{0}", cameraTarget);
                console.DebugText += string.Format("\nmouseState cameraTarget: \n{0}, {1}",input.MouseState.X, input.MouseState.Y );
                
                
            }
            base.Update(gameTime);
        }
    }
}