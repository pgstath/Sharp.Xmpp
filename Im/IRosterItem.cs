using System.Collections.Generic;

namespace Sharp.Xmpp.Im
{
    /// <summary>
    /// Represents a roster item.
    /// </summary>
    /// <remarks>In XMPP jargon, the user's contact list is called a 'roster'.</remarks>
    public interface IRosterItem
    {
        /// <summary>
        /// The groups or categories this item is part of.
        /// </summary>
        IEnumerable<string> Groups { get; }

        /// <summary>
        /// The JID of the user this item is associated with.
        /// </summary>
        Jid Jid { get; }

        /// <summary>
        /// The nickname associated with the JID. This may be null.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Determines whether the user has sent a subscription request and is
        /// awaiting approval or refusal from the contact.
        /// </summary>
        bool Pending { get; }

        /// <summary>
        /// The subscription state of this item.
        /// </summary>
        SubscriptionState SubscriptionState { get; }
    }
}