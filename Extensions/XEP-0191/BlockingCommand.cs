using XMPPEngineer.Core;
using XMPPEngineer.Im;
using System;
using System.Collections.Generic;
using System.Xml;

namespace XMPPEngineer.Extensions
{
    /// <summary>
    /// Implements the 'Blocking Command' extension as defined in XEP-0191.
    /// </summary>
    internal class BlockingCommand : XmppExtension, IInputFilter<Iq>
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
                return new string[] { "urn:xmpp:blocking" };
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
                return Extension.BlockingCommand;
            }
        }

        /// <summary>
        /// Determines whether our server supports the 'Blocking Command' extension.
        /// </summary>
        public bool Supported
        {
            get
            {
                return ecapa.Supports(im.Jid.Domain, Extension.BlockingCommand);
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
            if (stanza.Type != IqType.Set)
                return false;
            if (stanza.Data["block"] == null && stanza.Data["unblock"] == null)
                return false;
            bool isBlock = stanza.Data["block"] != null;
            var elem = isBlock ? stanza.Data["block"] : stanza.Data["unblock"];
            // Element could be part of another extension, so examine namespace.
            if (elem.NamespaceURI != "urn:xmpp:blocking")
                return false;
            // Acknowledge the "push" request.
            im.IqResult(stanza);
            // Raise events for blocked/unblocked items.
            foreach (XmlElement item in elem.GetElementsByTagName("item"))
            {
                try
                {
                    Jid jid = item.GetAttribute("jid");
                    if (isBlock)
                    {
                        // FIXME: Raise 'Blocked' event.
                    }
                    else
                    {
                        // FIXME: Raise 'Unblocked' event.
                    }
                }
                catch (FormatException)
                {
                    // Encountered an invalid JID, fall through.
                }
            }
            // We took care of this IQ request, so intercept it and don't pass it
            // on to other handlers.
            return true;
        }

        /// <summary>
        /// Returns an enumerable collection of blocked contacts.
        /// </summary>
        /// <returns>An enumerable collection of JIDs which are on the client's
        /// blocklist.</returns>
        /// <exception cref="NotSupportedException">The XMPP entity with
        /// the specified JID does not support the 'Blocking Command' XMPP
        /// extension.</exception>
        /// <exception cref="XmppErrorException">The server returned an XMPP error code.
        /// Use the Error property of the XmppErrorException to obtain the specific
        /// error condition.</exception>
        /// <exception cref="XmppException">The server returned invalid data or another
        /// unspecified XMPP error occurred.</exception>
        public IEnumerable<Jid> GetBlocklist()
        {
            // Probe for server support.
            if (!ecapa.Supports(im.Jid.Domain, Extension.BlockingCommand))
            {
                throw new NotSupportedException("The server does not support " +
                    "the 'Blocking Command' extension.");
            }
            Iq iq = im.IqRequest(IqType.Get, null, im.Jid,
                Xml.Element("blocklist", "urn:xmpp:blocking"));
            if (iq.Type == IqType.Error)
                throw Util.ExceptionFromError(iq, "The blocklist could not be retrieved.");
            ISet<Jid> set = new HashSet<Jid>();
            var list = iq.Data["blocklist"];
            if (list == null || list.NamespaceURI != "urn:xmpp:blocking")
                throw new XmppException("Erroneous server response.");
            foreach (XmlElement item in list.GetElementsByTagName("item"))
            {
                try
                {
                    string jid = item.GetAttribute("jid");
                    set.Add(jid);
                }
                catch (FormatException e)
                {
                    throw new XmppException("Encountered an invalid JID.", e);
                }
            }
            return set;
        }

        /// <summary>
        /// Blocks all communication to and from the XMPP entity with the specified JID.
        /// </summary>
        /// <param name="jid">The JID of the XMPP entity to block.</param>
        /// <exception cref="ArgumentNullException">The jid parameter is
        /// null.</exception>
        /// <exception cref="NotSupportedException">The XMPP entity with
        /// the specified JID does not support the 'Blocking Command' XMPP
        /// extension.</exception>
        /// <exception cref="XmppErrorException">The server returned an XMPP error code.
        /// Use the Error property of the XmppErrorException to obtain the specific
        /// error condition.</exception>
        /// <exception cref="XmppException">The server returned invalid data or another
        /// unspecified XMPP error occurred.</exception>
        public void Block(Jid jid)
        {
            jid.ThrowIfNull("jid");
            // Probe for server support.
            if (!ecapa.Supports(im.Jid.Domain, Extension.BlockingCommand))
            {
                throw new NotSupportedException("The server does not support " +
                    "the 'Blocking Command' extension.");
            }
            Iq iq = im.IqRequest(IqType.Set, null, im.Jid,
                Xml.Element("block", "urn:xmpp:blocking").Child(
                Xml.Element("item").Attr("jid", jid.ToString())));
            if (iq.Type == IqType.Error)
                throw Util.ExceptionFromError(iq, "The XMPP entity could not be blocked.");
        }

        /// <summary>
        /// Unblocks all communication to and from the XMPP entity with the specified
        /// JID.
        /// </summary>
        /// <param name="jid">The JID of the XMPP entity to unblock.</param>
        /// <exception cref="ArgumentNullException">The jid parameter is
        /// null.</exception>
        /// <exception cref="NotSupportedException">The XMPP entity with
        /// the specified JID does not support the 'Blocking Command' XMPP
        /// extension.</exception>
        /// <exception cref="XmppErrorException">The server returned an XMPP error code.
        /// Use the Error property of the XmppErrorException to obtain the specific
        /// error condition.</exception>
        /// <exception cref="XmppException">The server returned invalid data or another
        /// unspecified XMPP error occurred.</exception>
        public void Unblock(Jid jid)
        {
            jid.ThrowIfNull("jid");
            // Probe for server support.
            if (!ecapa.Supports(im.Jid.Domain, Extension.BlockingCommand))
            {
                throw new NotSupportedException("The server does not support " +
                    "the 'Blocking Command' extension.");
            }
            Iq iq = im.IqRequest(IqType.Set, null, im.Jid,
                Xml.Element("unblock", "urn:xmpp:blocking").Child(
                Xml.Element("item").Attr("jid", jid.ToString())));
            if (iq.Type == IqType.Error)
                throw Util.ExceptionFromError(iq, "The XMPP entity could not be unblocked.");
        }

        /// <summary>
        /// Initializes a new instance of the BlockingCommand class.
        /// </summary>
        /// <param name="im">A reference to the XmppIm instance on whose behalf this
        /// instance is created.</param>
        public BlockingCommand(XmppIm im)
            : base(im)
        {
        }
    }
}