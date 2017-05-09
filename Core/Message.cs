using System;
using System.Globalization;
using System.Xml;

namespace XMPPEngineer.Core
{
    /// <summary>
    /// Represents a Message XML stanza.
    /// </summary>
    public class Message : Stanza
    {
        /// <summary>
        /// Initializes a new instance of the Message class.
        /// </summary>
        /// <param name="to">The JID of the intended recipient for the stanza.</param>
        /// <param name="from">The JID of the sender.</param>
        /// <param name="data">The content of the stanza.</param>
        /// <param name="id">The ID of the Message stanza.</param>
        /// <param name="language">The language of the XML character data of
        /// the stanza.</param>
        public Message(Jid to = null, Jid from = null, XmlElement data = null,
            string id = null, CultureInfo language = null)
            : base(null, to, from, id, language, data)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Message class from the specified
        /// Xml element.
        /// </summary>
        /// <param name="element">An Xml element representing an Message stanza.</param>
        /// <exception cref="ArgumentNullException">The element parameter is null.</exception>
        public Message(XmlElement element)
            : base(element)
        {
        }
    }
}