using System;

namespace XMPPEngineer.Im
{
    /// <summary>
    /// Provides data for the SubscriptionRequest Event.
    /// </summary>
    public class SubscriptionRequestEventArgs : EventArgs
    {
        /// <summary>
        /// The JID of the user or resource who wishes to subscribe to presence
        /// notifications.
        /// </summary>
        public Jid Jid
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the SubscriptionRequestEventArgs class.
        /// </summary>
        /// <exception cref="ArgumentNullException">The jid parameter is null.</exception>
        public SubscriptionRequestEventArgs(Jid jid)
        {
            jid.ThrowIfNull("jid");
            Jid = jid;
        }
    }
}