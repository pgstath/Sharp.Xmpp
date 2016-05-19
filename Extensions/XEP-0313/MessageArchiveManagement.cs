using Sharp.Xmpp.Extensions.Dataforms;
using Sharp.Xmpp.Im;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Sharp.Xmpp.Extensions
{
    internal class MessageArchiveManagement : XmppExtension, IInputFilter<Message>, IInputFilter<Sharp.Xmpp.Core.Iq>
    {
        /// <summary>
        /// Hold the state of pending queries
        /// </summary>
        private class ArchiveQueryTask
        {
            public List<Message> Messages { get; private set; }

            private TaskCompletionSource<XmppPage<Message>> taskCompletionSource;

            public ArchiveQueryTask(TaskCompletionSource<XmppPage<Message>> tcs)
            {
                Messages = new List<Message>();
                taskCompletionSource = tcs;
            }

            public void Finalise(XmlElement setNode)
            {
                taskCompletionSource.SetResult(new XmppPage<Message>(setNode, Messages));
            }

            public void SetException(Exception ex)
            {
                taskCompletionSource.SetException(ex);
            }
        }

        private const string xmlns = "urn:xmpp:mam:0";

        private ConcurrentDictionary<string, ArchiveQueryTask> pendingQueries = new ConcurrentDictionary<string, ArchiveQueryTask>();

        public override IEnumerable<string> Namespaces
        {
            get { return new string[] { xmlns }; }
        }

        public override Extension Xep
        {
            get { return Extension.MessageArchiveManagement; }
        }

        public MessageArchiveManagement(XmppIm im)
            : base(im)
        {
        }

        /// <summary>
        /// Fetch a page of archived messages
        /// </summary>
        /// <param name="pageRequest">Paging options</param>
        /// <param name="with">Optional filter to only return messages if they match the supplied JID</param>
        /// <param name="roomId">Optional filter to only return messages if they match the supplied JID</param>
        /// <param name="start">Optional filter to only return messages whose timestamp is equal to or later than the given timestamp.</param>
        /// <param name="end">Optional filter to only return messages whose timestamp is equal to or earlier than the timestamp given in the 'end' field.</param>
        internal Task<XmppPage<Message>> GetArchivedMessages(XmppPageRequest pageRequest, Jid with = null, Jid roomId = null, DateTimeOffset? start = null, DateTimeOffset? end = null)
        {
            string queryId = Guid.NewGuid().ToString().Replace("-", "");

            var request = Xml.Element("query", xmlns)
                .Attr("queryid", queryId)
                .Child(pageRequest.ToXmlElement());

            if (with != null || start != null || end != null)
            {
                var filterForm = new SubmitForm();

                filterForm.AddValue("FORM_TYPE", DataFieldType.Hidden, xmlns);

                if (with != null)
                {
                    filterForm.AddUntypedValue("with", with);
                }

                if (start != null)
                {
                    filterForm.AddUntypedValue("start", DateTimeProfiles.ToXmppDateTimeString(start.Value));
                }

                if (end != null)
                {
                    filterForm.AddUntypedValue("end", DateTimeProfiles.ToXmppDateTimeString(end.Value));
                }

                request.Child(filterForm.ToXmlElement());
            }

            var tcs = new TaskCompletionSource<XmppPage<Message>>();
            var queryTask = pendingQueries[queryId] = new ArchiveQueryTask(tcs);

            var response = im.IqRequest(Sharp.Xmpp.Core.IqType.Set, roomId, null, request);

            if (response.Type == Core.IqType.Error)
            {
                queryTask.SetException(Util.ExceptionFromError(response, "Failed to get archived messages"));
            }

            return tcs.Task;
        }

        /// <summary>
        /// Invoked when a message stanza has been received.
        /// </summary>
        /// <param name="stanza">The stanza which has been received.</param>
        /// <returns>true to intercept the stanza or false to pass the stanza
        /// on to the next handler.</returns>
        public bool Input(Message stanza)
        {
            var resultNode = stanza.Data["result"];
            if (resultNode != null && resultNode.NamespaceURI == xmlns)
            {
                var queryIdAttribute = resultNode.Attributes["queryid"];
                if (queryIdAttribute != null)
                {
                    ArchiveQueryTask queryTask = null;
                    if (pendingQueries.TryGetValue(queryIdAttribute.InnerText, out queryTask))
                    {
                        var forwardedMessageNode = resultNode["forwarded"];
                        if (forwardedMessageNode != null && forwardedMessageNode.NamespaceURI == "urn:xmpp:forward:0")
                        {
                            var forwardedTimestamp = DelayedDelivery.GetDelayedTimestampOrNow(forwardedMessageNode);
                            var message = new Message(forwardedMessageNode["message"], forwardedTimestamp);

                            lock (queryTask)
                            {
                                queryTask.Messages.Add(message);

                                return true;
                            }
                        }
                    }
                }
            }

            return TryFinaliseQuery(stanza.Data);
        }

        /// <summary>
        /// Invoked when an IQ stanza is being received.
        /// </summary>
        /// <param name="stanza">The stanza which is being received.</param>
        /// <returns>true to intercept the stanza or false to pass the stanza
        /// on to the next handler.</returns>
        public bool Input(Core.Iq stanza)
        {
            return TryFinaliseQuery(stanza.Data);
        }

        /// <summary>
        /// Try and grab finalisation information for a pending query
        /// </summary>
        /// <param name="xml">The xml node to search for a fin node</param>
        private bool TryFinaliseQuery(XmlElement xml)
        {
            //The spec says that the <fin> element should arrive in an <iq>, but Prosody seems to send it in a <message>...
            var finNode = xml["fin"];
            if (finNode != null && finNode.NamespaceURI == xmlns)
            {
                var queryIdAttribute = finNode.Attributes["queryid"];
                if (queryIdAttribute != null)
                {
                    ArchiveQueryTask queryTask = null;
                    if (pendingQueries.TryGetValue(queryIdAttribute.InnerText, out queryTask))
                    {
                        var setNode = finNode["set"];
                        if (setNode != null && setNode.NamespaceURI == "http://jabber.org/protocol/rsm")
                        {
                            queryTask.Finalise(setNode);
                            return true;
                        }
                        else
                        {
                            queryTask.SetException(new XmppException("Received notification that the archived messages query has finished, but the notification did not contain result set information"));
                        }
                    }
                }
            }

            return false;
        }
    }
}