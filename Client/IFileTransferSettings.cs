using System.Collections.Generic;
using System.Net;
using Sharp.Xmpp.Extensions;

namespace Sharp.Xmpp.Client
{
    public interface IFileTransferSettings
    {
        /// <summary>
        /// Determines whether the in-band bytestreams method should be used, even if
        /// the preferred SOCKS5 method is available.
        /// </summary>
        bool ForceInBandBytestreams { get; set; }

        /// <summary>
        /// A collection of user-defined SOCKS5 proxy servers.
        /// </summary>
        ICollection<Streamhost> Proxies { get; }

        /// <summary>
        /// Determines whether usage of a SOCKS5 proxy server is allowed.
        /// </summary>
        bool ProxyAllowed { get; set; }

        /// <summary>
        /// Defines, along with the Socks5ServerPortTo property, a range of ports
        /// eligible for creating SOCKS5 servers on.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The property is being set
        /// and the value is negative, or the value is greater than the value of the
        /// Socks5ServerPortTo property.</exception>
        int Socks5ServerPortFrom { get; set; }

        /// <summary>
        /// Defines, along with the Socks5ServerPortFrom property, a range of ports
        /// eligible for creating SOCKS5 servers on.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The property is being set
        /// and the value the value is smaller than the value of the
        /// Socks5ServerPortFrom property, or the value is greater than
        /// 65535.</exception>
        int Socks5ServerPortTo { get; set; }

        /// <summary>
        /// The STUN server to use for determining the external IP address of the
        /// XMPP client.
        /// </summary>
        /// <remarks>
        /// The default STUN server is "stun.l.google.com:19302".
        /// </remarks>
        DnsEndPoint StunServer { get; set; }

        /// <summary>
        /// Determines whether usage of UPnP for automatic port-forwarding is allowed.
        /// </summary>
        bool UseUPnP { get; set; }
    }
}