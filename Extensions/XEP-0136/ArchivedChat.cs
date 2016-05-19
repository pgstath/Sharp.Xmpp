using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Sharp.Xmpp.Extensions
{
    /// <summary>
    /// Represents a page of messages within an archived chat, as specified in XEP-0136
    /// </summary>
    public class ArchivedChatPage : XmppPage<ArchivedMessage>
    {
        /// <summary>
        /// The jid of the entity that the chat was with
        /// </summary>
        public Jid With { get; private set; }

        /// <summary>
        /// The start time of the chat
        /// </summary>
        public DateTimeOffset Start { get; private set; }

        /// <summary>
        /// The subject of the chat
        /// </summary>
        public string Subject { get; private set; }

        /// <summary>
        /// The version of this chat collection
        /// </summary>
        public int Version { get; private set; }

        /// <summary>
        /// Create an archived chat page from an xml response
        /// </summary>
        /// <param name="xml"></param>
        internal ArchivedChatPage(XmlElement xml)
            : base(xml, GetChatMessagesFromStanza)
        {
            var withAttribute = xml.Attributes["with"];
            if (withAttribute != null)
            {
                With = withAttribute.InnerText;
            }

            var startAttribute = xml.Attributes["start"];
            if (startAttribute != null)
            {
                Start = DateTimeProfiles.FromXmppString(startAttribute.InnerText);
            }

            var subjectAttribute = xml.Attributes["subject"];
            if (subjectAttribute != null)
            {
                Subject = subjectAttribute.InnerText;
            }

            var versionAttribute = xml.Attributes["version"];
            if (versionAttribute != null)
            {
                int version = 0;

                int.TryParse(versionAttribute.InnerText, out version);

                Version = version;
            }
        }

        private static IList<ArchivedMessage> GetChatMessagesFromStanza(XmlElement xml)
        {
            List<ArchivedMessage> messages = new List<ArchivedMessage>();

            DateTimeOffset startTime = default(DateTimeOffset);
            var startAttribute = xml.Attributes["start"];
            if (startAttribute != null)
            {
                startTime = DateTimeProfiles.FromXmppString(startAttribute.InnerText);
            }

            var messageNodes = xml.GetElementsByTagName("from");

            foreach (XmlElement node in messageNodes)
            {
                messages.Add(GetChatMessageFromNode(startTime, node));
            }

            messageNodes = xml.GetElementsByTagName("to");

            foreach (XmlElement node in messageNodes)
            {
                messages.Add(GetChatMessageFromNode(startTime, node));
            }

            return messages.OrderBy(m => m.Timestamp).ToList();
        }

        private static ArchivedMessage GetChatMessageFromNode(DateTimeOffset chatStartTime, XmlElement xml)
        {
            ArchivedMessage message = new ArchivedMessage();

            if (xml.LocalName == "from")
            {
                message.Type = ArchivedMessageType.Received;
            }

            if (xml.LocalName == "to")
            {
                message.Type = ArchivedMessageType.Sent;
            }

            var bodyNode = xml["body"];
            if (bodyNode != null)
            {
                message.Text = bodyNode.InnerText;
            }

            var secsAttribute = xml.Attributes["secs"];
            if (secsAttribute != null)
            {
                int secs = 0;
                if (int.TryParse(secsAttribute.InnerText, out secs))
                {
                    message.Timestamp = chatStartTime + TimeSpan.FromSeconds(secs);
                }
            }

            var nameAttribute = xml.Attributes["name"];
            if (nameAttribute != null)
            {
                message.SenderName = nameAttribute.InnerText;
            }

            var jidAttribute = xml.Attributes["jid"];
            if (jidAttribute != null)
            {
                message.SenderJid = jidAttribute.InnerText;
            }

            return message;
        }
    }
}