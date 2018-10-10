using XMPPEngineer.Core;
using XMPPEngineer.Im;
using System;
using System.Collections.Generic;
using System.Xml;

namespace XMPPEngineer.Extensions
{
    /// <summary>
    /// Implements the 'Personal Eventing Protocol' extension as defined in XEP-0163.
    /// </summary>
    internal class Pep : XmppExtension, IInputFilter<Im.Message>
    {
        /// <summary>
        /// A reference to the 'Entity Capabilities' extension instance.
        /// </summary>
        private EntityCapabilities ecapa;

        /// <summary>
        /// Determines whether our server supports the PEP extension.
        /// </summary>
        private bool supported;

        /// <summary>
        /// true if the initial identity query has been performed.
        /// </summary>
        private bool initialized;

        /// <summary>
        /// A dictionary of callback methods registered for specific events.
        /// </summary>
        private IDictionary<string, Action<Jid, XmlElement>> callbacks =
            new Dictionary<string, Action<Jid, XmlElement>>();

        /// <summary>
        /// An enumerable collection of XMPP namespaces the extension implements.
        /// </summary>
        /// <remarks>This is used for compiling the list of supported extensions
        /// advertised by the 'Service Discovery' extension.</remarks>
        public override IEnumerable<string> Namespaces
        {
            get
            {
                return new string[] { "http://jabber.org/protocol/pubsub" };
            }
        }

        /// <summary>
        /// The named constant of the Extension enumeration that corresponds to this
        /// extension.
        /// </summary>
        public override Extension Xep
        {
            get
            {
                return Extension.PersonalEventingProcotol;
            }
        }

        /// <summary>
        /// Determines whether our server supports the personal eventing protocol.
        /// </summary>
        public bool Supported
        {
            get
            {
                if (!initialized)
                    return QueryServer();
                return supported;
            }
        }

        /// <summary>
        /// Invoked after all extensions have been loaded.
        /// </summary>
        public override void Initialize()
        {
            ecapa = im.GetExtension<EntityCapabilities>();
        }

