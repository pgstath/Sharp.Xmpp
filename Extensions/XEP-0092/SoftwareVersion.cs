using XMPPEngineer.Core;
using XMPPEngineer.Im;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace XMPPEngineer.Extensions
{
    /// <summary>
    /// Implements the 'Software Version' extension as defined in XEP-0092.
    /// </summary>
    internal class SoftwareVersion : XmppExtension, IInputFilter<Iq>
    {
        /// <summary>
        /// A reference to the 'Entity Capabilities' extension instance.
        /// </summary>
        private EntityCapabilities ecapa;

        /// <summary>
        /// The current software version.
        /// </summary>
        public VersionInformation Version
        {
            get;
            private set;
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
                return new string[] { "jabber:iq:version" };
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
                return Extension.SoftwareVersion;
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
            var query = stanza.Data["query"];
            if (query == null || query.NamespaceURI != "jabber:iq:version")
                return false;
            // Construct and send a response stanza.
            var xml = Xml.Element("query", "jabber:iq:version").Child(
                Xml.Element("name").Text(Version.Name)).Child(
                Xml.Element("version").Text(Version.Version)).Child(
                Xml.Element("os").Text(Version.Os));
            im.IqResult(stanza, xml);
            // We took care of this IQ request, so intercept it and don't pass it
            // on to other handlers.
            return true;
        }

        /// <summary>
        /// Queries the XMPP entity with the specified JID for its software
        /// version.
        /// </summary>
        /// <param name="jid">The JID of the XMPP entity to query.</param>
        /// <returns>An instance of the VersionInformation class containing the
        /// entity's software version.</returns>
        /// <exception cref="ArgumentNullException">The jid parameter is
        /// null.</exception>
        /// <exception cref="NotSupportedException">The XMPP entity with
        /// the specified JID does not support the 'Software Version' XMPP
        /// extension.</exception>
        /// <exception cref="XmppErrorException">The server returned an XMPP error code.
        /// Use the Error property of the XmppErrorException to obtain the specific
        /// error condition.</exception>
        /// <exception cref="XmppException">The server returned invalid data or another
        /// unspecified XMPP error occurred.</exception>
        public VersionInformation GetVersion(Jid jid)
        {
            jid.ThrowIfNull("jid");
            if (!ecapa.Supports(jid, Extension.SoftwareVersion))
            {
                throw new NotSupportedException("The XMPP entity does not support the " +
                    "'Software Version' extension.");
            }
            Iq response = im.IqRequest(IqType.Get, jid, im.Jid,
                Xml.Element("query", "jabber:iq:version"));
            if (response.Type == IqType.Error)
                throw Util.ExceptionFromError(response, "The version could not be retrieved.");
            // Parse the response.
            var query = response.Data["query"];
            if (query == null || query.NamespaceURI != "jabber:iq:version")
                throw new XmppException("Erroneous server response: " + response);
            if (query["name"] == null || query["version"] == null)
                throw new XmppException("Missing name or version element: " + response);
            string os = query["os"] != null ? query["os"].InnerText : null;
            return new VersionInformation(query["name"].InnerText,
                query["version"].InnerText, os);
        }

        /// <summary>
        /// Initializes a new instance of the SoftwareVersion class.
        /// </summary>
        /// <param name="im">A reference to the XmppIm instance on whose behalf this
        /// instance is created.</param>
        public SoftwareVersion(XmppIm im)
            : base(im)
        {
            // Collect name and version attributes from the assembly's metadata.
            Attribute attr = Assembly.GetExecutingAssembly().
                GetCustomAttribute(typeof(AssemblyProductAttribute));
            string name = attr != null ? ((AssemblyProductAttribute)attr).Product :
                "XMPPEngineer";
            string version = Assembly.GetExecutingAssembly().GetName().
                Version.ToString();
            Version = new VersionInformation(name, version,
                Environment.OSVersion.ToString());
        }
    }
}