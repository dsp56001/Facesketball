using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoGameLibrary.Util;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input;

namespace MonoGameLibrary.GameComponents.Audio
{
    class SoundSimpleComponent : GameComponent
    {

        InputHandler input;
        GameConsole console;

        Song backSong;   //using Microsoft.Xna.Framework.Media
        SoundEffect pacSpawn, pacDie, pacChomp;  //using Microsoft.Xna.Framework.Audio

        SoundEffectInstance pacSpawnInstance, pacChompInstance;

        private string outText;

        Dictionary<Keys, string> onReleasedKeyMap, onKeyDownMap;

        public SoundSimpleComponent(Game game) : base(game)
        {
            input = (InputHandler)this.Game.Services.GetService<IInputHandler>();
            if(input == null)
            {
                input = new InputHandler(this.Game);
                this.Game.Components.Add(input);

            }
            console = (GameConsole)this.Game.Services.GetService<IGameConsole>();
            if(console == null)
            {
                console = new GameConsole(this.Game);
                this.Game.Components.Add(console);
            }
            
            onReleasedKeyMap = new Dictionary<Keys, string>();
            onKeyDownMap = new Dictionary<Keys, string>();
        }

        private void MediaPlayer_MediaStateChanged(object sender, EventArgs e)
        {

            console.GameConsoleWrite(string.Format("MedaiPlayerState:{0}", MediaPlayer.State.ToString()));
            console.GameConsoleWrite(string.Format("MedaiPlayerVolume{0}", MediaPlayer.Volume.ToString()));

        }

        public override void Initialize()
        {
            this.backSong = this.Game.Content.Load<Song>("KungFuMetal-03");
            MediaPlayer.MediaStateChanged += MediaPlayer_MediaStateChanged;
            MediaPlayer.Play(backSong);     //Start the song playing

            pacSpawn = this.Game.Content.Load<SoundEffect>("GAMEBEGINNING");
            pacDie = this.Game.Content.Load<SoundEffect>("killed");
            pacChomp = this.Game.Content.Load<SoundEffect>("pacchomp");

            //  Uncomment the following line will also loop the song
            //  MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = .5f;

            outText = getOutText();

            //Song keys
            onReleasedKeyMap.Add(Keys.M, "Song Volume Up");
            onReleasedKeyMap.Add(Keys.N, "Song Volume Down");
            onReleasedKeyMap.Add(Keys.VolumeUp, "Song Volume Up");
            onReleasedKeyMap.Add(Keys.VolumeDown, "Song Volume Down");
            onReleasedKeyMap.Add(Keys.OemOpenBrackets, "Song Play");
            onReleasedKeyMap.Add(Keys.OemCloseBrackets, "Song Stop");
            onReleasedKeyMap.Add(Keys.OemPipe, "Song Pause");

            //Sound Effects Keys
            onReleasedKeyMap.Add(Keys.P, "Pac Spawn");
            onReleasedKeyMap.Add(Keys.D, "Pac Die");

            //Holding Key
            onKeyDownMap.Add(Keys.C, "Pac Chomp");
            onKeyDownMap.Add(Keys.Up, "Pac Chomp");
            onKeyDownMap.Add(Keys.Down, "Pac Chomp");
            onKeyDownMap.Add(Keys.Left, "Pac Chomp");
            onKeyDownMap.Add(Keys.Right, "Pac Chomp");
            onKeyDownMap.Add(Keys.W, "Pac Chomp");
            onKeyDownMap.Add(Keys.A, "Pac Chomp");
            onKeyDownMap.Add(Keys.S, "Pac Chomp");
            onKeyDownMap.Add(Keys.D, "Pac Chomp");


            outText = getOutText();
            this.console.DebugText = outText;

            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            UpdateInputAudio();
            base.Update(gameTime);
        }

        private void UpdateInputAudio()
        {
            //Has Released Keys
            foreach (var item in onReleasedKeyMap)
            {
                if (input.KeyboardState.HasReleasedKey(item.Key))
                {
                    console.GameConsoleWrite(string.Format("onReleasedKeyMap Key released {0}", item.Value.ToString())); //Log key to console
                    switch (item.Value)
                    {
                        case "Song Volume Up":
                            MediaPlayer.Volume += .1f; //Song Volume UP
                            break;
                        case "Song Volume Down":
                            MediaPlayer.Volume -= .1f; //Song Volume Down
                            break;
                        case "Song Play":
                            MediaPlayer.Play(backSong); //Song Play
                            break;
                        case "Song Stop":
                            MediaPlayer.Stop(); //Song Stop
                            break;
                        case "Song Pause":
                            //Song Pause and un Pause
                            if (MediaPlayer.State == MediaState.Paused)
                                MediaPlayer.Resume();
                            else
                                MediaPlayer.Pause();
                            break;
                        case "Pac Spawn":
                            //only create one instance of
                            if (pacSpawnInstance == null)
                            {
                                pacSpawnInstance = pacSpawn.CreateInstance();
                            }
                            if (pacSpawnInstance.State == SoundState.Stopped)
                                pacSpawnInstance.Play();
                            if (pacSpawnInstance.State == SoundState.Paused)
                                pacSpawnInstance.Resume();
                            break;
                        case "Pac Die":
                            SoundEffectInstance dieSound = pacDie.CreateInstance();
                            dieSound.Play();
                            break;
                    }
                }
            }
            //Holding Key Down Map
            foreach (var item in onKeyDownMap)
            {
                if (input.KeyboardState.IsHoldingKey(item.Key))
                {
                    console.GameConsoleWrite(string.Format("onKeyDownMap Key held {0}", item.Value.ToString())); //Log key to console
                    switch (item.Value)
                    {
                        case "Pac Chomp":
                            if (pacChompInstance == null) //lazy load 
                            {
                                pacChompInstance = pacChomp.CreateInstance();
                                pacChompInstance.IsLooped = true;
                            }
                            if (pacChompInstance.State == SoundState.Stopped)
                                pacChompInstance.Play();
                            else
                                pacChompInstance.Resume();
                            break;
                    }
                }
                if (input.KeyboardState.HasReleasedKey(item.Key))
                {
                    console.GameConsoleWrite(string.Format("onKeyDownMap Key released {0}", item.Value.ToString())); //Log key to console
                    switch (item.Value)
                    {
                        case "Pac Chomp":
                            if (pacChompInstance == null) //lazy load 
                            {
                                pacChompInstance = pacChomp.CreateInstance();
                                pacChompInstance.IsLooped = true;
                            }
                            pacChompInstance.Pause();
                            break;
                    }
                }
            }
        }

        private string getOutText()
        {
            string s = string.Empty;
            foreach (var item in onReleasedKeyMap)
            {
                s += string.Format("{0}:{1}\n", item.Value, item.Key.ToString());
            }
            foreach (var item in onKeyDownMap)
            {
                s += string.Format("{0}:{1}\n", item.Value, item.Key.ToString());
            }
            return s;
        }
    }
}
