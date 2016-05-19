using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharp.Xmpp.Extensions
{
    /// <summary>
    /// Type of archived message
    /// </summary>
    public enum ArchivedMessageType
    {
        /// <summary>
        /// Unknown message type
        /// </summary>
        Unknown,

        /// <summary>
        /// The message was sent
        /// </summary>
        Sent,

        /// <summary>
        /// The message was received
        /// </summary>
        Received
    }
}