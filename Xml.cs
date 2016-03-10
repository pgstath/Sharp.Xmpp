using System;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Sharp.Xmpp
{
    /// <summary>
    /// Provides a factory method for creating XmlElement instances and adds
    /// a couple of useful shortcut extensions to the XmlElement class.
    /// </summary>
    public static class Xml
    {
        /// <summary>
        /// Creates a new XmlElement instance.
        /// </summary>
        /// <param name="name">The name of the element.</param>
        /// <param name="namespace">The namespace of the element.</param>
        /// <returns>An initialized instance of the XmlElement class.</returns>
        /// <exception cref="ArgumentNullException">The name parameter is null.</exception>
        /// <exception cref="ArgumentException">The name parameter is the
        /// empty string.</exception>
        /// <exception cref="XmlException">The name or the namespace parameter
        /// is invalid.</exception>
        public static XmlElement Element(string name, string @namespace = null)
        {
            name.ThrowIfNullOrEmpty("name");
            return new XmlDocument().CreateElement(name, @namespace);
        }

        /// <summary>
        /// Adds the specified element to the end of the list of child nodes, of
        /// this node.
        /// </summary>
        /// <param name="e">The XmlElement instance the method is invoked for.</param>
        /// <param name="child">The node to add.</param>
        /// <returns>A reference to the XmlElement instance.</returns>
        public static XmlElement Child(this XmlElement e, XmlElement child)
        {
            XmlNode imported = e.OwnerDocument.ImportNode(child, true);
            e.AppendChild(imported);
            return e;
        }

        /// <summary>
        /// Sets the value of the attribute with the specified name.
        /// </summary>
        /// <param name="e">The XmlElement instance the method is invoked for.</param>
        /// <param name="name">The name of the attribute to create or alter.</param>
        /// <param name="value">The value to set for the attribute.</param>
        /// <returns>A reference to the XmlElement instance.</returns>
        public static XmlElement Attr(this XmlElement e, string name, string value)
        {
            e.SetAttribute(name, value);
            return e;
        }

        /// <summary>
        /// Adds the specified text to the end of the list of child nodes, of
        /// this node.
        /// </summary>
        /// <param name="e">The XmlElement instance the method is invoked for.</param>
        /// <param name="text">The text to add.</param>
        /// <returns>A reference to the XmlElement instance.</returns>
        public static XmlElement Text(this XmlElement e, string text)
        {
            e.AppendChild(e.OwnerDocument.CreateTextNode(text));
            return e;
        }

        /// <summary>
        /// Serializes the XmlElement instance into a string.
        /// </summary>
        /// <param name="e">The XmlElement instance the method is invoked for.</param>
        /// <param name="xmlDeclaration">true to include a XML declaration,
        /// otherwise false.</param>
        /// <param name="leaveOpen">true to leave the tag of an empty element
        /// open, otherwise false.</param>
        /// <returns>A textual representation of the XmlElement instance.</returns>
        public static string ToXmlString(this XmlElement e, bool xmlDeclaration = false,
            bool leaveOpen = false)
        {
            // Can't use e.OuterXml because it "messes up" namespaces for elements with
            // a prefix, i.e. stream:stream (What it does is probably correct, but just
            // not what we need for XMPP).
            StringBuilder b = new StringBuilder("<" + e.Name);
            if (!String.IsNullOrEmpty(e.NamespaceURI))
                b.Append(" xmlns='" + e.NamespaceURI + "'");
            foreach (XmlAttribute a in e.Attributes)
            {
                if (a.Name == "xmlns")
                    continue;
                if (a.Value != null)
                    b.Append(" " + a.Name + "='" + SecurityElement.Escape(a.Value.ToString())
                        + "'");
            }
            if (e.IsEmpty)
                b.Append("/>");
            else
            {
                b.Append(">");
                foreach (var child in e.ChildNodes)
                {
                    if (child is XmlElement)
                        b.Append(((XmlElement)child).ToXmlString());
                    else if (child is XmlText)
                        b.Append(((XmlText)child).InnerText);
                }
                b.Append("</" + e.Name + ">");
            }
            string xml = b.ToString();
            if (xmlDeclaration)
                xml = "<?xml version='1.0' encoding='UTF-8'?>" + xml;
            if (leaveOpen)
                return Regex.Replace(xml, "/>$", ">");
            return xml;
        }
    }
}