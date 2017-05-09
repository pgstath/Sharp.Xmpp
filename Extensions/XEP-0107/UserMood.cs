using XMPPEngineer.Im;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace XMPPEngineer.Extensions
{
    /// <summary>
    /// Implements the 'User Mood' extension as defined in XEP-0107.
    /// </summary>
    internal class UserMood : XmppExtension
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
					"http://jabber.org/protocol/mood",
					"http://jabber.org/protocol/mood+notify"
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
                return Extension.UserMood;
            }
        }

        /// <summary>
        /// Determines whether our server supports personal eventing and thusly
        /// the user mood extension.
        /// </summary>
        public bool Supported
        {
            get
            {
                return pep.Supported;
            }
        }

        /// <summary>
        /// The event that is raised when another XMPP entity has published mood
        /// information.
        /// </summary>
        public event EventHandler<MoodChangedEventArgs> MoodChanged;

        /// <summary>
        /// Invoked after all extensions have been loaded.
        /// </summary>
        public override void Initialize()
        {
            pep = im.GetExtension<Pep>();
            pep.Subscribe("http://jabber.org/protocol/mood", onMood);
        }

        /// <summary>
        /// Sets the user's mood to the specified mood value.
        /// </summary>
        /// <param name="mood">A value from the Mood enumeration to set the user's
        /// mood to.</param>
        /// <param name="description">A natural-language description of, or reason
        /// for, the mood.</param>
        public void SetMood(Mood mood, string description = null)
        {
            var xml = Xml.Element("mood", "http://jabber.org/protocol/mood")
                .Child(Xml.Element(MoodToTagName(mood)));
            if (description != null)
                xml.Child(Xml.Element("text").Text(description));
            pep.Publish("http://jabber.org/protocol/mood", null, xml);
        }

        /// <summary>
        /// Initializes a new instance of the UserMood class.
        /// </summary>
        /// <param name="im">A reference to the XmppIm instance on whose behalf this
        /// instance is created.</param>
        public UserMood(XmppIm im)
            : base(im)
        {
        }

        /// <summary>
        /// Invoked when a contact has published his or her mood.
        /// </summary>
        /// <param name="jid">The JID of the XMPP entity that published the
        /// mood information.</param>
        /// <param name="item">The 'item' Xml element of the pubsub publish
        /// event.</param>
        private void onMood(Jid jid, XmlElement item)
        {
            if (item == null || item["mood"] == null)
                return;
            var moodElement = item["mood"];
            Mood? mood = null;
            if (moodElement.IsEmpty)
            {
                mood = Mood.Undefined;
            }
            else
            {
                // Look for a mood value element.
                foreach (var v in Enum.GetValues(typeof(Mood)))
                {
                    string s = MoodToTagName((Mood)v);
                    if (moodElement[s] != null)
                        mood = (Mood)v;
                }
            }
            string text = moodElement["text"] != null ?
                moodElement["text"].InnerText : null;
            // Raise the 'MoodChanged' event.
            if (mood.HasValue)
                MoodChanged.Raise(this, new MoodChangedEventArgs(jid, mood.Value, text));
        }

        /// <summary>
        /// Returns the XMPP element name of the specified mood value.
        /// </summary>
        /// <param name="mood">A value from the Mood enumeration
        /// to convert into an element name.</param>
        /// <returns>The XML element name of the specified mood value.</returns>
        private string MoodToTagName(Mood mood)
        {
            StringBuilder b = new StringBuilder();
            string s = mood.ToString();
            for (int i = 0; i < s.Length; i++)
            {
                if (char.IsUpper(s, i) && i > 0)
                    b.Append('_');
                b.Append(char.ToLower(s[i]));
            }
            return b.ToString();
        }
    }
}