using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Sharp.Xmpp.Extensions.Dataforms
{
    /// <summary>
    /// The base class from which all implementations of data-fields must derive.
    /// </summary>
    /// <remarks>Unfortunately, this can not be an abstract class the way the
    /// data-forms extension has been designed; Forms of type 'submit' are not
    /// required to include a type-attribute for fields and thus make strongly
    /// typed data-fields impossible.</remarks>
    public class DataField
    {
        private const string xmlns = "jabber:x:data";

        /// <summary>
        /// The underlying XML element representing the data-field.
        /// </summary>
        protected XmlElement element;

        /// <summary>
        /// A human-readable name for the field.
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
        /// A natural-language description of the field, intended for presentation
        /// in a user-agent.
        /// </summary>
        public string Description
        {
            get
            {
                if (element["desc"] != null)
                    return element["desc"].InnerText;
                return null;
            }

            private set
            {
                var e = element["desc"];
                if (e != null)
                {
                    if (value == null)
                        element.RemoveChild(e);
                    else
                        e.InnerText = value;
                }
                else
                {
                    if (value != null)
                        element.Child(Xml.Element("desc", xmlns).Text(value));
                }
            }
        }

        /// <summary>
        /// Determines whether the field is required to fill out, or optional.
        /// </summary>
        public bool Required
        {
            get
            {
                return element["required"] != null;
            }

            private set
            {
                if (value == false)
                {
                    if (element["required"] != null)
                        element.RemoveChild(element["required"]);
                }
                else
                {
                    if (element["required"] == null)
                        element.Child(Xml.Element("required", xmlns));
                }
            }
        }

        /// <summary>
        /// The name of the field.
        /// </summary>
        public string Name
        {
            get
            {
                var v = element.GetAttribute("var");
                return String.IsNullOrEmpty(v) ? null : v;
            }

            private set
            {
                if (value == null)
                    element.RemoveAttribute("var");
                else
                    element.SetAttribute("var", value);
            }
        }

        /// <summary>
        /// The type of the field or null if no type has been specified.
        /// </summary>
        /// <exception cref="XmlException">The 'type' attribute of the underlying
        /// XML element is invalid.</exception>
        public DataFieldType? Type
        {
            get
            {
                return GetDataFieldType();
            }

            private set
            {
                SetType(value);
            }
        }

        /// <summary>
        /// Returns a textual XML representation of the data-field.
        /// </summary>
        /// <returns>A string containing the XML representation of the
        /// data-field.</returns>
        public override string ToString()
        {
            return element.ToXmlString();
        }

        /// <summary>
        /// Returns an XmlElement instance representing the data-field.
        /// </summary>
        /// <returns>An XmlElement representing the data-field.</returns>
        public XmlElement ToXmlElement()
        {
            return element;
        }

        /// <summary>
        /// Returns a collection of the data-field's values.
        /// </summary>
        public IEnumerable<string> Values
        {
            get
            {
                ISet<string> set = new HashSet<string>();
                foreach (XmlElement e in element.GetElementsByTagName("value"))
                    set.Add(e.InnerText);
                return set;
            }
        }

        /// <summary>
        /// Initializes a new instance of the DataField class for use in a
        /// requesting dataform.
        /// </summary>
        /// <param name="type">The type of the field.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="required">Determines whether the field is required or
        /// optional.</param>
        /// <param name="label">A human-readable name for the field.</param>
        /// <param name="description">A natural-language description of the field,
        /// intended for presentation in a user-agent.</param>
        public DataField(DataFieldType type, string name = null, bool required = false,
            string label = null, string description = null)
        {
            element = Xml.Element("field", xmlns);
            Type = type;
            Name = name;
            Required = required;
            Label = label;
            Description = description;
        }

        /// <summary>
        /// Initializes a new instance of the DataField class from the specified
        /// XML element.
        /// </summary>
        /// <param name="element">The XML 'field' element to initialize the instance
        /// with.</param>
        /// <exception cref="ArgumentNullException">The element parameter is
        /// null.</exception>
        /// <exception cref="ArgumentException">The specified XML element is not a
        /// valid data-field element.</exception>
        internal DataField(XmlElement element)
        {
            element.ThrowIfNull("element");
            this.element = element;
            try
            {
                // Call GetDataFieldType method to verify the 'type' attribute.
                GetDataFieldType();
            }
            catch (XmlException e)
            {
                throw new ArgumentException("The element parameter is not a valid " +
                    "data-field.", e);
            }
        }

        /// <summary>
        /// Asserts the data-field is of the specified type.
        /// </summary>
        /// <param name="expected">The type to assert.</param>
        /// <exception cref="ArgumentException">The data-field is not of the
        /// expected type.</exception>
        protected void AssertType(DataFieldType expected)
        {
            if (Type != expected)
            {
                throw new ArgumentException("The specified XML element is not a " +
                    "data-field of type '" + expected.ToString() + "'.");
            }
        }

        /// <summary>
        /// Sets the type of the data-field to the specified value.
        /// </summary>
        /// <param name="type">The value to set the type of the data-field to. Can be
        /// one of the values from the DataFieldType enumeration.</param>
        private void SetType(DataFieldType? type)
        {
            if (!type.HasValue)
                element.RemoveAttribute("type");
            else
            {
                string value = TypeToAttributeValue(type.Value);
                element.SetAttribute("type", value);
            }
        }

        /// <summary>
        /// Converts the specified value from the DataFieldType enumeration into
        /// its respective textual representation.
        /// </summary>
        /// <param name="type">The value to convert into a string.</param>
        /// <returns>A string representing the specified value.</returns>
        private string TypeToAttributeValue(DataFieldType type)
        {
            StringBuilder b = new StringBuilder();
            string s = type.ToString();
            for (int i = 0; i < s.Length; i++)
            {
                if (Char.IsUpper(s, i) && i > 0)
                    b.Append('-');
                b.Append(Char.ToLower(s[i]));
            }
            return b.ToString();
        }

        /// <summary>
        /// Converts the specified string into a value of the DataFieldType
        /// enumeration.
        /// </summary>
        /// <param name="value">The string value to convert.</param>
        /// <returns>An object of the DataFieldType enumeration whose value is
        /// represented by value.</returns>
        /// <exception cref="ArgumentNullException">The value parameter is
        /// null.</exception>
        /// <exception cref="ArgumentException">value is either an empty string
        /// or only contains white space, or value is a name, but not one of
        /// the named constants of the DataFieldType enumeration.</exception>
        private DataFieldType AttributeValueToType(string value)
        {
            value.ThrowIfNull("value");
            StringBuilder b = new StringBuilder();
            string s = value;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '-')
                    b.Append(Char.ToUpper(s[++i]));
                else
                    b.Append(s[i]);
            }
            value = b.ToString();
            return Util.ParseEnum<DataFieldType>(value);
        }

        /// <summary>
        /// Returns the type of the data-field.
        /// </summary>
        /// <returns>The type of the data-field or null if no type has been
        /// specified.</returns>
        /// <exception cref="XmlException">The 'type' attribute of the underlying
        /// XML element is invalid.</exception>
        private DataFieldType? GetDataFieldType()
        {
            try
            {
                string t = element.GetAttribute("type");
                if (String.IsNullOrEmpty(t))
                    return null;
                return AttributeValueToType(t);
            }
            catch (Exception e)
            {
                throw new XmlException("The 'type' attribute of the underlying " +
                    "XML element is invalid.", e);
            }
        }
    }
}
