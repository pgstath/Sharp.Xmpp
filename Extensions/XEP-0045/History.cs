using System;
using System.Xml;
using XMPPEngineer.Core;

namespace XMPPEngineer.Extensions
{
    /// <summary>
    /// Implements the message history request object as described in XEP-0045.
    /// </summary>
    public class History : Presence
    {
        private const string rootTag = "presence",
            xTag = "x",
            historyTag = "history",
            maxCharsAttribute = "maxchars",
            maxStanzasAttribute = "maxstanzas",
            secondsAttribute = "seconds",
            sinceAttribute = "since";

        /// <summary>
        /// Initialises a MUC message history request object.
        /// </summary>
        /// <param name="to"></param>
        /// <param name="from"></param>
        /// <param name="maxChars"></param>
        public History(Jid to, Jid from, int maxChars = 0)
            :base(to, from, null, null, Xml.Element(xTag, MucNs.NsMain))
        {
            XElement.Child(Xml.Element(historyTag));
            MaxChars = maxChars;
        }

        /// <summary>
        /// Initialises a MUC message history request object.
        /// </summary>
        /// <param name="to"></param>
        /// <param name="from"></param>
        /// <param name="since"></param>
        public History(Jid to, Jid from, DateTime since)
            : base(to, from, null, null, Xml.Element(xTag, MucNs.NsMain))
        {
            XElement.Child(Xml.Element(historyTag));
            Since = since;
        }

        /// <summary>
        /// Limit the total number of characters in the history to "X" 
        /// (where the character count is the characters of the complete XML stanzas,
        /// not only their XML character data).
        /// </summary>
        public int? MaxChars
        {
            get
            {
                return GetValueAsInteger(maxCharsAttribute);
            }

            set
            {
                string safeValue = SafeNumber(value);
                ReplaceValue(maxCharsAttribute, safeValue);
            }
        }

        /// <summary>
        /// Limit the total number of messages in the history to "X".
        /// </summary>
        public int? MaxStanzas
        {
            get
            {
                return GetValueAsInteger(maxStanzasAttribute);
            }

            set
            {
                string safeValue = SafeNumber(value);
                ReplaceValue(maxStanzasAttribute, safeValue);
            }
        }

        /// <summary>
        /// Send only the messages received in the last "X" seconds.
        /// </summary>
        public int? Seconds
        {
            get
            {
                return GetValueAsInteger(secondsAttribute);
            }

            set
            {
                string safeValue = SafeNumber(value);
                ReplaceValue(secondsAttribute, safeValue);
            }
        }

        /// <summary>
        /// Send only the messages received since the UTC datetime specified.
        /// </summary>
        public DateTime? Since
        {
            get
            {
                return GetValueAsDateTime(sinceAttribute);
            }

            set
            {
                string safeValue = null;

                if(value.HasValue)
                    safeValue = value.Value
                        .ToUniversalTime()
                        .ToString("yyyy-MM-ddTHH:mm:ssZ");

                ReplaceValue(sinceAttribute, safeValue);
            }
        }

        /// <summary>
        /// The tag name of the stanza's root element
        /// </summary>
        protected override string RootElementName { get { return rootTag; } }

        private XmlElement XElement { get { return element[xTag]; } }

        private XmlElement HistoryElement { get { return GetNode(xTag, historyTag); } }

        /// <summary>
        /// Prevents the user from entering numbers less than zero.
        /// </summary>
        /// <param name="number">user input.</param>
        /// <returns>null or any number equal to or greater than zero.</returns>
        private string SafeNumber(int? number)
        {
            string result = null;

            if (number != null)
            {
                int? safeNumber = null;

                if (number < 0)
                    safeNumber = 0;
                else
                    safeNumber = number;

                result = safeNumber.ToString();
            }

            return result;
        }

        private int? GetValueAsInteger(string attributeName)
        {
            XmlElement node = HistoryElement;
            string v = node == null ? null : node.GetAttribute(attributeName);

            int? result = null;
            if (!string.IsNullOrEmpty(v))
                result = int.Parse(v);

            return result;
        }

        private DateTime? GetValueAsDateTime(string attributeName)
        {
            XmlElement node = HistoryElement;
            string v = node == null ? null : node.GetAttribute(attributeName);

            DateTime? result = null;
            if (!string.IsNullOrEmpty(v))
                result = DateTime.Parse(v);

            return result;
        }

        private void ReplaceValue(string attributeName, string value)
        {
            const string zero = "0";

            HistoryElement.RemoveAllAttributes();
            if (value == null)
                HistoryElement.SetAttribute(maxCharsAttribute, zero);
            else
                HistoryElement.SetAttribute(attributeName, value);
        }
    }
}
