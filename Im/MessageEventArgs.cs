using System;

namespace Sharp.Xmpp.Im
{
    /// <summary>
    /// Provides data for the Message event.
    /// </summary>
    public class MessageEventArgs : EventArgs
    {
        /// <summary>
        /// The JID of the user or resource who sent the message.
        /// </summary>
        public Jid Jid
        {
            get;
            private set;
        }

        /// <summary>
        /// The received chat message.
        /// </summary>
        public Message Message
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the MessageEventArgs class.
        /// </summary>
        /// <exception cref="ArgumentNullException">The jid parameter or the message
        /// parameter is null.</exception>
        public MessageEventArgs(Jid jid, Message message)
        {
            jid.ThrowIfNull("jid");
            message.ThrowIfNull("message");
            Jid = jid;
            Message = message;
        }

        /// <summary>
        /// Initializes a new instance of the MessageEventArgs class for bodyless messages.
        /// </summary>
        /// <exception cref="ArgumentNullException">The message parameter is null.</exception>
        public MessageEventArgs(Message message)
        {
            message.ThrowIfNull("message");
            Message = message;
        }
    }
}