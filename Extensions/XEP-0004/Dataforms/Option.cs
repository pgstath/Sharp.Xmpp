using System;
using System.Xml;

namespace XMPPEngineer.Extensions.Dataforms
{
    /// <summary>
    /// Represents an option of a list data-field.
    /// </summary>
    public class Option
    {
        /// <summary>
        /// The underlying XML element.
        /// </summary>
        private XmlElement element;

        /// <summary>
        /// A human-readable name for the option.
        /// </summary>
        public string Label
        {
            get
            {
                var v = element.GetAttribute("label");
                return String.IsNullOrEmpty(v) ? null : v;
            }

            private set
            {
                if (value == null)
                    element.RemoveAttribute("label");
                else
                    element.SetAttribute("label", value);
            }
        }

        /// <summary>
        /// The value of the option field.
        /// </summary>
        public string Value
        {
            get
            {
                var v = element["value"];
                return v != null ? v.InnerText : null;
            }

            private set
            {
                if (element["value"] != null)
                {
                    if (value == null)
                        element.RemoveChild(element["value"]);
                    else
                        element["value"].InnerText = value;
                }
                else
                {
                    if (value != null)
                        element.Child(Xml.Element("value").Text(value));
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the Option class.
        /// </summary>
        /// <param name="value">The value of the option field.</param>
        /// <param name="label">A human-readable name for the option.</param>
        /// <exception cref="ArgumentNullException">The value parameter
        /// is null.</exception>
        public Option(string value, string label = null)
        {
            value.ThrowIfNull("value");
            element = Xml.Element("option");
            Value = value;
            Label = label;
        }

        /// <summary>
        /// Returns a textual XML representation of the Option instance.
        /// </summary>
        /// <returns>A textual XML representation of the Option instance.</returns>
        /// <remarks>This actually only returns the inner XML of the option
        /// XML element, so that the Option class can be used with the generic
        /// XmlCollection class.</remarks>
        public override string ToString()
        {
            return element.InnerXml;
        }
    }
}