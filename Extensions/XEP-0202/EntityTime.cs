using XMPPEngineer.Core;
using XMPPEngineer.Im;
using System;
using System.Collections.Generic;

namespace XMPPEngineer.Extensions
{
    /// <summary>
    /// Implements the 'Entity Time' extension as defined in XEP-0202.
    /// </summary>
    internal class EntityTime : XmppExtension, IInputFilter<Iq>
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
                return new string[] { "urn:xmpp:time" };
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
                return Extension.EntityTime;
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
            var e = stanza.Data["time"];
            if (e == null || e.NamespaceURI != "urn:xmpp:time")
                return false;
            TimeSpan span = TimeZoneInfo.Local.BaseUtcOffset;
            // The numeric time zone offset from UTC.
            string tzo = ((span < TimeSpan.Zero) ? "-" : "+") +
                span.ToString(@"hh\:mm");
            // The UTC time according to the responding entity.
            string utc = DateTime.UtcNow.ToUniversalTime()
                .ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'''Z'");
            var time = Xml.Element("time", "urn:xmpp:time")
                .Child(Xml.Element("tzo").Text(tzo))
                .Child(Xml.Element("utc").Text(utc));
            // Send the IQ response.
            im.IqResult(stanza, time);
            return true;
        }

        /// <summary>
        /// Retrieves the time of the XMPP entity with the specified JID.
        /// </summary>
        /// <param name="jid">The JID of the XMPP entity to retrieve the time
        /// for.</param>
        /// <returns>The time of the XMPP entity with the specified JID.</returns>
        /// <exception cref="ArgumentNullException">The jid parameter
        /// is null.</exception>
        /// <exception cref="NotSupportedException">The XMPP entity with
        /// the specified JID does not support the 'Entity Time' XMPP extension.</exception>
        /// <exception cref="XmppErrorException">The server returned an XMPP error code.
        /// Use the Error property of the XmppErrorException to obtain the specific
        /// error condition.</exception>
        /// <exception cref="XmppException">The server returned invalid data or another
        /// unspecified XMPP error occurred.</exception>
        public DateTime GetTime(Jid jid)
        {
            jid.ThrowIfNull("jid");
            if (!ecapa.Supports(jid, Extension.EntityTime))
            {
                throw new NotSupportedException("The XMPP entity does not support " +
                    "the 'Entity Time' extension.");
            }
            Iq iq = im.IqRequest(IqType.Get, jid, im.Jid,
                Xml.Element("time", "urn:xmpp:time"));
            if (iq.Type == IqType.Error)
                throw Util.ExceptionFromError(iq, "The time could not be retrieved.");
            var time = iq.Data["time"];
            if (time == null || time["tzo"] == null || time["utc"] == null)
                throw new XmppException("Erroneous IQ response.");
            string tzo = time["tzo"].InnerText;
            string utc = time["utc"].InnerText;
            // Try to parse utc into datetime, tzo into timespan.
            try
            {
                DateTime dt = DateTime.Parse(utc).ToUniversalTime();
                TimeSpan sp = TimeSpan.Parse(tzo.TrimStart('+'));
                return dt.Add(sp);
            }
            catch (FormatException e)
            {
                throw new XmppException("Invalid tzo or utc value.", e);
            }
        }

        /// <summary>
        /// Initializes a new instance of the EntityTime class.
        /// </summary>
        /// <param name="im">A reference to the XmppIm instance on whose behalf this
        /// instance is created.</param>
        public EntityTime(XmppIm im)
            : base(im)
        {
        }
    }
}