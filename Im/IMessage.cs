using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;

namespace Sharp.Xmpp.Im
{
    /// <summary>
    /// Represents an on-going file-transfer operation.
    /// </summary>
    public interface IMessage
    {
        /// <summary>
        /// The type of the message stanza.
        /// </summary>
        MessageType Type { get; set; }

        /// <summary>
        /// The time at which the message was originally sent.
        /// </summary>
        DateTime Timestamp { get; }

        /// <summary>
        /// The conversation thread this message belongs to.
        /// </summary>
        string Thread { get; set; }

        /// <summary>
        /// The subject of the message.
        /// </summary>
        string Subject { get; set; }

        /// <summary>
        /// The body of the message.
        /// </summary>
        string Body { get; set; }

        /// <summary>
        /// A dictionary of alternate forms of the message subjects. The keys of the
        /// dictionary denote ISO 2 language codes.
        /// </summary>
        IDictionary<string, string> AlternateSubjects { get; }

        /// <summary>
        /// A dictionary of alternate forms of the message bodies. The keys of the
        /// dictionary denote ISO 2 language codes.
        /// </summary>
        IDictionary<string, string> AlternateBodies { get; }

        /// <summary>
        /// Specifies the JID of the intended recipient for the stanza.
        /// </summary>
        Jid To { get; set; }

        /// <summary>
        /// Specifies the JID of the sender. If this is null, the stanza was generated
        /// by the client's server.
        /// </summary>
        Jid From { get; set; }

        /// <summary>
        /// The ID of the stanza, which may be used for internal tracking of stanzas.
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// The language of the XML character data if the stanza contains data that is
        /// intended to be presented to a human user.
        /// </summary>
        CultureInfo Language { get; set; }

        /// <summary>
        /// The data of the stanza.
        /// </summary>
        XmlElement Data { get; }

        /// <summary>
        /// Determines whether the stanza is empty, i.e. has no child nodes.
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// Returns a textual representation of this instance of the Stanza class.
        /// </summary>
        /// <returns>A textual representation of this Stanza instance.</returns>
        string ToString();

        /// <summary>
        /// Returns the instance as a Message.
        /// </summary>
        /// <returns>The message instance.</returns>
        Message ToMessage();
    }
}
