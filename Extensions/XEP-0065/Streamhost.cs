using System;

namespace XMPPEngineer.Extensions
{
    /// <summary>
    /// Represents a streamhost entry as is used during S5B negotiation.
    /// </summary>
    [Serializable]
    public class Streamhost
    {
        /// <summary>
        /// The JID of the streamhost.
        /// </summary>
        public Jid Jid
        {
            get;
            private set;
        }

        /// <summary>
        /// The hostname of the streamhost.
        /// </summary>
        public string Host
        {
            get;
            private set;
        }

        /// <summary>
        /// The port on which the streamhost is listening.
        /// </summary>
        public int Port
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the Streamhost class.
        /// </summary>
        /// <param name="jid">The JID of the streamhost.</param>
        /// <param name="host">The hostname of the streamhost.</param>
        /// <param name="port">The port on which the streamhost is accepting
        /// connections.</param>
        /// <exception cref="ArgumentNullException">The jid parameter or the
        /// host parameter is null.</exception>
        /// <exception cref="ArgumentException">The host parameter is the empty
        /// string.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The port parameter is
        /// not between 0 and 65535.</exception>
        public Streamhost(Jid jid, string host, int port)
        {
            jid.ThrowIfNull("jid");
            host.ThrowIfNullOrEmpty("host");
            port.ThrowIfOutOfRange("port", 0, 65535);
            Jid = jid;
            Host = host;
            Port = port;
        }
    }
}