using ARSoft.Tools.Net.Dns;
using Sharp.Xmpp.Core.Sasl;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Sharp.Xmpp.Core
{
    /// <summary>
    /// Implements the core features of the XMPP protocol.
    /// </summary>
    /// <remarks>For implementation details, refer to RFC 3920.</remarks>
    public class XmppCore : IDisposable
    {
        /// <summary>
        /// The DNS SRV name records
        /// </summary>
        private List<SrvRecord> dnsRecordList;

        /// <summary>
        /// The current SRV DNS record to use
        /// </summary>
        private SrvRecord dnsCurrent;

        /// <summary>
        /// Bool variable indicating whether DNS records are initialised
        /// </summary>
        private bool dnsIsInit = false;

        /// <summary>
        /// The TCP connection to the XMPP server.
        /// </summary>
        private TcpClient client;

        /// <summary>
        /// The (network) stream used for sending and receiving XML data.
        /// </summary>
        private Stream stream;

        /// <summary>
        /// The parser instance used for parsing incoming XMPP XML-stream data.
        /// </summary>
        private StreamParser parser;

        /// <summary>
        /// True if the instance has been disposed of.
        /// </summary>
        private bool disposed;

        /// <summary>
        /// Used for creating unique IQ stanza ids.
        /// </summary>
        private int id;

        /// <summary>
        /// The port number of the XMPP service of the server.
        /// </summary>
        private int port;

        /// <summary>
        /// The hostname of the XMPP server to connect to.
        /// </summary>
        private string hostname;

		/// <summary>
		/// The server IP or domain name of the XMPP server to connect to.
		/// </summary>
		private string server;

        /// <summary>
        /// The username with which to authenticate.
        /// </summary>
        private string username;

        /// <summary>
        /// The password with which to authenticate.
        /// </summary>
        private string password;

        /// <summary>
        /// The resource to use for binding.
        /// </summary>
        private string resource;

        /// <summary>
        /// Write lock for the network stream.
        /// </summary>
        private readonly object writeLock = new object();

        /// <summary>
        /// The default Time Out for IQ Requests
        /// </summary>
        private int millisecondsDefaultTimeout = -1;

        /// <summary>
        /// The default value for debugging stanzas is false
        /// </summary>
        private bool debugStanzas = false;

        /// <summary>
        /// A thread-safe dictionary of wait handles for pending IQ requests.
        /// </summary>
        private ConcurrentDictionary<string, AutoResetEvent> waitHandles =
            new ConcurrentDictionary<string, AutoResetEvent>();

        /// <summary>
        /// A thread-safe dictionary of IQ responses for pending IQ requests.
        /// </summary>
        private ConcurrentDictionary<string, Iq> iqResponses =
         new ConcurrentDictionary<string, Iq>();

        /// <summary>
        /// A thread-safe dictionary of callback methods for asynchronous IQ requests.
        /// </summary>
        private ConcurrentDictionary<string, Action<string, Iq>> iqCallbacks =
         new ConcurrentDictionary<string, Action<string, Iq>>();

        /// <summary>
        /// A cancellation token source that is set when the listener threads shuts
        /// down due to an exception.
        /// </summary>
        private CancellationTokenSource cancelIq = new CancellationTokenSource();

        /// <summary>
        /// A FIFO of stanzas waiting to be processed.
        /// </summary>
        private BlockingCollection<Stanza> stanzaQueue = new BlockingCollection<Stanza>();

        /// <summary>
        /// A cancellation token source for cancelling the dispatcher, if neccessary.
        /// </summary>
        private CancellationTokenSource cancelDispatch = new CancellationTokenSource();

        /// <summary>
        /// The hostname of the XMPP server to connect to.
        /// </summary>
        /// <exception cref="ArgumentNullException">The Hostname property is being
        /// set and the value is null.</exception>
        /// <exception cref="ArgumentException">The Hostname property is being set
        /// and the value is the empty string.</exception>
        public string Hostname
        {
            get
            {
                return hostname;
            }

            set
            {
                value.ThrowIfNullOrEmpty("Hostname");
                hostname = value;
            }
        }

		/// <summary>
		/// The server IP address or domain name of the XMPP server, if different from the Hostname.
		/// </summary>
		public string Server
		{
			get
			{
				return server;
			}

			set
			{
				server = value;
			}
		}

        /// <summary>
        /// The port number of the XMPP service of the server.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The Port property is being
        /// set and the value is not between 0 and 65536.</exception>
        public int Port
        {
            get
            {
                return port;
            }

            set
            {
                value.ThrowIfOutOfRange("Port", 0, 65536);
                port = value;
            }
        }

        /// <summary>
        /// The username with which to authenticate. In XMPP jargon this is known
        /// as the 'node' part of the JID.
        /// </summary>
        /// <exception cref="ArgumentNullException">The Username property is being
        /// set and the value is null.</exception>
        /// <exception cref="ArgumentException">The Username property is being set
        /// and the value is the empty string.</exception>
        public string Username
        {
            get
            {
                return username;
            }

            set
            {
                value.ThrowIfNullOrEmpty("Username");
                username = value;
            }
        }

        /// <summary>
        /// The password with which to authenticate.
        /// </summary>
        /// <exception cref="ArgumentNullException">The Password property is being
        /// set and the value is null.</exception>
        public string Password
        {
            get
            {
                return password;
            }

            set
            {
                value.ThrowIfNull("Password");
                password = value;
            }
        }

        /// <summary>
        /// The Default IQ Set /Request message timeout
        /// </summary>
        public int MillisecondsDefaultTimeout
        {
            get { return millisecondsDefaultTimeout; }
            set { millisecondsDefaultTimeout = value; }
        }

        /// <summary>
        /// Print XML stanzas for debugging purposes
        /// </summary>
        public bool DebugStanzas
        {
            get { return debugStanzas; }
            set { debugStanzas = value; }
        }

        /// <summary>
        /// If true the session will be TLS/SSL-encrypted if the server supports it.
        /// </summary>
        public bool Tls
        {
            get;
            set;
        }

        /// <summary>
        /// A delegate used for verifying the remote Secure Sockets Layer (SSL)
        /// certificate which is used for authentication.
        /// </summary>
        public RemoteCertificateValidationCallback Validate
        {
            get;
            set;
        }

        /// <summary>
        /// Determines whether the session with the server is TLS/SSL encrypted.
        /// </summary>
        public bool IsEncrypted
        {
            get;
            private set;
        }

        /// <summary>
        /// The address of the Xmpp entity.
        /// </summary>
        public Jid Jid
        {
            get;
            private set;
        }

        /// <summary>
        /// The default language of the XML stream.
        /// </summary>
        public CultureInfo Language
        {
            get;
            private set;
        }

        /// <summary>
        /// Determines whether the instance is connected to the XMPP server.
        /// </summary>
        public bool Connected
        {
            get;
            private set;
        }

        /// <summary>
        /// Determines whether the instance has been authenticated.
        /// </summary>
        public bool Authenticated
        {
            get;
            private set;
        }

        /// <summary>
        /// The event that is raised when an unrecoverable error condition occurs.
        /// </summary>
        public event EventHandler<ErrorEventArgs> Error;

        /// <summary>
        /// The event that is raised when an IQ-request stanza has been received.
        /// </summary>
        public event EventHandler<IqEventArgs> Iq;

        /// <summary>
        /// The event that is raised when a Message stanza has been received.
        /// </summary>
        public event EventHandler<MessageEventArgs> Message;

        /// <summary>
        /// The event that is raised when a Presence stanza has been received.
        /// </summary>
        public event EventHandler<PresenceEventArgs> Presence;

        /// <summary>
        /// Initializes a new instance of the XmppCore class.
        /// </summary>
        /// <param name="hostname">The hostname of the XMPP server to connect to.</param>
        /// <param name="username">The username with which to authenticate. In XMPP jargon
        /// this is known as the 'node' part of the JID.</param>
        /// <param name="password">The password with which to authenticate.</param>
        /// <param name="port">The port number of the XMPP service of the server.</param>
        /// <param name="tls">If true the session will be TLS/SSL-encrypted if the server
        /// supports TLS/SSL-encryption.</param>
        /// <param name="validate">A delegate used for verifying the remote Secure Sockets
        /// Layer (SSL) certificate which is used for authentication. Can be null if not
        /// needed.</param>
        /// <exception cref="ArgumentNullException">The hostname parameter or the
        /// username parameter or the password parameter is null.</exception>
        /// <exception cref="ArgumentException">The hostname parameter or the username
        /// parameter is the empty string.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The value of the port parameter
        /// is not a valid port number.</exception>
        public XmppCore(string hostname, string username, string password,
            int port = 5222, bool tls = true, RemoteCertificateValidationCallback validate = null):
		this(hostname, username, password, null, port, tls, validate) { }

		/// <summary>
		/// Initializes a new instance of the XmppCore class.
		/// </summary>
		/// <param name="hostname">The hostname of the XMPP server to connect to.</param>
		/// <param name="username">The username with which to authenticate. In XMPP jargon
		/// this is known as the 'node' part of the JID.</param>
		/// <param name="password">The password with which to authenticate.</param>
		/// <param name="server">The IP address or domain of the XMPP server, if different from the hostname eg. xmpp.server.com</param>
		/// <param name="port">The port number of the XMPP service of the server.</param>
		/// <param name="tls">If true the session will be TLS/SSL-encrypted if the server
		/// supports TLS/SSL-encryption.</param>
		/// <param name="validate">A delegate used for verifying the remote Secure Sockets
		/// Layer (SSL) certificate which is used for authentication. Can be null if not
		/// needed.</param>
		/// <exception cref="ArgumentNullException">The hostname parameter or the
		/// username parameter or the password parameter is null.</exception>
		/// <exception cref="ArgumentException">The hostname parameter or the username
		/// parameter is the empty string.</exception>
		/// <exception cref="ArgumentOutOfRangeException">The value of the port parameter
		/// is not a valid port number.</exception>
		public XmppCore(string hostname, string username, string password, string server,
			int port = 5222, bool tls = true, RemoteCertificateValidationCallback validate = null)
		{
			if (String.IsNullOrWhiteSpace(server)) {
				moveNextSrvDNS(hostname);
				if (dnsCurrent != null)
				{
					Hostname = dnsCurrent.Target.ToString();
					Port = dnsCurrent.Port;
				}
				else
				{
					Hostname = hostname;
					Server = hostname;
					Port = port;
				}
			}
			else {
				Server = server;
				Hostname = hostname;
				Port = port;
			}
			    
			Username = username;
			Password = password;
			Tls = tls;
			Validate = validate;
		}

        /// <summary>
        /// Initializes a new instance of the XmppCore class.
        /// </summary>
        /// <param name="hostname">The hostname of the XMPP server to connect to.</param>
        /// <param name="port">The port number of the XMPP service of the server.</param>
        /// <param name="tls">If true the session will be TLS/SSL-encrypted if the server
        /// supports TLS/SSL-encryption.</param>
        /// <param name="validate">A delegate used for verifying the remote Secure Sockets
        /// Layer (SSL) certificate which is used for authentication. Can be null if not
        /// needed.</param>
        /// <exception cref="ArgumentNullException">The hostname parameter is
        /// null.</exception>
        /// <exception cref="ArgumentException">The hostname parameter is the empty
        /// string.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The value of the port parameter
        /// is not a valid port number.</exception>
        public XmppCore(string hostname, int port = 5222, bool tls = true,
            RemoteCertificateValidationCallback validate = null):
		this(hostname, null, port, tls, validate) { }

		/// <summary>
		/// Initializes a new instance of the XmppCore class.
		/// </summary>
		/// <param name="hostname">The hostname of the XMPP server to connect to.</param>
		/// <param name="server">The IP address or domain of the XMPP server, if different from the hostname eg. xmpp.server.com</param>
		/// <param name="port">The port number of the XMPP service of the server.</param>
		/// <param name="tls">If true the session will be TLS/SSL-encrypted if the server
		/// supports TLS/SSL-encryption.</param>
		/// <param name="validate">A delegate used for verifying the remote Secure Sockets
		/// Layer (SSL) certificate which is used for authentication. Can be null if not
		/// needed.</param>
		/// <exception cref="ArgumentNullException">The hostname parameter is
		/// null.</exception>
		/// <exception cref="ArgumentException">The hostname parameter is the empty
		/// string.</exception>
		/// <exception cref="ArgumentOutOfRangeException">The value of the port parameter
		/// is not a valid port number.</exception>
		public XmppCore(string hostname, string server, int port = 5222, bool tls = true,
			RemoteCertificateValidationCallback validate = null)
		{
			if (String.IsNullOrWhiteSpace(server))
			{
				moveNextSrvDNS(hostname);
				if (dnsCurrent != null)
				{
					Hostname = dnsCurrent.Target.ToString();
					Port = dnsCurrent.Port;
				}
				else
				{
					Hostname = hostname;
					Server = hostname;
					Port = port;
				}
			}
			else {
				Server = server;
				Hostname = hostname;
				Port = port;				
			}

			Tls = tls;
			Validate = validate;
		}

        /// <summary>
        /// Initialises and resolves the DNS Domain, and set to dnsCurrent the next
        /// SRV record to use
        /// </summary>
        /// <param name="domain">XMPP Domain</param>
        /// <returns>XMPP server hostname for the Domain</returns>
        private SrvRecord moveNextSrvDNS(string domain)
        {
            domain.ThrowIfNullOrEmpty("domain");
            //If already a lookup has being made return
            if (dnsIsInit)
            {
                //If it is already init we remove the current
                if (dnsRecordList != null && dnsCurrent != null) dnsRecordList.Remove(dnsCurrent);
                dnsCurrent = dnsRecordList.FirstOrDefault();
                return dnsCurrent;
            };
            dnsIsInit = true;

            var domainName = ARSoft.Tools.Net.DomainName.Parse(("_xmpp-client._tcp." + domain));
            DnsMessage dnsMessage = DnsClient.Default.Resolve(domainName, RecordType.Srv);
            if ((dnsMessage == null) || ((dnsMessage.ReturnCode != ReturnCode.NoError) && (dnsMessage.ReturnCode != ReturnCode.NxDomain)))
            {
                //If DNS SRV records lookup fails then continue with the host name
#if DEBUG
                System.Diagnostics.Debug.WriteLine("DNS Lookup Failed");
#endif
                return null;
            }
            else
            {
                var tempList = new List<SrvRecord>();

                foreach (DnsRecordBase dnsRecord in dnsMessage.AnswerRecords)
                {
                    SrvRecord srvRecord = dnsRecord as SrvRecord;
                    if (srvRecord != null)
                    {
                        tempList.Add(srvRecord);
                        Console.WriteLine(srvRecord.ToString());
                        Console.WriteLine("  |--- Name " + srvRecord.Name);
                        Console.WriteLine("  |--- Port: " + srvRecord.Port);
                        Console.WriteLine("  |--- Priority" + srvRecord.Priority);
                        Console.WriteLine("  |--- Type " + srvRecord.RecordType);
                        Console.WriteLine("  |--- Target: " + srvRecord.Target);
                        Console.WriteLine();
                    }
                }
                dnsRecordList = tempList.OrderBy(o => o.Priority).ThenBy(order => order.Weight).ToList();

                dnsCurrent = dnsRecordList.FirstOrDefault();
                return dnsCurrent;
            }
        }

        /// <summary>
        /// Establishes a connection to the XMPP server.
        /// </summary>
        /// <param name="resource">The resource identifier to bind with. If this is null,
        /// it is assigned by the server.</param>
        /// <exception cref="SocketException">An error occurred while accessing the socket
        /// used for establishing the connection to the XMPP server. Use the ErrorCode
        /// property to obtain the specific error code.</exception>
        /// <exception cref="AuthenticationException">An authentication error occured while
        /// trying to establish a secure connection, or the provided credentials were
        /// rejected by the server, or the server requires TLS/SSL and TLS has been
        /// turned off.</exception>
        /// <exception cref="XmppException">An XMPP error occurred while negotiating the
        /// XML stream with the server.</exception>
        /// <exception cref="ObjectDisposedException">The XmppCore object has been
        /// disposed.</exception>
        /// <remarks>If a username has been supplied, this method automatically performs
        /// authentication.</remarks>
        public void Connect(string resource = null, bool bind = true)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);
            this.resource = resource;
            try
            {
                client = new TcpClient(Server, Port);
                stream = client.GetStream();
                // Sets up the connection which includes TLS and possibly SASL negotiation.
                SetupConnection(this.resource, bind);
                // We are connected.
                Connected = true;
                // Set up the listener and dispatcher tasks.
                Task.Factory.StartNew(ReadXmlStream, TaskCreationOptions.LongRunning);
                Task.Factory.StartNew(DispatchEvents, TaskCreationOptions.LongRunning);
            }
            catch (XmlException e)
            {
                throw new XmppException("The XML stream could not be negotiated.", e);
            }
        }

        /// <summary>
        /// Authenticates with the XMPP server using the specified username and
        /// password.
        /// </summary>
        /// <param name="username">The username to authenticate with.</param>
        /// <param name="password">The password to authenticate with.</param>
        /// <exception cref="ArgumentNullException">The username parameter or the
        /// password parameter is null.</exception>
        /// <exception cref="SocketException">An error occurred while accessing the socket
        /// used for establishing the connection to the XMPP server. Use the ErrorCode
        /// property to obtain the specific error code.</exception>
        /// <exception cref="AuthenticationException">An authentication error occured while
        /// trying to establish a secure connection, or the provided credentials were
        /// rejected by the server, or the server requires TLS/SSL and TLS has been
        /// turned off.</exception>
        /// <exception cref="InvalidOperationException">The XmppCore instance is not
        /// connected to a remote host.</exception>
        /// <exception cref="XmppException">Authentication has already been performed, or
        /// an XMPP error occurred while negotiating the XML stream with the
        /// server.</exception>
        /// <exception cref="ObjectDisposedException">The XmppCore object has been
        /// disposed.</exception>
        public void Authenticate(string username, string password)
        {
            AssertValid();
            username.ThrowIfNull("username");
            password.ThrowIfNull("password");
            if (Authenticated)
                throw new XmppException("Authentication has already been performed.");
            // Unfortunately, SASL authentication does not follow the standard XMPP
            // IQ-semantics. At this stage it really is easier to simply perform a
            // reconnect.
            Username = username;
            Password = password;
            Disconnect();
            Connect(this.resource);
        }

        /// <summary>
        /// Sends a Message stanza with the specified attributes and content to the
        /// server.
        /// </summary>
        /// <param name="to">The JID of the intended recipient for the stanza.</param>
        /// <param name="from">The JID of the sender.</param>
        /// <param name="data">he content of the stanza.</param>
        /// <param name="id">The ID of the stanza.</param>
        /// <param name="language">The language of the XML character data of
        /// the stanza.</param>
        /// <exception cref="ObjectDisposedException">The XmppCore object has been
        /// disposed.</exception>
        /// <exception cref="InvalidOperationException">The XmppCore instance is not
        /// connected to a remote host.</exception>
        /// <exception cref="IOException">There was a failure while writing to the
        /// network.</exception>
        public void SendMessage(Jid to = null, Jid from = null, XmlElement data = null,
            string id = null, CultureInfo language = null)
        {
            AssertValid();
            Send(new Message(to, from, data, id, language));
        }

        /// <summary>
        /// Sends the specified message stanza to the server.
        /// </summary>
        /// <param name="message">The message stanza to send to the server.</param>
        /// <exception cref="ArgumentNullException">The message parameter is
        /// null.</exception>
        /// <exception cref="ObjectDisposedException">The XmppCore object has been
        /// disposed.</exception>
        /// <exception cref="InvalidOperationException">The XmppCore instance is not
        /// connected to a remote host.</exception>
        /// <exception cref="IOException">There was a failure while writing to the
        /// network.</exception>
        public void SendMessage(Message message)
        {
            AssertValid();
            message.ThrowIfNull("message");
            Send(message);
        }

        /// <summary>
        /// Sends a Presence stanza with the specified attributes and content to the
        /// server.
        /// </summary>
        /// <param name="to">The JID of the intended recipient for the stanza.</param>
        /// <param name="from">The JID of the sender.</param>
        /// <param name="data">he content of the stanza.</param>
        /// <param name="id">The ID of the stanza.</param>
        /// <param name="language">The language of the XML character data of
        /// the stanza.</param>
        /// <exception cref="ObjectDisposedException">The XmppCore object has been
        /// disposed.</exception>
        /// <exception cref="InvalidOperationException">The XmppCore instance is not
        /// connected to a remote host.</exception>
        /// <exception cref="IOException">There was a failure while writing to the
        /// network.</exception>
        public void SendPresence(Jid to = null, Jid from = null, string id = null,
            CultureInfo language = null, params XmlElement[] data)
        {
            AssertValid();
            Send(new Presence(to, from, id, language, data));
        }

        /// <summary>
        /// Sends the specified presence stanza to the server.
        /// </summary>
        /// <param name="presence">The presence stanza to send to the server.</param>
        /// <exception cref="ArgumentNullException">The presence parameter
        /// is null.</exception>
        /// <exception cref="ObjectDisposedException">The XmppCore object has been
        /// disposed.</exception>
        /// <exception cref="InvalidOperationException">The XmppCore instance is not
        /// connected to a remote host.</exception>
        /// <exception cref="IOException">There was a failure while writing to the
        /// network.</exception>
        public void SendPresence(Presence presence)
        {
            AssertValid();
            presence.ThrowIfNull("presence");
            Send(presence);
        }

        /// <summary>
        /// Performs an IQ set/get request and blocks until the response IQ comes in.
        /// </summary>
        /// <param name="type">The type of the request. This must be either
        /// IqType.Set or IqType.Get.</param>
        /// <param name="to">The JID of the intended recipient for the stanza.</param>
        /// <param name="from">The JID of the sender.</param>
        /// <param name="data">he content of the stanza.</param>
        /// <param name="language">The language of the XML character data of
        /// the stanza.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait
        /// for the arrival of the IQ response or -1 to wait indefinitely.</param>
        /// <returns>The IQ response sent by the server.</returns>
        /// <exception cref="ArgumentException">The type parameter is not
        /// IqType.Set or IqType.Get.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The value of millisecondsTimeout
        /// is a negative number other than -1, which represents an indefinite
        /// timeout.</exception>
        /// <exception cref="ObjectDisposedException">The XmppCore object has been
        /// disposed.</exception>
        /// <exception cref="InvalidOperationException">The XmppCore instance is not
        /// connected to a remote host.</exception>
        /// <exception cref="IOException">There was a failure while writing to the
        /// network, or there was a failure reading from the network.</exception>
        /// <exception cref="TimeoutException">A timeout was specified and it
        /// expired.</exception>
        public Iq IqRequest(IqType type, Jid to = null, Jid from = null,
            XmlElement data = null, CultureInfo language = null,
            int millisecondsTimeout = -1)
        {
            AssertValid();
            return IqRequest(new Iq(type, null, to, from, data, language), millisecondsTimeout);
        }

        /// <summary>
        /// Performs an IQ set/get request and blocks until the response IQ comes in.
        /// </summary>
        /// <param name="request">The IQ request to send.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait
        /// for the arrival of the IQ response or -1 to wait indefinitely.</param>
        /// <returns>The IQ response sent by the server.</returns>
        /// <exception cref="ArgumentNullException">The request parameter is null.</exception>
        /// <exception cref="ArgumentException">The type parameter is not IqType.Set
        /// or IqType.Get.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The value of millisecondsTimeout
        /// is a negative number other than -1, which represents an indefinite
        /// timeout.</exception>
        /// <exception cref="ObjectDisposedException">The XmppCore object has been
        /// disposed.</exception>
        /// <exception cref="InvalidOperationException">The XmppCore instance is not
        /// connected to a remote host.</exception>
        /// <exception cref="IOException">There was a failure while writing to the
        /// network, or there was a failure reading from the network.</exception>
        /// <exception cref="TimeoutException">A timeout was specified and it
        /// expired.</exception>
        public Iq IqRequest(Iq request, int millisecondsTimeout = -1)
        {
            int timeOut = -1;
            AssertValid();
            request.ThrowIfNull("request");
            if (request.Type != IqType.Set && request.Type != IqType.Get)
                throw new ArgumentException("The IQ type must be either 'set' or 'get'.");
            if (millisecondsTimeout == -1)
            {
                timeOut = millisecondsDefaultTimeout;
            }
            else timeOut = millisecondsTimeout;
            // Generate a unique ID for the IQ request.
            request.Id = GetId();
            AutoResetEvent ev = new AutoResetEvent(false);
            Send(request);
            // Wait for event to be signaled by task that processes the incoming
            // XML stream.
            waitHandles[request.Id] = ev;
            int index = WaitHandle.WaitAny(new WaitHandle[] { ev, cancelIq.Token.WaitHandle },
                timeOut);
            if (index == WaitHandle.WaitTimeout)
            {
                //An entity that receives an IQ request of type "get" or "set" MUST reply with an IQ response of type
                //"result" or "error" (the response MUST preserve the 'id' attribute of the request).
                //http://xmpp.org/rfcs/rfc3920.html#stanzas
                //if (request.Type == IqType.Set || request.Type == IqType.Get)

                //Make sure that its a request towards the server and not towards any client
                var ping = request.Data["ping"];

                if (request.To.Domain == Jid.Domain && (request.To.Node == null || request.To.Node == "") && (ping != null && ping.NamespaceURI == "urn:xmpp:ping"))
                {
                    Connected = false;
                    var e = new XmppDisconnectionException("Timeout Disconnection happened at IqRequest");
                    if (!disposed)
                        Error.Raise(this, new ErrorEventArgs(e));
                    //throw new TimeoutException();
                }

                //This check is somehow not really needed doue to the IQ must be either set or get
            }
            // Reader task errored out.
            if (index == 1)
                throw new IOException("The incoming XML stream could not read.");
            // Fetch response stanza.
            Iq response;
            if (iqResponses.TryRemove(request.Id, out response))
                return response;
            // Shouldn't happen.

            throw new InvalidOperationException();
        }

        /// <summary>
        /// Performs an IQ set/get request asynchronously and optionally invokes a
        /// callback method when the IQ response comes in.
        /// </summary>
        /// <param name="type">The type of the request. This must be either
        /// IqType.Set or IqType.Get.</param>
        /// <param name="to">The JID of the intended recipient for the stanza.</param>
        /// <param name="from">The JID of the sender.</param>
        /// <param name="data">he content of the stanza.</param>
        /// <param name="language">The language of the XML character data of
        /// the stanza.</param>
        /// <param name="callback">A callback method which is invoked once the
        /// IQ response from the server comes in.</param>
        /// <returns>The ID value of the pending IQ stanza request.</returns>
        /// <exception cref="ArgumentException">The type parameter is not IqType.Set
        /// or IqType.Get.</exception>
        /// <exception cref="ObjectDisposedException">The XmppCore object has been
        /// disposed.</exception>
        /// <exception cref="InvalidOperationException">The XmppCore instance is not
        /// connected to a remote host.</exception>
        /// <exception cref="IOException">There was a failure while writing to the
        /// network.</exception>
        public string IqRequestAsync(IqType type, Jid to = null, Jid from = null,
            XmlElement data = null, CultureInfo language = null,
            Action<string, Iq> callback = null)
        {
            AssertValid();
            return IqRequestAsync(new Iq(type, null, to, from, data, language), callback);
        }

        /// <summary>
        /// Performs an IQ set/get request asynchronously and optionally invokes a
        /// callback method when the IQ response comes in.
        /// </summary>
        /// <param name="request">The IQ request to send.</param>
        /// <param name="callback">A callback method which is invoked once the
        /// IQ response from the server comes in.</param>
        /// <returns>The ID value of the pending IQ stanza request.</returns>
        /// <exception cref="ArgumentNullException">The request parameter is null.</exception>
        /// <exception cref="ArgumentException">The type parameter is not IqType.Set
        /// or IqType.Get.</exception>
        /// <exception cref="ObjectDisposedException">The XmppCore object has been
        /// disposed.</exception>
        /// <exception cref="InvalidOperationException">The XmppCore instance is not
        /// connected to a remote host.</exception>
        /// <exception cref="IOException">There was a failure while writing to the
        /// network.</exception>
        public string IqRequestAsync(Iq request, Action<string, Iq> callback = null)
        {
            AssertValid();
            request.ThrowIfNull("request");
            if (request.Type != IqType.Set && request.Type != IqType.Get)
                throw new ArgumentException("The IQ type must be either 'set' or 'get'.");
            request.Id = GetId();
            // Register the callback.
            if (callback != null)
                iqCallbacks[request.Id] = callback;
            Send(request);
            return request.Id;
        }

        /// <summary>
        /// Sends an IQ response for the IQ request with the specified id.
        /// </summary>
        /// <param name="type">The type of the response. This must be either
        /// IqType.Result or IqType.Error.</param>
        /// <param name="id">The id of the IQ request.</param>
        /// <param name="to">The JID of the intended recipient for the stanza.</param>
        /// <param name="from">The JID of the sender.</param>
        /// <param name="data">he content of the stanza.</param>
        /// <param name="language">The language of the XML character data of
        /// the stanza.</param>
        /// <exception cref="ArgumentException">The type parameter is not IqType.Result
        /// or IqType.Error.</exception>
        /// <exception cref="ObjectDisposedException">The XmppCore object has been
        /// disposed.</exception>
        /// <exception cref="InvalidOperationException">The XmppCore instance is not
        /// connected to a remote host.</exception>
        /// <exception cref="IOException">There was a failure while writing to the
        /// network.</exception>
        public void IqResponse(IqType type, string id, Jid to = null, Jid from = null,
            XmlElement data = null, CultureInfo language = null)
        {
            AssertValid();
            IqResponse(new Iq(type, id, to, from, data, null));
        }

        /// <summary>
        /// Sends an IQ response for the IQ request with the specified id.
        /// </summary>
        /// <param name="response">The IQ response to send.</param>
        /// <exception cref="ArgumentNullException">The response parameter is
        /// null.</exception>
        /// <exception cref="ArgumentException">The Type property of the response
        /// parameter is not IqType.Result or IqType.Error.</exception>
        /// <exception cref="ObjectDisposedException">The XmppCore object has been
        /// disposed.</exception>
        /// <exception cref="InvalidOperationException">The XmppCore instance is not
        /// connected to a remote host.</exception>
        /// <exception cref="IOException">There was a failure while writing to the
        /// network.</exception>
        public void IqResponse(Iq response)
        {
            AssertValid();
            response.ThrowIfNull("response");
            if (response.Type != IqType.Result && response.Type != IqType.Error)
                throw new ArgumentException("The IQ type must be either 'result' or 'error'.");
            Send(response);
        }

        /// <summary>
        /// Closes the connection with the XMPP server. This automatically disposes
        /// of the object.
        /// </summary>
        /// <exception cref="InvalidOperationException">The XmppCore instance is not
        /// connected to a remote host.</exception>
        /// <exception cref="IOException">There was a failure while writing to the
        /// network.</exception>
        public void Close()
        {
            //FIXME, instead of asert valid I have ifs, only for the closing
            //AssertValid();
            // Close the XML stream.
            if (Connected) Disconnect();
            if (!disposed) Dispose();
        }

        /// <summary>
        /// Releases all resources used by the current instance of the XmppCore class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources used by the current instance of the XmppCore
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
                    if (parser != null)
                        parser.Close();
                    parser = null;
                    if (client != null)
                        client.Close();
                    client = null;
                }
                // Get rid of unmanaged resources.
            }
        }

        /// <summary>
        /// Asserts the instance has not been disposed of and is connected to the
        /// XMPP server.
        /// </summary>
        private void AssertValid()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);
            //FIXME-FIXED: if it is not connected it will be found out by a lower
            //level exception. Dont throw an exception about connection
            if (!Connected)
            {
                System.Diagnostics.Debug.WriteLine("Assert Valid: Client is disconnected, however no exception is thrown");
                //throw new InvalidOperationException("Not connected to XMPP server.");
            }
            //FIXME
        }

        /// <summary>
        /// Negotiates an XML stream over which XML stanzas can be sent.
        /// </summary>
        /// <param name="resource">The resource identifier to bind with. If this is null,
        /// it is assigned by the server.</param>
        /// <param name="bind">Do we bind - this is false in Stream Resumption but usually true.</param>
        /// <exception cref="XmppException">The resource binding process failed.</exception>
        /// <exception cref="XmlException">Invalid or unexpected XML data has been
        /// received from the XMPP server.</exception>
        /// <exception cref="AuthenticationException">An authentication error occured while
        /// trying to establish a secure connection, or the provided credentials were
        /// rejected by the server, or the server requires TLS/SSL and TLS has been
        /// turned off.</exception>
        private void SetupConnection(string resource = null, bool bind = true)
        {
            // Request the initial stream.
            XmlElement feats = InitiateStream(Hostname);
            // Server supports TLS/SSL via STARTTLS.
            if (feats["starttls"] != null)
            {
                // TLS is mandatory and user opted out of it.
                if (feats["starttls"]["required"] != null && Tls == false)
                    throw new AuthenticationException("The server requires TLS/SSL.");
                if (Tls)
                    feats = StartTls(Hostname, Validate);
            }
            // If no Username has been provided, don't perform authentication.
            if (Username == null)
                return;
            // Construct a list of SASL mechanisms supported by the server.
            var m = feats["mechanisms"];
            if (m == null || !m.HasChildNodes)
                throw new AuthenticationException("No SASL mechanisms advertised.");
            var mech = m.FirstChild;
            var list = new HashSet<string>();
            while (mech != null)
            {
                list.Add(mech.InnerText);
                mech = mech.NextSibling;
            }
            // Continue with SASL authentication.
            try
            {
                feats = Authenticate(list, Username, Password, Hostname);
                // FIXME: How is the client's JID constructed if the server does not support
                // resource binding?
                if (bind && feats["bind"] != null)
                    Jid = BindResource(resource);
            }
            catch (SaslException e)
            {
                throw new AuthenticationException("Authentication failed.", e);
            }
        }

        /// <summary>
        /// Initiates an XML stream with the specified entity.
        /// </summary>
        /// <param name="hostname">The name of the receiving entity with which to
        /// initiate an XML stream.</param>
        /// <returns>The 'stream:features' XML element as received from the
        /// receiving entity upon stream establishment.</returns>
        /// <exception cref="XmlException">The XML parser has encountered invalid
        /// or unexpected XML data.</exception>
        /// <exception cref="CultureNotFoundException">The culture specified by the
        /// XML-stream in it's 'xml:lang' attribute could not be found.</exception>
        /// <exception cref="IOException">There was a failure while writing to the
        /// network, or there was a failure while reading from the network.</exception>
        private XmlElement InitiateStream(string hostname)
        {
            var xml = Xml.Element("stream:stream", "jabber:client")
                .Attr("to", hostname)
                .Attr("version", "1.0")
                .Attr("xmlns:stream", "http://etherx.jabber.org/streams")
                .Attr("xml:lang", CultureInfo.CurrentCulture.Name);
            Send(xml.ToXmlString(xmlDeclaration: true, leaveOpen: true));
            // Create a new parser instance.
            if (parser != null)
                parser.Close();
            parser = new StreamParser(stream, true);
            // Remember the default language of the stream. The server is required to
            // include this, but we make sure nonetheless.
            Language = parser.Language ?? new CultureInfo("en");
            // The first element of the stream must be <stream:features>.
            return parser.NextElement("stream:features");
        }

        /// <summary>
        /// Secures the network stream by negotiating TLS-encryption with the server.
        /// </summary>
        /// <param name="hostname">The hostname of the XMPP server.</param>
        /// <param name="validate">A delegate used for verifying the remote Secure
        /// Sockets Layer (SSL) certificate which is used for authentication. Can be
        /// null if not needed.</param>
        /// <returns>The 'stream:features' XML element as received from the
        /// receiving entity upon establishment of a new XML stream.</returns>
        /// <exception cref="AuthenticationException">An
        /// authentication error occured while trying to establish a secure
        /// connection.</exception>
        /// <exception cref="XmlException">The XML parser has encountered invalid
        /// or unexpected XML data.</exception>
        /// <exception cref="CultureNotFoundException">The culture specified by the
        /// XML-stream in it's 'xml:lang' attribute could not be found.</exception>
        /// <exception cref="IOException">There was a failure while writing to the
        /// network, or there was a failure while reading from the network.</exception>
        private XmlElement StartTls(string hostname,
            RemoteCertificateValidationCallback validate)
        {
            // Send STARTTLS command and ensure the server acknowledges the request.
            SendAndReceive(Xml.Element("starttls",
                "urn:ietf:params:xml:ns:xmpp-tls"), "proceed");
            // Complete TLS negotiation and switch to secure stream.
            SslStream sslStream = new SslStream(stream, false, validate ??
                ((sender, cert, chain, err) => true));
            sslStream.AuthenticateAsClient(hostname);
            stream = sslStream;
            IsEncrypted = true;
            // Initiate a new stream to server.
            return InitiateStream(hostname);
        }

        /// <summary>
        /// Performs SASL authentication.
        /// </summary>
        /// <param name="mechanisms">An enumerable collection of SASL mechanisms
        /// supported by the server.</param>
        /// <param name="username">The username to authenticate with.</param>
        /// <param name="password">The password to authenticate with.</param>
        /// <param name="hostname">The hostname of the XMPP server.</param>
        /// <returns>The 'stream:features' XML element as received from the
        /// receiving entity upon establishment of a new XML stream.</returns>
        /// <remarks>Refer to RFC 3920, Section 6 (Use of SASL).</remarks>
        /// <exception cref="SaslException">A SASL error condition occured.</exception>
        /// <exception cref="XmlException">The XML parser has encountered invalid
        /// or unexpected XML data.</exception>
        /// <exception cref="CultureNotFoundException">The culture specified by the
        /// XML-stream in it's 'xml:lang' attribute could not be found.</exception>
        /// <exception cref="IOException">There was a failure while writing to the
        /// network, or there was a failure while reading from the network.</exception>
        private XmlElement Authenticate(IEnumerable<string> mechanisms, string username,
            string password, string hostname)
        {
            string name = SelectMechanism(mechanisms);
            SaslMechanism m = SaslFactory.Create(name);
            m.Properties.Add("Username", username);
            m.Properties.Add("Password", password);
            var xml = Xml.Element("auth", "urn:ietf:params:xml:ns:xmpp-sasl")
                .Attr("mechanism", name)
                .Text(m.HasInitial ? m.GetResponse(String.Empty) : String.Empty);
            Send(xml);
            while (true)
            {
                XmlElement ret = parser.NextElement("challenge", "success", "failure");
                if (ret.Name == "failure")
                    throw new SaslException("SASL authentication failed.");
                if (ret.Name == "success" && m.IsCompleted)
                    break;
                // Server has successfully authenticated us, but mechanism still needs
                // to verify server's signature.
                string response = m.GetResponse(ret.InnerText);
                // If the response is the empty string, the server's signature has been
                // verified.
                if (ret.Name == "success")
                {
                    if (response == String.Empty)
                        break;
                    throw new SaslException("Could not verify server's signature.");
                }
                xml = Xml.Element("response",
                    "urn:ietf:params:xml:ns:xmpp-sasl").Text(response);
                Send(xml);
            }
            // The instance is now authenticated.
            Authenticated = true;
            // Finally, initiate a new XML-stream.
            return InitiateStream(hostname);
        }

        /// <summary>
        /// Selects the best SASL mechanism that we support from the list of mechanisms
        /// advertised by the server.
        /// </summary>
        /// <param name="mechanisms">An enumerable collection of SASL mechanisms
        /// advertised by the server.</param>
        /// <returns>The IANA name of the selcted SASL mechanism.</returns>
        /// <exception cref="SaslException">No supported mechanism could be found in
        /// the list of mechanisms advertised by the server.</exception>
        private string SelectMechanism(IEnumerable<string> mechanisms)
        {
            // Precedence: SCRAM-SHA-1, DIGEST-MD5, PLAIN.
            string[] m = new string[] { "SCRAM-SHA-1", "DIGEST-MD5", "PLAIN" };
            for (int i = 0; i < m.Length; i++)
            {
                if (mechanisms.Contains(m[i], StringComparer.InvariantCultureIgnoreCase))
                    return m[i];
            }
            throw new SaslException("No supported SASL mechanism found.");
        }

        /// <summary>
        /// Performs resource binding and returns the 'full JID' with which this
        /// session associated.
        /// </summary>
        /// <param name="resourceName">The resource identifier to bind to. If this
        /// is null, the server generates a random identifier.</param>
        /// <returns>The full JID to which this session has been bound.</returns>
        /// <remarks>Refer to RFC 3920, Section 7 (Resource Binding).</remarks>
        /// <exception cref="XmppException">The resource binding process
        /// failed due to an erroneous server response.</exception>
        /// <exception cref="XmlException">The XML parser has encountered invalid
        /// or unexpected XML data.</exception>
        /// <exception cref="IOException">There was a failure while writing to the
        /// network, or there was a failure while reading from the network.</exception>
        private Jid BindResource(string resourceName = null)
        {
            var xml = Xml.Element("iq")
                .Attr("type", "set")
                .Attr("id", "bind-0");
            var bind = Xml.Element("bind", "urn:ietf:params:xml:ns:xmpp-bind");
            if (resourceName != null)
                bind.Child(Xml.Element("resource").Text(resourceName));
            xml.Child(bind);
            XmlElement res = SendAndReceive(xml, "iq");
            if (res["bind"] == null || res["bind"]["jid"] == null)
                throw new XmppException("Erroneous server response.");
            return new Jid(res["bind"]["jid"].InnerText);
        }

        /// <summary>
        /// Serializes and sends the specified XML element to the server.
        /// </summary>
        /// <param name="element">The XML element to send.</param>
        /// <exception cref="ArgumentNullException">The element parameter
        /// is null.</exception>
        /// <exception cref="IOException">There was a failure while writing
        /// to the network.</exception>
        private void Send(XmlElement element)
        {
            element.ThrowIfNull("element");
            Send(element.ToXmlString());
        }

        /// <summary>
        /// Sends the specified string to the server.
        /// </summary>
        /// <param name="xml">The string to send.</param>
        /// <exception cref="ArgumentNullException">The xml parameter is null.</exception>
        /// <exception cref="IOException">There was a failure while writing to
        /// the network.</exception>
        private void Send(string xml)
        {
            xml.ThrowIfNull("xml");
            // XMPP is guaranteed to be UTF-8.
            byte[] buf = Encoding.UTF8.GetBytes(xml);
            lock (writeLock)
            {
                //FIXME
                //If we have an IOexception immediatelly we make a disconnection, is it correct?
                try
                {
                    stream.Write(buf, 0, buf.Length);
                    if (debugStanzas) System.Diagnostics.Debug.WriteLine(xml);
                }
                catch (IOException e)
                {
                    Connected = false;
                    throw new XmppDisconnectionException(e.Message, e);
                }
                //FIXME
            }
        }

        /// <summary>
        /// Sends the specified stanza to the server.
        /// </summary>
        /// <param name="stanza">The stanza to send.</param>
        /// <exception cref="ArgumentNullException">The stanza parameter is null.</exception>
        /// <exception cref="IOException">There was a failure while writing to
        /// the network.</exception>
        private void Send(Stanza stanza, bool addToCache = true)
        {
			stanza.ThrowIfNull("stanza");
            Send(stanza.ToString());

			// we only want to cache specific stanzas if they are not being resent
			if (addToCache &&
                (stanza is Sharp.Xmpp.Core.Presence || stanza is Sharp.Xmpp.Core.Iq || stanza is Sharp.Xmpp.Core.Message))
			{
                // cache until receipt is confirmed
                stanzaQueueCache.Add(stanza);

                // add one to the sequence
                currentOutboundStanzaSequence++;
			}
		}

        /// <summary>
        /// Serializes and sends the specified XML element to the server and
        /// subsequently waits for a response.
        /// </summary>
        /// <param name="element">The XML element to send.</param>
        /// <param name="expected">A list of element names that are expected. If
        /// provided, and the read element does not match any of the provided names,
        /// an XmmpException is thrown.</param>
        /// <returns>The XML element read from the stream.</returns>
        /// <exception cref="XmlException">The XML parser has encountered invalid
        /// or unexpected XML data.</exception>
        /// <exception cref="ArgumentNullException">The element parameter is null.</exception>
        /// <exception cref="IOException">There was a failure while writing to
        /// the network, or there was a failure while reading from the network.</exception>
        private XmlElement SendAndReceive(XmlElement element,
            params string[] expected)
        {
            Send(element);
            try
            {
                return parser.NextElement(expected);
            }
            catch (XmppDisconnectionException e)
            {
                Connected = false;
                throw e;
            }
        }

        /// <summary>
        /// Listens for incoming XML stanzas and raises the appropriate events.
        /// </summary>
        /// <remarks>This runs in the context of a separate thread. In case of an
        /// exception, the Error event is raised and the thread is shutdown.</remarks>
        private void ReadXmlStream()
        {
            try
            {
                while (true)
                {
                    XmlElement elem = parser.NextElement("iq", "message", "presence", "enabled", "resumed", "a", "r", "failed");
                    // Parse element and dispatch.
                    switch (elem.Name)
                    {
                        case "iq":
                            currentInboundStanzaSequence++;

                            Iq iq = new Iq(elem);
                            if (iq.IsRequest)
                                stanzaQueue.Add(iq);
                            else
                                HandleIqResponse(iq);
                            break;

                        case "message":
                            currentInboundStanzaSequence++;

                            stanzaQueue.Add(new Message(elem));
                            break;

                        case "presence":
                            currentInboundStanzaSequence++;

                            stanzaQueue.Add(new Presence(elem));
                            break;

						// xep 1098 ###
						case "failed":
							HandleStreamManagementFailedResponse(elem);
							break;

						case "enabled":
                            HandleStreamManagementEnabledResponse(elem);
							break;

						case "resumed":
                            HandleResumedStreamResponse(elem);
							break;

						case "a":
                            currentInboundStanzaSequence++;
							HandleAcknowledgementResponse(elem);
							break;

                        case "r":
                            // we tell the server about the number of items we have received in this stream
                            Send("<a h='" + currentInboundStanzaSequence.ToString() + "' xmlns='urn:xmpp:sm:3'/>");
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                // Shut down the dispatcher task.
                cancelDispatch.Cancel();
                cancelDispatch = new CancellationTokenSource();
                // Unblock any threads blocking on pending IQ requests.
                cancelIq.Cancel();
                cancelIq = new CancellationTokenSource();
                //Add the failed connection
                if ((e is IOException) || (e is XmppDisconnectionException))
                {
                    Connected = false;
                    var ex = new XmppDisconnectionException(e.ToString());
                    e = ex;
                }
                // Raise the error event.
                if (!disposed)
                    Error.Raise(this, new ErrorEventArgs(e));
            }
        }

		#region xep-0198 ###

		/// <summary>
		/// The event that is raised when stream management is enabled.
		/// </summary>
		public event EventHandler<EventArgs> StreamManagementEnabled;

		/// <summary>
		/// The event that is raised when a stream is resumed.
		/// </summary>
		public event EventHandler<EventArgs> StreamResumed;

        /// <summary>
        /// Is stream management enabled.
        /// </summary>
        private bool streamManagementEnabled = false;

        /// <summary>
        /// Is stream management resumption enabled.
        /// </summary>
        private bool resumptionEnabled = false;

        /// <summary>
        /// The resumption id that can be used if the stream drops.
        /// </summary>
        private string resumptionId = null;

        /// <summary>
        /// The cycle that checks for items to process and timeouts.
        /// </summary>
        private int streamCycleCheckTimeInSeconds = 10;

        /// <summary>
        /// The maximum time between a connection being dropped and being allowed to reconnect the stream.
        /// The server can choose to override what is set here.
        /// </summary>
        private int maxResumptionPeriodInSeconds = 30;

        /// <summary>
        /// The maximum number of times we can try to resume a broken connection
        /// </summary>
        private const int MAX_RESUMPTION_ATTEMPTS = 3;

		/// <summary>
		/// The maximum number of times we can try to create a broken stream
		/// </summary>
		private const int MAX_STREAM_ATTEMPTS = 3;

        /// <summary>
        /// The current resumption attempt downward counter
        /// </summary>
        private int currentResumptionAttempt = 0;

		/// <summary>
		/// The current stream attempt downward counter
		/// </summary>
		private int currentStreamAttempt = 0;

		/// <summary>
		/// The last time any kind of confirmation was asked of the server.
        /// Acknowledgements, Resumption and so on. DO NOT KNOW IF I NEED THIS??
		/// </summary>
		private DateTime lastConfirmationAttemptServerTime = DateTime.MinValue;

        /// <summary>
        /// The last sequence number we have that was confirmed by the server
        /// </summary>
        private int lastConfirmedServerSequence = 0;

        /// <summary>
        /// When the server confirmed the above sequence number.
        /// </summary>
        private DateTime lastConfirmedServerTime = DateTime.MinValue;

        /// <summary>
        /// The maximum time without any kind of confirmation from the server.
        /// </summary>
        private int maxTimeBetweenConfirmationsInSeconds = 60;

        /// <summary>
        /// The namespace for stream management.
        /// </summary>
        const string STREAM_MANAGEMENT_NS = "urn:xmpp:sm:3";

		/// <summary>
		/// A global so we know if we are in the process of trying to resume the stream
		/// </summary>
		private bool isAttemptingStreamResumption = false;

		/// <summary>
		/// A record of when the last attempt to resume the stream started - must reset it on success.
		/// </summary>
		private DateTime? lastAttemptAtStreamResumptionTime = null;

		/// <summary>
		/// The max time we will wait before we regard resumption is failed and lost.
		/// </summary>
		private int maxStreamResumptionTimeoutInSecond = 30;

		/// <summary>
		/// A global so we know if we are in the process of trying to create a new stream
		/// </summary>
		private bool isAttemptingNewStream = false;

		/// <summary>
		/// A record of when the last attempt to create a new stream - must reset it on success.
		/// </summary>
		private DateTime? lastAttemptAtNewStreamTime = null;

		/// <summary>
		/// The max time we will wait before we regard creating a new stream has failed.
		/// </summary>
		private int maxNewStreamTimeoutInSecond = 30;

		/// <summary>
		/// The maximum number of times we can try to resume a broken connection
		/// </summary>
		private const int MAX_STANZAS_BEFORE_ACK_REQUEST = 3;

		/// <summary>
		/// The maximum time without any kind of acknowledgement request to the server.
		/// </summary>
		private int maxTimeBetweenAcknowledgementInSeconds = 20;


		/// <summary>
		/// The sequence of stanzas that has been sent for this connection.
		/// </summary>
		private int currentOutboundStanzaSequence = 0;

		/// <summary>
		/// The number of messages that have been receieved by this client.
		/// </summary>
		private int currentInboundStanzaSequence = 0;

        /// <summary>
        /// A cache of items that have been sent but not confirmed.
        /// </summary>
        private BlockingCollection<Stanza> stanzaQueueCache = new BlockingCollection<Stanza>();

		/// <summary>
		/// Stores the sequence identifier from the previous stream if there was a failure.
		/// Allows us to at least attempt a recovery.
		/// </summary>
		int? resumedStreamServerSequence = null;

		/// <summary>
		/// Enables stream management. You should listen for the StreamManagementEnabled event
		/// to know when it is ready.
		/// <param name="withresumption">Whether we should enabled resumption on the stream.</param>
		/// <param name="maxTimeout">The max timeout client request - the server can override this.</param>
		/// </summary>
		public void EnableStreamManagement(bool withresumption = true, int maxTimeout = 60)
        {
            // Send <enable xmlns='urn:xmpp:sm:3'/>
            XmlElement sm = Xml.Element("enable", STREAM_MANAGEMENT_NS);
            sm.SetAttribute("resume", withresumption.ToString().ToLower());
            sm.SetAttribute("max", maxTimeout.ToString());

            // send to the server - a message will be sent back later
            Send(sm);
		}

        /// <summary>
        /// The callback when stream management is enabled.
        /// </summary>
        /// <param name="enabled">Enabled.</param>
        private void HandleStreamManagementEnabledResponse(XmlElement enabled)
        {
            // IF WE ARE COMING FROM A STREAM THAT WAS BROUGHT BACK FROM A PREVIOUS FAILURE
            if (resumedStreamServerSequence.HasValue)
            {
                // update as the last sequence
                lastConfirmedServerSequence = resumedStreamServerSequence.Value;

                // from the last confirmed value up to the one it has now, remove from the cache
                for (int i = lastConfirmedServerSequence; i < resumedStreamServerSequence; i++)
                {
                    stanzaQueueCache.Take();
                }

                // now resend anything left over
                for (int i = 0; i < stanzaQueueCache.Count; i++)
                {
                    Stanza stanza = stanzaQueueCache.ElementAt(i);
                    Send(stanza, false);
                }

                // reset
                resumedStreamServerSequence = null;
            }
            else
            {
                // if we have anything in the cache (from a previously fauiled session) then send it
                for (int i = 0; i < stanzaQueueCache.Count; i++)
                {
                    Stanza stanza = stanzaQueueCache.ElementAt(i);
                    Send(stanza, false);
                }
            }

            // normal behaviour below ...

			// reset other variables - usually these are set when there was a previous stream
			isAttemptingNewStream = false;
			lastAttemptAtNewStreamTime = null;
			isAttemptingStreamResumption = false;
			lastAttemptAtStreamResumptionTime = null;
			lastConfirmedServerTime = DateTime.Now;
            currentResumptionAttempt = 0;   //reset for next time
            currentStreamAttempt = 0;
            currentOutboundStanzaSequence = 0;  //resets when it is a new stream
            currentInboundStanzaSequence = 0;

			// we have stream management enabled so lets get started
			streamManagementEnabled = true;
            resumptionEnabled = Boolean.Parse(enabled.GetAttribute("resume"));
            resumptionId = enabled.GetAttribute("id");
            int.TryParse(enabled.GetAttribute("max"), out maxResumptionPeriodInSeconds);

            // manage the stream uptime
            CheckStreamCycle();

            // throw an event to say we're ready with resumption
            StreamManagementEnabled.Raise(this, null);
        }

        /// <summary>
        /// This will periodically check whether the server connection is up and 
        /// if not it will kick of a process to try and resume it, or create a new stream.
        /// </summary>
        private void CheckStreamCycle()
        {
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = streamCycleCheckTimeInSeconds * 1000;
            timer.Enabled = true;

            // inside here we manage the stream uptime - AT THE MOMENT we assume one attempt at stream resumption and then one attempt at connecting
            timer.Elapsed += (sender, e) => {

                // if we are in the process of trying to create a new stream and that has been going on too long throw an error (for now)

                if (isAttemptingNewStream
                   && currentStreamAttempt > MAX_STREAM_ATTEMPTS
                   && lastAttemptAtNewStreamTime.HasValue
                   && DateTime.Now > lastAttemptAtNewStreamTime.Value.AddSeconds(maxNewStreamTimeoutInSecond))
                {
                    var connex = new XmppDisconnectionException("Unable to create a new connection in the time period.");
                    Error.Raise(this, new ErrorEventArgs(connex));
				}
				else if (isAttemptingNewStream && currentStreamAttempt > MAX_STREAM_ATTEMPTS)  // we are in the process of creating so let it run
					return;

                // if we are in the process of trying to resume and that has been going on too long, consider it failed and restart the stream
                if (isAttemptingStreamResumption
                   && currentResumptionAttempt > MAX_RESUMPTION_ATTEMPTS
                   && lastAttemptAtStreamResumptionTime.HasValue
                   && DateTime.Now > lastAttemptAtStreamResumptionTime.Value.AddSeconds(maxStreamResumptionTimeoutInSecond))
                {
                    // full stream restart - we cannot use resumption in this case as it is a brand new stream
                    isAttemptingNewStream = true;
                    lastAttemptAtNewStreamTime = DateTime.Now;

                    try
                    {
						// we will try getting the connection back again
						currentStreamAttempt++;
                                                
                        // try to create a new connection
                        Connect(this.resource);

                        // finally, we enable stream management if it is on - IF WE DO THIS HERE
                        // ARE THERE ANY RACE CONDITIONS BY RESETING THE VARIBLES ABOVE BEFORE THE RESPONSE ?
                        if (streamManagementEnabled)
                        {
                            // we will reset the variables below when we get a stream management response
                            EnableStreamManagement(resumptionEnabled, maxResumptionPeriodInSeconds);

							// send items we have in the cache - we don't know what failed
							for (int i = 0; i < stanzaQueueCache.Count; i++)
							{
								Stanza stanza = stanzaQueueCache.ElementAt(i);
								Send(stanza, false);
							}

                        } else {
                            
							// if successful then reset
							isAttemptingNewStream = false;
							lastAttemptAtNewStreamTime = null;
							isAttemptingStreamResumption = false;
							lastAttemptAtStreamResumptionTime = null;
							lastConfirmedServerTime = DateTime.Now;
                            currentResumptionAttempt = 0;   //reset for next time
                            currentStreamAttempt = 0;
                            currentOutboundStanzaSequence = 0;  //resets when it is a new stream
                            currentInboundStanzaSequence = 0;
						}

                        return;
                    }
                    catch {
                        
                        // a network error of some kind - timer will ensure a rerty is done shortly.
                        return;
                    }
                }

                // If we are not trying to resume the connection && have had no response from the server at all in a given period despite an attempt we will try to resume the stream
                if (!isAttemptingStreamResumption 
                    && DateTime.Now > lastConfirmedServerTime.AddSeconds(maxTimeBetweenConfirmationsInSeconds))
                {
                    // we will try getting the connection back again
                    currentResumptionAttempt++;

                    // try to resume the connection
                    ResumeStream();

                    // we don't want to send through a request until the stream says it is ready
                    return;
                }

				// if you ARE attempting a resumption but it has been going on too long then we need to try again
				if (isAttemptingStreamResumption
                    && lastAttemptAtStreamResumptionTime.HasValue
				    && DateTime.Now > lastAttemptAtStreamResumptionTime.Value.AddSeconds(maxStreamResumptionTimeoutInSecond))
				{
					// we will try getting the connection back again
					currentResumptionAttempt++;

					// try to resume the connection
					ResumeStream();

					// we don't want to send through a request until the stream says it is ready
					return;
				}

				// Normal path here - have we got to the threshhold of items added or the timeout for checks for acks?
				if ((currentOutboundStanzaSequence > 0 
                     && currentOutboundStanzaSequence % MAX_STANZAS_BEFORE_ACK_REQUEST == 0)
                        || DateTime.Now > lastConfirmedServerTime.AddSeconds(maxTimeBetweenAcknowledgementInSeconds))
				{
					// request for acknowlegement
					Send("<r xmlns='urn:xmpp:sm:3'/>");
				}
            };

            timer.Start();
        }

        /// <summary>
        /// This will try to resume a stream, often caused by a dropped connection.
        /// </summary>
        private void ResumeStream()
        {
            // don't run multiple of these
            if (isAttemptingStreamResumption) return;

            // set these management vars
            isAttemptingStreamResumption = true;
            lastAttemptAtStreamResumptionTime = DateTime.Now;

            // recreate the connection without binding
            Connect(this.resource, false);

			// Send <enable xmlns='urn:xmpp:sm:3'/>
			XmlElement rs = Xml.Element("resume", STREAM_MANAGEMENT_NS);
			rs.SetAttribute("h", lastConfirmedServerSequence.ToString());
			rs.SetAttribute("previd", resumptionId);

			// send to the server - a message will be sent back later
			Send(rs);            
        }

		/// <summary>
		/// Thrown when we receive an exception in stream management.
		/// </summary>
		/// <param name="failed">Failure element.</param>
		private void HandleStreamManagementFailedResponse(XmlElement failed)
		{
            /* 
             <failed xmlns='urn:xmpp:sm:3'
                    h='another-sequence-number'>
              <item-not-found xmlns='urn:ietf:params:xml:ns:xmpp-stanzas'/>
            </failed>
            */

            // I think it is supposed to create a new session when this happens - even if it is not the same
            // as the previous one, but this doesn't *seem* to be working. In this case, let's create a brand new
            // session.

            if (failed.FirstChild.LocalName == "item-not-found")
            {
                // store this so we can resend after the stream is back up
                if (failed.HasAttribute("h"))
                {
					// store the sequence so we can resend once we've connected
					resumedStreamServerSequence = int.Parse(failed.GetAttribute("h"));
                }

                // create a new connection
				Connect(this.resource);
				lastConfirmedServerTime = DateTime.Now;

				// ### reset as we are now no longer trying to resume the stream
				isAttemptingStreamResumption = false;
				lastAttemptAtStreamResumptionTime = null;
				currentResumptionAttempt = 0;   //reset for next time
				// ### 

				///// we will reset the variables below when we get a stream management response
				EnableStreamManagement(resumptionEnabled, maxResumptionPeriodInSeconds);
			}
			else
			{
				// we have some other issue
				var err = new XmppErrorException(new XmppError(failed));
				Error.Raise(this, new ErrorEventArgs(err));
			}

			// throw an event to say we're ready with resumption
			StreamResumed.Raise(this, null);
		}

		/// <summary>
		/// The callback when stream is resumed.
		/// </summary>
		/// <param name="resumed">Resumed xml element.</param>
		private void HandleResumedStreamResponse(XmlElement resumed)
		{
            // reset as we are now no longer trying to resume the stream
            isAttemptingStreamResumption = false;
            lastAttemptAtStreamResumptionTime = null;
            currentResumptionAttempt = 0;   //reset for next time

            // what is the last item the server is aware of and record at what point that is
            int sequence = int.Parse(resumed.GetAttribute("h"));

			// from the last confirmed value up to the one it has now, remove from the cache
			for (int i = lastConfirmedServerSequence; i < sequence; i++)
			{
				stanzaQueueCache.Take();
			}

            // now resend anything left over
            for (int i = 0; i < stanzaQueueCache.Count; i++)
            {
                Stanza stanza = stanzaQueueCache.ElementAt(i);
                Send(stanza, false);
            }

			// update as the last sequence
			lastConfirmedServerSequence = sequence;
			lastConfirmedServerTime = DateTime.Now;

			// throw an event to say we're ready with resumption
			StreamResumed.Raise(this, null);
		}

		/// <summary>
		/// When an acknowledgement event is recieved.
		/// </summary>
		/// <param name="ack">Ack element.</param>
		private void HandleAcknowledgementResponse(XmlElement ack)
        {
            // what sequence does the server have>
            int sequence = int.Parse(ack.GetAttribute("h"));

            // from the last confirmed value up to the one it has now, remove from the cache
            for (int i = lastConfirmedServerSequence; i < sequence; i++)
            {
                stanzaQueueCache.Take();
            }

            // ###
			isAttemptingStreamResumption = false;
			lastAttemptAtStreamResumptionTime = null;
			currentResumptionAttempt = 0;   //reset for next time

            // update as the last sequence
			lastConfirmedServerSequence = sequence;
            lastConfirmedServerTime = DateTime.Now;
        }

        #endregion

        /// <summary>
        /// Continously removes stanzas from the FIFO of incoming stanzas and raises
        /// the respective events.
        /// </summary>
        /// <remarks>This runs in the context of a separate thread. All stanza events
        /// are streamlined and execute in the context of this thread.</remarks>
        private void DispatchEvents()
        {
            while (true)
            {
                try
                {
                    Stanza stanza = stanzaQueue.Take(cancelDispatch.Token);
                    if (debugStanzas) System.Diagnostics.Debug.WriteLine(stanza.ToString());
                    if (stanza is Iq)
                        Iq.Raise(this, new IqEventArgs(stanza as Iq));
                    else if (stanza is Message)
                        Message.Raise(this, new MessageEventArgs(stanza as Message));
                    else if (stanza is Presence)
                        Presence.Raise(this, new PresenceEventArgs(stanza as Presence));
                }
                catch (OperationCanceledException)
                {
                    // Quit the task if it's been cancelled.
                    return;
                }
                catch (Exception e)
                {
                    // FIXME: What should we do if an exception is thrown in one of the
                    // event handlers?
                    System.Diagnostics.Debug.WriteLine("Error in XMPP Core: " + e.StackTrace + e.ToString());
                    //throw e;
                }
            }
        }

        /// <summary>
        /// Handles incoming IQ responses for previously issued IQ requests.
        /// </summary>
        /// <param name="iq">The received IQ response stanza.</param>
        private void HandleIqResponse(Iq iq)
        {
            string id = iq.Id;
            AutoResetEvent ev;
            Action<string, Iq> cb;
            iqResponses[id] = iq;
            // Signal the event if it's a blocking call.
            if (waitHandles.TryRemove(id, out ev))
                ev.Set();
            // Call the callback if it's an asynchronous call.
            else if (iqCallbacks.TryRemove(id, out cb))
                Task.Factory.StartNew(() => { cb(id, iq); });
        }

        /// <summary>
        /// Generates a unique id.
        /// </summary>
        /// <returns>A unique id.</returns>
        private string GetId()
        {
            Interlocked.Increment(ref id);
            return id.ToString();
        }

        /// <summary>
        /// Disconnects from the XMPP server.
        /// </summary>
        private void Disconnect()
        {
            if (!Connected)
                return;
            // Close the XML stream.
            Send("</stream:stream>");
            Connected = false;
            Authenticated = false;
        }
    }
}