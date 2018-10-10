using System;
using System.Collections.Generic;
using System.Xml;

namespace XMPPEngineer.Extensions.Dataforms
{
    /// <summary>
    /// Represents a field for gathering or providing one option from among many.
    /// </summary>
    /// <remarks>
    /// This corresponds to a Winforms ComboBox control or a Listview control
    /// in which only a single option can be selected.
    /// </remarks>
    public class ListField : DataField
    {
        /// <summary>
        /// The options of the field.
        /// </summary>
        private XmlCollection<Option> options;

        /// <summary>
        /// Gets an enumerable collection of options.
        /// </summary>
        public ICollection<Option> Options
        {
            get
            {
                return options;
            }
        }

        /// <summary>
        /// The selected value.
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
        /// Initializes a new instance of the ListField class for use in a
        /// requesting dataform.
        /// </summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="required">Determines whether the field is required or
        /// optional.</param>
        /// <param name="label">A human-readable name for the field.</param>
        /// <param name="description">A natural-language description of the field,
        /// intended for presentation in a user-agent.</param>
        /// <param name="options">An enumerable collection of options to add to
        /// the field.</param>
        /// <param name="value">The default value of the field.</param>
        /// <exception cref="ArgumentNullException">The name parameter is
        /// null.</exception>
        public ListField(string name, bool required = false, string label = null,
            string description = null, IEnumerable<Option> options = null, string value = null)
            : base(DataFieldType.ListSingle, name, required, label, description)
        {
            this.options = new XmlCollection<Option>(element, "option", OptionFromElement);
            if (options != null)
            {
                foreach (Option o in options)
                {
                    Options.Add(o);
                }
            }
            if (value != null)
                Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the ListField class for use in a
        /// submitting dataform.
        /// </summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        /// <exception cref="ArgumentNullException">The name parameter is
        /// null.</exception>
        public ListField(string name, string value)
            : this(name, false, null, null, null, value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ListField class from the specified
        /// XML element.
        /// </summary>
        /// <param name="e">The XML 'field' element to initialize the instance
        /// with.</param>
        /// <exception cref="ArgumentNullException">The e parameter is
        /// null.</exception>
        /// <exception cref="ArgumentException">The specified XML element is not a
        /// valid data-field element, or the element is not a data-field of type
        /// 'list-single'.</exception>
        internal ListField(XmlElement e)
            : base(e)
        {
            AssertType(DataFieldType.ListSingle);
            options = new XmlCollection<Option>(element, "option", OptionFromElement);
        }

        /// <summary>
        /// Creates an Option instance from the specified XML element.
        /// </summary>
        /// <param name="element">The XML element to create an option from.</param>
        /// <returns>An initialized instance of the Option class.</returns>
        /// <exception cref="ArgumentNullException">The element parameter is
        /// null.</exception>
        /// <exception cref="ArgumentException">The specified XML element is not
        /// a valid 'option' element.</exception>
        private Option OptionFromElement(XmlElement element)
        {
            element.ThrowIfNull("element");
            string label = element.GetAttribute("label");
            if (label == String.Empty)
                label = null;
            if (element["value"] == null)
                throw new ArgumentException("Missing 'value' child.");
            string value = element["value"].InnerText;
            return new Option(value, label);
        }
    }
}