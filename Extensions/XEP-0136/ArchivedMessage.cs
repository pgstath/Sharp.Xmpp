using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharp.Xmpp.Extensions
{
    /// <summary>
    /// Represents an archived chat message as defined in XEP-0136
    /// </summary>
    public class ArchivedMessage
    {
        /// <summary>
        /// Indicates whether the message was sent or received
        /// </summary>
        public ArchivedMessageType Type { get; set; }

        /// <summary>
        /// The time that the message was sent
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// The text content of the message
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The name of the entity that sent the message
        /// </summary>
        public string SenderName { get; set; }

        /// <summary>
        /// The (optional) id of the entitiy that sent the message
        /// </summary>
        public Jid SenderJid { get; set; }
    }
}