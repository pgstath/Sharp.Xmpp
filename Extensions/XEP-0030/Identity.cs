using System;
using System.Collections.Generic;

namespace XMPPEngineer.Extensions
{
    /// <summary>
    /// Represents an identity of an XMPP entity as defined in XEP-0030.
    /// </summary>
    [Serializable]
    internal class Identity
    {
        /// <summary>
        /// The category the XMPP entity belongs to (i.e. 'client', 'server').
        /// </summary>
        public string Category
        {
            get;
            private set;
        }

        /// <summary>
        /// The type of the XMPP entity (i.e. 'pc', 'mobile', etc.).
        /// </summary>
        public string Type
        {
            get;
            private set;
        }

        /// <summary>
        /// The name of the XMPP entity.
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the Identity class.
        /// </summary>
        /// <param name="category">The category to which the XMPP entity belongs.</param>
        /// <param name="type">The type of the XMPP entity.</param>
        /// <param name="name">The name of the XMPP entity.</param>
        /// <exception cref="ArgumentNullException">The category parameter or the
        /// type parameter is null.</exception>
        public Identity(string category, string type, string name = null)
        {
            category.ThrowIfNull("category");
            type.ThrowIfNull("type");
            Category = category;
            Type = type;
            Name = name;
        }
    }
}