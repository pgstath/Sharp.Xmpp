using XMPPEngineer.Core;
using XMPPEngineer.Im;
using System;
using System.Collections.Generic;

namespace XMPPEngineer.Extensions
{
    /// <summary>
    /// Implements the 'Ping' extension as defined in XEP-0199.
    /// </summary>
    internal class Ping : XmppExtension, IInputFilter<Iq>
    {
        /// <summary>
        /// A reference to the 'Entity Capabilities' extension instance.
        /// </summary>
        private EntityCapabilities ecapa;

        /// <summary>
        /// An enumerable collection of XMPP namespaces the extension implements.
        /// </summary>
        /// <remarks>This is used for compiling the list of supported extensions
        /// advertised by the 'Service Discovery' extension.</remarks>
        public override IEnumerable<string> Namespaces
        {
            get
            {
                return new string[] { "urn:xmpp:ping" };
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
                return Extension.Ping;
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
        /// Invoked when an IQ stanza is being received.
        /// </summary>
        /// <param name="stanza">The stanza which is being received.</param>
        /// <returns>true to intercept the stanza or false to pass the stanza
        /// on to the next handler.</returns>
        public bool Input(Iq stanza)
        {
            if (stanza.Type != IqType.Get)
                return false;
            var ping = stanza.Data["ping"];
            if (ping == null || ping.NamespaceURI != "urn:xmpp:ping")
                return false;
            im.IqResult(stanza);
            // We took care of this IQ request, so intercept it and don't pass it
            // on to other handlers.
            return true;
        }

        /// <summary>
        /// Pings the XMPP entity with the specified JID.
        /// </summary>
        /// <param name="jid">The JID of the XMPP entity to ping.</param>
        /// <returns>The time it took to ping the XMPP entity with the specified
        /// JID.</returns>
        /// <exception cref="ArgumentNullException">The jid parameter
        /// is null.</exception>
        /// <exception cref="NotSupportedException">The XMPP entity with
        /// the specified JID does not support the 'Ping' XMPP extension.</exception>
        /// <exception cref="XmppErrorException">The server returned an XMPP error code.
        /// Use the Error property of the XmppErrorException to obtain the specific
        /// error condition.</exception>
        /// <exception cref="XmppException">The server returned invalid data or another
        /// unspecified XMPP error occurred.</exception>
        public TimeSpan PingEntity(Jid jid)
        {
            jid.ThrowIfNull("jid");
            if (!ecapa.Supports(jid, Extension.Ping))
            {
                throw new NotSupportedException("The XMPP entity does not support the " +
                    "'Ping' extension.");
            }
            DateTime start = DateTime.Now;
            Iq iq = im.IqRequest(IqType.Get, jid, im.Jid,
                Xml.Element("ping", "urn:xmpp:ping"));
            if (iq.Type == IqType.Error)
                throw Util.ExceptionFromError(iq, "Could not ping XMPP entity.");
            return DateTime.Now.Subtract(start);
        }

        /// <summary>
        /// Initializes a new instance of the Ping class.
        /// </summary>
        /// <param name="im">A reference to the XmppIm instance on whose behalf this
        /// instance is created.</param>
        public Ping(XmppIm im)
            : base(im)
        {
        }
    }
}