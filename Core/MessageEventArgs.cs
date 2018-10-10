using System;

namespace XMPPEngineer.Core
{
    /// <summary>
    /// Provides data for the Message event.
    /// </summary>
    public class MessageEventArgs : EventArgs
    {
        /// <summary>
        /// The Message stanza.
        /// </summary>
        public Message Stanza
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the MessageEventArgs class.
        /// </summary>
        /// <param name="stanza">The Message stanza on whose behalf the event is
        /// raised.</param>
        /// <exception cref="ArgumentNullException">The stanza parameter
        /// is null.</exception>
        public MessageEventArgs(Message stanza)
        {
            stanza.ThrowIfNull("stanza");
            Stanza = stanza;
        }
    }
}