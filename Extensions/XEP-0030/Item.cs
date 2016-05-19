using System;

namespace Sharp.Xmpp.Extensions
{
    /// <summary>
    /// Represents an item of an XMPP entity as defined in XEP-0030.
    /// </summary>
    [Serializable]
    public class XmppItem
    {
        /// <summary>
        /// The JID of the item.
        /// </summary>
        public Jid Jid
        {
            get;
            private set;
        }

        /// <summary>
        /// The node identifier of the item. This may be null.
        /// </summary>
        public string Node
        {
            get;
            private set;
        }

        /// <summary>
        /// The name of the item. This may be null.
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the Item class.
        /// </summary>
        /// <param name="jid">The JID of the item.</param>
        /// <param name="node">The node identifier of the item.</param>
        /// <param name="name">The name of the item.</param>
        /// <exception cref="ArgumentNullException">The jid parameter is
        /// null.</exception>
        public XmppItem(Jid jid, string node = null, string name = null)
        {
            jid.ThrowIfNull("jid");
            Jid = jid;
            Node = node;
            Name = name;
        }
    }
}