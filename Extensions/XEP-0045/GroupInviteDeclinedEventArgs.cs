using System;

namespace XMPPEngineer.Extensions
{
    /// <summary>
    /// Represents a group invite event in a group chat. Ref XEP-0045
    /// </summary>
    public class GroupInviteDeclinedEventArgs : EventArgs
    {
        /// <summary>
        /// The full invite object.
        /// </summary>
        public InviteDeclined Data { get; private set; }
        
        /// <summary>
        /// Person who sent the invitation.
        /// </summary>
        public Jid From { get { return Data.ReceivedFrom; } }

        /// <summary>
        /// Chat room specified in the invitation.
        /// </summary>
        public Jid ChatRoom { get { return Data.From; } }

        /// <summary>
        /// Message contained in the invitation.
        /// </summary>
        public string Reason { get { return Data.Reason; } }

        /// <summary>
        /// Constructs a GroupInviteEventArgs.
        /// </summary>
        /// <param name="invite">Group Chat Invitation.</param>
        public GroupInviteDeclinedEventArgs(InviteDeclined invite)
        {
            invite.ThrowIfNull("invite");
            Data = invite;
        }
    }
}
