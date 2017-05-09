namespace XMPPEngineer.Extensions
{
    /// <summary>
    /// Describes the Affiliation of a participant in a group chat.
    /// </summary>
    public enum Affiliation
    {
        /// <summary>
        /// None Member Participant.
        /// </summary>
        None,

        /// <summary>
        /// Banned User.
        /// </summary>
        Outcast,

        /// <summary>
        /// Room Member.
        /// </summary>
        Member,

        /// <summary>
        /// Room Admin.
        /// </summary>
        Admin,

        /// <summary>
        /// Room Owner.
        /// </summary>
        Owner
    }
}