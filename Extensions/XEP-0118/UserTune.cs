using XMPPEngineer.Im;
using System;
using System.Collections.Generic;
using System.Xml;

namespace XMPPEngineer.Extensions
{
    /// <summary>
    /// Implements the 'User Tune' extension as defined in XEP-0118.
    /// </summary>
    internal class UserTune : XmppExtension
    {
        /// <summary>
        /// A reference to the 'Personal Eventing Protocol' extension instance.
        /// </summary>
        private Pep pep;

        /// <summary>
        /// An enumerable collection of XMPP namespaces the extension implements.
        /// </summary>
        /// <remarks>This is used for compiling the list of supported extensions
        /// advertised by the 'Service Discovery' extension.</remarks>
        public override IEnumerable<string> Namespaces
        {
            get
            {
                return new string[] {
					"http://jabber.org/protocol/tune",
					"http://jabber.org/protocol/tune+notify"
				};
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
                return Extension.UserTune;
            }
        }

        /// <summary>
        /// Determines whether our server supports personal eventing and thusly
        /// the user tune extension.
        /// </summary>
        public bool Supported
        {
            get
            {
                return pep.Supported;
            }
        }

        /// <summary>
        /// The event that is raised when another XMPP entity has published tune
        /// information.
        /// </summary>
        public event EventHandler<TuneEventArgs> Tune;

        /// <summary>
        /// Invoked after all extensions have been loaded.
        /// </summary>
        public override void Initialize()
        {
            pep = im.GetExtension<Pep>();
            pep.Subscribe("http://jabber.org/protocol/tune", onTune);
        }

        /// <summary>
        /// Publishes the specified music information to contacts on the user's
        /// roster.
        /// </summary>
        /// <param name="title">The title of the song or piece.</param>
        /// <param name="artist">The artist or performer of the song or piece.</param>
        /// <param name="track">A unique identifier for the tune; e.g., the track number
        /// within a collection or the specific URI for the object (e.g., a
        /// stream or audio file).</param>
        /// <param name="length">The duration of the song or piece in seconds.</param>
        /// <param name="rating">The user's rating of the song or piece, from 1
        /// (lowest) to 10 (highest).</param>
        /// <param name="source">The collection (e.g., album) or other source
        /// (e.g., a band website that hosts streams or audio files).</param>
        /// <param name="uri">A URI or URL pointing to information about the song,
        /// collection, or artist</param>
        /// <exception cref="NotSupportedException">The server does not support the
        /// 'Personal Eventing Protocol' extension.</exception>
        /// <exception cref="XmppErrorException">The server returned an XMPP error code.
        /// Use the Error property of the XmppErrorException to obtain the specific
        /// error condition.</exception>
        /// <exception cref="XmppException">The server returned invalid data or another
        /// unspecified XMPP error occurred.</exception>
        /// <remarks>Publishing no information (i.e. calling Publish without any parameters
        /// is considered a "stop command" to disable publishing).</remarks>
        public void Publish(string title = null, string artist = null, string track = null,
            int length = 0, int rating = 0, string source = null, string uri = null)
        {
            length.ThrowIfOutOfRange(0, Int16.MaxValue);
            rating.ThrowIfOutOfRange(0, 10);
            var tune = Xml.Element("tune", "http://jabber.org/protocol/tune");
            if (!String.IsNullOrEmpty(title))
                tune.Child(Xml.Element("title").Text(title));
            if (!String.IsNullOrEmpty(artist))
                tune.Child(Xml.Element("artist").Text(artist));
            if (!String.IsNullOrEmpty(track))
                tune.Child(Xml.Element("track").Text(track));
            if (length > 0)
                tune.Child(Xml.Element("length").Text(length.ToString()));
            if (rating > 0)
                tune.Child(Xml.Element("rating").Text(rating.ToString()));
            if (!String.IsNullOrEmpty(source))
                tune.Child(Xml.Element("source").Text(source));
            if (!String.IsNullOrEmpty(uri))
                tune.Child(Xml.Element("uri").Text(uri));
            pep.Publish("http://jabber.org/protocol/tune", null, tune);
        }

        /// <summary>
        /// Publishes the specified music information to contacts on the user's
        /// roster.
        /// </summary>
        /// <param name="tune">The tune information to publish.</param>
        /// <exception cref="ArgumentNullException">The tune parameter is
        /// null.</exception>
        /// <exception cref="NotSupportedException">The server does not support the
        /// 'Personal Eventing Protocol' extension.</exception>
        /// <exception cref="XmppErrorException">The server returned an XMPP error code.
        /// Use the Error property of the XmppErrorException to obtain the specific
        /// error condition.</exception>
        /// <exception cref="XmppException">The server returned invalid data or another
        /// unspecified XMPP error occurred.</exception>
        /// <remarks>Publishing no information (i.e. calling Publish without any parameters
        /// is considered a "stop command" to disable publishing).</remarks>
        public void Publish(TuneInformation tune)
        {
            tune.ThrowIfNull("tune");
            Publish(tune.Title, tune.Artist, tune.Track, tune.Length,
                tune.Rating, tune.Source, tune.Uri);
        }

        /// <summary>
        /// Initializes a new instance of the UserTune class.
        /// </summary>
        /// <param name="im">A reference to the XmppIm instance on whose behalf this
        /// instance is created.</param>
        public UserTune(XmppIm im)
            : base(im)
        {
        }

        /// <summary>
        /// Invoked when a contact has published music information.
        /// </summary>
        /// <param name="jid">The JID of the XMPP entity that published the tune
        /// information.</param>
        /// <param name="item">The 'item' Xml element of the pubsub publish
        /// event.</param>
        private void onTune(Jid jid, XmlElement item)
        {
            if (item == null || item["tune"] == null)
                return;
            var tune = item["tune"];
            if (tune.IsEmpty)
            {
                // Raise the 'Tune' event without information.
                Tune.Raise(this, new TuneEventArgs(jid));
                return;
            }
            // Parse 'tune' element.
            int length = 0;
            if (tune["length"] != null)
                length = int.Parse(tune["length"].InnerText);
            int rating = 0;
            if (tune["rating"] != null)
                rating = int.Parse(tune["rating"].InnerText);
            TuneInformation info = new TuneInformation(
                GetField(tune, "title"), GetField(tune, "artist"), GetField(tune, "track"),
                length, rating, GetField(tune, "source"), GetField(tune, "uri"));
            // Raise the 'Tune' event.
            Tune.Raise(this, new TuneEventArgs(jid, info));
        }

        /// <summary>
        /// Returns the inner text of the element with the specified name.
        /// </summary>
        /// <param name="tune">The 'tune' element of the event.</param>
        /// <param name="name">The name of the element whose inner text to
        /// retrieve.</param>
        /// <returns>The inner text of the specified element or null.</returns>
        private string GetField(XmlElement tune, string name)
        {
            return tune[name] != null ? tune[name].InnerText : null;
        }
    }
}