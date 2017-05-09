using System;
using System.Xml;

namespace XMPPEngineer.Extensions.Dataforms
{
    /// <summary>
    /// Represents a field that provides an either-or choice between two options.
    /// </summary>
    public class BooleanField : DataField
    {
        /// <summary>
        /// The value of the field. If no choice has been made, this may be null.
        /// </summary>
        public bool? Value
        {
            get
            {
                var v = element["value"];
                if (v == null)
                    return null;
                return ParseValue(v.InnerText);
            }

            private set
            {
                if (element["value"] != null)
                {
                    if (value == null)
                        element.RemoveChild(element["value"]);
                    else
                        element["value"].InnerText = value.ToString().ToLower();
                }
                else
                {
                    if (value != null)
                        element.Child(Xml.Element("value").Text(value.ToString().ToLower()));
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the BooleanField class for use in a
        /// requesting dataform.
        /// </summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="required">Determines whether the field is required or
        /// optional.</param>
        /// <param name="label">A human-readable name for the field.</param>
        /// <param name="description">A natural-language description of the field,
        /// intended for presentation in a user-agent.</param>
        /// <param name="value">The default value of the field.</param>
        /// <exception cref="ArgumentNullException">The name parameter is
        /// null.</exception>
        public BooleanField(string name, bool required = false, string label = null,
            string description = null, bool? value = null)
            : base(DataFieldType.Boolean, name, required, label, description)
        {
            name.ThrowIfNull("name");
            Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the BooleanField class for use in a
        /// submitting dataform.
        /// </summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        /// <exception cref="ArgumentNullException">The name parameter is
        /// null.</exception>
        public BooleanField(string name, bool value)
            : this(name, false, null, null, value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the BooleanField class from the specified
        /// XML element.
        /// </summary>
        /// <param name="element">The XML 'field' element to initialize the instance
        /// with.</param>
        /// <exception cref="ArgumentNullException">The element parameter is
        /// null.</exception>
        /// <exception cref="ArgumentException">The specified XML element is not a
        /// valid data-field element, or the element is not a data-field of type
        /// 'boolean'.</exception>
        internal BooleanField(XmlElement element)
            : base(element)
        {
            AssertType(DataFieldType.Boolean);
        }

        /// <summary>
        /// Converts the specified string representation of a logical value to its
        /// boolean equivalent.
        /// </summary>
        /// <param name="value">The string value to pars.</param>
        /// <returns>The boolean equivalent of the string value.</returns>
        /// <exception cref="ArgumentNullException">The value paramter is
        /// null.</exception>
        private bool ParseValue(string value)
        {
            value.ThrowIfNull("value");
            if (value == "0" || value == "false")
                return false;
            // Be lenient.
            return true;
        }
    }
}