using XMPPEngineer.Core;
using XMPPEngineer.Extensions.Dataforms;
using XMPPEngineer.Im;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace XMPPEngineer.Extensions
{
    /// <summary>
    /// Implements the 'In-Band Registration' extension as defined in XEP-0077.
    /// </summary>
    internal class InBandRegistration : XmppExtension
    {
        /// <summary>
        /// A reference to the 'Entity Capabilities' extension instance.
        /// </summary>
        private EntityCapabilities ecapa;

        /// <summary>
        /// A reference to the 'Bits of Binary' extension instance.
        /// </summary>
        private BitsOfBinary bob;

        /// <summary>
        /// An enumerable collection of XMPP namespaces the extension implements.
        /// </summary>
        /// <remarks>This is used for compiling the list of supported extensions
        /// advertised by the 'Service Discovery' extension.</remarks>
        public override IEnumerable<string> Namespaces
        {
            get
            {
                return new string[] { "jabber:iq:register" };
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
                return Extension.InBandRegistration;
            }
        }

        /// <summary>
        /// Invoked after all extensions have been loaded.
        /// </summary>
        public override void Initialize()
        {
            ecapa = im.GetExtension<EntityCapabilities>();
            bob = im.GetExtension<BitsOfBinary>();
        }

        /// <summary>
        /// Registers a new XMPP account on the connected XMPP server.
        /// </summary>
        /// <param name="callback">A callback method invoked during the registration
        /// process to gather user information.</param>
        /// <exception cref="ArgumentNullException">The callback parameter is
        /// null.</exception>
        /// <exception cref="NotSupportedException">The XMPP entity with
        /// the specified JID does not support the 'In-Band Registration' XMPP
        /// extension.</exception>
        /// <exception cref="XmppErrorException">The server returned an XMPP error code.
        /// Use the Error property of the XmppErrorException to obtain the specific
        /// error condition.</exception>
        /// <exception cref="XmppException">The server returned invalid data or another
        /// unspecified XMPP error occurred.</exception>
        public void Register(RegistrationCallback callback)
        {
            callback.ThrowIfNull("callback");
            Iq iq = im.IqRequest(IqType.Get, null, null,
                Xml.Element("query", "jabber:iq:register"));
            if (iq.Type == IqType.Error)
                throw new NotSupportedException("The XMPP server does not support the " +
                    "'In-Band Registration' extension.");
            var query = iq.Data["query"];
            if (query == null || query.NamespaceURI != "jabber:iq:register")
                throw new XmppException("Erroneous server response: " + iq);
            if (query["registered"] != null)
                throw new XmppException("The XMPP entity is already registered.");
            // If the IQ contains binary data, cache it.
            var data = query["data"];
            if (data != null && data.NamespaceURI == "urn:xmpp:bob")
            {
                BobData bobData = BobData.Parse(data);
                bob.Add(bobData);
            }
            RequestForm form = null;
            bool xdata = query["x"] != null;
            if (xdata)
                form = DataFormFactory.Create(query["x"]) as RequestForm;
            // "Traditional" registration, create a data-form off the provided fields.
            else
                form = CreateDataForm(query);
            // Hand the data-form to the caller to have it filled-out.
            var submit = callback.Invoke(form);
            // Construct the response element.
            var xml = Xml.Element("query", "jabber:iq:register");
            // Convert the data-form back to traditional fields if needed.
            if (xdata)
                xml.Child(submit.ToXmlElement());
            else
            {
                foreach (var field in submit.Fields)
                {
                    xml.Child(Xml.Element(field.Name).Text(
                        field.Values.FirstOrDefault()));
                }
            }
            iq = im.IqRequest(IqType.Set, null, null, xml);
            if (iq.Type == IqType.Error)
                throw Util.ExceptionFromError(iq, "The registration could not be " +
                    "completed.");
            // Reconnect.
        }

        /// <summary>
        /// Changes the current password to the specified new password.
        /// </summary>
        /// <param name="newPassword">The new password to use.</param>
        /// <exception cref="ArgumentNullException">The newPassword parameter
        /// is null.</exception>
        /// <exception cref="XmppErrorException">The server or the XMPP entity
        /// with the specified JID returned an XMPP error code. Use the Error
        /// property of the XmppErrorException to obtain the specific error
        /// condition.</exception>
        /// <exception cref="XmppException">The server returned invalid data or
        /// another unspecified XMPP error occurred.</exception>
        public void ChangePassword(string newPassword)
        {
            newPassword.ThrowIfNull("newPassword");
            Iq iq = im.IqRequest(IqType.Set, null, null,
                Xml.Element("query", "jabber:iq:register")
                    .Child(Xml.Element("username").Text(im.Username))
                    .Child(Xml.Element("password").Text(newPassword)));
            if (iq.Type == IqType.Error)
            {
                throw Util.ExceptionFromError(iq, "The password could not be " +
                    "changed.");
            }
        }

        /// <summary>
        /// Initializes a new instance of the InBandRegistration class.
        /// </summary>
        /// <param name="im">A reference to the XmppIm instance on whose behalf this
        /// instance is created.</param>
        public InBandRegistration(XmppIm im)
            : base(im)
        {
        }

        /// <summary>
        /// Creates a DataForm from the traditional registration fields contained
        /// in the specified XML element.
        /// </summary>
        /// <param name="query">A 'query' XML element containing registration
        /// fields.</param>
        /// <returns>An initialized instance of the RequestForm class representing
        /// the registration fields contained in the specified query XML
        /// element.</returns>
        private RequestForm CreateDataForm(XmlElement query)
        {
            string instructions = query["instructions"] != null ?
                query["instructions"].InnerText : null;
            RequestForm form = new RequestForm(null, instructions);
            foreach (XmlElement child in query.ChildNodes)
            {
                if (child.Name == "instructions")
                    continue;
                form.Fields.Add(new TextField(child.Name, true));
            }
            return form;
        }
    }
}