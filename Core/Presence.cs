using System;
using System.Globalization;
using System.Xml;

namespace XMPPEngineer.Core
{
    /// <summary>
    /// Represents a Presence XML stanza.
    /// </summary>
    public class Presence : Stanza
    {
        /// <summary>
        /// Initializes a new instance of the Presence class.
        /// </summary>
        /// <param name="to">The JID of the intended recipient for the stanza.</param>
        /// <param name="from">The JID of the sender.</param>
        /// <param name="data">The content of the stanza.</param>
        /// <param name="id">The ID of the Presence stanza.</param>
        /// <param name="language">The language of the XML character data of
        /// the stanza.</param>
        public Presence(Jid to = null, Jid from = null, string id = null,
            CultureInfo language = null, params XmlElement[] data)
            : base(null, to, from, id, language, data)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Presence class from the specified
        /// Xml element.
        /// </summary>
        /// <param name="element">An Xml element representing an Presence stanza.</param>
        /// <exception cref="ArgumentNullException">The element parameter is
        /// null.</exception>
        public Presence(XmlElement element)
            : base(element)
        {
        }
    }
}