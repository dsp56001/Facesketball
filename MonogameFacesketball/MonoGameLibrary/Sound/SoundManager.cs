
/*#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
#endregion

namespace IntroGameLibrary.Sound
{
    //Interface for Game Service
    public interface ISoundManager { }
    
    /// <summary>
    /// This is a game service that manages sound.
    /// </summary>
    public partial class SoundManager : Microsoft.Xna.Framework.GameComponent, ISoundManager
    {
        public bool RepeatPlayList = true;
        private AudioEngine engine;
        private WaveBank waveBank;
        private SoundBank soundBank;

        Dictionary<string, Cue> cues = new Dictionary<string, Cue>();

        private string[] playList;
        private int currentSong;
        private Cue currentlyPlaying;
        private Dictionary<string, AudioCategory> categories = new Dictionary<string, AudioCategory>();

        public SoundManager(Game game, string xactProjectName)
            : this(game, xactProjectName, xactProjectName)
        {
        }

        public SoundManager(Game game, string xactProjectName, string xactFileName)
            : this(game, xactProjectName, xactFileName, @"Content\Audio\")
        {
        }

        public SoundManager(Game game, string xactProjectName, string xactFileName, string contentPath)
            : base(game)
        {
            xactFileName = xactFileName.Replace(".xap", "");

            engine = new AudioEngine(contentPath + xactFileName + ".xgs");
            waveBank = new WaveBank(engine, contentPath + "Wave Bank.xwb");
            soundBank = new SoundBank(engine, contentPath + "Sound Bank.xsb");
        }


        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
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
            
            
            engine.Update();

            #region Music List
            if (currentlyPlaying != null) //are we playing a list?
            {
                //check current cue to see if it is playing
                //if not, go to next cue in list
                if (!currentlyPlaying.IsPlaying)
                {
                    currentSong++;

                    if (currentSong == playList.Length)
                    {
                        if (RepeatPlayList)
                            currentSong = 0;
                        else
                            StopPlayList();
                    }

                    if (currentlyPlaying != null) //may have been set to null, if we finished our list
                    {
                        currentlyPlaying = soundBank.GetCue(playList[currentSong]);
                        currentlyPlaying.Play();
                    }
                }
            }
            #endregion

            base.Update(gameTime);
        }

        protected override void Dispose(bool disposing)
        {
            soundBank.Dispose();
            waveBank.Dispose();
            engine.Dispose();

            playList = null;
            currentlyPlaying = null;
            cues = null;
            soundBank = null;
            waveBank = null;
            engine = null;
            
            base.Dispose(disposing);
        }

        public void SetGlobalVariable(string name, float amount)
        {
            engine.SetGlobalVariable(name, amount);
        }

        /// <summary>
        /// Pause all sounds
        /// </summary>
        public void PauseAllSounds()
        {
            foreach (AudioCategory c in categories.Values)
            {
                PauseCategory(c.Name);
            }
        }

        /// <summary>
        /// Resume all sounds
        /// </summary>
        public void ResumesAllSounds()
        {
            foreach (AudioCategory c in categories.Values)
            {
                ResumeCategory(c.Name);
            }
        }

        #region Category Methods
        private void CheckCategory(string categoryName)
        {
            if (!categories.ContainsKey(categoryName))
                categories.Add(categoryName, engine.GetCategory(categoryName));
        }

        /// <summary>
        /// Sets volume for all sounds in a category
        /// </summary>
        /// <param name="categoryName">Name of category</param>
        /// <param name="volumeAmount">Volume</param>
        public void SetVolume(string categoryName, float volumeAmount)
        {
            CheckCategory(categoryName);
            categories[categoryName].SetVolume(volumeAmount);
        }

        /// <summary>
        /// Pauses all sounds in a category
        /// </summary>
        /// <param name="categoryName">Name of Category to Pause</param>
        public void PauseCategory(string categoryName)
        {
            CheckCategory(categoryName);
            categories[categoryName].Pause();
        }

        /// <summary>
        /// Resumes all sounds in a category
        /// </summary>
        /// <param name="categoryName">Name of category to resume</param>
        public void ResumeCategory(string categoryName)
        {
            CheckCategory(categoryName);
            categories[categoryName].Resume();
        }
        #endregion


        public bool IsPlaying(string cueName)
        {
            if (cues.ContainsKey(cueName))
                return (cues[cueName].IsPlaying);

            return (false);
        }

        public void Play(string cueName)
        {
            Cue prevCue = null;

            if (!cues.ContainsKey(cueName))
                cues.Add(cueName, soundBank.GetCue(cueName));
            else
            {
                //store our cue if we were playing
                if (cues[cueName].IsPlaying)
                    prevCue = cues[cueName];

                cues[cueName] = soundBank.GetCue(cueName);
            }

            //if we weren't playing, then set previous to our current cue name
            if (prevCue == null)
                prevCue = cues[cueName];

            try
            {
                cues[cueName].Play();
            }
            catch (Exception)
            {
                //hit limit exception, set our cue to the previous and let's stop it
                //and then start it up again ...
                cues[cueName] = prevCue;
                //could just ignore this error, for now we will have our library stop and restart
                //regardless of what XACT says to do ... just an example
                if (cues[cueName].IsPlaying)
                    cues[cueName].Stop(AudioStopOptions.AsAuthored);

                Toggle(cueName);
            }
        }

        public void Pause(string cueName)
        {
            if (cues.ContainsKey(cueName))
                cues[cueName].Pause();
        }

        public void Resume(string cueName)
        {
            if (cues.ContainsKey(cueName))
                cues[cueName].Resume();
        }

        /// <summary>
        /// toggles playing paused state of cue
        /// </summary>
        /// <param name="cueName"></param>
        public void Toggle(string cueName)
        {
            if (cues.ContainsKey(cueName))
            {
                Cue cue = cues[cueName];

                if (cue.IsPaused)
                {
                    cue.Resume();
                }
                else if (cue.IsPlaying)
                {
                    cue.Pause();
                }
                else //played but stopped 
                {
                    //need to re-get Cue if stopped
                    Play(cueName);
                }
            }
            else //never played, need to re-get cue
                Play(cueName);

        }

        public void StopAll()
        {
            foreach (Cue cue in cues.Values)
                cue.Stop(AudioStopOptions.Immediate);
        }

        public void Stop(string cueName)
        {
            if (cues.ContainsKey(cueName))
                cues[cueName].Stop(AudioStopOptions.Immediate);
            cues.Remove(cueName);
        }

        public void StartPlayList(string[] playList)
        {
            StartPlayList(playList, 0);
        }

        public void StartPlayList(string[] playList, int startIndex)
        {
            if (playList.Length == 0)
                return;

            this.playList = playList;

            if (startIndex > playList.Length)
                startIndex = 0;

            StartPlayList(startIndex);
        }

        public void StartPlayList(int startIndex)
        {
            if (playList.Length == 0)
                return;

            currentSong = startIndex;
            currentlyPlaying = soundBank.GetCue(playList[currentSong]);
            currentlyPlaying.Play();
        }


        public void StopPlayList()
        {
            if (currentlyPlaying != null)
            {
                currentlyPlaying.Stop(AudioStopOptions.Immediate);
                currentlyPlaying = null;
            }
        }

    
        #region New Methods
        public Cue GetCue(string name)
        {
            Cue returnValue = this.soundBank.GetCue(name);
            return returnValue;
        }

        public void PauseCue(string pauseCueName)
        {
            Cue localSound = this.GetCue(pauseCueName);
            if (localSound.IsPlaying)
            {
                localSound.Pause();
            }
        }
        

        public void StopCue(string stopCueName)
        {
            Cue localSound = this.GetCue(stopCueName);
            if (localSound.IsPlaying)
            {
                localSound.Stop(AudioStopOptions.Immediate);
            }
        }
        
        public static void Stop(Cue cue)
        {
            cue.Stop(AudioStopOptions.Immediate);
        }
    
        #endregion          
    }
    
}


*/