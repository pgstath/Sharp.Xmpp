namespace XMPPEngineer.Im
{
    /// <summary>
    /// Defines the possible values for a user's availability status.
    /// </summary>
    public enum Availability
    {
        /// <summary>
        /// The user or resource is offline and unavailable.
        /// </summary>
        Offline,

        /// <summary>
        /// The user or resource is online and available.
        /// </summary>
        Online,

        /// <summary>
        /// The user or resource is temporarily away.
        /// </summary>
        Away,

        /// <summary>
        /// The user or resource is actively interested in chatting.
        /// </summary>
        Chat,

        /// <summary>
        /// The user or resource is busy.
        /// </summary>
        Dnd,

        /// <summary>
        /// The user or resource is away for an extended period.
        /// </summary>
        Xa
    }
}