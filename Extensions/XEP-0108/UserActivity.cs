using XMPPEngineer.Im;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace XMPPEngineer.Extensions
{
    /// <summary>
    /// Implements the 'User Activity' extension as defined in XEP-0108.
    /// </summary>
    internal class UserActivity : XmppExtension
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
					"http://jabber.org/protocol/activity",
					"http://jabber.org/protocol/activity+notify"
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
                return Extension.UserActivity;
            }
        }

        /// <summary>
        /// Determines whether our server supports personal eventing and thusly
        /// the user activity extension.
        /// </summary>
        public bool Supported
        {
            get
            {
                return pep.Supported;
            }
        }

        /// <summary>
        /// The event that is raised when another XMPP entity has published activity
        /// information.
        /// </summary>
        public event EventHandler<ActivityChangedEventArgs> ActivityChanged;

        /// <summary>
        /// Invoked after all extensions have been loaded.
        /// </summary>
        public override void Initialize()
        {
            pep = im.GetExtension<Pep>();
            pep.Subscribe("http://jabber.org/protocol/activity", onActivity);
        }

        /// <summary>
        /// Sets the user's activity to the specified activity value(s).
        /// </summary>
        /// <param name="activity">A value from the GeneralActivity enumeration to
        /// set the user's general activity to.</param>
        /// <param name="specific">A value from the SpecificActivity enumeration
        /// best describing the user's activity in more detail.</param>
        /// <param name="description">A natural-language description of, or reason
        /// for, the activity.</param>
        public void SetActivity(GeneralActivity activity, SpecificActivity specific =
            SpecificActivity.Other, string description = null)
        {
            var xml = Xml.Element("activity", "http://jabber.org/protocol/activity");
            var e = Xml.Element(GeneralActivityToTagName(activity));
            if (specific != SpecificActivity.Other)
                e.Child(Xml.Element(SpecificActivityToTagName(specific)));
            xml.Child(e);
            if (description != null)
                xml.Child(Xml.Element("text").Text(description));
            pep.Publish("http://jabber.org/protocol/activity", null, xml);
        }

        /// <summary>
        /// Initializes a new instance of the UserActivity class.
        /// </summary>
        /// <param name="im">A reference to the XmppIm instance on whose behalf this
        /// instance is created.</param>
        public UserActivity(XmppIm im)
            : base(im)
        {
        }

        /// <summary>
        /// Invoked when a contact has published his or her activity.
        /// </summary>
        /// <param name="jid">The JID of the XMPP entity that published the
        /// activity information.</param>
        /// <param name="item">The 'item' Xml element of the pubsub publish
        /// event.</param>
        private void onActivity(Jid jid, XmlElement item)
        {
            if (item == null || item["activity"] == null)
                return;
            var activityElement = item["activity"];
            XmlElement temp = null;
            GeneralActivity? activity = null;
            if (activityElement.IsEmpty)
            {
                activity = GeneralActivity.Undefined;
            }
            else
            {
                // Look for a GeneralActivity value element.
                foreach (var v in Enum.GetValues(typeof(GeneralActivity)))
                {
                    string s = GeneralActivityToTagName((GeneralActivity)v);
                    if (activityElement[s] != null)
                    {
                        activity = (GeneralActivity)v;
                        temp = activityElement[s];
                    }
                }
            }
            SpecificActivity specific = SpecificActivity.Other;
            if (temp != null)
            {
                // Look for a SpecificActivity value element.
                foreach (var v in Enum.GetValues(typeof(SpecificActivity)))
                {
                    string s = SpecificActivityToTagName((SpecificActivity)v);
                    if (temp[s] != null)
                        specific = (SpecificActivity)v;
                }
            }
            string text = activityElement["text"] != null ?
                activityElement["text"].InnerText : null;
            // Raise the 'ActivityChanged' event.
            if (activity.HasValue)
            {
                ActivityChanged.Raise(this, new ActivityChangedEventArgs(jid,
                    activity.Value, specific, text));
            }
        }

        /// <summary>
        /// Returns the XMPP element name of the specified general activity value.
        /// </summary>
        /// <param name="activity">A value from the GeneralActivity enumeration
        /// to convert into an element name.</param>
        /// <returns>The XML element name of the specified activity value.</returns>
        private string GeneralActivityToTagName(GeneralActivity activity)
        {
            StringBuilder b = new StringBuilder();
            string s = activity.ToString();
            for (int i = 0; i < s.Length; i++)
            {
                if (char.IsUpper(s, i) && i > 0)
                    b.Append('_');
                b.Append(char.ToLower(s[i]));
            }
            return b.ToString();
        }

        /// <summary>
        /// Returns the XMPP element name of the specified specific activity value.
        /// </summary>
        /// <param name="activity">A value from the SpecificActivity enumeration
        /// to convert into an element name.</param>
        /// <returns>The XML element name of the specified activity value.</returns>
        private string SpecificActivityToTagName(SpecificActivity activity)
        {
            StringBuilder b = new StringBuilder();
            string s = activity.ToString();
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