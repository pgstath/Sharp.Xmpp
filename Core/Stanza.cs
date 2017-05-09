using System;
using System.Globalization;
using System.Xml;

namespace XMPPEngineer.Core
{
    /// <summary>
    /// Represents the base class for XML stanzas as are used by XMPP from which
    /// all implementations must derive.
    /// </summary>
    public abstract class Stanza
    {
        /// <summary>
        /// The XmlElement containing the actual data.
        /// </summary>
        protected XmlElement element;

        /// <summary>
        /// The tag name of the stanza's root element
        /// Allows the element tag name to be overridden.
        /// </summary>
        protected virtual string RootElementName
        {
            get { return GetType().Name.ToLowerInvariant(); }
        }

        /// <summary>
        /// Specifies the JID of the intended recipient for the stanza.
        /// </summary>
        public Jid To
        {
            get
            {
                string v = element.GetAttribute("to");
                return String.IsNullOrEmpty(v) ? null : new Jid(v);
            }

            set
            {
                if (value == null)
                    element.RemoveAttribute("to");
                else
                    element.SetAttribute("to", value.ToString());
            }
        }

        /// <summary>
        /// Specifies the JID of the sender. If this is null, the stanza was generated
        /// by the client's server.
        /// </summary>
        public Jid From
        {
            get
            {
                string v = element.GetAttribute("from");
                return String.IsNullOrEmpty(v) ? null : new Jid(v);
            }

            set
            {
                if (value == null)
                    element.RemoveAttribute("from");
                else
                    element.SetAttribute("from", value.ToString());
            }
        }

        /// <summary>
        /// The ID of the stanza, which may be used for internal tracking of stanzas.
        /// </summary>
        public string Id
        {
            get
            {
                var v = element.GetAttribute("id");
                return String.IsNullOrEmpty(v) ? null : v;
            }

            set
            {
                if (value == null)
                    element.RemoveAttribute("id");
                else
                    element.SetAttribute("id", value);
            }
        }

        /// <summary>
        /// The language of the XML character data if the stanza contains data that is
        /// intended to be presented to a human user.
        /// </summary>
        public CultureInfo Language
        {
            get
            {
                string v = element.GetAttribute("xml:lang");
                return String.IsNullOrEmpty(v) ? null : new CultureInfo(v);
            }

            set
            {
                if (value == null)
                    element.RemoveAttribute("xml:lang");
                else
                    element.SetAttribute("xml:lang", value.Name);
            }
        }

        /// <summary>
        /// The data of the stanza.
        /// </summary>
        public XmlElement Data
        {
            get
            {
                return element;
            }
        }

        /// <summary>
        /// Determines whether the stanza is empty, i.e. has no child nodes.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return Data.IsEmpty;
            }
        }

        /// <summary>
        /// Initializes a new instance of the Stanza class.
        /// </summary>
        /// <param name="namespace">The xml namespace of the stanza, if any.</param>
        /// <param name="to">The JID of the intended recipient for the stanza.</param>
        /// <param name="from">The JID of the sender.</param>
        /// <param name="id">The ID of the stanza.</param>
        /// <param name="language">The language of the XML character data of
        /// the stanza.</param>
        /// <param name="data">The content of the stanza.</param>
        public Stanza(string @namespace = null, Jid to = null,
            Jid from = null, string id = null, CultureInfo language = null,
            params XmlElement[] data)
        {
            element = Xml.Element(RootElementName, @namespace);
            To = to;
            From = from;
            Id = id;
            Language = language;
            foreach (XmlElement e in data)
            {
                if (e != null)
                    element.Child(e);
            }
        }

        /// <summary>
        /// Initializes a new instance of the Stanza class using the specified
        /// XmlElement.
        /// </summary>
        /// <param name="element">The XmlElement to create the stanza from.</param>
        /// <exception cref="ArgumentNullException">The element parameter is
        /// null.</exception>
        protected Stanza(XmlElement element)
        {
            element.ThrowIfNull("element");
            this.element = element;
        }

        /// <summary>
        /// Returns a textual representation of this instance of the Stanza class.
        /// </summary>
        /// <returns>A textual representation of this Stanza instance.</returns>
        public override string ToString()
        {
            return element.ToXmlString();
        }

        /// <summary>
        /// Recursively traverses the element using the given node list.
        /// </summary>
        /// <param name="nodeList">Tree of element name tags.</param>
        /// <returns>null or with the requested xml element.</returns>
        protected XmlElement GetNode(params string[] nodeList)
        {
            return GetNode(element, 0, nodeList);
        }

        /// <summary>
        /// Recursively traverses the element using the given node list.
        /// </summary>
        /// <param name="node">Current node in the node list.</param>
        /// <param name="depth">Current depth in the node list.</param>
        /// <param name="nodeList">Tree of element name tags.</param>
        /// <returns>null or with the requested xml element.</returns>
        private XmlElement GetNode(XmlElement node, int depth, params string[] nodeList)
        {
            XmlElement child = node[nodeList[depth]];
            return child == null || depth == nodeList.Length - 1 ? child : GetNode(child, depth + 1, nodeList);
        }
    }
}