namespace Sharp.Xmpp.Extensions
{
    /// <summary>
    /// Information contained within a direct MUC invitation
    /// </summary>
    public class DirectMucInvitation
    {
        /// <summary>
        /// The Jid of the user that sent the invitation
        /// </summary>
        public Jid From { get; set; }

        /// <summary>
        /// The Jid of the MUC room that the invitation is for
        /// </summary>
        public Jid RoomJid { get; set; }

        /// <summary>
        /// The (optional) password required to enter the room
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// The (optional) reason message attached to the invitation
        /// </summary>
        public string Reason { get; set; }
    }
}