#if !XBOX360
#region Using Statements


using Microsoft.Xna.Framework;


#endregion

namespace MonoGameLibrary.Events
{
    /// <summary>Encapsulates data that can be subscribed to events.</summary>
    public static class EventData
    {
        #region Helper Methods


        /// <summary>Data triggered when a controller is disconnected.</summary>
        /// <param name="sender">The object that invoked the event.</param>
        /// <param name="e">The event argument describing the target.</param>
        public static void OnDisconnect(object sender, DisconnectedEventArgs e)
        {
        
        }


        /// <summary>Data triggered when a controller is reconnected.</summary>
        /// <param name="sender">The object that invoked the event.</param>
        /// <param name="e">The event argument describing the target.</param>
        public static void OnReconnect(object sender, ReconnectedEventArgs e)
        {
        
        }



        #endregion
    }
}
#endif