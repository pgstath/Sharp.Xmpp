using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;

namespace Sharp.Xmpp.Core
{
    /// <summary>
    /// Represents an IQ XML stanza.
    /// </summary>
    public class Iq : Stanza
    {
        /// <summary>
        /// The type of the IQ stanza.
        /// </summary>
        public IqType Type
        {
            get
            {
                return ParseType(element.GetAttribute("type"));
            }

            set
            {
                var v = value.ToString().ToLowerInvariant();
                element.SetAttribute("type", v);
            }
        }

        /// <summary>
        /// Determines whether the IQ stanza is a request.
        /// </summary>
        public bool IsRequest
        {
            get
            {
                var t = Type;
                return t == IqType.Set || t == IqType.Get;
            }
        }

        /// <summary>
        /// Determines whether the IQ stanza is a response.
        /// </summary>
        public bool IsResponse
        {
            get
            {
                return !IsRequest;
            }
        }

        /// <summary>
        /// Initializes a new instance of the Iq class.
        /// </summary>
        /// <param name="type">The type of the IQ stanza.</param>
        /// <param name="id">The ID of the IQ stanza.</param>
        /// <param name="to">The JID of the intended recipient for the stanza.</param>
        /// <param name="from">The JID of the sender.</param>
        /// <param name="data">The content of the stanza.</param>
        /// <param name="language">The language of the XML character data of
        /// the stanza.</param>
        public Iq(IqType type, string id, Jid to = null, Jid from = null,
            XmlElement data = null, CultureInfo language = null)
            : base(null, to, from, id, language, data)
        {
            Type = type;
        }

        /// <summary>
        /// Initializes a new instance of the Iq class from the specified
        /// Xml element.
        /// </summary>
        /// <param name="element">An Xml element representing an IQ stanza.</param>
        /// <exception cref="ArgumentNullException">The element parameter is
        /// null.</exception>
        public Iq(XmlElement element)
            : base(element)
        {
        }

        /// <summary>
        /// Parses the IQ type from the specified string.
        /// </summary>
        /// <param name="value">The string to parse.</param>
        /// <returns>The IqType value parsed from the string.</returns>
        /// <exception cref="ArgumentNullException">The value parameter is
        /// null.</exception>
        /// <exception cref="KeyNotFoundException"></exception>
        private IqType ParseType(string value)
        {
            value.ThrowIfNull("value");
            var dict = new Dictionary<string, IqType>() {
				{ "set", IqType.Set },
				{ "get", IqType.Get },
				{ "result", IqType.Result },
				{ "error", IqType.Error }
			};
            return dict[value];
        }

        /// <summary>
        /// Converts the IQ XML to an XDocument
        /// </summary>
        public XDocument ToXDocument()
        {
            return XDocument.Parse(ToString());
        }
    }
}