using System;
using System.Collections.Generic;
using System.Xml;

namespace XMPPEngineer.Extensions.Dataforms
{
    /// <summary>
    /// Represents a field that is intended to be invisible to the form-submitting
    /// entity, and is returned with the form.
    /// </summary>
    /// <remarks>
    /// Hidden fields should not be shown to the user but just be submitted with
    /// the form, much like a hidden field in an HTML form.
    /// </remarks>
    public class HiddenField : DataField
    {
        /// <summary>
        /// The values of the field.
        /// </summary>
        private XmlCollection<string> values;

        /// <summary>
        /// Gets an enumerable collection of values set on the field.
        /// </summary>
        public new ICollection<string> Values
        {
            get
            {
                return values;
            }
        }

        /// <summary>
        /// Initializes a new instance of the HiddenField class for use in a
        /// requesting dataform.
        /// </summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="required">Determines whether the field is required or
        /// optional.</param>
        /// <param name="label">A human-readable name for the field.</param>
        /// <param name="description">A natural-language description of the field,
        /// intended for presentation in a user-agent.</param>
        /// <param name="values">The default values of the field.</param>
        /// <exception cref="ArgumentNullException">The name parameter is
        /// null.</exception>
        public HiddenField(string name, bool required = false, string label = null,
            string description = null, params string[] values)
            : base(DataFieldType.Hidden, name, required, label, description)
        {
            this.values = new XmlCollection<string>(element, "value", elem => elem.InnerText);
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
        /// Initializes a new instance of the HiddenField class for use in a
        /// submitting dataform.
        /// </summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="values">The values of the field.</param>
        /// <exception cref="ArgumentNullException">The name parameter is
        /// null.</exception>
        public HiddenField(string name, params string[] values)
            : this(name, false, null, null, values)
        {
        }

        /// <summary>
        /// Initializes a new instance of the HiddenField class from the specified
        /// XML element.
        /// </summary>
        /// <param name="element">The XML 'field' element to initialize the instance
        /// with.</param>
        /// <exception cref="ArgumentNullException">The element parameter is
        /// null.</exception>
        /// <exception cref="ArgumentException">The specified XML element is not a
        /// valid data-field element, or the element is not a data-field of type
        /// 'hidden'.</exception>
        internal HiddenField(XmlElement element)
            : base(element)
        {
            AssertType(DataFieldType.Hidden);
        }
    }
}