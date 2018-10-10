using System;

namespace XMPPEngineer.Im
{
    /// <summary>
    /// Provides data for the Unsubscribed Event.
    /// </summary>
    public class UnsubscribedEventArgs : EventArgs
    {
        /// <summary>
        /// The JID of the user or resource who unsubscribed from receiving presence
        /// information of the JID associated with this instance.
        /// </summary>
        public Jid Jid
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the UnsubscribedEventArgs class.
        /// </summary>
        /// <exception cref="ArgumentNullException">The jid parameter is null.</exception>
        public UnsubscribedEventArgs(Jid jid)
        {
            jid.ThrowIfNull("jid");
            Jid = jid;
        }
    }
}