using Sharp.Xmpp.Core;
using Sharp.Xmpp.Im;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Sharp.Xmpp.Extensions
{
    internal class MessageArchiving : XmppExtension
    {
        private const string xmlns = "urn:xmpp:archive";

        public override IEnumerable<string> Namespaces
        {
            get
            {
                return new string[] { xmlns };
            }
        }

        public override Extension Xep
        {
            get { return Extension.MessageArchiving; }
        }

        public MessageArchiving(XmppIm im)
            : base(im)
        {
        }

        private static IList<ArchivedChatId> GetChatIdsFromStanza(XmlElement xml)
        {
            List<ArchivedChatId> chats = new List<ArchivedChatId>();
            var chatNodes = xml.GetElementsByTagName("chat");

            foreach (XmlNode node in chatNodes)
            {
                string with = null;
                try
                {
                    with = node.Attributes["with"].InnerText;
                }
                catch
                {
                }

                DateTimeOffset start = default(DateTimeOffset);
                try
                {
                    string startText = node.Attributes["start"].InnerText;
                    start = DateTimeProfiles.FromXmppString(startText);
                }
                catch
                {
                }

                chats.Add(new ArchivedChatId(with, start));
            }

            return chats;
        }

        /// <summary>
        /// Fetch message history from the server.
        ///
        /// The 'start' and 'end' attributes MAY be specified to indicate a date range.
        ///
        /// If the 'with' attribute is omitted then collections with any JID are returned.
        ///
        /// If only 'start' is specified then all collections on or after that date should be returned.
        ///
        /// If only 'end' is specified then all collections prior to that date should be returned.
        /// </summary>
        /// <param name="pageRequest">Paging options</param>
        /// <param name="start">Optional start date range to query</param>
        /// <param name="end">Optional enddate range to query</param>
        /// <param name="with">Optional JID to filter archive results by</param>
        public XmppPage<ArchivedChatId> GetArchivedChatIds(XmppPageRequest pageRequest, DateTimeOffset? start = null, DateTimeOffset? end = null, Jid with = null)
        {
            pageRequest.ThrowIfNull();

            var request = Xml.Element("list", xmlns);

            if (with != null)
            {
                request.Attr("with", with.ToString());
            }

            if (start != null)
            {
                request.Attr("start", start.Value.ToXmppDateTimeString());
            }

            if (end != null)
            {
                request.Attr("end", end.Value.ToXmppDateTimeString());
            }

            var setNode = pageRequest.ToXmlElement();
            request.Child(setNode);

            var response = im.IqRequest(IqType.Get, null, null, request);

            if (response.Type == IqType.Error)
            {
                throw Util.ExceptionFromError(response, "Failed to get archived chat ids");
            }

            return new XmppPage<ArchivedChatId>(response.Data["list"], GetChatIdsFromStanza);
        }

        /// <summary>
        /// Fetch a page of archived messages from a chat
        /// </summary>
        /// <param name="pageRequest">Paging options</param>
        /// <param name="chatId">The id of the chat</param>
        public ArchivedChatPage GetArchivedChat(XmppPageRequest pageRequest, ArchivedChatId chatId)
        {
            return GetArchivedChat(pageRequest, chatId.With, chatId.Start);
        }

        /// <summary>
        /// Fetch a page of archived messages from a chat
        /// </summary>
        /// <param name="pageRequest">Paging options</param>
        /// <param name="with">The id of the entity that the chat was with</param>
        /// <param name="start">The start time of the chat</param>
        public ArchivedChatPage GetArchivedChat(XmppPageRequest pageRequest, Jid with, DateTimeOffset start)
        {
            pageRequest.ThrowIfNull();
            with.ThrowIfNull();

            var request = Xml.Element("retrieve", xmlns);
            request.Attr("with", with.ToString());
            request.Attr("start", start.ToXmppDateTimeString());

            var setNode = pageRequest.ToXmlElement();
            request.Child(setNode);

            var response = im.IqRequest(IqType.Get, null, null, request);

            if (response.Type == IqType.Error)
            {
                throw Util.ExceptionFromError(response, "Failed to get archived chat messages");
            }

            return new ArchivedChatPage(response.Data["chat"]);
        }
    }
}