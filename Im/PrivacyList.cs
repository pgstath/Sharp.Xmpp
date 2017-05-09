using System;
using System.Collections;
using System.Collections.Generic;

namespace XMPPEngineer.Im
{
    /// <summary>
    /// Represents an XMPP privacy list.
    /// </summary>
    public class PrivacyList : ICollection<PrivacyRule>
    {
        /// <summary>
        /// The set of rules that make up the privacy list.
        /// </summary>
        private ISet<PrivacyRule> rules = new HashSet<PrivacyRule>();

        /// <summary>
        /// The name of the privacy list.
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the number of elements contained in the privacy list.
        /// </summary>
        public int Count
        {
            get
            {
                return rules.Count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the privacy list is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return rules.IsReadOnly;
            }
        }

        /// <summary>
        /// Initializes a new instance of the PrivacyList class.
        /// </summary>
        /// <param name="name">The name of the privacy list.</param>
        /// <exception cref="ArgumentNullException">The name parameter is null.</exception>
        public PrivacyList(string name)
        {
            name.ThrowIfNull("name");
            Name = name;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the privacy list.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the privacy
        /// list.</returns>
        public IEnumerator<PrivacyRule> GetEnumerator()
        {
            return rules.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the privacy list.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the privacy
        /// list.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return rules.GetEnumerator();
        }

        /// <summary>
        /// Adds the specified privacy rule to the privacy list.
        /// </summary>
        /// <param name="item">The item to add to the list.</param>
        /// <exception cref="ArgumentNullException">The item parameter is null.</exception>
        /// <exception cref="ArgumentException">The privacy list already contains
        /// a rule with the same order value as the item specified.</exception>
        public void Add(PrivacyRule item)
        {
            item.ThrowIfNull("item");
            foreach (PrivacyRule rule in rules)
            {
                if (rule.Order == item.Order)
                    throw new ArgumentException("A rule with an order value of " +
                        rule.Order + " already exists.");
            }
            rules.Add(item);
        }

        /// <summary>
        /// Adds the specified privacy rule to the privacy list.
        /// </summary>
        /// <param name="item">The item to add to the list.</param>
        /// <param name="overWriteOrder">If true, the order value of the privacy-rule
        /// may be overwritten, if a rule with the same order value already
        /// exists.</param>
        /// <returns>The possibly modified order value of the privacy-rule.</returns>
        /// <exception cref="ArgumentNullException">The item parameter is null.</exception>
        public uint Add(PrivacyRule item, bool overWriteOrder)
        {
            item.ThrowIfNull("item");
            uint? highest = null;
            foreach (PrivacyRule rule in rules)
            {
                if (!overWriteOrder && rule.Order == item.Order)
                    throw new ArgumentException("A rule with an order value of " +
                        rule.Order + " already exists.");
                if (!highest.HasValue)
                    highest = rule.Order;
                if (rule.Order > highest)
                    highest = rule.Order;
            }
            if (highest.HasValue)
                item.Order = highest.Value + 1;
            rules.Add(item);
            return item.Order;
        }

        /// <summary>
        /// Removes all rules from the privacy list.
        /// </summary>
        public void Clear()
        {
            rules.Clear();
        }

        /// <summary>
        /// Determines whether the privacy list contains the specified privacy rule.
        /// </summary>
        /// <param name="item">The privacy rule to locate.</param>
        /// <returns>true if the privacy rule is contained in the privacy list;
        /// Otherwise false.</returns>
        /// <exception cref="ArgumentNullException">The item parameter is null.</exception>
        public bool Contains(PrivacyRule item)
        {
            item.ThrowIfNull("item");
            return rules.Contains(item);
        }

        /// <summary>
        /// Copies the elements of the privacy list to the specified array, starting
        /// at the specified index.
        /// </summary>
        /// <param name="array">The one-dimensional System.Array that is the destination
        /// of the elements copied from the privacy list.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying
        /// begins.</param>
        /// <exception cref="ArgumentNullException">The array parameter is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The arrayIndex parameter is
        /// less than 0.</exception>
        /// <exception cref="ArgumentException">The number of elements in
        /// the source collection is greater than the available space from arrayIndex
        /// to the end of the destination array.</exception>
        public void CopyTo(PrivacyRule[] array, int arrayIndex)
        {
            array.ThrowIfNull("array");
            rules.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Removes the specified privacy rule from the privacy list.
        /// </summary>
        /// <param name="item">The privacy rule to remove.</param>
        /// <returns>true if item was successfully removed from the privacy list;
        /// Otherwise false. This method also returns false if item is not found in
        /// the original privacy list.</returns>
        /// <exception cref="ArgumentNullException">The item parameter is
        /// null.</exception>
        /// <exception cref="NotSupportedException">The privacy list is
        /// read-only.</exception>
        public bool Remove(PrivacyRule item)
        {
            item.ThrowIfNull("item");
            return rules.Remove(item);
        }
    }
}