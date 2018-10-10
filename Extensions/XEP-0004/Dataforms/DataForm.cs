using System;
using System.Xml;

namespace XMPPEngineer.Extensions.Dataforms
{
    /// <summary>
    /// The abstract base class from which all implementations of concrete
    /// data-forms must derive.
    /// </summary>
    public abstract class DataForm
    {
        /// <summary>
        /// The fields contained in the data-form.
        /// </summary>
        private FieldList fields;

        /// <summary>
        /// The underlying XML element representing the data-form.
        /// </summary>
        protected XmlElement element;

        /// <summary>
        /// The title of the data-form.
        /// </summary>
        public string Title
        {
            get
            {
                if (element["title"] != null)
                    return element["title"].InnerText;
                return null;
            }

            set
            {
                var e = element["title"];
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
                        element.Child(Xml.Element("title").Text(value));
                }
            }
        }

        /// <summary>
        /// The natural-language instructions to be followed by the
        /// form-submitting entity.
        /// </summary>
        public string Instructions
        {
            get
            {
                if (element["instructions"] != null)
                    return element["instructions"].InnerText;
                return null;
            }

            set
            {
                var e = element["instructions"];
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
                        element.Child(Xml.Element("instructions").Text(value));
                }
            }
        }

        /// <summary>
        /// The type of the data-form.
        /// </summary>
        /// <exception cref="XmlException">The 'type' attribute of the underlying
        /// XML element is invalid.</exception>
        public DataFormType Type
        {
            get
            {
                return GetDataFormType();
            }

            protected set
            {
                element.SetAttribute("type", value.ToString().ToLower());
            }
        }

        /// <summary>
        /// A list of fields contained in the data-form.
        /// </summary>
        public FieldList Fields
        {
            get
            {
                return fields;
            }
        }

        /// <summary>
        /// Returns a textual XML representation of the data-form.
        /// </summary>
        /// <returns>A string containing the XML representation of the
        /// data-form.</returns>
        public override string ToString()
        {
            return element.ToXmlString();
        }

        /// <summary>
        /// Returns an XML element representing the data-form.
        /// </summary>
        /// <returns>An XML element representing the data-form.</returns>
        public XmlElement ToXmlElement()
        {
            return element;
        }

        /// <summary>
        /// Initializes a new instance of the DataForm class.
        /// </summary>
        /// <param name="title">The title of the data-form.</param>
        /// <param name="instructions">The natural-language instructions to be
        /// followed by the form-submitting entity.</param>
        /// <param name="readOnly">Set to true to create a read-only form to
        /// which no fields may be added, otherwise false.</param>
        /// <param name="fields">One or several data-fields to add to the
        /// form.</param>
        internal DataForm(string title = null, string instructions = null,
            bool readOnly = false, params DataField[] fields)
        {
            element = Xml.Element("x", "jabber:x:data");
            Title = title;
            Instructions = instructions;
            this.fields = new FieldList(element, readOnly);
            if (fields != null)
            {
                foreach (var f in fields)
                    if (f != null)
                        this.fields.Add(f);
            }
        }

        /// <summary>
        /// Initializes a new instance of the DataForm class from the specified
        /// XML element.
        /// </summary>
        /// <param name="element">The XML 'field' element to initialize the instance
        /// with.</param>
        /// <param name="readOnly">Set to true to create a read-only form to
        /// which no fields may be added, otherwise false.</param>
        /// <exception cref="ArgumentNullException">The element parameter is
        /// null.</exception>
        /// <exception cref="ArgumentException">The specified XML element is not a
        /// valid data-form element.</exception>
        internal DataForm(XmlElement element, bool readOnly = false)
        {
            element.ThrowIfNull("element");
            this.element = element;
            try
            {
                fields = new FieldList(element, readOnly);
                // Call GetDataFormType method to verify the 'type' attribute.
                GetDataFormType();
            }
            catch (XmlException e)
            {
                throw new ArgumentException("The element parameter is not a valid " +
                    "data-form.", e);
            }
        }

        /// <summary>
        /// Asserts the data-form is of the specified type.
        /// </summary>
        /// <param name="expected">The type to assert.</param>
        /// <exception cref="ArgumentException">The data-form is not of the
        /// expected type.</exception>
        protected void AssertType(DataFormType expected)
        {
            if (Type != expected)
            {
                throw new ArgumentException("The specified XML element is not a " +
                    "data-form of type '" + expected.ToString() + "'.");
            }
        }

        /// <summary>
        /// Returns the type of the data-form.
        /// </summary>
        /// <returns>The type of the data-form.</returns>
        /// <exception cref="XmlException">The 'type' attribute of the underlying
        /// XML element is invalid.</exception>
        private DataFormType GetDataFormType()
        {
            try
            {
                string t = element.GetAttribute("type");
                return Util.ParseEnum<DataFormType>(t);
            }
            catch (Exception e)
            {
                throw new XmlException("The 'type' attribute of the underlying " +
                    "XML element is invalid.", e);
            }
        }
    }
}