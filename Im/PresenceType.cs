namespace XMPPEngineer.Im
{
    /// <summary>
    /// Defines the possible types for Presence stanzas.
    /// </summary>
    internal enum PresenceType
    {
        /// <summary>
        /// Signals that the user is available for communication.
        /// </summary>
        Available,

        /// <summary>
        /// Signals that the user is no longer available for communication.
        /// </summary>
        Unavailable,

        /// <summary>
        /// The sender wishes to subscribe to the recipient's presence.
        /// </summary>
        Subscribe,

        /// <summary>
        /// The sender has allowed the recipient to receive their presence.
        /// </summary>
        Subscribed,

        /// <summary>
        /// The sender is unsubscribing from another entity's presence.
        /// </summary>
        Unsubscribe,

        /// <summary>
        /// The subscription request has been denied or a previously-granted
        /// subscription has been cancelled.
        /// </summary>
        Unsubscribed,

        /// <summary>
        /// A request for a user's current presence.
        /// </summary>
        Probe,

        /// <summary>
        /// An error has occurred regarding processing or delivery of a
        /// previously-sent presence stanza.
        /// </summary>
        Error
    }
}