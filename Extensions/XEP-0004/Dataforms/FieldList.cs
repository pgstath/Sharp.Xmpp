using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace XMPPEngineer.Extensions.Dataforms
{
    /// <summary>
    /// Represents a list of data-fields contained in a data-form.
    /// </summary>
    public class FieldList : IEnumerable<DataField>, IReadOnlyCollection<DataField>
    {
        /// <summary>
        /// The underlying XML element of the data-form.
        /// </summary>
        private XmlElement element;

        /// <summary>
        /// Determines whether the collection of data-fields is read-only.
        /// </summary>
        private bool readOnly;

        /// <summary>
        /// Gets the number of elements contained in the list of data-fields.
        /// </summary>
        public int Count
        {
            get
            {
                return GetFieldElements().Count;
            }
        }

        /// <summary>
        /// Determines whether the FieldList is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return readOnly;
            }
        }

        /// <summary>
        /// Returns the data-field at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the data-field in the list
        /// of data-fields to return.</param>
        /// <returns>The data-field instance at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The specified index is
        /// negative, or the specified index is greater or equal to the number of
        /// data-fields in the list of data-fields.</exception>
        public DataField this[int index]
        {
            get
            {
                var fields = GetFields();
                index.ThrowIfOutOfRange(0, fields.Count - 1);
                return fields[index];
            }
        }

        /// <summary>
        /// Returns the data-field with the specified name.
        /// </summary>
        /// <param name="name">The name of the data-field to return.</param>
        /// <returns>The data-field with the specified name or null if no such
        /// data-field exists in the list of data-fields-</returns>
        public DataField this[string name]
        {
            get
            {
                foreach (var field in GetFields())
                {
                    if (field.Name == name)
                        return field;
                }
                return null;
            }
        }

        /// <summary>
        /// Adds the specified data-field to the end of the list of data-fields.
        /// </summary>
        /// <param name="item">The data-field to add to the end of the list of
        /// data-fields.</param>
        /// <exception cref="ArgumentNullException">The item parameter is
        /// null.</exception>
        /// <exception cref="ArgumentException">A field with the same name already
        /// exists in the list of data-fields.</exception>
        /// <exception cref="NotSupportedException">The instance of the FieldList
        /// class is read-only.</exception>
        public void Add(DataField item)
        {
            item.ThrowIfNull("item");
            if (IsReadOnly)
                throw new NotSupportedException("The list is read-only.");
            if (item.Name != null && Contains(item.Name))
                throw new ArgumentException("A field with the same name already exists.");
            element.Child(item.ToXmlElement());
        }

        /// <summary>
        /// Removes the specified data-field from the list of data-fields.
        /// </summary>
        /// <param name="item">The data-field to remove from the list of
        /// data-fields.</param>
        /// <remarks>If the specified data-field is not contained in the list of
        /// data-fields, this method does nothing.</remarks>
        /// <exception cref="ArgumentNullException">The item parameter is
        /// null.</exception>
        public void Remove(DataField item)
        {
            item.ThrowIfNull("item");
            // FIXME: This won't work for 'fixed' items that don't have names.
            Remove(item.Name);
        }

        /// <summary>
        /// Remove the data-field with the specifid name from the list of data-fields.
        /// </summary>
        /// <param name="name">The name of the data-field to remove.</param>
        /// <remarks>If a data-field element with the specified name is not contained
        /// in the list of data-fields, this method does nothing.</remarks>
        public void Remove(string name)
        {
            if (name == null)
                return;
            XmlElement e = GetFieldElementByName(name);
            if (e != null)
                element.RemoveChild(e);
        }

        /// <summary>
        /// Removes all data-fields.
        /// </summary>
        /// <exception cref="NotSupportedException">The FieldList instance is
        /// read-only.</exception>
        public void Clear()
        {
            if (IsReadOnly)
                throw new NotSupportedException("The list is read-only.");
            ISet<XmlElement> set = new HashSet<XmlElement>();
            foreach (XmlElement e in element.GetElementsByTagName("field"))
                if (e.ParentNode == element)
                    set.Add(e);
            foreach (XmlElement e in set)
                element.RemoveChild(e);
        }

        /// <summary>
        /// Determines whether a data-field with the specified name is contained
        /// in the list of data-fields.
        /// </summary>
        /// <param name="name">The name of the data-field to look for.</param>
        /// <returns>true if a data-field with the specified name exists within
        /// list of data-fields; Otherwise false.</returns>
        /// <exception cref="ArgumentNullException">The name parameter is
        /// null.</exception>
        public bool Contains(string name)
        {
            name.ThrowIfNull("name");
            foreach (var field in GetFields())
            {
                if (field.Name == name)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the FieldList instance.
        /// </summary>
        /// <returns>An enumerator that iterates through the FieldList
        /// instance.</returns>
        public IEnumerator<DataField> GetEnumerator()
        {
            return GetFields().GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the FieldList instance.
        /// </summary>
        /// <returns>An enumerator that iterates through the FieldList
        /// instance.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Initializes a new instance of the FieldList class from the specified
        /// XML element.
        /// </summary>
        /// <param name="element">The XML element to initialize this instance
        /// from.</param>
        /// <param name="readOnly">If true, the list of data-fields can not be
        /// modified.</param>
        /// <exception cref="ArgumentNullException">The element parameter is
        /// null.</exception>
        /// <exception cref="ArgumentException">The specified element is not a
        /// valid XML data-form.</exception>
        public FieldList(XmlElement element, bool readOnly = false)
        {
            element.ThrowIfNull("element");
            this.element = element;
            this.readOnly = readOnly;
            try
            {
                // Call GetFields to verify all fields are valid.
                GetFields();
            }
            catch (Exception e)
            {
                throw new ArgumentException("The specified XML element is not a " +
                    "valid data-form.", e);
            }
        }

        /// <summary>
        /// Returns a list of XML data-field elements contained in the underlying
        /// data-form.
        /// </summary>
        /// <returns>A list of XML data-field elements.</returns>
        private IList<XmlElement> GetFieldElements()
        {
            IList<XmlElement> elems = new List<XmlElement>();
            foreach (XmlElement e in element.GetElementsByTagName("field"))
            {
                if (e.ParentNode == element)
                    elems.Add(e);
            }
            return elems;
        }

        /// <summary>
        /// Returns a list of DataField instances contained in the FieldList.
        /// </summary>
        /// <returns>A list of DataField instances contained in the FieldList
        /// instance.</returns>
        /// <exception cref="XmlException">A 'field' XML element in the underlying
        /// data-form is invalid.</exception>
        private IList<DataField> GetFields()
        {
            IList<DataField> fields = new List<DataField>();
            foreach (XmlElement e in GetFieldElements())
                fields.Add(FieldFromXml(e));
            return fields;
        }

        /// <summary>
        /// Creates a strongly-typed data-field instance of the proper type from the
        /// specified data-field XML element.
        /// </summary>
        /// <param name="element">The data-field XML element to create a DataField
        /// instance from.</param>
        /// <returns>An instance of a class derived from the DataField class
        /// representing the specified data-field element.</returns>
        /// <exception cref="ArgumentNullException">The element parameter is
        /// null.</exception>
        /// <exception cref="ArgumentException">The specified XML element is not a
        /// valid data-field.</exception>
        private DataField FieldFromXml(XmlElement element)
        {
            element.ThrowIfNull("element");
            try
            {
                // If the element does not have a 'type' attribute, we can only
                // return a weakly-typed data-field.
                DataFieldType? type = GetDataFieldType(element);
                if (type.HasValue == false)
                    return new DataField(element);
                switch (type.Value)
                {
                    case DataFieldType.Boolean:
                        return new BooleanField(element);

                    case DataFieldType.Fixed:
                        return new FixedField(element);

                    case DataFieldType.TextSingle:
                        return new TextField(element);

                    case DataFieldType.TextPrivate:
                        return new PasswordField(element);

                    case DataFieldType.JidSingle:
                        return new JidField(element);

                    case DataFieldType.Hidden:
                        return new HiddenField(element);

                    case DataFieldType.TextMulti:
                        return new TextMultiField(element);

                    case DataFieldType.JidMulti:
                        return new JidMultiField(element);

                    case DataFieldType.ListMulti:
                        return new ListMultiField(element);

                    case DataFieldType.ListSingle:
                        return new ListField(element);

                    default:
                        throw new XmlException("Invalid 'type' attribute: " + type);
                }
            }
            catch (Exception e)
            {
                throw new ArgumentException("Invalid 'field' XML element.", e);
            }
        }

        /// <summary>
        /// Returns the data-field XML element with the specified name.
        /// </summary>
        /// <param name="name">The name of the data-field element to retrieve.</param>
        /// <returns>The data-field XML element with the specified name or null
        /// if no such element exists in the list of data-fields.</returns>
        /// <exception cref="ArgumentNullException">The name parameter is
        /// null.</exception>
        private XmlElement GetFieldElementByName(string name)
        {
            name.ThrowIfNull("name");
            foreach (XmlElement e in GetFieldElements())
            {
                String s = e.GetAttribute("var");
                if (s == name)
                    return e;
            }
            return null;
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
        /// <param name="element">The data-field element whose type to
        /// retrieve.</param>
        /// <returns>The type of the specified data-field.</returns>
        /// <exception cref="XmlException">The 'type' attribute of the underlying
        /// XML element is invalid.</exception>
        private DataFieldType? GetDataFieldType(XmlElement element)
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