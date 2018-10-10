namespace XMPPEngineer.Extensions
{
    /// <summary>
    /// Describes the role of a participant in a group chat.
    /// </summary>
    public enum Role
    {
        /// <summary>
        /// No Role / Not in Room.
        /// </summary>
        None,

        /// <summary>
        /// Room Occupant.
        /// </summary>
        Visitor,

        /// <summary>
        /// Room Occupant with Voice.
        /// </summary>
        Participant,

        /// <summary>
        /// Room Moderator.
        /// </summary>
        Moderator,
    }
}