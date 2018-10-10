using System;
using System.Text;
using System.Text.RegularExpressions;

namespace XMPPEngineer
{
    /// <summary>
    /// Represents the address of an XMPP entity, also known as Jabber Identifier (JID).
    /// </summary>
    [Serializable]
    public sealed class Jid
    {
        /// <summary>
        /// The domain identifier of the JID.
        /// </summary>
        public string Domain
        {
            get;
            private set;
        }

        /// <summary>
        /// The node identifier of the JID. This may be null or empty.
        /// </summary>
        public string Node
        {
            get;
            private set;
        }

        /// <summary>
        /// The resource identifier of the JID. This may be null or empty.
        /// </summary>
        public string Resource
        {
            get;
            private set;
        }

        /// <summary>
        /// Determines whether the JID is a 'bare JID', i.e. a JID without resource
        /// identifier.
        /// </summary>
        public bool IsBareJid
        {
            get
            {
                return !String.IsNullOrEmpty(Node) &&
                    !String.IsNullOrEmpty(Domain) && String.IsNullOrEmpty(Resource);
            }
        }

        /// <summary>
        /// Determines whether the JID is a 'full JID', i.e. a JID with both a node
        /// and a resource identifier.
        /// </summary>
        public bool IsFullJid
        {
            get
            {
                return !String.IsNullOrEmpty(Node) &&
                    !String.IsNullOrEmpty(Domain) && !String.IsNullOrEmpty(Resource);
            }
        }

        /// <summary>
        /// Initializes a new instance of the JID class.
        /// </summary>
        /// <param name="jid">A string from which to construct the JID.</param>
        /// <exception cref="ArgumentNullException">The jid parameter is
        /// null.</exception>
        /// <exception cref="ArgumentException">The jid parameter does not
        /// represent a valid JID.</exception>
        public Jid(string jid)
        {
            jid.ThrowIfNullOrEmpty("jid");
            Match m = Regex.Match(jid,
                "(?:(?<node>[^@]+)@)?(?<domain>[^/]+)(?:/(?<resource>.+))?");
            if (!m.Success)
                throw new ArgumentException("The argument is not a valid JID.");
            Domain = m.Groups["domain"].Value;
            Node = m.Groups["node"].Value;
            if (Node == String.Empty)
                Node = null;
            Resource = m.Groups["resource"].Value;
            if (Resource == String.Empty)
                Resource = null;
        }

        /// <summary>
        /// Initializes a new instance of the JID class using the specified domain,
        /// node and optionally resource.
        /// </summary>
        /// <param name="domain">The domain of the JID.</param>
        /// <param name="node">The node of the JID.</param>
        /// <param name="resource">The resource of the JID. This may be omitted.</param>
        /// <exception cref="ArgumentNullException">The domain parameter is null.</exception>
        /// <exception cref="ArgumentException">The domain parameter is the
        /// empty string.</exception>
        public Jid(string domain, string node, string resource = null)
        {
            domain.ThrowIfNullOrEmpty("domain");
            Domain = domain;
            Node = node;
            Resource = resource;
        }

        /// <summary>
        /// Implicit conversion operator for type string to type Jid.
        /// </summary>
        /// <param name="jid">The string to convert into a Jid instance.</param>
        /// <returns>A Jid instance created from the specified string.</returns>
        public static implicit operator Jid(string jid)
        {
            return new Jid(jid);
        }

        /// <summary>
        /// Returns a new JID instance representing the 'bare JID' constructd from
        /// this JID.
        /// </summary>
        /// <returns>A bare JID constructed from this JID instance.</returns>
        public Jid GetBareJid()
        {
            return new Jid(Domain, Node);
        }

        /// <summary>
        /// Returns a textual representation of the JID.
        /// </summary>
        /// <returns>A textual representation of this JID instance.</returns>
        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            if (!String.IsNullOrEmpty(Node))
                b.Append(Node + "@");
            b.Append(Domain);
            if (!String.IsNullOrEmpty(Resource))
                b.Append("/" + Resource);
            return b.ToString();
        }

        /// <summary>
        /// Determines whether the specified object is equal to this Jid
        /// instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>True if the specified object is semantically equal to this
        /// Jid instance; Otherwise false.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            Jid other = obj as Jid;
            if (other == null)
                return false;
            return Node == other.Node && Domain == other.Domain &&
                Resource == other.Resource;
        }

        /// <summary>
        /// Returns the hash code of this instance.
        /// </summary>
        /// <returns>The hash code of this JID instance.</returns>
        public override int GetHashCode()
        {
            int hash = 13;
            if (Node != null)
                hash = (hash * 7) + Node.GetHashCode();
            hash = (hash * 7) + Domain.GetHashCode();
            if (Resource != null)
                hash = (hash * 7) + Resource.GetHashCode();
            return hash;
        }

        /// <summary>
        /// Determines whether the specified Jid objects are equal.
        /// </summary>
        /// <param name="a">The first object.</param>
        /// <param name="b">The second object.</param>
        /// <returns>True if the specified objects are semantically equal;
        /// Otherwise false.</returns>
        public static bool operator ==(Jid a, Jid b)
        {
            if (System.Object.ReferenceEquals(a, b))
                return true;
            if (((object)a == null) || ((object)b == null))
                return false;
            return a.Node == b.Node && a.Domain == b.Domain &&
                a.Resource == b.Resource;
        }

        /// <summary>
        /// Determines whether the specified Jid objects are unequal.
        /// </summary>
        /// <param name="a">The first object.</param>
        /// <param name="b">The second object.</param>
        /// <returns>True if the specified objects are not semantically equal;
        /// Otherwise false.</returns>
        public static bool operator !=(Jid a, Jid b)
        {
            return !(a == b);
        }
    }
}