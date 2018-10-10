using XMPPEngineer.Core;
using XMPPEngineer.Im;
using System;
using System.Collections.Generic;

namespace XMPPEngineer.Extensions
{
    /// <summary>
    /// Implements the 'Bits of Binary' extension as defined in XEP-0231.
    /// </summary>
    internal class BitsOfBinary : XmppExtension, IInputFilter<Iq>
    {
        /// <summary>
        /// A reference to the 'Entity Capabilities' extension instance.
        /// </summary>
        private EntityCapabilities ecapa;

        /// <summary>
        /// A cache of binary data items indexed by cid.
        /// </summary>
        private IDictionary<string, BobData> cache = new Dictionary<string, BobData>();

        /// <summary>
        /// An enumerable collection of XMPP namespaces the extension implements.
        /// </summary>
        /// <remarks>This is used for compiling the list of supported extensions
        /// advertised by the 'Service Discovery' extension.</remarks>
        public override IEnumerable<string> Namespaces
        {
            get
            {
                return new string[] { "urn:xmpp:bob" };
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
                return Extension.BitsOfBinary;
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
        /// Creates a new bit of binary data.
        /// </summary>
        /// <param name="data">The data to embed.</param>
        /// <param name="type">The content-type of the data.</param>
        /// <param name="cache">true to cache the newly created bit of binary
        /// data.</param>
        /// <returns>An initialized instance of the BobData class representing the
        /// bit of binary data.</returns>
        /// <exception cref="ArgumentNullException">The data parameter or the type
        /// parameter is null.</exception>
        public BobData Create(byte[] data, string type, bool cache = true)
        {
            data.ThrowIfNull("data");
            type.ThrowIfNull("type");
            BobData b = new BobData(data, type);
            if (cache)
                this.cache[b.Cid] = b;
            return b;
        }

        /// <summary>
        /// Adds the specified BobData object to the cache of cached bits-of-binary
        /// data.
        /// </summary>
        /// <param name="bob">The BobData object to add to the cache.</param>
        /// <exception cref="ArgumentNullException">The bob parameter is
        /// null.</exception>
        public void Add(BobData bob)
        {
            bob.ThrowIfNull("bob");
            cache[bob.Cid] = bob;
        }

        /// <summary>
        /// Retrieves the data-item with the specified CID from the cache of
        /// data-items.
        /// </summary>
        /// <param name="cid">The CID of the binary data to retrieve.</param>
        /// <returns>The data-item with the specified CID.</returns>
        /// <exception cref="ArgumentNullException">The cid parameter is
        /// null.</exception>
        /// <exception cref="ArgumentException">A data-item with the specified CID
        /// does not exist in the local cache of data-items.</exception>
        public BobData Get(string cid)
        {
            cid.ThrowIfNull("cid");
            if (cache.ContainsKey(cid))
                return cache[cid];
            throw new ArgumentException("A data-item with the specified CID does " +
                "not exist.");
        }

        /// <summary>
        /// Retrieves the data-item with the specified CID from the XMPP entity
        /// with the specified JID.
        /// </summary>
        /// <param name="cid">The CID of the binary data to retrieve.</param>
        /// <param name="from">The JID of the XMPP entity to request the data
        /// from.</param>
        /// <param name="cache">true to store the requested item in the local
        /// cache for future references.</param>
        /// <returns>The data-item with the specified CID.</returns>
        /// <exception cref="ArgumentNullException">The cid parameter or the from
        /// parameter is null.</exception>
        /// <exception cref="NotSupportedException">The XMPP entity with
        /// the specified JID does not support the 'Bits of Binary' XMPP
        /// extension.</exception>
        /// <exception cref="XmppErrorException">The server returned an XMPP error code.
        /// Use the Error property of the XmppErrorException to obtain the specific
        /// error condition.</exception>
        /// <exception cref="XmppException">The server returned invalid data or another
        /// unspecified XMPP error occurred.</exception>
        public BobData Get(string cid, Jid from, bool cache = true)
        {
            cid.ThrowIfNull("cid");
            from.ThrowIfNull("from");
            // If the data is already in the cache, return it.
            if (this.cache.ContainsKey(cid))
                return this.cache[cid];
            if (!ecapa.Supports(from, Extension.BitsOfBinary))
            {
                throw new NotSupportedException("The XMPP entity does not support " +
                    "the 'Bits of Binary' extension.");
            }
            // Request the data.
            Iq iq = im.IqRequest(IqType.Get, from, im.Jid,
                Xml.Element("data", "urn:xmpp:bob").Attr("cid", cid));
            if (iq.Type == IqType.Error)
                throw Util.ExceptionFromError(iq, "The data-item with the specified " +
                    "CID could not be retrieved.");
            var data = iq.Data["data"];
            if (data == null || data.NamespaceURI != "urn:xmpp:bob")
                throw new XmppException("Erroneous response.");
            try
            {
                // Parse the response 'data' element.
                BobData b = BobData.Parse(data);
                if (cache)
                    this.cache[cid] = b;
                return b;
            }
            catch (ArgumentException e)
            {
                throw new XmppException("The retrieved data-item could not be " +
                    "processed.", e);
            }
        }

        /// <summary>
        /// Initializes a new instance of the BitsOfBinary class.
        /// </summary>
        /// <param name="im">A reference to the XmppIm instance on whose behalf this
        /// instance is created.</param>
        public BitsOfBinary(XmppIm im)
            : base(im)
        {
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
            var e = stanza.Data["data"];
            if (e == null || e.NamespaceURI != "urn:xmpp:bob")
                return false;
            string cid = e.GetAttribute("cid");
            if (cache.ContainsKey(cid))
            {
                BobData b = cache[cid];
                var data = Xml.Element("data", "urn:xmpp:bob").Attr("cid", cid)
                    .Attr("type", b.Type)
                    .Text(Convert.ToBase64String(b.Data));
                im.IqResult(stanza, data);
            }
            else
            {
                // If we don't have the item, return an 'item not found' error.
                im.IqError(stanza, ErrorType.Cancel, ErrorCondition.ItemNotFound);
            }
            // We took care of this IQ request, so intercept it and don't pass it
            // on to other handlers.
            return true;
        }
    }
}