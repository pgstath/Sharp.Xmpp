namespace XMPPEngineer.Extensions
{
    /// <summary>
    /// Describes whether a conference room is moderated.
    /// </summary>
    public enum RoomModeration
    {
        /// <summary>
        /// Not specified by the server.
        /// </summary>
        Undefined,

        /// <summary>
        /// A room in which any occupant is allowed to send messages to all occupants.
        /// </summary>
        Unmoderated,

        /// <summary>
        /// A room in which only those with "voice" are allowed to send messages to all occupants.
        /// </summary>
        Moderated
    }
}
