using System;

namespace XMPPEngineer.Core
{
    /// <summary>
    /// Provides data for the Iq event.
    /// </summary>
    public class IqEventArgs : EventArgs
    {
        /// <summary>
        /// The IQ stanza.
        /// </summary>
        public Iq Stanza
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the IqEventArgs class.
        /// </summary>
        /// <param name="stanza">The IQ stanza on whose behalf the event is
        /// raised.</param>
        /// <exception cref="ArgumentNullException">The stanza parameter is null.</exception>
        public IqEventArgs(Iq stanza)
        {
            stanza.ThrowIfNull("stanza");
            Stanza = stanza;
        }
    }
}