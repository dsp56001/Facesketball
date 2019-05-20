
#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
#endregion

namespace MonoGameLibrary.Util
{
    /// <summary>
    /// Interface for InputHandle game service. This INterface allows the InputHandler class to function as
    /// a Game Service
    /// </summary>
    public interface IInputHandler
    {
        bool WasPressed(int playerIndex, InputHandler.ButtonType button, Keys keys);
        bool WasButtonPressed(int playerIndex, InputHandler.ButtonType button);
        bool WasKeyPressed(Keys keys);

        KeyboardHandler KeyboardState { get; }

        GamePadState[] GamePads { get; }
        GamePadHandler ButtonHandler { get; }

#if !XBOX360
        MouseState MouseState { get; }
        MouseState PreviousMouseState { get; }
#endif
    }

    /// <summary>
    /// This is a game component that implements GameComponent and IInuputHandler
    /// </summary>
    public partial class InputHandler
        : Microsoft.Xna.Framework.GameComponent, IInputHandler
    {
        //Local enum wraps xna buttons
        public enum ButtonType { A, B, Back, LeftShoulder, LeftStick, RightShoulder, RightStick, Start, X, Y }

        //Wrapper for keyboard
        private KeyboardHandler keyboard;
        //Wrapper for GamePads Monogame similar to XNA default to 4 gamepads
        private GamePadHandler gamePadHandler = new GamePadHandler();
        private GamePadState[] gamePads = new GamePadState[4];          //Array of gamecontrollers

#if !XBOX360
        //Two mice states used to compare where the mouse was on previous update to it current position
        private MouseState mouseState;      //current mouse state
        private MouseState prevMouseState;  //previous mouse state
        public Vector2 MouseDelta
        {
            get
            {
                return new Vector2(mouseState.Position.X - prevMouseState.Position.X, 
                    mouseState.Position.Y- prevMouseState.Position.Y);
            }
        }
#endif

        private bool allowsExiting;     //use the back button or escape to exit 
        //XBox used to force games to allow exiting monogame curretly does't

        //Call Contructor and pass false for allowsExiting by default
        public InputHandler(Game game) : this(game, false) { }

        public InputHandler(Game game, bool allowsExiting)
            : base(game)
        {
            this.allowsExiting = allowsExiting;

            //Add IInputHandler as Game Service the constructor of this class added the class as a service
            game.Services.AddService(typeof(IInputHandler), this);
            
            //initialize our local member fields
            keyboard = new KeyboardHandler();

#if !XBOX360
            Game.IsMouseVisible = true;
            prevMouseState = Mouse.GetState();
#endif
        }

        public override void Initialize()
        {
            
            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            keyboard.Update();
            gamePadHandler.Update();

            if (allowsExiting)
            {
                if (keyboard.IsKeyDown(Keys.Escape))
                    //Monogame does't allow this anymore
                    //Game.Exit();

                // Allows the default game to exit on Xbox 360 and Windows
                if (gamePadHandler.WasButtonPressed(0, ButtonType.Back)) { }
                    //Monogame doesn't allow this anymore
                    //Game.Exit();
                    
            }

#if !XBOX360
            //Set our previous state
            prevMouseState = mouseState;
            //Get our new state
            mouseState = Mouse.GetState();
#endif

            //Update Local GamePads
            if (GamePad.GetState(PlayerIndex.One).IsConnected)
                gamePads[0] = gamePadHandler.GamePadStates[0];
            if (GamePad.GetState(PlayerIndex.Two).IsConnected)
                gamePads[1] = gamePadHandler.GamePadStates[1];
            if (GamePad.GetState(PlayerIndex.Three).IsConnected)
                gamePads[2] = gamePadHandler.GamePadStates[2];
            if (GamePad.GetState(PlayerIndex.Four).IsConnected)
                gamePads[3] = gamePadHandler.GamePadStates[3];
            base.Update(gameTime);
        }

        /// <summary>
        /// Implements members from IInputHandler
        /// </summary>
        /// <param name="playerIndex"></param>
        /// <param name="button"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        #region IInputHandler Members
        public bool WasPressed(int playerIndex, ButtonType button, Keys keys)
        {
            if (keyboard.WasKeyPressed(keys) || gamePadHandler.WasButtonPressed(playerIndex, button))
                return (true);
            else
                return (false);
        }

        public bool WasButtonPressed(int playerIndex, ButtonType button)
        {
            return gamePadHandler.WasButtonPressed(playerIndex, button);
        }

        public bool WasKeyPressed(Keys keys)
        {
            return keyboard.WasKeyPressed(keys);
        }

        public KeyboardHandler KeyboardState
        {
            get { return (keyboard); }
        }

        public GamePadHandler ButtonHandler
        {
            get { return (gamePadHandler); }
        }

        public GamePadState[] GamePads
        {
            get { return(gamePads); }
        }

#if !XBOX360
        public MouseState MouseState
        {
            get { return(mouseState); }
        }

        public MouseState PreviousMouseState
        {
            get { return(prevMouseState); }
        }
#endif
        #endregion
    }

    
    public class GamePadHandler //: IButtonHandler
    {
        //Current and Pervious state for all 4 gamepads
        private GamePadState[] prevGamePadsState = new GamePadState[4];
        private GamePadState[] gamePadsState = new GamePadState[4];
        public GamePadState[] GamePadStates { get { return gamePadsState; } }

