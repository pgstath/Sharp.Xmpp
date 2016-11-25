using System.Collections;
using System.Collections.Generic;

namespace Sharp.Xmpp.Im
{
    /// <summary>
    /// Represents the user's roster.
    /// </summary>
    /// <remarks>In XMPP jargon, the user's contact list is called a 'roster'.</remarks>
    public class Roster : IEnumerable<IRosterItem>, IRoster
    {
        /// <summary>
        /// The set of items that form the roster.
        /// </summary>
        private ISet<IRosterItem> items = new HashSet<IRosterItem>();

        /// <summary>
        /// The number of items on the roster.
        /// </summary>
        public int Count
        {
            get
            {
                return items.Count;
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection of
        /// roster items.
        /// </summary>
        /// <returns>An enumerator that iterates through the collection of
        /// roster items.</returns>
        public IEnumerator<IRosterItem> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection of
        /// roster items.
        /// </summary>
        /// <returns>An enumerator that iterates through the collection of
        /// roster items.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }

        /// <summary>
        /// Initializes a new instance of the Roster class.
        /// </summary>
        /// <param name="items">An enumerable collection of items to add to the
        /// roster.</param>
        internal Roster(IEnumerable<IRosterItem> items = null)
        {
            if (items != null)
            {
                foreach (IRosterItem s in items)
                    this.items.Add(s);
            }
        }

        /// <summary>
        /// Adds the specified item to the roster.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>True if the item was added to the collection of roster items;
        /// Otherwise false.</returns>
        internal bool Add(IRosterItem item)
        {
            return items.Add(item);
        }
    }
}