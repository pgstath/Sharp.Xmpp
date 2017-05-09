using System;
using System.Collections.Generic;
using System.Xml;

namespace XMPPEngineer.Extensions.Dataforms
{
    /// <summary>
    /// Represents a field for gathering or providing multiple Jabber IDs.
    /// </summary>
    /// <remarks>
    /// This corresponds to a Winforms ListBox control in which multiple options
    /// can be selected with the added requirement that each option be a valid
    /// JID.
    /// </remarks>
    public class JidMultiField : DataField
    {
        /// <summary>
        /// The values of the field.
        /// </summary>
        private XmlCollection<Jid> values;

        /// <summary>
        /// Gets an enumerable collection of values set on the field.
        /// </summary>
        public new ICollection<Jid> Values
        {
            get
            {
                return values;
            }
        }

        /// <summary>
        /// Initializes a new instance of the JidMultiField class for use in a
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
        public JidMultiField(string name, bool required = false, string label = null,
            string description = null, params Jid[] values)
            : base(DataFieldType.TextMulti, name, required, label, description)
        {
            this.values = new XmlCollection<Jid>(element, "value", e => new Jid(element.InnerText));
            if (values != null)
            {
                foreach (Jid s in values)
                {
                    if (s == null)
                        continue;
                    this.values.Add(s);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the JidMultiField class for use in a
        /// submitting dataform.
        /// </summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="values">The values of the field.</param>
        /// <exception cref="ArgumentNullException">The name parameter is
        /// null.</exception>
        public JidMultiField(string name, params Jid[] values)
            : this(name, false, null, null, values)
        {
        }

        /// <summary>
        /// Initializes a new instance of the JidMultiField class from the specified
        /// XML element.
        /// </summary>
        /// <param name="element">The XML 'field' element to initialize the instance
        /// with.</param>
        /// <exception cref="ArgumentNullException">The element parameter is
        /// null.</exception>
        /// <exception cref="ArgumentException">The specified XML element is not a
        /// valid data-field element, or the element is not a data-field of type
        /// 'jid-multi'.</exception>
        internal JidMultiField(XmlElement element)
            : base(element)
        {
            AssertType(DataFieldType.JidMulti);
            // FIXME: Assert existing values are valid JIDs?
        }
    }
}