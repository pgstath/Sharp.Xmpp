using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace XMPPEngineer.Extensions.Socks5
{
    /// <summary>
    /// Listens for connections from SOCKS5 network clients.
    /// </summary>
    /// <remarks>This is specifically tailored to our needs for XMPP and is not
    /// a full-blown SOCKS5 implementation.</remarks>
    internal class Socks5Server : IDisposable
    {
        /// <summary>
        /// The underlying TCP listener.
        /// </summary>
        private TcpListener listener;

        /// <summary>
        /// The client connection.
        /// </summary>
        private TcpClient client;

        /// <summary>
        /// The underlying network-stream instance.
        /// </summary>
        private NetworkStream stream;

        /// <summary>
        /// Determines whether the instance has been disposed.
        /// </summary>
        private bool disposed;

        /// <summary>
        /// The number of milliseconds to wait on a blocking receive before returning.
        /// </summary>
        private const int receiveTimeout = 1000 * 10;

        /// <summary>
        /// The port to which the SOCKS5 server is bound.
        /// </summary>
        public int Port
        {
            get;
            private set;
        }

        /// <summary>
        /// Accepts a pending connection request and subsequently blocks
        /// until a SOCKS5 request has been made over the accepted connection.
        /// </summary>
        /// <param name="timeout">The number of milliseconds to wait for a
        /// client connection before returning, or -1 to wait indefinitely.</param>
        /// <returns>The received SOCKS5 request.</returns>
        /// <exception cref="SocketException">An error occurred when accessing the
        /// socket.</exception>
        /// <exception cref="Socks5Exception">The connected client sent invalid
        /// data.</exception>
        /// <exception cref="TimeoutException">A timeout was specified and it
        /// expired.</exception>
        /// <exception cref="IOException">The stream could not be read, or the
        /// operation timed out.</exception>
        public SocksRequest Accept(int timeout = -1)
        {
            AssertValid();
            // Wait for the client to establish a connection.
            client = listener.AcceptTcpClient(timeout);
            // Setup the receive timeout.
            client.ReceiveTimeout = receiveTimeout;
            // Initialize the SOCKS5 connection.
            InitializeConnection();
            // Wait for a SOCKS5 connect request from the client.
            return WaitForRequest();
        }

        /// <summary>
        /// Sends the specified SOCKS5 reply to the connected client.
        /// </summary>
        /// <param name="status">The status code of the reply.</param>
        /// <param name="address">The address sent as part of the reply.</param>
        /// <param name="port">The port sent as part of the reply.</param>
        /// <exception cref="ArgumentNullException">The domain parameter is
        /// null.</exception>
        /// <exception cref="ObjectDisposedException">The object has been
        /// disposed.</exception>
        public void Reply(ReplyStatus status, IPAddress address, ushort port)
        {
            Reply(new SocksReply(status, address, port));
        }

        /// <summary>
        /// Sends the specified SOCKS5 reply to the connected client.
        /// </summary>
        /// <param name="status">The status code of the reply.</param>
        /// <param name="domain">The domain sent as part of the reply.</param>
        /// <param name="port">The port sent as part of the reply.</param>
        /// <exception cref="ArgumentNullException">The domain parameter is
        /// null.</exception>
        /// <exception cref="ObjectDisposedException">The object has been
        /// disposed.</exception>
        public void Reply(ReplyStatus status, string domain, ushort port)
        {
            Reply(new SocksReply(status, domain, port));
        }

        /// <summary>
        /// Sends the specified SOCKS5 reply to the connected client.
        /// </summary>
        /// <param name="reply">The SOCKS5 reply to send to the connected
        /// client.</param>
        /// <exception cref="ArgumentNullException">The reply parameter is
        /// null.</exception>
        /// <exception cref="ObjectDisposedException">The object has been
        /// disposed.</exception>
        public void Reply(SocksReply reply)
        {
            reply.ThrowIfNull("reply");
            AssertValid();
            var bytes = reply.Serialize();
            stream.Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Returns the NetworkStream used to send and receive data.
        /// </summary>
        /// <returns>The underlying NetworkStream.</returns>
        /// <exception cref="ObjectDisposedException">The object has been
        /// disposed.</exception>
        public NetworkStream GetStream()
        {
            AssertValid();
            return stream;
        }

        /// <summary>
        /// Initializes a new instance of the Socks5Server class.
        /// </summary>
        /// <param name="port">The port on which to listen for incoming client
        /// connections.</param>
        /// <param name="localaddress">The IP address of the network interface
        /// to listen on for incoming connections. If this is null connections
        /// from all interfaces will be accepted.</param>
        /// <exception cref="SocketException">An error occurred when accessing the
        /// socket.</exception>
        public Socks5Server(int port, IPAddress localaddress = null)
        {
            if (localaddress == null)
                localaddress = IPAddress.Any;
            listener = new TcpListener(localaddress, port);
            listener.Start();
            Port = port;
        }

        /// <summary>
        /// Closes and disposes of the SOCKS5 server instance.
        /// </summary>
        public void Close()
        {
            Dispose();
        }

        /// <summary>
        /// Releases all resources used by the current instance of the Socks5Server class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources used by the current instance of the Socks5Server
        /// class, optionally disposing of managed resource.
        /// </summary>
        /// <param name="disposing">true to dispose of managed resources, otherwise
        /// false.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                // Indicate that the instance has been disposed.
                disposed = true;
                // Get rid of managed resources.
                if (disposing)
                {
                    if (stream != null)
                        stream.Dispose();
                    stream = null;
                    if (client != null)
                        client.Close();
                    client = null;
                }
                // Get rid of unmanaged resources.
            }
        }

        /// <summary>
        /// Initializes the connection to the SOCKS5 client.
        /// </summary>
        /// <exception cref="Socks5Exception">The client sent invalid data, or
        /// requires authentication.</exception>
        /// <exception cref="IOException">The stream could not be read, or the
        /// operation timed out.</exception>
        private void InitializeConnection()
        {
            stream = client.GetStream();
            // Read the client's greeting message.
            PerformGreeting();
        }

        /// <summary>
        /// Performs the initial greeting.
        /// </summary>
        /// <exception cref="Socks5Exception">The client sent invalid data, or
        /// requires authentication.</exception>
        /// <exception cref="IOException">The stream could not be read, or the
        /// operation timed out.</exception>
        private void PerformGreeting()
        {
            ByteBuilder b = new ByteBuilder();
            using (var r = new BinaryReader(stream, Encoding.UTF8, true))
            {
                byte[] bytes = r.ReadBytes(2);
                b.Append(bytes);
                // The number of method-bytes following is contained in the second byte.
                b.Append(r.ReadBytes(bytes[1]));
            }
            ClientGreeting greeting = ClientGreeting.Deserialize(b.ToArray());
            // We only accept an authentication method of 'none'.
            if (!greeting.Methods.Contains(AuthMethod.None))
            {
                Dispose();
                throw new Socks5Exception("Client requires authentication.");
            }
            // Send back our greeting response.
            var response = new ServerGreeting(AuthMethod.None).Serialize();
            stream.Write(response, 0, response.Length);
        }

        /// <summary>
        /// Waits until the connected client sends a SOCKS5 request.
        /// </summary>
        /// <returns>The SOCKS5 request sent by the client.</returns>
        /// <exception cref="Socks5Exception">The data sent by the client
        /// is not a valid SOCKS5 request.</exception>
        private SocksRequest WaitForRequest()
        {
            ByteBuilder b = new ByteBuilder();
            using (var r = new BinaryReader(stream, Encoding.UTF8, true))
            {
                byte[] bytes = r.ReadBytes(4);
                b.Append(bytes);
                ATyp atyp = (ATyp)bytes[3];
                switch (atyp)
                {
                    case ATyp.IPv4:
                    case ATyp.IPv6:
                        b.Append(r.ReadBytes(atyp == ATyp.IPv4 ? 4 : 16));
                        break;

                    case ATyp.Domain:
                        byte length = r.ReadByte();
                        b.Append(length).Append(r.ReadBytes(length));
                        break;
                }
                b.Append(r.ReadBytes(2));
            }
            try
            {
                return SocksRequest.Deserialize(b.ToArray());
            }
            catch (Exception e)
            {
                throw new Socks5Exception("The request could not be serialized.", e);
            }
        }

        /// <summary>
        /// Asserts the instance has not been disposed of.
        /// </summary>
        private void AssertValid()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);
        }
    }
}