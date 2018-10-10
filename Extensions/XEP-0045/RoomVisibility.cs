namespace XMPPEngineer.Extensions
{
    /// <summary>
    /// Describes the visibility of a conference room.
    /// </summary>
    public enum RoomVisibility
    {
        /// <summary>
        /// Not specified by the server.
        /// </summary>
        Undefined,

        /// <summary>
        /// A room that can be found by any user through normal means
        /// such as searching and service discovery.
        /// </summary>
        Public,

        /// <summary>
        /// A room that cannot be found by any user through normal means
        /// such as searching and service discovery.
        /// </summary>
        Hidden
    }
}
