using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharp.Xmpp.Extensions
{
    /// <summary>
    /// Represents the id of an archived chat collection
    /// </summary>
    public class ArchivedChatId
    {
        /// <summary>
        /// The start date of the conversion
        /// </summary>
        public DateTimeOffset Start { get; private set; }

        /// <summary>
        /// Id of the entity that the conversation was with
        /// </summary>
        public Jid With { get; internal set; }

        /// <summary>
        /// Create an archived chat id
        /// </summary>
        /// <param name="with">The id of the entity that the conversation was with</param>
        /// <param name="start">The start date of the conversation</param>
        public ArchivedChatId(Jid with, DateTimeOffset start)
        {
            with.ThrowIfNull("with");

            With = with;
            Start = start;
        }
    }
}