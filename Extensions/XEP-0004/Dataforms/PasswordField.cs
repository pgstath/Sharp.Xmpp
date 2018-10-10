using System;
using System.Xml;

namespace XMPPEngineer.Extensions.Dataforms
{
    /// <summary>
    /// Represents a field for gathering or providing a single line or word of
    /// text, which shall be obscured in an interface (e.g., with multiple
    /// instances of the asterisk character).
    /// </summary>
    /// <remarks>
    /// This corresponds to a Winforms TextBox control with the added
    /// requirement that entered characters be obscured.
    /// </remarks>
    public class PasswordField : DataField
    {
        /// <summary>
        /// The value of the field.
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
        /// Initializes a new instance of the PasswordFild class for use in a
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
        public PasswordField(string name, bool required = false, string label = null,
            string description = null, string value = null)
            : base(DataFieldType.TextPrivate, name, required, label, description)
        {
            name.ThrowIfNull("name");
            Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the PasswordField class for use in a
        /// submitting dataform.
        /// </summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        /// <exception cref="ArgumentNullException">The name parameter is
        /// null.</exception>
        public PasswordField(string name, string value)
            : this(name, false, null, null, value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the PasswordField class from the specified
        /// XML element.
        /// </summary>
        /// <param name="element">The XML 'field' element to initialize the instance
        /// with.</param>
        /// <exception cref="ArgumentNullException">The element parameter is
        /// null.</exception>
        /// <exception cref="ArgumentException">The specified XML element is not a
        /// valid data-field element, or the element is not a data-field of type
        /// 'text-private'.</exception>
        internal PasswordField(XmlElement element)
            : base(element)
        {
            AssertType(DataFieldType.TextPrivate);
        }
    }
}