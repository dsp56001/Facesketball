#if !XBOX360
#region Using Statements


using System;
using System.Collections.Specialized;


#endregion

namespace MonoGameLibrary.Events
{
    /// <summary>Encapsulates the functionality of maintaining events and event data.</summary>
    public static class Event
    {
        #region Fields


        /// <summary>An ordered dictionary used for storing events and event data.</summary>
        private static OrderedDictionary events = new OrderedDictionary();

        /// <summary>A reusable type used for organizing event data.</summary>
        private static Type key;

        
        #endregion


        #region Helper Methods


        /// <summary>Adds the specified event data to an event.</summary>
        /// <typeparam name="T">The type of event argument.</typeparam>
        /// <param name="e">The event data to add to the event.</param>
        public static void Subscribe<T>(EventHandler<T> e) where T : EventArgs
        {
            key = typeof(T);
            
            if (events.Contains(key))
                events[key] = Delegate.Combine(events[key] as Delegate, e);
            else
                events.Add(key, e);
        }


        /// <summary>Removes the specified event data from an event.</summary>
        /// <typeparam name="T">The type of event argument.</typeparam>
        /// <param name="e">The event data to remove from the event.</param>
        public static void Unsubscribe<T>(EventHandler<T> e) where T : EventArgs
        {
            key = typeof(T);

            if (events.Contains(key))
                events[key] = Delegate.Remove(events[key] as Delegate, e);
        }


        /// <summary>Invokes an event using the specified event argument.</summary>
        /// <typeparam name="T">The type of event argument.</typeparam>
        /// <param name="e">The event argument used to invoke the event.</param>
        public static void Invoke<T>(T e) where T : EventArgs
        {
            Invoke<T>(null, e);
        }


        /// <summary>Invokes an event using the specified event argument.</summary>
        /// <typeparam name="T">The type of event argument.</typeparam>
        /// <param name="sender">The object that invoked the event.</param>
        /// <param name="e">The event argument used to invoke the event.</param>
        public static void Invoke<T>(object sender, T e) where T : EventArgs
        {
            key = typeof(T);

            if (events.Contains(key))
                (events[key] as Delegate).DynamicInvoke(sender, e);
        }


        /// <summary>Invokes the event data subscribed to the System.EventArgs event.</summary>
        public static void InvokeEventArgs()
        {
            key = typeof(EventArgs);

            if (events.Contains(key))
                (events[key] as Delegate).DynamicInvoke(null, EventArgs.Empty);
        }


        /// <summary>Removes an event and all event data subscribed to it.</summary>
        /// <typeparam name="T">The type of event argument.</typeparam>
        public static void Remove<T>()
        {
            key = typeof(T);

            if (events.Contains(key))
                events.Remove(key);
        }


        #endregion
    }
}
#endif