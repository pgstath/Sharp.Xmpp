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
    /// Implements the 'Stream Initiation' extension as defined in XEP-0095.
    /// </summary>
    internal class StreamInitiation : XmppExtension, IInputFilter<Iq>
    {
        /// <summary>
        /// A reference to the 'Entity Capabilities' extension instance.
        /// </summary>
        private EntityCapabilities ecapa;

        /// <summary>
        /// A dictionary of registered profiles.
        /// </summary>
        private IDictionary<string, Func<Jid, XmlElement, XmlElement>> profiles =
            new Dictionary<string, Func<Jid, XmlElement, XmlElement>>();

        /// <summary>
        /// An enumerable collection of XMPP namespaces the extension implements.
        /// </summary>
        /// <remarks>This is used for compiling the list of supported extensions
        /// advertised by the 'Service Discovery' extension.</remarks>
        public override IEnumerable<string> Namespaces
        {
            get
            {
                return new string[] { "http://jabber.org/protocol/si" };
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
                return Extension.StreamInitiation;
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
            var si = stanza.Data["si"];
            if (si == null || si.NamespaceURI != "http://jabber.org/protocol/si")
                return false;
            var profile = si.GetAttribute("profile");
            // If it's an unknown profile, send back an error response.
            if (profiles.ContainsKey(profile) == false)
            {
                im.IqError(stanza, ErrorType.Cancel, ErrorCondition.BadRequest,
                    "Unknown SI profile", Xml.Element("bad-profile",
                    "http://jabber.org/protocol/si"));
            }
            else
            {
                try
                {
                    // Invoke the profile's callback.
                    var response = profiles[profile].Invoke(stanza.From, stanza.Data["si"]);
                    // If response is an error element, send back an error response.
                    im.IqResponse(response.Name == "error" ? IqType.Error : IqType.Result,
                        stanza.Id, stanza.From, im.Jid, response);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Exception was raised during Stream Initiation " + ex.ToString());
                    // Send back an error response in case the callback method threw
                    // an exception.
                    im.IqError(stanza, ErrorType.Cancel, ErrorCondition.ServiceUnavailable);
                }
            }
            // We took care of this IQ request, so intercept it and don't pass it
            // on to other handlers.
            return true;
        }

        /// <summary>
        /// Attempts to initiate a data stream with the XMPP entity with the specified
        /// JID.
        /// </summary>
        /// <param name="to">The JID of the XMPP entity to initiate a data-stream
        /// with.</param>
        /// <param name="mimeType">The MIME type of the data to be transferred across
        /// the stream.</param>
        /// <param name="profile">The 'Stream Initiation' profile to use.</param>
        /// <param name="streamOptions">An enumerable collection of supported
        /// stream methods which are advertised to the receiving XMPP
        /// entity.</param>
        /// <param name="data">An XML element containing any additional data the
        /// specified 'Stream Initiation' profile may require.</param>
        /// <returns>An initialized instance of the InitiationResult class containing
        /// the negotiated stream-method and session identifier.</returns>
        /// <exception cref="ArgumentNullException">The to parameter or the mimeType
        /// parameter or the profile parameter or the streamOptions parameter
        /// is null.</exception>
        /// <exception cref="ArgumentException">The streamOptions enumerable contains
        /// no elements, or the stream-initiation response received contained no
        /// selection for the stream-method.</exception>
        /// <exception cref="NotSupportedException">The XMPP entity with
        /// the specified JID does not support the 'Stream Initiation' XMPP
        /// extension.</exception>
        /// <exception cref="XmppErrorException">The server or the receiving XMPP
        /// entity returned an XMPP error code. Use the Error property of the
        /// XmppErrorException to obtain the specific error condition.</exception>
        /// <exception cref="XmppException">The server returned invalid data or another
        /// unspecified XMPP error occurred.</exception>
        public InitiationResult InitiateStream(Jid to, string mimeType, string profile,
            IEnumerable<string> streamOptions, XmlElement data = null)
        {
            to.ThrowIfNull("to");
            mimeType.ThrowIfNull("mimeType");
            profile.ThrowIfNull("profile");
            streamOptions.ThrowIfNull("streamOptions");
            if (streamOptions.Count() == 0)
            {
                throw new ArgumentException("The streamOptions enumerable must " +
                    "include one or more stream-options.");
            }
            if (!ecapa.Supports(to, Extension.StreamInitiation))
            {
                throw new NotSupportedException("The XMPP entity does not support " +
                    "the 'Stream Initiation' extension.");
            }
            string sid = GenerateSessionId();
            var si = CreateSiElement(sid, mimeType, profile, streamOptions, data);
            // Perform the actual request.
            Iq iq = im.IqRequest(IqType.Set, to, im.Jid, si);
            if (iq.Type == IqType.Error)
                throw Util.ExceptionFromError(iq, "Stream initiation failed.");
            // Result must contain a 'feature' element.
            var feat = iq.Data["si"]["feature"];
            string selected = ParseStreamMethod(feat);
            // Construct and return the initiation result.
            return new InitiationResult(sid, selected, iq.Data["si"]);
        }

        /// <summary>
        /// Attempts to initiate a data stream with the XMPP entity with the specified
        /// JID.
        /// </summary>
        /// <param name="to">The JID of the XMPP entity to initiate a data-stream
        /// with.</param>
        /// <param name="mimeType">The MIME type of the data to be transferred across
        /// the stream.</param>
        /// <param name="profile">The 'Stream Initiation' profile to use.</param>
        /// <param name="streamOptions">An enumerable collection of supported
        /// stream methods which are advertised to the receiving XMPP
        /// entity.</param>
        /// <param name="data">An XML element containing any additional data the
        /// specified 'Stream Initiation' profile may require.</param>
        /// <param name="cb">A callback method to invoke once the result of the
        /// stream-initation operation has been received.</param>
        /// <returns>An initialized instance of the InitiationResult class containing
        /// the negotiated stream-method and session identifier Sid.</returns>
        /// <exception cref="ArgumentNullException">The to parameter or the mimeType
        /// parameter or the profile parameter or the streamOptions parameter
        /// is null.</exception>
        /// <exception cref="ArgumentException">The streamOptions enumerable contains
        /// no elements, or the stream-initiation response received contained no
        /// selection for the stream-method.</exception>
        /// <exception cref="NotSupportedException">The XMPP entity with
        /// the specified JID does not support the 'Stream Initiation' XMPP
        /// extension.</exception>
        public string InitiateStreamAsync(Jid to, string mimeType, string profile,
            IEnumerable<string> streamOptions, XmlElement data = null,
            Action<InitiationResult, Iq> cb = null)
        {
            to.ThrowIfNull("to");
            mimeType.ThrowIfNull("mimeType");
            profile.ThrowIfNull("profile");
            streamOptions.ThrowIfNull("streamOptions");
            if (streamOptions.Count() == 0)
            {
                throw new ArgumentException("The streamOptions enumerable must " +
                    "include one or more stream-options.");
            }
            //FIX ME
            //if (!ecapa.Supports(to, Extension.StreamInitiation)) {
            //    throw new NotSupportedException("The XMPP entity does not support " +
            //        "the 'Stream Initiation' extension.");
            //}
            //FIX ME
            string sid = GenerateSessionId();
            var si = CreateSiElement(sid, mimeType, profile, streamOptions, data);
            // Perform the actual request.
            im.IqRequestAsync(IqType.Set, to, im.Jid, si, null, (id, iq) =>
            {
                if (cb == null)
                    return;
                InitiationResult result = null;
                if (iq.Type != IqType.Error)
                {
                    // Result must contain a 'feature' element.
                    var feat = iq.Data["si"]["feature"];
                    string selected = ParseStreamMethod(feat);
                    // Construct the initiation result and call the provided callback.
                    result = new InitiationResult(sid, selected, iq.Data["si"]);
                }
                cb(result, iq);
            });
            return sid;
        }

        /// <summary>
        /// Registers the profile with the specified name.
        /// </summary>
        /// <param name="name">The name of the SI profile to register.</param>
        /// <param name="cb">A callback method invoked whenever an initiation
        /// request for the specified profile is received.</param>
        /// <exception cref="ArgumentNullException">The name parameter or the
        /// cb parameter is null.</exception>
        /// <exception cref="ArgumentException">A profile with the specified name
        /// has already been registered.</exception>
        public void RegisterProfile(string name, Func<Jid, XmlElement, XmlElement> cb)
        {
            name.ThrowIfNull("name");
            cb.ThrowIfNull("cb");
            profiles.Add(name, cb);
        }

        /// <summary>
        /// Initializes a new instance of the StreamInitiation class.
        /// </summary>
        /// <param name="im">A reference to the XmppIm instance on whose behalf this
        /// instance is created.</param>
        public StreamInitiation(XmppIm im)
            : base(im)
        {
        }

        /// <summary>
        /// Creates the 'feature' XML element which is part of a 'Stream Initiation'
        /// request.
        /// </summary>
        /// <param name="streamOptions">An enumerable collection of accepted stream
        /// methods.</param>
        /// <returns>An XML 'feature' element.</returns>
        /// <exception cref="ArgumentNullException">The streamOptions parameter
        /// is null.</exception>
        private XmlElement CreateFeatureElement(IEnumerable<string> streamOptions)
        {
            streamOptions.ThrowIfNull("streamOptions");
            // Create the data-form for stream-method selection.
            DataForm form = new RequestForm();
            HashSet<Option> options = new HashSet<Option>();
            foreach (string opt in streamOptions)
                options.Add(new Option(opt));
            form.Fields.Add(new ListField("stream-method", true, null, null,
                options));
            // Wrap it in a 'feature' element to create the offer.
            return FeatureNegotiation.Create(form);
        }

        /// <summary>
        /// Creates the 'si' element which contains the stream-initiation options.
        /// </summary>
        /// <param name="sid">The session id for the stream-initiation.</param>
        /// <param name="mimeType">The MIME type of the data to be transferred across
        /// the stream.</param>
        /// <param name="profile">The 'Stream Initiation' profile to use.</param>
        /// <param name="streamOptions">An enumerable collection of supported
        /// stream methods which are advertised to the receiving XMPP
        /// entity.</param>
        /// <param name="data">An XML element containing any additional data the
        /// specified 'Stream Initiation' profile may require.</param>
        /// <returns>The 'si' element to include in the IQ request.</returns>
        private XmlElement CreateSiElement(string sid, string mimeType, string profile,
            IEnumerable<string> streamOptions, XmlElement data = null)
        {
            var feat = CreateFeatureElement(streamOptions);
            // Construct the 'si' element.
            var si = Xml.Element("si", "http://jabber.org/protocol/si")
                .Attr("id", sid)
                .Attr("mime-type", mimeType)
                .Attr("profile", profile)
                .Child(feat);
            if (data != null)
                si.Child(data);
            return si;
        }

        /// <summary>
        /// Parses the selected stream-method from the specified 'feature' XML
        /// element.
        /// </summary>
        /// <param name="feature">The 'feature' XML element.</param>
        /// <returns>The stream method contained in the 'feature' XML
        /// element.</returns>
        /// <exception cref="ArgumentNullException">The feature parameter is
        /// null.</exception>
        /// <exception cref="ArgumentException">The feature element contains
        /// invalid data.</exception>
        private string ParseStreamMethod(XmlElement feature)
        {
            feature.ThrowIfNull("feature");
            DataForm form = FeatureNegotiation.Parse(feature);
            // The data-form must contain a field named 'stream-method'.
            var field = form.Fields["stream-method"];
            if (field == null)
                throw new ArgumentException("Missing or erroneous 'stream-method' field.");
            string selected = field.Values.FirstOrDefault();
            if (selected == null)
                throw new ArgumentException("No stream-method selected.");
            return selected;
        }

        /// <summary>
        /// Generates a random session id which is an opaque identifier which
        /// must be used throughout a session when talking to the Receiver.
        /// </summary>
        /// <returns>A random session id string.</returns>
        private static string GenerateSessionId()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 16);
        }
    }
}