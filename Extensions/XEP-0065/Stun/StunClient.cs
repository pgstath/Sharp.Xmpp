using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;

namespace XMPPEngineer.Extensions.Stun
{
    /// <summary>
    /// Implements a rudimentary STUN client for allowing an end host to discover
    /// its public IP address if it is located behind a NAT.
    /// </summary>
    /// <remarks>For details on STUN, refer to RFC 5389.</remarks>
    internal static class StunClient
    {
        /// <summary>
        /// The initial retransmission timeout for UDP transmissions, in milliseconds.
        /// </summary>
        private const int initialRto = 500;

        /// <summary>
        /// The maximum number of retransmissions after which to give up.
        /// </summary>
        private const int rc = 7;

        /// <summary>
        /// Multiplier for last timeout value.
        /// </summary>
        private const int rm = 16;

        /// <summary>
        /// The Winsock error code for a connection timeout.
        /// </summary>
        private const int connectionTimeout = 10060;

        /// <summary>
        /// Queries the STUN server with the specified hostname for the public IP
        /// address of the requesting host.
        /// </summary>
        /// <param name="host">The hostname or IP address of the STUN server to
        /// query.</param>
        /// <param name="port">The port on which the STUN service is running at
        /// the specified host.</param>
        /// <param name="timeout">The maximum number of milliseconds to wait for
        /// a server response before returning to the caller.</param>
        /// <returns>The public IP address of the requesting host.</returns>
        /// <exception cref="ArgumentNullException">The host parameter is
        /// null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The port is not between
        /// 0 and 65535.</exception>
        /// <exception cref="ProtocolViolationException">The specified STUN
        /// server returned an erroneous response.</exception>
        /// <exception cref="SocketException">The specified hostname could not be
        /// resolved, or an error occurred while sending the request or while
        /// retrieving the response, or the specified STUN server could not be
        /// reached.</exception>
        /// <exception cref="TimeoutException">The specified timeout has
        /// expired.</exception>
        public static IPAddress Query(string host, int port = 3478,
            int timeout = Int32.MaxValue)
        {
            host.ThrowIfNull("host");
            port.ThrowIfOutOfRange("port", 0, 65535);
            var addresses = Dns.GetHostAddresses(host);
            // Prefer IPv4 addresses if any.
            var ipAddress = addresses.FirstOrDefault(
                a => a.AddressFamily == AddressFamily.InterNetwork);
            if (ipAddress == null)
                ipAddress = addresses[0];
            return Query(ipAddress, port, timeout);
        }

        /// <summary>
        /// Queries the STUN server with the specified IP address for the public IP
        /// address of the requesting host.
        /// </summary>
        /// <param name="address">The IP address of the STUN server to query.</param>
        /// <param name="port">The port on which the STUN service is running at
        /// the specified host.</param>
        /// <param name="timeout">The maximum number of milliseconds to wait for
        /// a server response before returning to the caller.</param>
        /// <returns>The public IP address of the requesting host.</returns>
        /// <exception cref="ArgumentNullException">The address parameter is
        /// null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The port is not between
        /// 0 and 65535.</exception>
        /// <exception cref="ProtocolViolationException">The specified STUN
        /// server returned an erroneous response.</exception>
        /// <exception cref="SocketException">The specified hostname could not be
        /// resolved, or an error occurred while sending the request or while
        /// retrieving the response, or the specified STUN server could not be
        /// reached.</exception>
        /// <exception cref="TimeoutException">The specified timeout has
        /// expired.</exception>
        public static IPAddress Query(IPAddress address, int port = 3478,
            int timeout = Int32.MaxValue)
        {
            address.ThrowIfNull("address");
            port.ThrowIfOutOfRange("port", 0, 65535);
            IPEndPoint IpEp = new IPEndPoint(address, port);
            var request = new BindingRequest().Serialize();
            int rto = initialRto;
            using (UdpClient udp = new UdpClient())
            {
                // The timeout mechanism is similar to TCP. For details,
                // refer to RFC 5389, Section 7.2.1. Sending over UDP.
                for (int tries = 0; tries < rc; tries++)
                {
                    // Transmit the datagram.
                    udp.Send(request, request.Length, IpEp);
                    // Set the timeout value on the socket.
                    udp.Client.ReceiveTimeout = rto;
                    try
                    {
                        byte[] datagram = udp.Receive(ref IpEp);
                        return BindingResponse.Deserialize(datagram).Address;
                    }
                    catch (SocketException e)
                    {
                        if (e.ErrorCode != connectionTimeout)
                            throw;
                        timeout = timeout - rto;
                        if (timeout <= 0)
                            throw new TimeoutException("The timeout has expired.");
                    }
                    catch (SerializationException)
                    {
                        throw new ProtocolViolationException("The STUN " +
                            "Binding Response is invalid.");
                    }
                    // Increase the timeout value.
                    if (tries < (rc - 1))
                        rto = rto * 2;
                    else
                        rto = initialRto * rm;
                    if (timeout < rto)
                        rto = timeout;
                }
                // Give up.
                throw new SocketException(connectionTimeout);
            }
        }
    }
}