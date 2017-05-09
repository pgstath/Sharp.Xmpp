using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace XMPPEngineer.Im
{
    /// <summary>
    /// An XML-backed dictionary.
    /// </summary>
    internal class XmlDictionary : IDictionary<string, string>
    {
        /// <summary>
        /// The parent XML element of the dictionary items.
        /// </summary>
        private XmlElement element;

        /// <summary>
        /// The tag name of a single dictionary item.
        /// </summary>
        private string tag;

        /// <summary>
        /// The attribute name of the items which acts as key.
        /// </summary>
        private string key;

        /// <summary>
        /// Initializes a new instance of the XmlDictionary class.
        /// </summary>
        /// <param name="element">The XML element which is the parant of the
        /// dictionary XML entries.</param>
        /// <param name="tag">The XML tag name of an entry.</param>
        /// <param name="key">The XML attribute of the entries whose value is used
        /// as the key into the dictionary.</param>
        /// <exception cref="ArgumentNullException">Any of the parameters are null.</exception>
        public XmlDictionary(XmlElement element, string tag, string key)
        {
            element.ThrowIfNull("element");
            tag.ThrowIfNull("tag");
            key.ThrowIfNull("key");
            this.element = element;
            this.tag = tag;
            this.key = key;
        }

        /// <summary>
        /// Adds an element with the provided key and value to the dictionary.
        /// </summary>
        /// <param name="key">The string to use as the key of the element to
        /// add.</param>
        /// <param name="value">The string to use as the value of the element
        /// to add.</param>
        /// <exception cref="ArgumentNullException">The key parameter is null.</exception>
        /// <exception cref="ArgumentException">An element with the same key
        /// already exists in the dictionary.</exception>
        /// <exception cref="NotSupportedException">The dictionary is
        /// read-only.</exception>
        public void Add(string key, string value)
        {
            key.ThrowIfNull("key");
            if (ContainsKey(key))
                throw new ArgumentException("An element with the same key already " +
                    "exists in the dictionary.");
            if (element.IsReadOnly)
                throw new NotSupportedException("The dictionary is read-only.");
            element.Child(Xml.Element(tag).Attr(this.key, key).Text(value));
        }

        /// <summary>
        /// Determines whether the dictionary contains an element with the specified
        /// key.
        /// </summary>
        /// <param name="key">The key to locate in the dictionary.</param>
        /// <returns>true if the dictionary contains an element with the key;
        /// otherwise false.</returns>
        /// <exception cref="ArgumentNullException">The key parameter is null.</exception>
        public bool ContainsKey(string key)
        {
            key.ThrowIfNull("key");
            return GetElement(key) != null;
        }

        /// <summary>
        /// Gets an enumerable collection containing the keys of the dictionary.
        /// </summary>
        public ICollection<string> Keys
        {
            get
            {
                ISet<string> set = new HashSet<string>();
                foreach (XmlElement e in element.GetElementsByTagName(tag))
                {
                    string k = e.GetAttribute(this.key);
                    if (!String.IsNullOrEmpty(k))
                        set.Add(k);
                }
                return set;
            }
        }

        /// <summary>
        /// Removes the element with the specified key from the dictionary.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>true if the element is successfully removed; otherwise, false.
        /// This method also returns false if key was not found in the original
        /// dictionary.</returns>
        /// <exception cref="ArgumentNullException">The key parameter is null.</exception>
        public bool Remove(string key)
        {
            key.ThrowIfNull("key");
            XmlElement e = GetElement(key);
            if (e != null)
                element.RemoveChild(e);
            return e != null;
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">When this method returns, the value associated with
        /// the specified key, if the key is found; otherwise, the default value for
        /// the type of the value parameter. This parameter is passed
        /// uninitialized.</param>
        /// <returns>true if the dictionary contains an element with the specified
        /// key; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">The key parameter is null.</exception>
        public bool TryGetValue(string key, out string value)
        {
            key.ThrowIfNull("key");
            XmlElement e = GetElement(key);
            value = e != null ? e.InnerText : null;
            return e != null;
        }

        /// <summary>
        /// Gets an enumerable collection containing the values in the dictionary.
        /// </summary>
        public ICollection<string> Values
        {
            get
            {
                ISet<string> set = new HashSet<string>();
                foreach (XmlElement e in element.GetElementsByTagName(tag))
                {
                    string k = e.GetAttribute(this.key);
                    if (!String.IsNullOrEmpty(k))
                        set.Add(e.InnerText);
                }
                return set;
            }
        }

        /// <summary>
        /// Gets or sets the element with the specified key.
        /// </summary>
        /// <param name="key">The key of the element to get or set.</param>
        /// <returns>The element with the specified key.</returns>
        /// <exception cref="ArgumentNullException">The key parameter is null.</exception>
        /// <exception cref="NotSupportedException">The property is set and the
        /// dictionary is read-only.</exception>
        public string this[string key]
        {
            get
            {
                key.ThrowIfNull("key");
                foreach (XmlElement e in element.GetElementsByTagName(tag))
                {
                    string k = e.GetAttribute(this.key);
                    if (k == key)
                        return e.InnerText;
                }
                return null;
            }

            set
            {
                key.ThrowIfNull("key");
                if (element.IsReadOnly)
                    throw new NotSupportedException("The dictionary is read-only.");
                XmlElement e = GetElement(key);
                if (e != null)
                    e.InnerText = value;
                else
                    element.Child(Xml.Element(tag).Attr(this.key, key).Text(value));
            }
        }

        /// <summary>
        /// Adds an item to the dictionary.
        /// </summary>
        /// <param name="item">The object to add to the dictionary.</param>
        /// <exception cref="ArgumentNullException">The key value of the specified
        /// item is null.</exception>
        /// <exception cref="ArgumentException">An element with the same key
        /// already exists in the dictionary.</exception>
        /// <exception cref="NotSupportedException">The dictionary is
        /// read-only.</exception>
        public void Add(KeyValuePair<string, string> item)
        {
            Add(item.Key, item.Value);
        }

        /// <summary>
        /// Removes all items from the dictionary.
        /// </summary>
        /// <exception cref="NotSupportedException">The dictionary is read-only.</exception>
        public void Clear()
        {
            if (element.IsReadOnly)
                throw new NotSupportedException("The dictionary is read-only.");
            ISet<XmlElement> set = new HashSet<XmlElement>();
            foreach (XmlElement e in element.GetElementsByTagName(tag))
            {
                string k = e.GetAttribute(key);
                if (!String.IsNullOrEmpty(k))
                    set.Add(e);
            }
            foreach (var e in set)
                element.RemoveChild(e);
        }

        /// <summary>
        /// Determines whether the dictionary contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the dictionary.</param>
        /// <returns>true if item is found in the dictionary; otherwise false.</returns>
        /// <exception cref="ArgumentNullException">The key value of the specified
        /// item is null.</exception>
        public bool Contains(KeyValuePair<string, string> item)
        {
            XmlElement e = GetElement(item.Key);
            if (e != null)
                return e.InnerText == item.Value;
            return false;
        }

        /// <summary>
        /// Copies the elements of the dictionary to an System.Array, starting
        /// at a particular System.Array index.
        /// </summary>
        /// <param name="array">The one-dimensional System.Array that is the
        /// destination of the elements copied from the dictionary. The System.Array
        /// must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying
        /// begins.</param>
        /// <exception cref="ArgumentNullException">The array parameter is null.</exception>
        /// <exception cref="IndexOutOfRangeException">The arrayIndex parameter
        /// is less than 0.</exception>
        /// <exception cref="ArgumentException">The number of elements in the
        /// source dictionary is greater than the available space from arrayIndex
        /// to the end of the destination array.</exception>
        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            array.ThrowIfNull("array");
            if (arrayIndex < 0)
                throw new IndexOutOfRangeException("arrayIndex");
            int size = array.Length - arrayIndex;
            if (Count > size)
                throw new ArgumentException();
            foreach (var pair in this)
                array[arrayIndex++] = new KeyValuePair<string, string>(pair.Key,
                    pair.Value);
        }

        /// <summary>
        /// Gets the number of elements contained in the dictionary.
        /// </summary>
        public int Count
        {
            get
            {
                return Keys.Count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the dictionary is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return element.IsReadOnly;
            }
        }

        /// <summary>
        /// Removes the specific object from the dictionary.
        /// </summary>
        /// <param name="item">The object to remove from the dictionary.</param>
        /// <returns>true if item was successfully removed from the dictionary;
        /// otherwise, false. This method also returns false if item is not found
        /// in the original dictionary.</returns>
        /// <exception cref="ArgumentNullException">The key value of the specified
        /// item is null.</exception>
        public bool Remove(KeyValuePair<string, string> item)
        {
            XmlElement e = GetElement(item.Key);
            if (e != null && e.InnerText == item.Value)
            {
                element.RemoveChild(e);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the dictionary.
        /// </summary>
        /// <returns>An iterator that can be used to iterate through the
        /// dictionary.</returns>
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            var dict = new Dictionary<string, string>();
            foreach (string k in Keys)
                dict.Add(k, this[k]);
            return dict.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the dictionary.
        /// </summary>
        /// <returns>An iterator that can be used to iterate through the
        /// dictionary.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Returns the XML element with an attribute value of the specified
        /// key.
        /// </summary>
        /// <param name="key">The key value of the XML element to
        /// locate.</param>
        /// <returns>The XML element with an attribute value equal to the
        /// specified key or null if no such element exists.</returns>
        /// <exception cref="ArgumentNullException">The key parameter is null.</exception>
        private XmlElement GetElement(string key)
        {
            key.ThrowIfNull("key");
            foreach (XmlElement e in element.GetElementsByTagName(tag))
            {
                string k = e.GetAttribute(this.key);
                if (k == key)
                    return e;
            }
            return null;
        }
    }
}