        /// <summary>
        /// Invoked when a Message stanza is being received.
        /// </summary>
        /// <param name="stanza">The stanza which is being received.</param>
        /// <returns>true to intercept the stanza or false to pass the stanza
        /// on to the next handler.</returns>
        public bool Input(Im.Message stanza)
        {
            var ev = stanza.Data["event"];
            if (ev == null || ev.NamespaceURI != "http://jabber.org/protocol/pubsub#event")
                return false;
            var items = ev["items"];
            if (items == null)
                return false;
            string nodeId = items.GetAttribute("node");
            if (String.IsNullOrEmpty(nodeId))
                return false;
            // FIXME: Should we let the callback decide whether the message stanza
            // should be swallowed or passed on?
            if (callbacks.ContainsKey(nodeId))
            {
                callbacks[nodeId].Invoke(stanza.From, items["item"]);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Publishes the specified data to the specified node.
        /// </summary>
        /// <param name="node">The Id of the node to publish to.</param>
        /// <param name="itemId">The id of the 'item' element which is being
        /// published.</param>
        /// <param name="data">The data to include in the 'item' element of the
        /// publish request. If this is omitted, no data will be added to the publish
        /// request.</param>
        /// <exception cref="ArgumentNullException">The node parameter is null.</exception>
        /// <exception cref="NotSupportedException">The server does not support the
        /// 'Personal Eventing Protocol' extension.</exception>
        /// <exception cref="XmppErrorException">The server returned an XMPP error code.
        /// Use the Error property of the XmppErrorException to obtain the specific
        /// error condition.</exception>
        /// <exception cref="XmppException">The server returned invalid data or another
        /// unspecified XMPP error occurred.</exception>
        public void Publish(string node, string itemId = null, params XmlElement[] data)
        {
            node.ThrowIfNull("node");
            if (!Supported)
            {
                throw new NotSupportedException("The server does not support publishing " +
                    "of information.");
            }
            var xml = Xml.Element("pubsub", "http://jabber.org/protocol/pubsub")
                .Child(Xml.Element("publish").Attr("node", node));
            if (data != null)
            {
                var item = Xml.Element("item");
                if (itemId != null)
                    item.Attr("id", itemId);
                foreach (var d in data)
                {
                    if (d != null)
                        item.Child(d);
                }
                if (!item.IsEmpty)
                    xml["publish"].Child(item);
            }
            Iq iq = im.IqRequest(IqType.Set, null, im.Jid, xml);
            if (iq.Type == IqType.Error)
                throw Util.ExceptionFromError(iq, "The data could not be published.");
        }

        /// <summary>
        /// Subscribes to the specified node.
        /// </summary>
        /// <param name="node">The id of the node to subscribe to.</param>
        /// <param name="cb">A callback method to invoke whenever an event
        /// notification for the node specified is received.</param>
        /// <exception cref="ArgumentNullException">The node parameter is null or the
        /// cb parameter is null.</exception>
        /// <exception cref="ArgumentException">A callback for the specified node id
        /// has already been installed.</exception>
        public void Subscribe(string node, Action<Jid, XmlElement> cb)
        {
            node.ThrowIfNull("node");
            cb.ThrowIfNull("cb");
            callbacks.Add(node, cb);
        }

        /// <summary>
        /// Unsubscribes from the specified node.
        /// </summary>
        /// <param name="node">The id of the node to unsubscribe from.</param>
        /// <exception cref="ArgumentNullException">The node parameter is null.</exception>
        /// <remarks>If no subscription for the specified node id is found, the
        /// method does nothing.</remarks>
        public void Unsubscribe(string node)
        {
            node.ThrowIfNull("node");
            if (callbacks.ContainsKey(node))
                callbacks.Remove(node);
        }

        /// <summary>
        /// Requests the items of the node with the specified node id.
        /// </summary>
        /// <param name="jid">The JID of the node service.</param>
        /// <param name="node">The Id of the node whose items to retrieve.</param>
        /// <returns>An enumerable collection of items maintained by the node
        /// with the specified node id.</returns>
        /// <exception cref="ArgumentNullException">The jid parameter or the node
        /// parameter is null.</exception>
        /// <exception cref="XmppErrorException">The server returned an XMPP error code.
        /// Use the Error property of the XmppErrorException to obtain the specific
        /// error condition.</exception>
        /// <exception cref="XmppException">The server returned invalid data or another
        /// unspecified XMPP error occurred.</exception>
        public IEnumerable<XmlElement> RetrieveItems(Jid jid, string node)
        {
            jid.ThrowIfNull("jid");
            node.ThrowIfNull("node");
            var xml = Xml.Element("pubsub", "http://jabber.org/protocol/pubsub")
                .Child(Xml.Element("items").Attr("node", node));
            Iq iq = im.IqRequest(IqType.Get, jid, im.Jid, xml);
            if (iq.Type == IqType.Error)
                throw Util.ExceptionFromError(iq, "The items could not be retrieved.");
            var pubsub = iq.Data["pubsub"];
            if (pubsub == null || pubsub.NamespaceURI != "http://jabber.org/protocol/pubsub")
                throw new XmppException("Expected 'pubsub' element: " + iq);
            var items = pubsub["items"];
            if (items == null || items.GetAttribute("node") != node)
                throw new XmppException("Expected 'items' element: " + iq);
            ISet<XmlElement> set = new HashSet<XmlElement>();
            foreach (XmlElement e in items.GetElementsByTagName("item"))
                set.Add(e);
            return set;
        }

        /// <summary>
        /// Retrieves the item with the specified id from the collection of items
        /// of the node with the specified node id.
        /// </summary>
        /// <param name="jid">The JID of the node service.</param>
        /// <param name="node">The Id of the node whose item to retrieve.</param>
        /// <param name="itemId">The id of the item to retrieve.</param>
        /// <returns>The item with the specified item id.</returns>
        /// <exception cref="ArgumentNullException">The jid parameter or the node
        /// parameter or the itemId parameter is null.</exception>
        /// <exception cref="XmppErrorException">The server returned an XMPP error code.
        /// Use the Error property of the XmppErrorException to obtain the specific
        /// error condition.</exception>
        /// <exception cref="XmppException">The server returned invalid data or another
        /// unspecified XMPP error occurred.</exception>
        public XmlElement RetrieveItem(Jid jid, string node, string itemId)
        {
            jid.ThrowIfNull("jid");
            node.ThrowIfNull("node");
            itemId.ThrowIfNull("itemId");
            var xml = Xml.Element("pubsub", "http://jabber.org/protocol/pubsub").Child(
                Xml.Element("items").Attr("node", node).Child(
                Xml.Element("item").Attr("id", itemId))
            );
            Iq iq = im.IqRequest(IqType.Get, jid, im.Jid, xml);
            if (iq.Type == IqType.Error)
                throw Util.ExceptionFromError(iq, "The item could not be retrieved.");
            var pubsub = iq.Data["pubsub"];
            if (pubsub == null || pubsub.NamespaceURI != "http://jabber.org/protocol/pubsub")
                throw new XmppException("Expected 'pubsub' element: " + iq);
            var items = pubsub["items"];
            if (items == null || items.GetAttribute("node") != node)
                throw new XmppException("Expected 'items' element: " + iq);
            if (items["item"] == null || items["item"].GetAttribute("id") != itemId)
                throw new XmppException("Expected 'item' element: " + items.ToXmlString());
            return items["item"];
        }

        /// <summary>
        /// Initializes a new instance of the Pep class.
        /// </summary>
        /// <param name="im">A reference to the XmppIm instance on whose behalf this
        /// instance is created.</param>
        public Pep(XmppIm im)
            : base(im)
        {
        }

        /// <summary>
        /// Queries our own server to see if we support the PEP extension.
        /// </summary>
        /// <returns>true if our server supports the PEP profile; Otherwise
        /// false.</returns>
        private bool QueryServer()
        {
            // See if our own server advertises the pubsub identity.
            foreach (var ident in ecapa.GetIdentities(im.Jid.GetBareJid()))
            {
                if (ident.Type == "pep" && ident.Category == "pubsub")
                    supported = true;
            }
            // Cache the result.
            initialized = true;
            return supported;
        }
    }
}