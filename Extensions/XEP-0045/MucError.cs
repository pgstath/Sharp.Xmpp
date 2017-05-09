using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace XMPPEngineer.Extensions
{
    /// <summary>
    /// Implements a MUC error response which may be contained in either an IQ or a Presence.
    /// </summary>
    internal class MucError : Core.Stanza
    {
        private const string errorTag = "error",
            byAttribute = "by",
            typeAttribute = "type";

        public MucError(Core.Stanza stanza) : base(stanza.Data)
        {
        }

        /// <summary>
        /// Only filled in on a presence, may be null.
        /// </summary>
        public Jid By
        {
            get
            {
                XmlNode node = ErrorNode;
                string v = node == null ? null : node.Attributes?[byAttribute]?.Value;
                return string.IsNullOrEmpty(v) ? null : new Jid(v);
            }
        }

        /// <summary>
        /// The type of error.
        /// </summary>
        public ErrorType ErrorType
        {
            get
            {
                // It's possible for the error tag to be either inside or outside the x tag.
                string errorTypeString = ErrorNode?.Attributes?[typeAttribute]?.Value;

                ErrorType error;
                const bool ignoreCase = true;

                // It should always parse, otherwise the message doesn't meet the protocol.
                if (!string.IsNullOrEmpty(errorTypeString) || !Enum.TryParse(errorTypeString, ignoreCase, out error))
                    error = ErrorType.Cancel;

                return error;
            }
        }

        /// <summary>
        /// The reason for the error.
        /// </summary>
        public ErrorCondition ErrorCondition
        {
            get
            {
                const string allDashses = "-";

                // It's possible for the error tag to be either inside or outside the x tag.
                string nodeName = ErrorNode?.FirstChild?.Name?.Replace(allDashses, string.Empty);

                ErrorCondition reason;
                const bool ignoreCase = true;

                // It should always parse, otherwise the message doesn't meet the protocol.
                if (string.IsNullOrEmpty(nodeName) || !Enum.TryParse(nodeName, ignoreCase, out reason))
                    reason = ErrorCondition.BadRequest;

                return reason;
            }
        }

        private XmlNode ErrorNode
        {
            get
            {
                // It's possible for the error tag to be either inside or outside the x tag.
                return element.GetElementsByTagName(errorTag)?.Item(0);
            }
        }

        /// <summary>
        /// Determines whether the stanza contains an error.
        /// </summary>
        /// <param name="stanza">input stanza</param>
        /// <returns>true if contains an error</returns>
        public static bool IsError(Core.Stanza stanza)
        {
            // Not every response has a namespace in it
            return (new MucError(stanza)).ErrorNode != null;
        }        
    }
}
