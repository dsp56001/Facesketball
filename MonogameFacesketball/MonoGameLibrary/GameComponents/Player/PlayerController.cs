using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGameLibrary.GameComponents.Player
{
    public class PlayerController : Microsoft.Xna.Framework.GameComponent
    {
        public Vector2 StickDir;
        public Vector2 DPadDir;
        public Vector2 KeyDir;
        public Vector2 Direction;
        public float Rotate;

        private float rotationAngle;
        private float gamePadRotationAngle;
        private float dPadRotationAngleKey;

        KeyboardState keyboardState;
        GamePadState gamePad1State;

        //Player controller depends on MonogaemLibrary.Util.InputHandler
        public InputHandler input;

        //Checks to see if there is any movement
        public bool hasInputForMoverment
        {
            get
            {
                if (Direction.Length() == 0) return false;
                return true;
            }
        }

        public PlayerController(Game game) : base(game)
        {
            this.Rotate = 0;
            this.StickDir = Vector2.Zero;

            //get input from game service
            input = (InputHandler)game.Services.GetService(typeof(IInputHandler));
            if (input == null) //failed to get input
            {
                //add inputHandler if it hasn't been added to the game yet
                input = new InputHandler(game);
                game.Components.Add(input);
            }
        }

        /// <summary>
        /// Update the current frame
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {

            HandleGamePad();    //Get input from gampads

            HandleKeyboard();   //Get input from keyboard

            #region SumAllInput
            this.Direction = this.KeyDir + this.DPadDir + this.StickDir;
            if (this.Direction.Length() > 0)
            {
                this.Direction = Vector2.Normalize(this.Direction);

                //calculate angle in radians
                rotationAngle = (float)Math.Atan2(
                        this.Direction.X,
                        this.Direction.Y * -1);

                //This converts angle back to degree and uses art facing left as 0 degreees
                //Art that start sfacing left = rotationAngle - (float)(Math.PI / 2)
                //right = 
                //TODO add rotations in radians
                this.Rotate = (float)MathHelper.ToDegrees(rotationAngle - (float)(Math.PI / 2));
            }
            #endregion

            base.Update(gameTime);
        }

        private void HandleKeyboard()
        {
            //Update for input from Keyboard
#if !XBOX360
            #region KeyBoard
            keyboardState = Keyboard.GetState(); //Keyboard is static no need to get it from input

            KeyDir = new Vector2(0, 0);

            if (keyboardState.IsKeyDown(Keys.Left))
            {
                KeyDir += new Vector2(-1, 0);
            }
            if (keyboardState.IsKeyDown(Keys.Right))
            {
                KeyDir += new Vector2(1, 0);
            }
            if (keyboardState.IsKeyDown(Keys.Up))
            {
                KeyDir += new Vector2(0, -1);
            }
            if (keyboardState.IsKeyDown(Keys.Down))
            {
                KeyDir += new Vector2(0, 1);
            }
            if (KeyDir.Length() > 0)
            {
                //Normalize NewDir to keep agled movement at same speed as horilontal/Vert
                KeyDir = Vector2.Normalize(KeyDir);
            }
            #endregion
#endif
        }

        private void HandleGamePad()
        {
            HandleLeftThumbStick();

            HandleDPad();
        }

        private void HandleDPad()
        {
            //Update for input from DPad
            #region DPad
            DPadDir = Vector2.Zero;
            if (gamePad1State.DPad.Left == ButtonState.Pressed)
            {
                //Orginal Position is Right so flip X
                DPadDir += new Vector2(-1, 0);
            }
            if (gamePad1State.DPad.Right == ButtonState.Pressed)
            {
                //Original Position is Right
                DPadDir += new Vector2(1, 0);
            }
            if (gamePad1State.DPad.Up == ButtonState.Pressed)
            {
                //Up
                DPadDir += new Vector2(0, -1);
            }
            if (gamePad1State.DPad.Down == ButtonState.Pressed)
            {
                //Down
                DPadDir += new Vector2(0, 1);
            }
            if (DPadDir.Length() > 0)
            {

                //Normalize NewDir to keep agled movement at same speed as horilontal/Vert
                DPadDir = Vector2.Normalize(DPadDir);

                //Angle in radians from vector
                dPadRotationAngleKey = (float)Math.Atan2(
                        DPadDir.X,
                        DPadDir.Y * -1);
            }
            #endregion
        }

        private void HandleLeftThumbStick()
        {
            gamePad1State = input.GamePads[0];
            //Input for update from analog stick
            #region LeftStick
            StickDir = Vector2.Zero;
            if (gamePad1State.ThumbSticks.Left.Length() > 0.0f)
            {
                StickDir = gamePad1State.ThumbSticks.Left;
                StickDir.Y *= -1;      //Invert Y Axis

                //this is private and calculating will slow processor down may not want to do this for all input
                gamePadRotationAngle = (float)Math.Atan2(
                    gamePad1State.ThumbSticks.Left.X,
                    gamePad1State.ThumbSticks.Left.Y);
            }
            #endregion
        }
    }
}
