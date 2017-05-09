namespace XMPPEngineer.Extensions
{
    /// <summary>
    /// Defines possible values for the chat state of an XMPP user.
    /// </summary>
    /// <remarks>
    /// Refer to XEP-0085 for a detailed description of the different states
    /// defined here.
    /// </remarks>
    public enum ChatState
    {
        /// <summary>
        /// The user is actively participating in the chat session.
        /// </summary>
        Active,

        /// <summary>
        /// The user has not been actively participating in the chat session.
        /// </summary>
        Inactive,

        /// <summary>
        /// The user has effectively ended their participation in the chat session.
        /// </summary>
        Gone,

        /// <summary>
        /// The user is composing a message.
        /// </summary>
        Composing,

        /// <summary>
        /// The user had been composing but now has stopped.
        /// </summary>
        Paused
    }
}