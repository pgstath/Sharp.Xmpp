using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace XMPPEngineer.Extensions.Dataforms
{
    /// <summary>
    /// Represents an XML-backed generic collection.
    /// </summary>
    /// <typeparam name="T">The type of the elements to store in the
    /// collection.</typeparam>
    /// <remarks>As an added type constraint, the specified type must
    /// override the ToString method to return an XML representation of
    /// the calling instance.</remarks>
    internal class XmlCollection<T> : ICollection<T> where T : class
    {
        /// <summary>
        /// The underlying XML element used for implementing the collection.
        /// </summary>
        private XmlElement element;

        /// <summary>
        /// The tag name of a single collection item.
        /// </summary>
        private string tag;

        /// <summary>
        /// A callback for converting an XML element into the specified type.
        /// </summary>
        private Func<XmlElement, T> conversion;

        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        public int Count
        {
            get
            {
                return GetElements().Count;
            }
        }

        /// <summary>
        /// Determines whether the collection is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return element.IsReadOnly;
            }
        }

        /// <summary>
        /// Adds the specified item to the collection.
        /// </summary>
        /// <param name="item">The item to add to the collection.</param>
        /// <exception cref="ArgumentNullException">The item parameter is
        /// null.</exception>
        /// <exception cref="ArgumentException">The specified item could not be
        /// serialized into XML.</exception>
        public void Add(T item)
        {
            item.ThrowIfNull("item");
            XmlElement elem = Xml.Element(tag);
            try
            {
                elem.InnerXml = item.ToString();
            }
            catch (XmlException e)
            {
                throw new ArgumentException("The specified item could not be serialized " +
                    "into XML.", e);
            }
            element.Child(elem);
        }

        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        public void Clear()
        {
            foreach (var e in GetElements())
                element.RemoveChild(e);
        }

        /// <summary>
        /// Determines whether the collection contains the specified item.
        /// </summary>
        /// <param name="item">The item to look for.</param>
        /// <returns>true if the collection contains the specified item; Otherwise
        /// false.</returns>
        /// <exception cref="ArgumentNullException">The item parameter is
        /// null.</exception>
        public bool Contains(T item)
        {
            item.ThrowIfNull("item");
            foreach (var i in GetItems())
            {
                if (item.Equals(i))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Copies the elements of the System.Collections.Generic.ICollection to an
        /// System.Array, starting at a particular System.Array index.
        /// </summary>
        /// <param name="array">The one-dimensional System.Array that is the destination
        /// of the elements copied from System.Collections.Generic.ICollection. The
        /// System.Array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying
        /// begins.</param>
        /// <exception cref="NotImplementedException">The method is not
        /// implemented.</exception>
        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Remove the specified item from the collection.
        /// </summary>
        /// <param name="item">The item to remove from the collection.</param>
        /// <returns>true if the item was removed from the collection; Otherwise
        /// false. This method also returns false if the specified item is not
        /// contained in the collection.</returns>
        /// <exception cref="ArgumentNullException">The item parameter is
        /// null.</exception>
        public bool Remove(T item)
        {
            item.ThrowIfNull("item");
            XmlElement elem = null;
            foreach (var e in GetElements())
            {
                if (e.InnerText == item.ToString())
                    elem = e;
            }
            if (elem == null)
                return false;
            element.RemoveChild(elem);
            return true;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the XmlCollection instance.
        /// </summary>
        /// <returns>An enumerator that iterates through the XmlCollection
        /// instance.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return GetItems().GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the XmlCollection instance.
        /// </summary>
        /// <returns>An enumerator that iterates through the XmlCollection
        /// instance.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Returns a textual XML representation of the collection.
        /// </summary>
        /// <returns>A textual XML representation of the collection.</returns>
        public override string ToString()
        {
            return element.ToXmlString();
        }

        /// <summary>
        /// Initializes a new instance of the XmlCollection class from the specified
        /// XML element.
        /// </summary>
        /// <param name="element">The XML element to initialize this instance
        /// from.</param>
        /// <param name="tag">The XML tag name of an entry.</param>
        /// <param name="conversion">A callback method which is invoked to convert
        /// an XML element into an instance of the specified type.</param>
        /// <exception cref="ArgumentNullException">The element parameter or the tag
        /// parameter or the conversion parameter is null.</exception>
        public XmlCollection(XmlElement element, string tag, Func<XmlElement, T> conversion)
        {
            element.ThrowIfNull("element");
            tag.ThrowIfNull("tag");
            conversion.ThrowIfNull("conversion");
            this.element = element;
            this.tag = tag;
            this.conversion = conversion;
            // Call GetItems method to verify each item in the collection can be
            // converted into an instance of the requested type.
            try
            {
                GetItems();
            }
            catch (Exception e)
            {
                throw new ArgumentException("The specified element contained invalid " +
                    "data.", e);
            }
        }

        /// <summary>
        /// Returns an enumerable collection of XML elements contained in the collection.
        /// </summary>
        /// <returns>An enumerable collection of XML elements.</returns>
        private ICollection<XmlElement> GetElements()
        {
            ISet<XmlElement> set = new HashSet<XmlElement>();
            foreach (XmlElement e in element.GetElementsByTagName(tag))
                if (e.ParentNode == element)
                    set.Add(e);
            return set;
        }

        /// <summary>
        /// Returns an enumerable collection of items contained in the collection.
        /// </summary>
        /// <returns>An enumerable collection of items.</returns>
        /// <exception cref="XmlException">An XML element in the collection could
        /// not be converted into an instance of the specified type.</exception>
        private ICollection<T> GetItems()
        {
            ISet<T> set = new HashSet<T>();
            foreach (XmlElement e in element.GetElementsByTagName(tag))
            {
                if (e.ParentNode == element)
                {
                    try
                    {
                        set.Add(conversion(e));
                    }
                    catch (Exception ex)
                    {
                        throw new XmlException("Could not convert XML element into an " +
                            "instance of type " + typeof(T) + ".", ex);
                    }
                }
            }
            return set;
        }
    }
}