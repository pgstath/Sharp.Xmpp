using Sharp.Xmpp.Extensions;
using System;
using System.Collections.Generic;
using System.Net;

namespace Sharp.Xmpp.Client
{
    /// <summary>
    /// Contains settings for configuring various file-transfer options.
    /// </summary>
    public class FileTransferSettings : IFileTransferSettings
    {
        /// <summary>
        /// A reference to the Socks5Bytestreams extension.
        /// </summary>
        private Socks5Bytestreams socks5;

        /// <summary>
        /// A reference to the SIFileTransfer extension.
        /// </summary>
        private SIFileTransfer siFileTransfer;

        /// <summary>
        /// Determines whether usage of a SOCKS5 proxy server is allowed.
        /// </summary>
        public bool ProxyAllowed
        {
            get
            {
                return socks5.ProxyAllowed;
            }

            set
            {
                socks5.ProxyAllowed = value;
            }
        }

        /// <summary>
        /// A collection of user-defined SOCKS5 proxy servers.
        /// </summary>
        public ICollection<Streamhost> Proxies
        {
            get
            {
                return socks5.Proxies;
            }
        }

        /// <summary>
        /// Defines, along with the Socks5ServerPortTo property, a range of ports
        /// eligible for creating SOCKS5 servers on.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The property is being set
        /// and the value is negative, or the value is greater than the value of the
        /// Socks5ServerPortTo property.</exception>
        public int Socks5ServerPortFrom
        {
            get
            {
                return socks5.ServerPortFrom;
            }

            set
            {
                socks5.ServerPortFrom = value;
            }
        }

        /// <summary>
        /// Defines, along with the Socks5ServerPortFrom property, a range of ports
        /// eligible for creating SOCKS5 servers on.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The property is being set
        /// and the value the value is smaller than the value of the
        /// Socks5ServerPortFrom property, or the value is greater than
        /// 65535.</exception>
        public int Socks5ServerPortTo
        {
            get
            {
                return socks5.ServerPortTo;
            }

            set
            {
                socks5.ServerPortTo = value;
            }
        }

        /// <summary>
        /// Determines whether usage of UPnP for automatic port-forwarding is allowed.
        /// </summary>
        public bool UseUPnP
        {
            get
            {
                return socks5.UseUPnP;
            }

            set
            {
                socks5.UseUPnP = value;
            }
        }

        /// <summary>
        /// The STUN server to use for determining the external IP address of the
        /// XMPP client.
        /// </summary>
        /// <remarks>
        /// The default STUN server is "stun.l.google.com:19302".
        /// </remarks>
        public DnsEndPoint StunServer
        {
            get
            {
                return socks5.StunServer;
            }

            set
            {
                socks5.StunServer = value;
            }
        }

        /// <summary>
        /// Determines whether the in-band bytestreams method should be used, even if
        /// the preferred SOCKS5 method is available.
        /// </summary>
        public bool ForceInBandBytestreams
        {
            get
            {
                return siFileTransfer.ForceInBandBytestreams;
            }

            set
            {
                siFileTransfer.ForceInBandBytestreams = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the FileTransferSettings class.
        /// </summary>
        /// <param name="socks5">A reference to the Socks5Bytestreams
        /// instance.</param>
        /// <param name="siFileTransfer">A reference to the SIFileTransfer
        /// instance.</param>
        /// <exception cref="ArgumentNullException">The socks5 parameter is
        /// null.</exception>
        internal FileTransferSettings(Socks5Bytestreams socks5,
            SIFileTransfer siFileTransfer)
        {
            socks5.ThrowIfNull("socks5");
            siFileTransfer.ThrowIfNull("siFileTransfer");
            this.socks5 = socks5;
            this.siFileTransfer = siFileTransfer;
        }
    }
}