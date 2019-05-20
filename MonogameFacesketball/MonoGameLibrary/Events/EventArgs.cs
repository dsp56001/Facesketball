#if !XBOX360
#region Using Statements


using System;
using Microsoft.Xna.Framework;


#endregion

namespace MonoGameLibrary.Events
{
    #region Disconnected


    /// <summary>Event arguments that are triggered when a controller is disconnected.</summary>
    public class DisconnectedEventArgs : EventArgs
    {
        #region Fields


        /// <summary>Specifies the index of the player that was disconnected.</summary>
        public PlayerIndex PlayerIndex;


        #endregion


        #region Initialization


        public DisconnectedEventArgs() { }


        public DisconnectedEventArgs(PlayerIndex playerIndex)
        {
            PlayerIndex = playerIndex;
        }


        #endregion
    }


    #endregion


    #region Reconnected


    /// <summary>Event arguments that are triggered when a controller is reconnected.</summary>
    public class ReconnectedEventArgs : EventArgs
    {
        #region Fields


        /// <summary>Specifies the index of the player that was reconnected.</summary>
        public PlayerIndex PlayerIndex;


        #endregion


        #region Initialization


        public ReconnectedEventArgs() { }


        public ReconnectedEventArgs(PlayerIndex playerIndex)
        {
            PlayerIndex = playerIndex;
        }


        #endregion
    }


    #endregion
}
#endif