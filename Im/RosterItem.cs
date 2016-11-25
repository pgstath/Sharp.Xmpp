using System;
using System.Collections.Generic;

namespace Sharp.Xmpp.Im
{
    /// <summary>
    /// Represents a roster item.
    /// </summary>
    /// <remarks>In XMPP jargon, the user's contact list is called a 'roster'.</remarks>
    public class RosterItem : IRosterItem
    {
        /// <summary>
        /// The groups this roster item is part of.
        /// </summary>
        private ISet<string> groups = new HashSet<string>();

        /// <summary>
        /// The JID of the user this item is associated with.
        /// </summary>
        public Jid Jid
        {
            get;
            private set;
        }

        /// <summary>
        /// The nickname associated with the JID. This may be null.
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// The groups or categories this item is part of.
        /// </summary>
        public IEnumerable<string> Groups
        {
            get
            {
                return groups;
            }
        }

        /// <summary>
        /// The subscription state of this item.
        /// </summary>
        public SubscriptionState SubscriptionState
        {
            get;
            private set;
        }

        /// <summary>
        /// Determines whether the user has sent a subscription request and is
        /// awaiting approval or refusal from the contact.
        /// </summary>
        public bool Pending
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the RosterItem class.
        /// </summary>
        /// <param name="jid">The JID of the user this item will be associated
        /// with.</param>
        /// <param name="name">The nickname with which to associate the JID.</param>
        /// <param name="groups">An array of groups or categories this roster item
        /// will be added to.</param>
        /// <exception cref="ArgumentNullException">The jid parameter is null.</exception>
        public RosterItem(Jid jid, string name = null, params string[] groups)
            : this(jid, name, SubscriptionState.None, false, groups)
        {
        }

        /// <summary>
        /// Initializes a new instance of the RosterItem class.
        /// </summary>
        /// <param name="jid">The JID of the user this item will be associated
        /// with.</param>
        /// <param name="name">The nickname with which to associate the JID.</param>
        /// <param name="state">One of the values from the SubscriptionState
        /// enumeration.</param>
        /// <param name="pending">True if the user has requested subscription but
        /// has not received the contact's response.</param>
        /// <param name="groups">An enumerable collection of groups to categorize
        /// this item in.</param>
        /// <exception cref="ArgumentNullException">The jid parameter is null.</exception>
        internal RosterItem(Jid jid, string name, SubscriptionState state,
            bool pending, IEnumerable<string> groups)
        {
            jid.ThrowIfNull("jid");
            Jid = jid;
            Name = name;
            if (groups != null)
            {
                foreach (string s in groups)
                {
                    if (String.IsNullOrEmpty(s))
                        continue;
                    this.groups.Add(s);
                }
            }
            SubscriptionState = state;
            Pending = pending;
        }
    }
}