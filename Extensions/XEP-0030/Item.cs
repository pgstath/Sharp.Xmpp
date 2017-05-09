using System;

namespace XMPPEngineer.Extensions
{
    /// <summary>
    /// Represents an item of an XMPP entity as defined in XEP-0030.
    /// </summary>
    [Serializable]
    internal class Item
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
        /// The nickname of the item. This may be null.
        /// </summary>
        public string Nick
        {
            get;
            private set;
        }

        /// <summary>
        /// The affiliation of the item. This may be null.
        /// </summary>

        public string Affiliation
        {
            get;
            private set;
        }

        /// <summary>
        /// The role of the item. This may be null.
        /// </summary>

        public string Role
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
        public Item(Jid jid, string node = null, string name = null)
        {
            jid.ThrowIfNull("jid");
            Jid = jid;
            Node = node;
            Name = name;
            Affiliation = null;
            Nick = null;
            Role = null;
        }

        /// <summary>
        /// Initialised a new instance of the item class.
        /// This instance is used for member list creation and modification.
        /// </summary>
        /// <param name="affiliation">A long-lived association or connection with a room.</param>
        /// <param name="jid">JID</param>
        /// <param name="nickname">Occupant nickname</param>
        /// <param name="role">Privilege level within a room.</param>
        public Item(string affiliation, Jid jid, string nickname = null, string role = null)
        {
            affiliation.ThrowIfNull("affiliation");
            jid.ThrowIfNull("jid");
            Affiliation = affiliation;
            Jid = jid;
            Nick = nickname;
            Role = role;
            Node = null;
            Name = null;
        }

        /// <summary>
        /// Initialised a new instance of the item class.
        /// This instance is used for member list creation and modification.
        /// </summary>
        /// <param name="jid">JID</param>
        /// <param name="node">The node identifier of the item.</param>
        /// <param name="name">The name of the item.</param>
        /// <param name="nickname">Occupant nickname</param>
        /// <param name="role">Privilege level within a room.</param>
        /// <param name="affiliation">A long-lived association or connection with a room.</param>
        public Item(Jid jid, string node = null, string name = null, string nickname = null, string role = null, string affiliation = null)
        {
            jid.ThrowIfNull("jid");
            Jid = jid;
            Node = node;
            Name = name;
            Nick = nickname;
            Role = role;
            Affiliation = affiliation;
        }
    }
}