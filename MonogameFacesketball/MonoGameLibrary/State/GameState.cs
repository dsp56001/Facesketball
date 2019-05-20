
#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGameLibrary.Util;
#endregion

namespace MonoGameLibrary.State
{
    public interface IGameState
    {
        GameState Value { get; }
        
    }
    
    public abstract partial class GameState : DrawableGameComponent, IGameState
    {
        protected IGameStateManager GameManager;    //reference to GameManger
        protected IInputHandler Input;              //for input
        
        public GameState(Game game)
            : base(game)
        {
            GameManager = (IGameStateManager)game.Services.GetService(typeof(IGameStateManager));
            Input = (IInputHandler)game.Services.GetService(typeof(IInputHandler));
        }

        protected override void LoadContent()
        {
            base.LoadContent();
        }

        /// <summary>
        /// Called whenever state is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal protected virtual void StateChanged(object sender, EventArgs e)
        {
            if (GameManager.State == this.Value)
                Visible = Enabled = true;
            else
                Visible = Enabled = false;
        }

        #region IGameState Members

        public GameState Value
        {
            get { return (this); }
        }

        #endregion
    }
}


