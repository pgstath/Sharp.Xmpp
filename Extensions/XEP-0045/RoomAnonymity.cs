namespace XMPPEngineer.Extensions
{
    /// <summary>
    /// Describes whether a conference room occupant's full JID is visible
    /// to all other room occupants.
    /// </summary>
    public enum RoomAnonymity
    {
        /// <summary>
        /// Not specified by the server.
        /// </summary>
        Undefined,

        /// <summary>
        /// A room in which an occupant's full JID is exposed to all other occupants,
        /// although the occupant can request any desired room nickname.
        /// </summary>
        NonAnonymous,

        /// <summary>
        /// A room in which an occupant's full JID can be discovered by room admins only.
        /// </summary>
        SemiAnonymous
    }
}
