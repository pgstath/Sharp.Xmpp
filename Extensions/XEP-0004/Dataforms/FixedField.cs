using System;
using System.Xml;

namespace XMPPEngineer.Extensions.Dataforms
{
    /// <summary>
    /// Represents a field that is intended for data description rather
    /// than data gathering.
    /// </summary>
    /// <remarks>
    /// This is a label of static text and corresponds to the Winforms Label
    /// control.
    /// </remarks>
    public class FixedField : DataField
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
        /// Initializes a new instance of the FixedField class for use in a
        /// requesting dataform.
        /// </summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="label">A human-readable name for the field.</param>
        /// <param name="description">A natural-language description of the field,
        /// intended for presentation in a user-agent.</param>
        /// <param name="value">The default value of the field.</param>
        /// <exception cref="ArgumentNullException">The name parameter is
        /// null.</exception>
        public FixedField(string name, string label = null, string description = null,
            string value = null)
            : base(DataFieldType.Fixed, name, false, label, description)
        {
            name.ThrowIfNull("name");
            Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the FixedField class for use in a
        /// requesting dataform.
        /// </summary>
        /// <param name="value">The value of the field.</param>
        public FixedField(string value)
            : base(DataFieldType.Fixed)
        {
            Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the FixedField class from the specified
        /// XML element.
        /// </summary>
        /// <param name="element">The XML 'field' element to initialize the instance
        /// with.</param>
        /// <exception cref="ArgumentNullException">The element parameter is
        /// null.</exception>
        /// <exception cref="ArgumentException">The specified XML element is not a
        /// valid data-field element, or the element is not a data-field of type
        /// 'fixed'.</exception>
        internal FixedField(XmlElement element)
            : base(element)
        {
            AssertType(DataFieldType.Fixed);
        }
    }
}