using System;
using System.Collections.Generic;
using System.Xml;

namespace XMPPEngineer.Extensions.Dataforms
{
    /// <summary>
    /// Represents a field for gathering or providing one or more options
    /// from among many.
    /// </summary>
    /// <remarks>
    /// This corresponds to a Winforms ListBox control in which multiple options
    /// can be selected.
    /// </remarks>
    public class ListMultiField : DataField
    {
        /// <summary>
        /// The values of the field.
        /// </summary>
        private XmlCollection<string> values;

        /// <summary>
        /// The options of the field.
        /// </summary>
        private XmlCollection<Option> options;

        /// <summary>
        /// Gets an enumerable collection of selected values.
        /// </summary>
        public new ICollection<string> Values
        {
            get
            {
                return values;
            }
        }

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
        /// <param name="values">The default values of the field.</param>
        /// <exception cref="ArgumentNullException">The name parameter is
        /// null.</exception>
        public ListMultiField(string name, bool required = false, string label = null,
            string description = null, IEnumerable<Option> options = null, params string[] values)
            : base(DataFieldType.ListMulti, name, required, label, description)
        {
            this.values = new XmlCollection<string>(element, "value", elem => elem.InnerText);
            this.options = new XmlCollection<Option>(element, "option", OptionFromElement);
            if (options != null)
            {
                foreach (Option o in options)
                {
                    Options.Add(o);
                }
            }
            if (values != null)
            {
                foreach (string s in values)
                {
                    if (s == null)
                        continue;
                    this.values.Add(s);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the ListMultiField class for use in a
        /// submitting dataform.
        /// </summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="values">The values of the field.</param>
        /// <exception cref="ArgumentNullException">The name parameter is
        /// null.</exception>
        public ListMultiField(string name, params string[] values)
            : this(name, false, null, null, null, values)
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
        /// 'list-multi'.</exception>
        internal ListMultiField(XmlElement e)
            : base(e)
        {
            AssertType(DataFieldType.ListMulti);
            this.values = new XmlCollection<string>(element, "value", elem => elem.InnerText);
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