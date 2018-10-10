using System;
using System.Globalization;
using System.Xml;

namespace XMPPEngineer.Im
{
    /// <summary>
    /// Represents a Presence XML stanza as defined in XMPP:IM.
    /// </summary>
    internal class Presence : Core.Presence
    {
        /// <summary>
        /// The type of the presence stanza.
        /// </summary>
        public PresenceType Type
        {
            get
            {
                return type;
            }

            set
            {
                type = value;
                // Availability is indicated in XMPP by a missing 'type' attribute.
                if (value == PresenceType.Available)
                    element.RemoveAttribute("type");
                else
                {
                    var v = value.ToString().ToLowerInvariant();
                    element.SetAttribute("type", v);
                }
            }
        }

        /// <summary>
        /// The type of the presence stanza.
        /// </summary>
        private PresenceType type;

        /// <summary>
        /// Initializes a new instance of the Presence class from the specified
        /// instance.
        /// </summary>
        /// <param name="presence">An instance of the Core.Presence class to
        /// initialize this instance with.</param>
        /// <exception cref="ArgumentNullException">The presence parameter is null.</exception>
        /// <exception cref="ArgumentException">The 'type' attribute of the
        /// specified presence stanza is invalid.</exception>
        public Presence(Core.Presence presence)
        {
            presence.ThrowIfNull("presence");
            type = ParseType(presence.Data.GetAttribute("type"));
            element = presence.Data;
        }

        /// <summary>
        /// Initializes a new instance of the Presence class.
        /// </summary>
        /// <param name="to">The JID of the intended recipient for the stanza.</param>
        /// <param name="from">The JID of the sender.</param>
        /// <param name="type">The type of the presence stanza.</param>
        /// <param name="data">The content of the stanza.</param>
        /// <param name="id">The ID of the Presence stanza.</param>
        /// <param name="language">The language of the XML character data of
        /// the stanza.</param>
        public Presence(Jid to = null, Jid from = null,
            PresenceType type = PresenceType.Available, string id = null,
            CultureInfo language = null, params XmlElement[] data)
            : base(to, from, id, language, data)
        {
            Type = type;
        }

        /// <summary>
        /// Parses the Presence type from the specified string.
        /// </summary>
        /// <param name="value">The string to parse.</param>
        /// <returns>The PresenceType value parsed from the string.</returns>
        /// <exception cref="ArgumentException">The specified value for the stanza
        /// type is invalid.</exception>
        private PresenceType ParseType(string value)
        {
            // The 'type' attribute of presence-stanzas is optional and if absent
            // availability is assumed.
            if (String.IsNullOrEmpty(value))
                return PresenceType.Available;
            return (PresenceType)Enum.Parse(typeof(PresenceType),
                value.Capitalize());
        }
    }
}