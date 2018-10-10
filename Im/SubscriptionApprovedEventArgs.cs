using System;

namespace XMPPEngineer.Im
{
    /// <summary>
    /// Provides data for the SubscriptionApproved Event.
    /// </summary>
    public class SubscriptionApprovedEventArgs : EventArgs
    {
        /// <summary>
        /// The JID of the user or resource who accepted a pending subscription
        /// request.
        /// </summary>
        public Jid Jid
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the SubscriptionApprovedEventArgs class.
        /// </summary>
        /// <exception cref="ArgumentNullException">The jid parameter is null.</exception>
        public SubscriptionApprovedEventArgs(Jid jid)
        {
            jid.ThrowIfNull("jid");
            Jid = jid;
        }
    }
}