        public GamePadHandler()
        {
            if(GamePad.GetState(PlayerIndex.One).IsConnected)
                prevGamePadsState[0] = GamePad.GetState(PlayerIndex.One);
            if (GamePad.GetState(PlayerIndex.Two).IsConnected)
                prevGamePadsState[1] = GamePad.GetState(PlayerIndex.Two);
            if (GamePad.GetState(PlayerIndex.Three).IsConnected)
                prevGamePadsState[2] = GamePad.GetState(PlayerIndex.Three);
            if (GamePad.GetState(PlayerIndex.Four).IsConnected)
                prevGamePadsState[3] = GamePad.GetState(PlayerIndex.Four);
        }

        public void Update()
        {
            //set our previous state to our new state
            prevGamePadsState[0] = gamePadsState[0];
            prevGamePadsState[1] = gamePadsState[1];
            prevGamePadsState[2] = gamePadsState[2];
            prevGamePadsState[3] = gamePadsState[3];

            //get our new state
            //gamePadsState = GamePad.State .GetState();
            if (GamePad.GetState(PlayerIndex.One).IsConnected)
                gamePadsState[0] = GamePad.GetState(PlayerIndex.One);
            if (GamePad.GetState(PlayerIndex.Two).IsConnected)
                gamePadsState[1] = GamePad.GetState(PlayerIndex.Two);
            if (GamePad.GetState(PlayerIndex.Three).IsConnected)
                gamePadsState[2] = GamePad.GetState(PlayerIndex.Three);
            if (GamePad.GetState(PlayerIndex.Four).IsConnected)
                gamePadsState[3] = GamePad.GetState(PlayerIndex.Four);
        }        

        public bool WasButtonPressed(int playerIndex, InputHandler.ButtonType button)
        {

            int pi = playerIndex;
            switch(button)
            {
                case InputHandler.ButtonType.A:
                    {
                        return (gamePadsState[pi].Buttons.A == ButtonState.Pressed &&
                            prevGamePadsState[pi].Buttons.A == ButtonState.Released);
                    }
                case InputHandler.ButtonType.B:
                    {
                        return (gamePadsState[pi].Buttons.B == ButtonState.Pressed &&
                            prevGamePadsState[pi].Buttons.B == ButtonState.Released);
                    }
                case InputHandler.ButtonType.Back:
                    {
                        return (gamePadsState[pi].Buttons.Back == ButtonState.Pressed &&
                            prevGamePadsState[pi].Buttons.Back == ButtonState.Released);
                    }
                case InputHandler.ButtonType.LeftShoulder:
                    {
                        return (gamePadsState[pi].Buttons.LeftShoulder == ButtonState.Pressed &&
                            prevGamePadsState[pi].Buttons.LeftShoulder == ButtonState.Released);
                    }
                case InputHandler.ButtonType.LeftStick:
                    {
                        return (gamePadsState[pi].Buttons.LeftStick == ButtonState.Pressed &&
                            prevGamePadsState[pi].Buttons.LeftStick == ButtonState.Released);
                    }
                case InputHandler.ButtonType.RightShoulder:
                    {
                        return (gamePadsState[pi].Buttons.RightShoulder == ButtonState.Pressed &&
                            prevGamePadsState[pi].Buttons.RightShoulder == ButtonState.Released);
                    }
                case InputHandler.ButtonType.RightStick:
                    {
                        return (gamePadsState[pi].Buttons.RightStick == ButtonState.Pressed &&
                            prevGamePadsState[pi].Buttons.RightStick == ButtonState.Released);
                    }
                case InputHandler.ButtonType.Start:
                    {
                        return (gamePadsState[pi].Buttons.Start == ButtonState.Pressed &&
                            prevGamePadsState[pi].Buttons.Start == ButtonState.Released);
                    }
                case InputHandler.ButtonType.X:
                    {
                        return (gamePadsState[pi].Buttons.X == ButtonState.Pressed &&
                            prevGamePadsState[pi].Buttons.X == ButtonState.Released);
                    }
                case InputHandler.ButtonType.Y:
                    {
                        return (gamePadsState[pi].Buttons.Y == ButtonState.Pressed &&
                            prevGamePadsState[pi].Buttons.Y == ButtonState.Released);
                    }
                default:
                    throw (new ArgumentException());
            }
        }
    }

    public class KeyboardHandler //: IKeyboardHandler
    {
        private KeyboardState prevKeyboardState;
        private KeyboardState keyboardState;

        public KeyboardHandler()
        {
            prevKeyboardState = Keyboard.GetState();
        }

        public bool IsKeyDown(Keys key)
        {
            return (keyboardState.IsKeyDown(key));
        }

        public bool IsHoldingKey(Keys key)
        {
            return(keyboardState.IsKeyDown(key) && prevKeyboardState.IsKeyDown(key));
        }

        public bool WasKeyPressed(Keys key)
        {
            return (keyboardState.IsKeyDown(key) && prevKeyboardState.IsKeyUp(key));
        }

        public bool HasReleasedKey(Keys key)
        {
            return (keyboardState.IsKeyUp(key) && prevKeyboardState.IsKeyDown(key));
        }

        public void Update()
        {
            //set our previous state to our new state
            prevKeyboardState = keyboardState;

            //get our new state
            keyboardState = Keyboard.GetState();
        }

        public bool WasAnyKeyPressed()
        {
            Keys[] keysPressed = keyboardState.GetPressedKeys();

            if (keysPressed.Length > 0)
            {
                foreach (Keys k in keysPressed)
                {
                    if (prevKeyboardState.IsKeyUp(k))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}


