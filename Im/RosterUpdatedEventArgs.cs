using System;

namespace Sharp.Xmpp.Im
{
    /// <summary>
    /// Provides data for the RosterUpdated Event.
    /// </summary>
    public class RosterUpdatedEventArgs : EventArgs
    {
        /// <summary>
        /// The contact that has been added to the user's roster.
        /// </summary>
        public IRosterItem Item
        {
            get;
            private set;
        }

        /// <summary>
        /// Determines whether the item has been removed from the roster or
        /// added/updated.
        /// </summary>
        public bool Removed
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the RosterUpdatedEventArgs class.
        /// </summary>
        /// <param name="item">The item the RosterUpdated event is being raised
        /// for.</param>
        /// <param name="removed">Set to true if the item is being removed from
        /// the roster.</param>
        /// <exception cref="ArgumentNullException">The item parameter is null.</exception>
        public RosterUpdatedEventArgs(IRosterItem item, bool removed)
        {
            item.ThrowIfNull("item");
            Item = item;
            Removed = removed;
        }
    }
}