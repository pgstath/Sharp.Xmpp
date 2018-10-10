using XMPPEngineer.Core;
using XMPPEngineer.Im;
using System;
using System.Collections.Generic;
using System.Net;

namespace XMPPEngineer.Extensions
{
    /// <summary>
    /// Implements the 'Server IP Check' extension as defined in XEP-0279.
    /// </summary>
    internal class ServerIpCheck : XmppExtension
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
                return new string[] { "urn:xmpp:sic:1" };
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
                return Extension.ServerIpCheck;
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
        /// Retrieves the client's external IP address.
        /// </summary>
        /// <returns>The XMPP client's external IP address.</returns>
        /// <exception cref="NotSupportedException">The XMPP server does not
        /// support the 'Server IP Check' XMPP extension.</exception>
        /// <exception cref="XmppErrorException">The server returned an XMPP error code.
        /// Use the Error property of the XmppErrorException to obtain the specific
        /// error condition.</exception>
        /// <exception cref="XmppException">The server returned invalid data or another
        /// unspecified XMPP error occurred.</exception>
        public IPAddress GetExternalAddress()
        {
            if (!ecapa.Supports(im.Jid.Domain, Extension.ServerIpCheck))
            {
                throw new NotSupportedException("The XMPP server does not support " +
                    "the 'Server IP Check' extension.");
            }
            Iq iq = im.IqRequest(IqType.Get, null, im.Jid,
                Xml.Element("address", "urn:xmpp:sic:1"));
            if (iq.Type == IqType.Error)
                throw Util.ExceptionFromError(iq, "The external IP address could not " +
                    "be retrieved.");
            var address = iq.Data["address"];
            if (address == null || address["ip"] == null)
                throw new XmppException("Erroneous IQ response.");
            string ip = address["ip"].InnerText;
            try
            {
                return IPAddress.Parse(ip);
            }
            catch (Exception e)
            {
                throw new XmppException("The returned address is not a valid IP " +
                    "address.", e);
            }
        }

        /// <summary>
        /// Initializes a new instance of the ServerIpCheck class.
        /// </summary>
        /// <param name="im">A reference to the XmppIm instance on whose behalf this
        /// instance is created.</param>
        public ServerIpCheck(XmppIm im)
            : base(im)
        {
        }
    }
}