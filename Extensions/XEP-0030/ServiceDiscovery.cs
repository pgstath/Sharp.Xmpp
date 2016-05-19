using Sharp.Xmpp.Core;
using Sharp.Xmpp.Im;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace Sharp.Xmpp.Extensions
{
    /// <summary>
    /// Implements the 'Service Discovery' extension as defined in XEP-0030.
    /// </summary>
    internal class ServiceDiscovery : XmppExtension, IInputFilter<Iq>
    {
        /// <summary>
        /// A dictionary for caching supported services of XMPP entities.
        /// </summary>
        private IDictionary<Jid, IEnumerable<Extension>> cache =
            new Dictionary<Jid, IEnumerable<Extension>>();

        /// <summary>
        /// An enumerable collection of XMPP namespaces the extension implements.
        /// </summary>
        /// <remarks>This is used for compiling the list of supported extensions
        /// advertised by the 'Service Discovery' extension.</remarks>
        public override IEnumerable<string> Namespaces
        {
            get
            {
                return new string[] {
					"http://jabber.org/protocol/disco#info",
					"http://jabber.org/protocol/disco#items"
				};
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
                return Extension.ServiceDiscovery;
            }
        }

        /// <summary>
        /// Returns an enumerable collection of xmlns extension namespaces supported by
        /// this XMPP implementation.
        /// </summary>
        public IEnumerable<string> Features
        {
            get
            {
                return CompileFeatureSet();
            }
        }

        /// <summary>
        /// The identity of this XMPP entity.
        /// </summary>
        public Identity Identity
        {
            get;
            private set;
        }

        /// <summary>
        /// Invoked when an IQ stanza is being received.
        /// </summary>
        /// <param name="stanza">The stanza which is being received.</param>
        /// <returns>true to intercept the stanza or false to pass the stanza
        /// on to the next handler.</returns>
        public bool Input(Iq stanza)
        {
            if (stanza.Type != IqType.Get)
                return false;
            var query = stanza.Data["query"];
            if (query == null)
                return false;
            // TODO: Implement item discovery.
            if (query.NamespaceURI == "http://jabber.org/protocol/disco#items")
            {
                im.IqResult(stanza, Xml.Element("query",
                    "http://jabber.org/protocol/disco#items"));
                return true;
            }
            if (query.NamespaceURI != "http://jabber.org/protocol/disco#info")
                return false;
            // Construct the response which consists of an 'identity' element and a set of
            // 'feature' elements.
            var ident = Xml.Element("identity").Attr("category", Identity.Category)
                .Attr("type", Identity.Type)
                .Attr("name", Identity.Name);
            var xml = Xml.Element("query", "http://jabber.org/protocol/disco#info")
                .Child(ident);
            foreach (string xmlns in CompileFeatureSet())
                xml.Child(Xml.Element("feature").Attr("var", xmlns));
            // Send the IQ response.
            im.IqResult(stanza, xml);
            // We took care of this IQ request, so intercept it, i.e. don't pass it
            // on to other handlers.
            return true;
        }

        /// <summary>
        /// Determines whether the XMPP entity with the specified JID supports the
        /// specified XMPP extension.
        /// </summary>
        /// <typeparam name="T">The XMPP extension to probe for.</typeparam>
        /// <param name="jid">The JID of the XMPP entity.</param>
        /// <returns>true if the XMPP entity with the specified JID supports the
        /// specified XMPP extension; Otherwise false.</returns>
        /// <exception cref="ArgumentNullException">The jid parameter is null.</exception>
        /// <exception cref="NotSupportedException">The XMPP entity with the specified
        /// JID does not support querying of feature information.</exception>
        public bool Supports<T>(Jid jid) where T : XmppExtension
        {
            jid.ThrowIfNull("jid");
            T ext = im.GetExtension<T>();
            return Supports(jid, ext.Xep);
        }

        /// <summary>
        /// Determines whether the XMPP entity with the specified JID supports the
        /// specified XMPP extension(s).
        /// </summary>
        /// <param name="jid">The JID of the XMPP entity.</param>
        /// <param name="extensions">The XMPP extensions to probe for.</param>
        /// <returns>true if the XMPP entity with the specified JID supports the
        /// specified XMPP extension(s); Otherwise false.</returns>
        /// <exception cref="ArgumentNullException">The jid paramter or the extensions
        /// parameter is null.</exception>
        /// <exception cref="NotSupportedException">The XMPP entity with the
        /// specified JID does not support querying of feature information.</exception>
        public bool Supports(Jid jid, params Extension[] extensions)
        {
            jid.ThrowIfNull("jid");
            extensions.ThrowIfNull("extensions");
            // Have the features of the JID been cached yet?
            if (!cache.ContainsKey(jid))
                // Perform SDisco request and cache the result.
                cache.Add(jid, QueryFeatures(jid));
            IEnumerable<Extension> supported = cache[jid];
            foreach (Extension ext in extensions)
            {
                if (!supported.Contains(ext))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Retrieves an enumerable collection of XMPP extensions that the XMPP
        /// entity with the specified JID supports.
        /// </summary>
        /// <param name="jid">The JID of the XMPP entity to retrieve supported
        /// extensions for.</param>
        /// <returns>An enumerable collection of XMPP extensions supported by the
        /// XMPP entity with the specified JID.</returns>
        /// <exception cref="ArgumentNullException">The jid parameter is
        /// null.</exception>
        /// <exception cref="NotSupportedException">The XMPP entity with the
        /// specified JID does not support querying of feature information.</exception>
        public IEnumerable<Extension> GetExtensions(Jid jid)
        {
            jid.ThrowIfNull("jid");
            if (!cache.ContainsKey(jid))
                cache.Add(jid, QueryFeatures(jid));
            return cache[jid];
        }

        /// <summary>
        /// Queries the XMPP entity with the specified JID for identity information.
        /// </summary>
        /// <param name="jid">The JID of the XMPP entity to query.</param>
        /// <returns>An enumerable collection of identities of the XMPP entity
        /// with the specified JID.</returns>
        /// <exception cref="ArgumentNullException">The jid parameter
        /// is null.</exception>
        /// <exception cref="NotSupportedException">The query could not be
        /// performed or the response was invalid.</exception>
        public IEnumerable<Identity> GetIdentities(Jid jid)
        {
            return QueryIdentities(jid);
        }

        /// <summary>
        /// Queries the XMPP entity with the specified JID for item information.
        /// </summary>
        /// <param name="jid">The JID of the XMPP entity to query.</param>
        /// <returns>An enumerable collection of items of the XMPP entity with
        /// the specified JID.</returns>
        /// <exception cref="ArgumentNullException">The jid parameter is
        /// null.</exception>
        /// <exception cref="NotSupportedException">The query could not be
        /// performed or the response was invalid.</exception>
        public IEnumerable<XmppItem> GetItems(Jid jid)
        {
            return QueryItems(jid);
        }

        /// <summary>
        /// Initializes a new instance of the XmppIm class.
        /// </summary>
        /// <param name="im">A reference to the XmppIm instance on whose behalf this
        /// instance is created.</param>
        public ServiceDiscovery(XmppIm im)
            : base(im)
        {
            Attribute attr = Assembly.GetExecutingAssembly().
                GetCustomAttribute(typeof(AssemblyProductAttribute));
            string name = attr != null ? ((AssemblyProductAttribute)attr).Product :
                "S22.Xmpp";
            Identity = new Identity("client", "pc", name);
        }

        /// <summary>
        /// Compiles the set of supported XMPP features.
        /// </summary>
        /// <returns>The set of supported XMPP extensions.</returns>
        private IEnumerable<string> CompileFeatureSet()
        {
            ISet<string> set = new HashSet<string>();
            foreach (XmppExtension ext in im.Extensions)
            {
                foreach (string xmlns in ext.Namespaces)
                    set.Add(xmlns);
            }
            return set;
        }

        /// <summary>
        /// Queries the XMPP entity with the specified JID for information.
        /// </summary>
        /// <param name="jid">The JID of the XMPP entity to query.</param>
        /// <returns>An enumerable collection of values from the Extension enumeration
        /// denoting the XMPP extensions the entity supports.</returns>
        /// <exception cref="ArgumentNullException">The jid parameter
        /// is null.</exception>
        /// <exception cref="NotSupportedException">The query could not be
        /// performed or the response was invalid.</exception>
        private IEnumerable<Extension> QueryFeatures(Jid jid)
        {
            jid.ThrowIfNull("jid");
            Iq iq = im.IqRequest(IqType.Get, jid, im.Jid,
                Xml.Element("query", "http://jabber.org/protocol/disco#info"));
            if (iq.Type != IqType.Result)
                throw new NotSupportedException("Could not query features: " + iq);
            // Parse the result.
            var query = iq.Data["query"];
            if (query == null || query.NamespaceURI != "http://jabber.org/protocol/disco#info")
                throw new NotSupportedException("Erroneous response: " + iq);
            ISet<string> ns = new HashSet<string>();
            foreach (XmlElement e in query.GetElementsByTagName("feature"))
                ns.Add(e.GetAttribute("var"));
            // Go through each extension we support and see if the entity supports
            // all of the extension's namespaces.
            ISet<Extension> feats = new HashSet<Extension>();
            foreach (XmppExtension ext in im.Extensions)
            {
                if (ns.IsSupersetOf(ext.Namespaces))
                    feats.Add(ext.Xep);
            }
            return feats;
        }

        /// <summary>
        /// Queries the XMPP entity with the specified JID for identity information.
        /// </summary>
        /// <param name="jid">The JID of the XMPP entity to query.</param>
        /// <returns>An enumerable collection of identities of the XMPP entity
        /// with the specified JID.</returns>
        /// <exception cref="ArgumentNullException">The jid parameter
        /// is null.</exception>
        /// <exception cref="NotSupportedException">The query could not be
        /// performed or the response was invalid.</exception>
        private IEnumerable<Identity> QueryIdentities(Jid jid)
        {
            jid.ThrowIfNull("jid");
            Iq iq = im.IqRequest(IqType.Get, jid, im.Jid,
                Xml.Element("query", "http://jabber.org/protocol/disco#info"));
            if (iq.Type != IqType.Result)
                throw new NotSupportedException("Could not query features: " + iq);
            // Parse the result.
            var query = iq.Data["query"];
            if (query == null || query.NamespaceURI != "http://jabber.org/protocol/disco#info")
                throw new NotSupportedException("Erroneous response: " + iq);
            ISet<Identity> idents = new HashSet<Identity>();
            foreach (XmlElement e in query.GetElementsByTagName("identity"))
            {
                string cat = e.GetAttribute("category"), type = e.GetAttribute("type"),
                    name = e.GetAttribute("name");
                if (String.IsNullOrEmpty(cat) || String.IsNullOrEmpty(type))
                    continue;
                idents.Add(new Identity(cat, type,
                    String.IsNullOrEmpty(name) ? null : name));
            }
            return idents;
        }

        /// <summary>
        /// Queries the XMPP entity with the specified JID for item information.
        /// </summary>
        /// <param name="jid">The JID of the XMPP entity to query.</param>
        /// <returns>An enumerable collection of itms of the XMPP entity
        /// with the specified JID.</returns>
        /// <exception cref="ArgumentNullException">The jid parameter
        /// is null.</exception>
        /// <exception cref="NotSupportedException">The query could not be
        /// performed or the response was invalid.</exception>
        private IEnumerable<XmppItem> QueryItems(Jid jid)
        {
            jid.ThrowIfNull("jid");
            Iq iq = im.IqRequest(IqType.Get, jid, im.Jid,
                Xml.Element("query", "http://jabber.org/protocol/disco#items"));
            if (iq.Type != IqType.Result)
                throw new NotSupportedException("Could not query items: " + iq);
            // Parse the result.
            var query = iq.Data["query"];
            if (query == null || query.NamespaceURI != "http://jabber.org/protocol/disco#items")
                throw new NotSupportedException("Erroneous response: " + iq);
            ISet<XmppItem> items = new HashSet<XmppItem>();
            foreach (XmlElement e in query.GetElementsByTagName("item"))
            {
                string _jid = e.GetAttribute("jid"), node = e.GetAttribute("node"),
                    name = e.GetAttribute("name");
                if (String.IsNullOrEmpty(_jid))
                    continue;
                try
                {
                    Jid itemJid = new Jid(_jid);
                    items.Add(new XmppItem(itemJid, String.IsNullOrEmpty(node) ? null : node,
                        String.IsNullOrEmpty(name) ? null : name));
                }
                catch (ArgumentException)
                {
                    // The JID is malformed, ignore the item.
                }
            }
            return items;
        }
    }
}
