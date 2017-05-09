using System;

namespace XMPPEngineer.Extensions
{
    /// <summary>
    /// Represents a group invite event in a group chat. Ref XEP-0045
    /// </summary>
    public class GroupInviteEventArgs : EventArgs
    {
        /// <summary>
        /// The full invite object.
        /// </summary>
        public Invite Data { get; private set; }
        
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
        /// Password (if any).
        /// </summary>
        public string Password { get { return Data.Password; } }

        /// <summary>
        /// Constructs a GroupInviteEventArgs.
        /// </summary>
        /// <param name="invite">Group Chat Invitation.</param>
        public GroupInviteEventArgs(Invite invite)
        {
            invite.ThrowIfNull("invite");
            Data = invite;
        }
    }
}
