using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace XMPPEngineer
{
    /// <summary>
    /// Represents an XMPP error.
    /// </summary>
    public class XmppError
    {
        /// <summary>
        /// The type of the error.
        /// </summary>
        private ErrorType type;

        /// <summary>
        /// The error condition of the error stanza.
        /// </summary>
        private ErrorCondition condition;

        /// <summary>
        /// The type of the error.
        /// </summary>
        public ErrorType Type
        {
            get
            {
                return type;
            }

            set
            {
                type = value;
                Data.SetAttribute("type", value.ToString().ToLowerInvariant());
            }
        }

        /// <summary>
        /// The XMPP error condition value of this error.
        /// </summary>
        public ErrorCondition Condition
        {
            get
            {
                return condition;
            }

            set
            {
                SetCondition(value);
            }
        }

        /// <summary>
        /// A textual description of the error in more detail. This may be null.
        /// </summary>
        public string Text
        {
            set
            {
                var text = Data["text"];
                if (text != null)
                {
                    if (value == null)
                        Data.RemoveChild(text);
                    else
                        text.InnerText = value;
                }
                else
                {
                    if (value != null)
                    {
                        Data.Child(Xml.Element("text",
                            "urn:ietf:params:xml:ns:xmpp-stanzas").Text(value));
                    }
                }
            }

            get
            {
                var text = Data["text"];
                if (text != null)
                    return String.IsNullOrEmpty(text.InnerText) ? null : text.InnerText;
                return null;
            }
        }

        /// <summary>
        /// Provides access to the XML of the underlying error stanza.
        /// </summary>
        public XmlElement Data
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns a textual representation of this Error instance.
        /// </summary>
        /// <returns>A textual representation of this Error instance.</returns>
        public override string ToString()
        {
            return Data.ToXmlString();
        }

        /// <summary>
        /// Initializes a new instance of the Error class.
        /// </summary>
        /// <param name="type">The type of the error.</param>
        /// <param name="condition">The condition of the error.</param>
        /// <param name="data">The content of the error element.</param>
        public XmppError(ErrorType type, ErrorCondition condition, params XmlElement[] data) :
            this(type, condition, null, data)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Error class.
        /// </summary>
        /// <param name="type">The type of the error.</param>
        /// <param name="condition">The condition of the error.</param>
        /// <param name="text">A more-detailed textual description of the error.</param>
        /// <param name="data">The content of the error element.</param>
        internal XmppError(ErrorType type, ErrorCondition condition, string text = null,
            params XmlElement[] data)
        {
            Data = Xml.Element("error");
            Type = type;
            Condition = condition;
            Text = text;
            if (data != null)
            {
                foreach (var e in data)
                {
                    if (e != null)
                        Data.Child(e);
                }
            }
        }

        /// <summary>
        /// Initializes a new Error instance from the specified XML element.
        /// </summary>
        /// <param name="error">The 'error' XML element to initialize this
        /// instance with.</param>
        /// <exception cref="ArgumentNullException">The error parameter is null.</exception>
        /// <exception cref="ArgumentException">The error parameter contains
        /// invalid XML data.</exception>
        internal XmppError(XmlElement error)
        {
            error.ThrowIfNull("error");
            // Verify mandatory error type attribute.
            ErrorType type = (ErrorType)Enum.Parse(typeof(ErrorType),
                error.GetAttribute("type"), true);
            // Look for mandatory error condition element.
            ErrorCondition? condition = null;
            foreach (var v in Enum.GetValues(typeof(ErrorCondition)))
            {
                string s = ErrorConditionToTagName((ErrorCondition)v);
                if (error[s] != null)
                    condition = (ErrorCondition)v;
            }
            if (!condition.HasValue)
                throw new ArgumentException("The error XML element does not contain a " +
                    "valid XMPP error condition element.");
            Data = error;
            Type = type;
            Condition = condition.Value;
        }

        /// <summary>
        /// Sets the error condition of the XMPP error to the specified value.
        /// </summary>
        /// <param name="condition">A value from the ErrorCondition enumeration.</param>
        private void SetCondition(ErrorCondition condition)
        {
            ISet<XmlElement> set = new HashSet<XmlElement>();
            // Remove old condition, if any.
            foreach (var v in Enum.GetValues(typeof(ErrorCondition)))
            {
                string s = ErrorConditionToTagName((ErrorCondition)v);
                if (Data[s] != null)
                    set.Add(Data[s]);
            }
            foreach (var e in set)
                Data.RemoveChild(e);
            // Add new condition element.
            string tag = ErrorConditionToTagName(condition);
            Data.Child(Xml.Element(tag,
                "urn:ietf:params:xml:ns:xmpp-stanzas"));
            this.condition = condition;
        }

        /// <summary>
        /// Returns the XMPP element name of the specified error condition.
        /// </summary>
        /// <param name="condition">A value from the ErrorCondition enumeration
        /// to convert into an element name.</param>
        /// <returns>The XML element name of the specified error condition.</returns>
        private string ErrorConditionToTagName(ErrorCondition condition)
        {
            StringBuilder b = new StringBuilder();
            string s = condition.ToString();
            for (int i = 0; i < s.Length; i++)
            {
                if (char.IsUpper(s, i) && i > 0)
                    b.Append('-');
                b.Append(char.ToLower(s[i]));
            }
            return b.ToString();
        }

        /// <summary>
        /// Returns the corresponding value from the ErrorCondition enumeration for
        /// the specified XML element name.
        /// </summary>
        /// <param name="tagName">An XML element name of a defined XMPP error
        /// condition.</param>
        /// <returns>The value from the ErrorCondition enumeration corresponding to
        /// the specified element name.</returns>
        /// <exception cref="ArgumentNullException">The tagName parameter is null.</exception>
        /// <exception cref="ArgumentException">The specified element name
        /// is not a valid XMPP error condition.</exception>
        private ErrorCondition TagNameToErrorCondition(string tagName)
        {
            tagName.ThrowIfNull("tagName");
            var values = Enum.GetValues(typeof(ErrorCondition));
            foreach (var v in values)
            {
                if (ErrorConditionToTagName((ErrorCondition)v) == tagName)
                    return (ErrorCondition)v;
            }
            throw new ArgumentException("The specified tag name is not a valid " +
                "XMPP error condition.");
        }
    }
}