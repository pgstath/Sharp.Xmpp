using System;

namespace XMPPEngineer.Im
{
    /// <summary>
    /// Provides data for the SubscriptionRefused Event.
    /// </summary>
    public class SubscriptionRefusedEventArgs : EventArgs
    {
        /// <summary>
        /// The JID of the user or resource who refused a pending subscription
        /// request.
        /// </summary>
        public Jid Jid
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the SubscriptionRefusedEventArgs class.
        /// </summary>
        /// <exception cref="ArgumentNullException">The jid parameter is null.</exception>
        public SubscriptionRefusedEventArgs(Jid jid)
        {
            jid.ThrowIfNull("jid");
            Jid = jid;
        }
    }
}