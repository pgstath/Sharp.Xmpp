using XMPPEngineer.Im;
using System;
using System.Collections.Generic;

namespace XMPPEngineer.Extensions
{
    /// <summary>
    /// Implements the 'Attention' extension as defined in XEP-0224.
    /// </summary>
    internal class Attention : XmppExtension
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
                return new string[] { "urn:xmpp:attention:0" };
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
                return Extension.Attention;
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
        /// Initializes a new instance of the Attention class.
        /// </summary>
        /// <param name="im">A reference to the XmppIm instance on whose behalf this
        /// instance is created.</param>
        public Attention(XmppIm im)
            : base(im)
        {
        }

        /// <summary>
        /// Gets the attention of the XMPP user with the specified JID.
        /// </summary>
        /// <param name="jid">The JID of the user to grab the attention of.</param>
        /// <param name="message">A message to sent along.</param>
        /// <exception cref="ArgumentNullException">The jid parameter
        /// is null.</exception>
        /// <exception cref="NotSupportedException">The XMPP entity with
        /// the specified JID does not support the 'Attention' XMPP
        /// extension.</exception>
        public void GetAttention(Jid jid, string message = null)
        {
            jid.ThrowIfNull("jid");
            if (!ecapa.Supports(jid, Extension.Attention))
            {
                throw new NotSupportedException("The XMPP entity does not support the " +
                    "'Attention' extension.");
            }
            Im.Message m = new Im.Message(jid, message);
            // Add the 'attention' element to the message.
            m.Data.Child(Xml.Element("attention", "urn:xmpp:attention:0"));
            im.SendMessage(m);
        }
    }
}