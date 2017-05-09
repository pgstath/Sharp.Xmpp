using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace XMPPEngineer.Extensions.Socks5
{
    /// <summary>
    /// Provides client connections for SOCKS5 network services.
    /// </summary>
    /// <remarks>This is specifically tailored to our needs for XMPP and is not
    /// a full-blown SOCKS5 implementation.</remarks>
    internal class Socks5Client : IDisposable
    {
        /// <summary>
        /// The underlying TCP connection.
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
        /// The username to authenticate with.
        /// </summary>
        public string Username
        {
            get;
            set;
        }

        /// <summary>
        /// The password to authenticate with.
        /// </summary>
        public string Password
        {
            get;
            set;
        }

        /// <summary>
        /// Performs the specified SOCKS5 request.
        /// </summary>
        /// <param name="command">The command of the SOCKS5 request.</param>
        /// <param name="address">The IP address of the remote host.</param>
        /// <param name="port">The port of the remote host.</param>
        /// <returns>The SOCKS5 reply sent by the server.</returns>
        /// <exception cref="ArgumentNullException">The request parameter is
        /// null.</exception>
        /// <exception cref="ObjectDisposedException">The object has been
        /// disposed.</exception>
        /// <exception cref="Socks5Exception">The request could not be performed.
        /// Consult the InnerException property of the Socks5Exception to learn
        /// the reason.</exception>
        public SocksReply Request(SocksCommand command, IPAddress address, ushort port)
        {
            return Request(new SocksRequest(command, address, port));
        }

        /// <summary>
        /// Performs the specified SOCKS5 request.
        /// </summary>
        /// <param name="command">The command of the SOCKS5 request.</param>
        /// <param name="domain">The domain of the remote host.</param>
        /// <param name="port">The port of the remote host.</param>
        /// <returns>The SOCKS5 reply sent by the server.</returns>
        /// <exception cref="ArgumentNullException">The request parameter is
        /// null.</exception>
        /// <exception cref="ObjectDisposedException">The object has been
        /// disposed.</exception>
        /// <exception cref="Socks5Exception">The request could not be performed.
        /// Consult the InnerException property of the Socks5Exception to learn
        /// the reason.</exception>
        public SocksReply Request(SocksCommand command, string domain, ushort port)
        {
            return Request(new SocksRequest(command, domain, port));
        }

        /// <summary>
        /// Performs the specified SOCKS5 request.
        /// </summary>
        /// <param name="request">The SOCKS5 request to issue to the server.</param>
        /// <returns>The SOCKS5 reply sent by the server.</returns>
        /// <exception cref="ArgumentNullException">The request parameter is
        /// null.</exception>
        /// <exception cref="ObjectDisposedException">The object has been
        /// disposed.</exception>
        /// <exception cref="Socks5Exception">The request could not be performed.
        /// Consult the InnerException property of the Socks5Exception to learn
        /// the reason.</exception>
        public SocksReply Request(SocksRequest request)
        {
            request.ThrowIfNull("request");
            AssertValid();
            try
            {
                byte[] bytes = request.Serialize();
                stream.Write(bytes, 0, bytes.Length);
                ByteBuilder b = new ByteBuilder();
                using (var r = new BinaryReader(stream, Encoding.UTF8, true))
                {
                    bytes = r.ReadBytes(4);
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
                return SocksReply.Deserialize(b.ToArray());
            }
            catch (Exception e)
            {
                throw new Socks5Exception("The request could not be performed.", e);
            }
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
        /// Initializes a new instance of the Socks5Client class and connects to the
        /// specified port on the specified host.
        /// </summary>
        /// <param name="hostname">The DNS name of the remote host to which you intend
        /// to connect.</param>
        /// <param name="port">The port number of the remote host to which you intend
        /// to connect.</param>
        /// <param name="username">The username with which to authenticate if the
        /// server requires authentication.</param>
        /// <param name="password">The password with which to authenticate if the
        /// server requires authentication.</param>
        /// <exception cref="ArgumentNullException">The hostname parameter is
        /// null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The port parameter is not
        /// between 0 and 65535.</exception>
        /// <exception cref="SocketException">An error occurred when accessing the
        /// socket.</exception>
        /// <exception cref="Socks5Exception">The server returned invalid data, or
        /// authentication with the SOCKS5 server failed.</exception>
        public Socks5Client(string hostname, int port, string username = null,
            string password = null)
        {
            hostname.ThrowIfNull("hostname");
            port.ThrowIfOutOfRange("port", 0, 65535);
            Username = username;
            Password = password;
            client = new TcpClient(hostname, port);
            InitializeConnection();
        }

        /// <summary>
        /// Initializes a new instance of the Socks5Client class and connects to the
        /// specified port on the specified host.
        /// </summary>
        /// <param name="address">The IP address of the remote host to which you intend
        /// to connect.</param>
        /// <param name="port">The port number of the remote host to which you intend
        /// to connect.</param>
        /// <param name="username">The username with which to authenticate if the
        /// server requires authentication.</param>
        /// <param name="password">The password with which to authenticate if the
        /// server requires authentication.</param>
        /// <exception cref="ArgumentNullException">The address parameter is
        /// null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The port parameter is not
        /// between 0 and 65535.</exception>
        /// <exception cref="SocketException">An error occurred when accessing the
        /// socket.</exception>
        /// <exception cref="Socks5Exception">The server returned invalid data, or
        /// authentication with the SOCKS5 server failed.</exception>
        public Socks5Client(IPAddress address, int port, string username = null,
            string password = null)
        {
            address.ThrowIfNull("address");
            port.ThrowIfOutOfRange("port", 0, 65535);
            Username = username;
            Password = password;
            client = new TcpClient();
            client.Connect(address, port);
            InitializeConnection();
        }

        /// <summary>
        /// Closes and disposes of the SOCKS5 client instance.
        /// </summary>
        public void Close()
        {
            Dispose();
        }

        /// <summary>
        /// Releases all resources used by the current instance of the Socks5Client class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources used by the current instance of the Socks5Client
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
        /// Initializes the connection to the SOCKS5 server.
        /// </summary>
        /// <exception cref="Socks5Exception">The server returned invalid data, or
        /// authentication failed.</exception>
        private void InitializeConnection()
        {
            stream = client.GetStream();
            var greeting = PerformGreeting();

            switch (greeting.Method)
            {
                case AuthMethod.None:
                    // We're done.
                    break;

                case AuthMethod.Username:
                    Authenticate();
                    break;

                default:
                    throw new Socks5Exception("No acceptable authentication method.");
            }
        }

        /// <summary>
        /// Performs the initial greeting.
        /// </summary>
        /// <returns>The greeting-message returned by the SOCKS5 server.</returns>
        /// <exception cref="Socks5Exception">The server returned invalid or
        /// unexpected data.</exception>
        private ServerGreeting PerformGreeting()
        {
            var methods = new HashSet<AuthMethod>() { AuthMethod.None };
            if (!String.IsNullOrEmpty(Username))
                methods.Add(AuthMethod.Username);
            byte[] bytes = new ClientGreeting(methods).Serialize();

            stream.Write(bytes, 0, bytes.Length);
            // Read the server's response.
            bytes = new byte[2];
            stream.Read(bytes, 0, 2);
            return ServerGreeting.Deserialize(bytes);
        }

        /// <summary>
        /// Performs Username/Password authentication.
        /// </summary>
        /// <exception cref="Socks5Exception">The server returned invalid or
        /// unexpected data, or authentication failed.</exception>
        private void Authenticate()
        {
            byte[] bytes = new AuthRequest(Username, Password).Serialize();
            stream.Write(bytes, 0, bytes.Length);
            // Read the server's response.
            bytes = new byte[2];
            stream.Read(bytes, 0, 2);
            AuthResponse response = AuthResponse.Deserialize(bytes);
            if (!response.Success)
                throw new Socks5Exception("Authentication failed.");
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