using XMPPEngineer.Im;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace XMPPEngineer.Extensions
{
    /// <summary>
    /// Implements the 'Entity Capabilities' extension as defined in XEP-0092.
    /// </summary>
    internal class EntityCapabilities : XmppExtension, IInputFilter<Presence>,
        IOutputFilter<Presence>
    {
        /// <summary>
        /// A reference to the 'Service Discovery' extension instance.
        /// </summary>
        private ServiceDiscovery sdisco;

        /// <summary>
        /// A dictionary for caching the 'ver' hash of each JID.
        /// </summary>
        private IDictionary<Jid, string> hashes = new Dictionary<Jid, string>();

        /// <summary>
        /// A dictionary of cached features.
        /// </summary>
        private IDictionary<string, IEnumerable<Extension>> cachedFeatures =
            new Dictionary<string, IEnumerable<Extension>>();

        /// <summary>
        /// The value of the 'node' attribute of the 'e' element, which should
        /// be an URI according to specification.
        /// </summary>
        private string nodeUri
        {
            get
            {
                // FIXME: Move this to a resource file or to assembly metadata?
                return "XMPPEngineer";
            }
        }

        /// <summary>
        /// An enumerable collection of XMPP namespaces the extension implements.
        /// </summary>
        /// <remarks>This is used for compiling the list of supported extensions
        /// advertised by the 'Service Discovery' extension.</remarks>
        public override IEnumerable<string> Namespaces
        {
            get
            {
                return new string[] { "http://jabber.org/protocol/caps" };
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
                return Extension.EntityCapabilities;
            }
        }

        /// <summary>
        /// Invoked after all extensions have been loaded.
        /// </summary>
        public override void Initialize()
        {
            // Get a reference to the SDisco extension.
            sdisco = im.GetExtension<ServiceDiscovery>();
        }

        /// <summary>
        /// Invoked when a presence stanza is being received.
        /// </summary>
        /// <param name="stanza">The stanza which is being received.</param>
        /// <returns>true to intercept the stanza or false to pass the stanza
        /// on to the next handler.</returns>
        public bool Input(Presence stanza)
        {
            var c = stanza.Data["c"];
            if (c == null || c.NamespaceURI != "http://jabber.org/protocol/caps")
                return false;
            string hash = c.GetAttribute("hash"), ver = c.GetAttribute("ver"),
                node = c.GetAttribute("node");
            if (String.IsNullOrEmpty(hash) || String.IsNullOrWhiteSpace(ver))
                return false;
            hashes[stanza.From] = ver;
            // Don't swallow the presence stanza.
            return false;
        }

        /// <summary>
        /// Invoked when a presence stanza is being sent.
        /// </summary>
        /// <param name="stanza">The stanza which is being sent.</param>
        public void Output(Presence stanza)
        {
            var c = Xml.Element("c", "http://jabber.org/protocol/caps")
                .Attr("hash", "sha-1")
                .Attr("node", nodeUri)
                .Attr("ver", GenerateVerificationString());
            // Add the <c/> element to the presence stanza.
            stanza.Data.Child(c);
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
        /// <exception cref="NotSupportedException">The XMPP entity with
        /// the specified JID does not support querying of feature
        /// information.</exception>
        public IEnumerable<Extension> GetExtensions(Jid jid)
        {
            jid.ThrowIfNull("jid");
            if (hashes.ContainsKey(jid))
            {
                string hash = hashes[jid];
                // If the feature set has already been cached, return it; Otherwise
                // request the feature set and subsequently cache it.
                // FIXME: Calculate hash of feature set and ensure it equals stored hash.
                if (!cachedFeatures.ContainsKey(hash))
                    cachedFeatures.Add(hash, sdisco.GetExtensions(jid));
                return cachedFeatures[hash];
            }
            // If we don't have a hash for the jid, the XMPP entity probably does not
            // support 'caps' so resort to a normal SDisco request.
            return sdisco.GetExtensions(jid);
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
            return sdisco.GetIdentities(jid);
        }

        /// <summary>
        /// Determines whether the XMPP entity with the specified JID supports the
        /// specified XMPP extension.
        /// </summary>
        /// <typeparam name="T">The XMPP extension to probe for.</typeparam>
        /// <param name="jid">The JID of the XMPP entity.</param>
        /// <returns>true if the XMPP entity with the specified JID supports the
        /// specified XMPP extension; Otherwise false.</returns>
        /// <exception cref="ArgumentNullException">The jid parameter
        /// is null.</exception>
        /// <exception cref="NotSupportedException">The XMPP entity with
        /// the specified JID does not support querying of feature
        /// information.</exception>
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
        /// <exception cref="ArgumentNullException">The jid parameter or the
        /// extensions parameter is null.</exception>
        /// <exception cref="NotSupportedException">The XMPP entity with
        /// the specified JID does not support querying of feature
        /// information.</exception>
        public bool Supports(Jid jid, params Extension[] extensions)
        {
            jid.ThrowIfNull("jid");
            extensions.ThrowIfNull("extensions");
            IEnumerable<Extension> supported = GetExtensions(jid);
            foreach (Extension ext in extensions)
            {
                if (!supported.Contains(ext))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Determines whether the XMPP entity with the specified JID has the
        /// identity with the specified category and type attributes.
        /// </summary>
        /// <param name="jid">The JID of the XMPP entity.</param>
        /// <param name="category">The category of the identity to probe for.</param>
        /// <param name="type">The type of the identity to probe for.</param>
        /// <returns>true if the XMPP entity with the specified JID has the
        /// identity with the specified attributes; Otherwise false.</returns>
        /// <exception cref="ArgumentNullException">The jid parameter or the
        /// category parameter or the type parameter is null.</exception>
        /// <exception cref="NotSupportedException">The XMPP entity with
        /// the specified JID does not support querying of feature
        /// information.</exception>
        public bool HasIdentity(Jid jid, string category, string type)
        {
            jid.ThrowIfNull("jid");
            category.ThrowIfNull("category");
            type.ThrowIfNull("type");
            foreach (Identity ident in GetIdentities(jid))
            {
                if (ident.Category == category && ident.Type == type)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Initializes a new instance of the EntityCapabilities class.
        /// </summary>
        /// <param name="im">A reference to the XmppIm instance on whose behalf this
        /// instance is created.</param>
        public EntityCapabilities(XmppIm im)
            : base(im)
        {
        }

        /// <summary>
        /// Generates the verification string.
        /// </summary>
        /// <returns>The verfication string of this XMPP entity, denoting this
        /// client's capabilities.</returns>
        private string GenerateVerificationString()
        {
            Identity ident = sdisco.Identity;
            StringBuilder s = new StringBuilder(ident.Category + "/" +
                ident.Type + "//" + ident.Name + "<");
            List<string> list = new List<string>(sdisco.Features);
            list.Sort();
            foreach (string xmlns in list)
                s.Append(xmlns + "<");
            byte[] bytes = Encoding.UTF8.GetBytes(s.ToString());
            using (var sha1 = new SHA1Managed())
                return Convert.ToBase64String(sha1.ComputeHash(bytes));
        }

        /// <summary>
        /// Generates a BASE64-encoded hash from the specified input string
        /// using the specified hashing algorithm.
        /// </summary>
        /// <param name="input">The input string to hash.</param>
        /// <param name="algorithm">The hashing algorithm to use.</param>
        /// <returns>The hashed input as a BASE64-encoded string.</returns>
        /// <exception cref="ArgumentNullException">The input parameter or the
        /// algorithm parameter is null.</exception>
        /// <exception cref="NotSupportedException">The specified
        /// algorithm is not supported.</exception>
        private string Hash(string input, HashAlgorithm algorithm)
        {
            input.ThrowIfNull("input");
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(algorithm.ComputeHash(bytes));
        }

        /// <summary>
        /// Parses the 'hash' attribute of the 'c' element.
        /// </summary>
        /// <param name="algorithm">The value of the 'hash' attribute.</param>
        /// <returns>An instance of the hashing algorithm to use or null if no
        /// matching type could be found.</returns>
        /// <exception cref="ArgumentNullException">The algorithm paramter
        /// is null.</exception>
        private HashAlgorithm ParseHashAlgorithm(string algorithm)
        {
            algorithm.ThrowIfNull("algorithm");
            var dict = new Dictionary<string, Func<HashAlgorithm>>
                (StringComparer.InvariantCultureIgnoreCase) {
				{ "sha-1",   () => new SHA1Managed() },
				{ "sha-256", () => new SHA256Managed() },
				{ "sha-384", () => new SHA384Managed() },
				{ "sha-512", () => new SHA512Managed() },
				{ "md5",     () => new MD5CryptoServiceProvider() },
			};
            return dict.ContainsKey(algorithm) ? dict[algorithm].Invoke() : null;
        }
    }
}