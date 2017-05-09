using System;

namespace XMPPEngineer.Extensions
{
    /// <summary>
    /// Provides data for the ChatStateChanged event.
    /// </summary>
    [Serializable]
    public class ChatStateChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The JID of the XMPP entity that published the chat state information.
        /// </summary>
        public Jid Jid
        {
            get;
            private set;
        }

        /// <summary>
        /// The chat-state of the XMPP entity.
        /// </summary>
        public ChatState ChatState
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the ChatStateChangedEventArgs class.
        /// </summary>
        /// <param name="jid">The JID of the XMPP entity that published the
        /// chat-state.</param>
        /// <param name="state">The chat-state of the XMPP entity with the specified
        /// JID.</param>
        /// <exception cref="ArgumentNullException">The jid parameter is
        /// null.</exception>
        public ChatStateChangedEventArgs(Jid jid, ChatState state)
        {
            jid.ThrowIfNull("jid");
            Jid = jid;
            ChatState = state;
        }
    }
}