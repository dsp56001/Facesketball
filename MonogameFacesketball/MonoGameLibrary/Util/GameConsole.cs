using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;

namespace MonoGameLibrary.Util
{
    /// <summary>
    /// This is a game component that implements DrawableGameComponent
    /// and defines the IGameConsole interface so that GameConsole
    /// may be uses as a game service
    /// </summary>

    public interface IGameConsole
    {
        string FontName { get; set;}    //Font to use for Game Console it must be loadable by pipline tool
        string DebugText { get; set; }  //Text that displays on every frame
        Dictionary<string, string> DebugTextOutput { get; set; } //Text that is added to the Debugtext with a specfic header

        string GetGameConsoleText();
        void GameConsoleWrite(string s);    //writes text to the game console

    }

    public class GameConsole : Microsoft.Xna.Framework.DrawableGameComponent, IGameConsole
    {
        protected string fontName;  //Private instance data member for FontName

        //Font to use for Game Console it must be loadable by pipline tool
        public string FontName { get { return fontName; } set { fontName = value; } }

        protected string debugText;
        //Text shows on every frame all the time. Can be diffucult to use if logging lots of data to the screen
        public string DebugText { get { return debugText; } set { debugText = value; } }

        protected Dictionary<string, string> debugTextOutput;

        //Dicationary of text to be logged to the screen on every frame simplifies use to DebugText
        //The Key Value pair of this dictionary is added to each frames Debug Text as Key: Value
        public Dictionary<string, string> DebugTextOutput { get { return debugTextOutput; } set { debugTextOutput = value; } }
        protected string debugTextOutString;  //used to create text for dictionary
        float debugTextStartX, debugTextStartY;


        protected int maxLines;
        //Max lines for the console
        public int MaxLines { get { return maxLines; } set { maxLines = value; } }

        SpriteFont font;
        SpriteBatch spriteBatch;
        
        protected List<string> gameConsoleText;
        protected GameConsoleState gameConsoleState;

        //Key to open and close console
        public Keys ToggleConsoleKey;

        InputHandler input;     //GameConsole depends on InputHandler
        
        public GameConsole(Game game)
            : base(game)
        {
            
            this.fontName = "Arial";        //default font
            this.gameConsoleText = new List<string>();
            this.maxLines = 20;             //deafult nuber of lines
            this.debugText = "Console default \ndebug text";
            this.ToggleConsoleKey = Keys.OemTilde;  //default key
            this.debugTextStartX = 400;     //Default locationX
            this.debugTextStartY = 0;       //Default locationY
            

            this.debugTextOutput = new Dictionary<string, string>();

            this.gameConsoleState = GameConsoleState.Open;

           
            input = (InputHandler)game.Services.GetService(typeof(IInputHandler));
            //Make sure input service exsists if not lazily add one
            if (input == null)
            {
                //try to add one
                input= new InputHandler(game);
                game.Components.Add(input);

                //Check again
                if (input == null)
                {
                    throw new Exception("GameConsole Depends on Input service please add input service before you add GameConsole.");
                }
            }
            game.Services.AddService(typeof(IGameConsole), this);
        }

        protected override void LoadContent()
        {

            try
            {
                font = this.Game.Content.Load<SpriteFont>("content/Arial");
            }
            catch
            {
                try
                {
                    font = this.Game.Content.Load<SpriteFont>("Arial");
                }
                catch
                {
                    font = this.Game.Content.Load<SpriteFont>("SpriteFont1");
                }
            }
            spriteBatch = new SpriteBatch(GraphicsDevice);
            this.gameConsoleText.Add("Console Initalized");
            base.LoadContent();
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            
            
            base.Initialize();
        }

        protected override void UnloadContent()
        {
            

            base.UnloadContent();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            
            
            if(input.KeyboardState.HasReleasedKey(ToggleConsoleKey))
            {
                this.ToggleConsole();   
            }
            
            base.Update(gameTime);
        }

        /// <summary>
        /// Opens or closes the Console Display
        /// </summary>
        public void ToggleConsole()
        {
            if (this.gameConsoleState == GameConsoleState.Closed)
            {
                this.gameConsoleState = GameConsoleState.Open;
            }
            else
            {
                this.gameConsoleState = GameConsoleState.Closed;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            if (this.gameConsoleState == GameConsoleState.Open)
            {
                spriteBatch.Begin();
                spriteBatch.DrawString(font, GetGameConsoleText(), Vector2.Zero, Color.Wheat);
                
                //Collect data added fron the dictionary
                debugTextOutString = String.Empty;
                foreach (var item in debugTextOutput)
                {
                    debugTextOutString += "\n" +  item.Key + " : " + item.Value;
                }
                
                spriteBatch.DrawString(font, debugText + debugTextOutString, new Vector2(debugTextStartX, debugTextStartY), Color.Wheat);

                spriteBatch.End();
            }
            base.Draw(gameTime);
        }

        public string GetGameConsoleText()
        {
            string Text = "";

            string[] current = new string[Math.Min(gameConsoleText.Count, MaxLines)];
            int offsetLines = (gameConsoleText.Count / maxLines) * maxLines;
            
            int offest = gameConsoleText.Count - offsetLines;

            int indexStart = offsetLines - (maxLines - offest);
            if (indexStart < 0)
                indexStart = 0;
            
            gameConsoleText.CopyTo(
                indexStart, current, 0 , Math.Min(gameConsoleText.Count, MaxLines));

            foreach (string s in current)
            {
                Text += s;
                Text += "\n";
            }
            return Text;
        }

        /// <summary>
        /// Add a new Key to the Dictionary the is displayed in the Debug Text
        /// Data is displayed on line per entry in the format
        /// DebugKey: DebugValue
        /// </summary>
        /// <param name="DebugKey">The key for the debug data must be unique</param>
        /// <param name="DebugValue">The vlaue to display must be a string</param>
        public void Log(string DebugKey, string DebugValue)
        {
            if(this.debugTextOutput.ContainsKey(DebugKey))
            {
                debugTextOutput[DebugKey] = DebugValue;
                return;
            }
            debugTextOutput.Add(DebugKey, DebugValue); 
        }

        /// <summary>
        /// Adds a line to the Debug Console
        /// </summary>
        /// <param name="s">String to add to the console</param>
        public void GameConsoleWrite(string s)
        {
            gameConsoleText.Add(s);
        }

        //Console State
        public enum GameConsoleState { Closed, Open};
    }
}