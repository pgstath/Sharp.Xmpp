using Sharp.Xmpp.Core;
using Sharp.Xmpp.Extensions.Socks5;
using Sharp.Xmpp.Extensions.Stun;
using Sharp.Xmpp.Im;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Sharp.Xmpp.Extensions
{
    /// <summary>
    /// Implements the 'SOCKS5 Bytestreams' extension as defined in XEP-0065.
    /// </summary>
    internal class Socks5Bytestreams : XmppExtension, IInputFilter<Iq>, IDataStream
    {
        /// <summary>
        /// A reference to the 'SI File Transfer' XMPP extension.
        /// </summary>
        private SIFileTransfer siFileTransfer;

        /// <summary>
        /// A reference to the 'Entity Capabilities' extension instance.
        /// </summary>
        private EntityCapabilities ecapa;

        /// <summary>
        /// A reference to the 'Service Discovery' extension instance.
        /// </summary>
        private ServiceDiscovery sdisco;

        /// <summary>
        /// A reference to the 'Server IP Check' extension instance.
        /// </summary>
        private ServerIpCheck serverIpCheck;

        /// <summary>
        /// The default port to listen on or connect to if none is specified.
        /// </summary>
        private const int defaultPort = 1080;

        /// <summary>
        /// The number of milliseconds the SOCKS5 server waits for an incoming
        /// client connection, before aborting.
        /// </summary>
        private const int acceptTimeout = 1000 * 60 * 3;

        /// <summary>
        /// Defines, along with serverPortTo, a range of ports eligible for creating
        /// SOCKS5 servers on.
        /// </summary>
        private int serverPortFrom = 52000;

        /// <summary>
        /// Defines, along with serverPortFrom, a range of ports eligible for creating
        /// SOCKS5 servers on.
        /// </summary>
        private int serverPortTo = 52100;

        /// <summary>
        /// An enumerable collection of XMPP namespaces the extension implements.
        /// </summary>
        /// <remarks>This is used for compiling the list of supported extensions
        /// advertised by the 'Service Discovery' extension.</remarks>
        public override IEnumerable<string> Namespaces
        {
            get
            {
                return new string[] { "http://jabber.org/protocol/bytestreams" };
            }
        }

        /// <summary>
        /// The named constant of the Extension enumeration that corresponds to this
        /// extension.
        /// </summary>
        public override Extension Xep
        {
            get
            {
                return Extension.Socks5Bytestreams;
            }
        }

        /// <summary>
        /// The STUN server to use for determining the external IP address.
        /// </summary>
        public DnsEndPoint StunServer
        {
            get;
            set;
        }

        /// <summary>
        /// Determines whether usage of a proxy server is allowed.
        /// </summary>
        public bool ProxyAllowed
        {
            get;
            set;
        }

        /// <summary>
        /// A collection of user-defined SOCKS5 proxy servers.
        /// </summary>
        public ICollection<Streamhost> Proxies
        {
            get;
            private set;
        }

        /// <summary>
        /// Defines, along with the ServerPortTo property, a range of ports eligible
        /// for creating SOCKS5 servers on.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The property is being set
        /// and the value is negative, or the value is greater than the value of the
        /// ServerPortTo property.</exception>
        public int ServerPortFrom
        {
            get
            {
                return serverPortFrom;
            }

            set
            {
                value.ThrowIfOutOfRange(0, ServerPortTo);
                serverPortFrom = value;
            }
        }

        /// <summary>
        /// Defines, along with the ServerPortFrom property, a range of ports eligible
        /// for creating SOCKS5 servers on.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The property is being set
        /// and the value the value is smaller than the value of the ServerPortFrom
        /// property, or the value is greater than 65535.</exception>
        public int ServerPortTo
        {
            get
            {
                return serverPortTo;
            }

            set
            {
                value.ThrowIfOutOfRange(ServerPortFrom, 65535);
                serverPortTo = value;
            }
        }

        /// <summary>
        /// Determines whether usage of UPnP for automatic port-forwarding is
        /// allowed.
        /// </summary>
        public bool UseUPnP
        {
            get;
            set;
        }

        /// <summary>
        /// The event that is raised whenever bytes have been transferred.
        /// </summary>
        public event EventHandler<BytesTransferredEventArgs> BytesTransferred;

        /// <summary>
        /// The event that is raised when an on-going transfer was aborted
        /// before it completed.
        /// </summary>
        public event EventHandler<TransferAbortedEventArgs> TransferAborted;

        /// <summary>
        /// Invoked after all extensions have been loaded.
        /// </summary>
        public override void Initialize()
        {
            ecapa = im.GetExtension<EntityCapabilities>();
            siFileTransfer = im.GetExtension<SIFileTransfer>();
            sdisco = im.GetExtension<ServiceDiscovery>();
            serverIpCheck = im.GetExtension<ServerIpCheck>();
        }

        /// <summary>
        /// Invoked when an IQ stanza has been received.
        /// </summary>
        /// <param name="stanza">The stanza which has been received.</param>
        /// <returns>true to intercept the stanza or false to pass the stanza
        /// on to the next handler.</returns>
        public bool Input(Iq stanza)
        {
            if (stanza.Type != IqType.Set)
                return false;
            var query = stanza.Data["query"];
            if (query == null || query.NamespaceURI != "http://jabber.org/protocol/bytestreams")
                return false;
            string sid = query.GetAttribute("sid");
            // Verify the sid attribute of the query element.
            if (!VerifySession(stanza, sid))
            {
                im.IqError(stanza, ErrorType.Modify, ErrorCondition.NotAcceptable);
                return true;
            }
            if (query.GetAttribute("mode") == "udp")
            {
                im.IqError(stanza, ErrorType.Modify, ErrorCondition.FeatureNotImplemented,
                    "UDP-mode is not supported.");
                return true;
            }
            // Build a collection of Streamhosts.
            var hosts = ParseStreamhosts(query);
            if (hosts.Count() == 0)
            {
                im.IqError(stanza, ErrorType.Modify, ErrorCondition.BadRequest,
                    "No streamhosts advertised.");
                return true;
            }
            // Try to establish a SOCKS5 connection with one of the streamhosts.
            // Spawn off another task for this, so that we don't block the
            // dispatcher thread.
            Task.Run(() =>
            {
                using (var client = EstablishConnection(stanza, sid, hosts))
                {
                    // We can not start reading data from the network-stream.
                    ReceiveData(stanza, sid, client.GetStream());
                }
            });
            // We took care of this IQ request, so intercept it and don't pass it
            // on to other handlers.
            return true;
        }

        /// <summary>
        /// Initializes a new instance of the Socks5Bytestreams class.
        /// </summary>
        /// <param name="im">A reference to the XmppIm instance on whose behalf this
        /// instance is created.</param>
        public Socks5Bytestreams(XmppIm im)
            : base(im)
        {
            StunServer = new DnsEndPoint("stun.l.google.com", 19302);
            ProxyAllowed = true;
            Proxies = new HashSet<Streamhost>();
            UseUPnP = false;
#if WINDOWSPLATFORM
			UseUPnP = true;
#endif
        }

        /// <summary>
        /// Performs the actual data-transfer implied by the specified
        /// SI session.
        /// </summary>
        /// <param name="session">The SI session whose data to transfer.</param>
        /// <exception cref="ArgumentNullException">The session parameter is
        /// null.</exception>
        /// <exception cref="NotSupportedException">The XMPP extension
        /// implementing this method is not supported by the intended recipient's
        /// XMPP client.</exception>
        /// <exception cref="XmppErrorException">The server or the XMPP entity
        /// with the specified JID returned an XMPP error code. Use the Error
        /// property of the XmppErrorException to obtain the specific error
        /// condition.</exception>
        /// <exception cref="XmppException">The server returned invalid data or
        /// another unspecified XMPP error occurred.</exception>
        public void Transfer(SISession session)
        {
            IEnumerable<Streamhost> proxies = null;
            // Determine if this is going to be a direct or a mediated transfer.
            if (ProxyAllowed)
            {
                try
                {
                    var externalAddresses = GetExternalAddresses();
                    // If all of our external addresses are behind NATs, we may need a proxy.
                    bool behindNAT = externalAddresses.All(addr => BehindNAT(addr));
                    if (behindNAT)
                    {
                        // Look for user-defined proxies first.
                        if (Proxies.Count > 0)
                            proxies = Proxies;
                        else
                        {
                            // Otherwise query XMPP server for a list of proxies.
                            proxies = GetProxyList();
                        }
                    }
                }
                catch
                {
                    // Retrieving the external addresses may fail as well as querying
                    // the XMPP server for proxies. In these cases, we at least try
                    // with a direct transfer.
                }
            }

            try
            {
                if (proxies != null && proxies.Count() > 0)
                    MediatedTransfer(session, proxies);
                else
                    DirectTransfer(session);
            }
            catch (Exception)
            {
                // Raise the 'TransferAborted' event.
                TransferAborted.Raise(this, new TransferAbortedEventArgs(session));
                // Invalidate the session.
                siFileTransfer.InvalidateSession(session.Sid);
            }
        }

        /// <summary>
        /// Cancels the data-transfer implied by the specified SI session.
        /// </summary>
        /// <param name="session">The SI session whose data-transfer to
        /// cancel.</param>
        /// <exception cref="ArgumentNullException">The session parameter is
        /// null.</exception>
        public void CancelTransfer(SISession session)
        {
            session.ThrowIfNull("session");
            siFileTransfer.InvalidateSession(session.Sid);
        }

        /// <summary>
        /// Calculates the SHA-1 hash value for the specified string.
        /// </summary>
        /// <param name="s">The string to calculate the SHA-1 hash value for.</param>
        /// <returns>A SHA-1 hash as a hex-string.</returns>
        /// <exception cref="ArgumentNullException">The s parameter is
        /// null.</exception>
        private string Sha1(string s)
        {
            s.ThrowIfNull("s");
            using (var sha1 = new SHA1Managed())
            {
                byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(s));
                StringBuilder builder = new StringBuilder();
                foreach (byte h in hash)
                    builder.Append(h.ToString("x2"));
                return builder.ToString();
            }
        }

        /// <summary>
        /// Verifies the specified session-id.
        /// </summary>
        /// <param name="stanza">The original IQ stanza.</param>
        /// <param name="sid">The session-id to verify.</param>
        /// <returns>true if the specified session-id is valid; Otherwise
        /// false.</returns>
        private bool VerifySession(Iq stanza, string sid)
        {
            if (String.IsNullOrEmpty(sid))
                return false;
            var session = siFileTransfer.GetSession(sid, stanza.From, im.Jid);
            return session != null;
        }

        /// <summary>
        /// Parses the 'streamhost' elements contained in the specified 'query'
        /// XML element.
        /// </summary>
        /// <param name="query">The 'query' XML element containing a list of
        /// streamhost child-elements.</param>
        /// <returns>An enumerable collection of Streamhost instances.</returns>
        private IEnumerable<Streamhost> ParseStreamhosts(XmlElement query)
        {
            // List preserves the order of the elements as is recommended in XEP-0065.
            IList<Streamhost> list = new List<Streamhost>();
            foreach (XmlElement e in query.GetElementsByTagName("streamhost"))
            {
                try
                {
                    string jid = e.GetAttribute("jid"),
                        host = e.GetAttribute("host"), p = e.GetAttribute("port");
                    // The 'port' attribute is optional.
                    int port = String.IsNullOrEmpty(p) ? defaultPort : int.Parse(p);
                    list.Add(new Streamhost(jid, host, port));
                }
                catch
                {
                    // Ignore any erroneous elements.
                }
            }
            return list;
        }

        /// <summary>
        /// Establishes a SOCKS5 connection with one of the streamhosts in the
        /// specified collection of streamhosts.
        /// </summary>
        /// <param name="stanza">The original requesting IQ stanza.</param>
        /// <param name="sid">The session-id associated with this request.</param>
        /// <param name="hosts">An enumerable collection of Streamhost
        /// instances.</param>
        /// <returns>An initialized instance of the Socks5Client class
        /// representing the established SOCKS5 connection.</returns>
        /// <exception cref="Socks5Exception">An error occurred during SOCKS5
        /// negotiation.</exception>
        /// <exception cref="XmppException">No connection to any of the provided
        /// streamhosts could be established.</exception>
        private Socks5Client EstablishConnection(Iq stanza, string sid, IEnumerable<Streamhost> hosts)
        {
            // Try to establish a SOCKS5 connection to any of the streamhosts in the
            // collection.
            bool connected = false;
            foreach (var host in hosts)
            {
                try
                {
                    var client = new Socks5Client(host.Host, host.Port);
                    connected = true;
                    // Send the SOCKS5 Connect command.
                    string hostname = Sha1(sid + stanza.From + stanza.To);
                    SocksReply reply = client.Request(SocksCommand.Connect, hostname, 0);
                    if (reply.Status != ReplyStatus.Succeeded)
                        throw new Socks5Exception("SOCKS5 Connect request failed.");
                    // Send acknowledging IQ-result.
                    im.IqResult(stanza,
                        Xml.Element("query", "http://jabber.org/protocol/bytestreams")
                        .Attr("sid", sid).Child(Xml.Element("streamhost-used")
                        .Attr("jid", host.Jid.ToString()))
                    );
                    return client;
                }
                catch
                {
                    // Fall through and try the next streamhost.
                    if (connected)
                        break;
                }
            }
            // Still here means we couldn't connect to any of the streamhosts or
            // an error occurred during SOCKS5 negotiation.
            im.IqError(stanza, ErrorType.Cancel, ErrorCondition.ItemNotFound);
            // Error out.
            throw new XmppException("Couldn't connect to streamhost.");
        }

        /// <summary>
        /// Receives the actual data after SOCKS5 negotiation has been completed.
        /// </summary>
        /// <param name="stanza">The original requesting IQ stanza.</param>
        /// <param name="sid">The session-id associated with the
        /// data-transfer.</param>
        /// <param name="stream">The stream from which to read the incoming
        /// data.</param>
        /// <exception cref="XmppException">The specifid SID does not denote a
        /// valid SI session.</exception>
        private void ReceiveData(Iq stanza, string sid, Stream stream)
        {
            SISession session = siFileTransfer.GetSession(sid, stanza.From, stanza.To);
            if (session == null)
                throw new XmppException("Invalid session-id: " + sid);
            long left = session.Size;
            try
            {
                while (left > 0)
                {
                    byte[] buffer = new byte[4096];
                    int read = stream.Read(buffer, 0, buffer.Length);
                    if (read <= 0)
                        break;
                    left = left - read;
                    session.Stream.Write(buffer, 0, read);
                    // Update the byte count and raise the 'BytesTransferred' event.
                    session.Count = session.Count + read;
                    BytesTransferred.Raise(this, new BytesTransferredEventArgs(session));
                }
            }
            catch (ObjectDisposedException)
            {
                // This means the IO-stream has been disposed because we cancelled
                // the transfer. Just fall through.
            }
            finally
            {
                // Tear down the SI session.
                siFileTransfer.InvalidateSession(sid);
                // If not all bytes have been transferred, the data-transfer must have
                // been aborted prematurely.
                if (session.Count < session.Size)
                    TransferAborted.Raise(this, new TransferAbortedEventArgs(session));
            }
        }

        /// <summary>
        /// Gets the full network address of the SOCKS5 proxy with the specified JID.
        /// </summary>
        /// <param name="jid">The JID of the SOCKS5 proxy to retrieve the full
        /// network address for.</param>
        /// <returns>The full network address of the SOCKS5 proxy with the specified
        /// JID.</returns>
        /// <exception cref="ArgumentNullException">The jid parameter is
        /// null.</exception>
        /// <exception cref="XmppErrorException">The server returned an XMPP error code.
        /// Use the Error property of the XmppErrorException to obtain the specific
        /// error condition.</exception>
        /// <exception cref="XmppException">The server returned invalid data or another
        /// unspecified XMPP error occurred.</exception>
        private Streamhost GetNetworkAddress(Jid jid)
        {
            jid.ThrowIfNull("jid");
            var iq = im.IqRequest(Core.IqType.Get, jid, im.Jid,
                Xml.Element("query", "http://jabber.org/protocol/bytestreams"));
            if (iq.Type == Core.IqType.Error)
            {
                throw Util.ExceptionFromError(iq, "The network address could not be " +
                    "retrieved.");
            }
            var query = iq.Data["query"];
            if (query == null || query.NamespaceURI !=
                "http://jabber.org/protocol/bytestreams")
            {
                throw new XmppException("Erroneous server response.");
            }
            // Parse the streamhost element.
            IEnumerable<Streamhost> hosts = ParseStreamhosts(query);
            if (hosts.Count() < 1)
                throw new XmppException("No streamhost element found.");
            return hosts.First();
        }

        /// <summary>
        /// Queries the client's server for a list of SOCKS5 proxies.
        /// </summary>
        /// <returns>An enumerable collection of SOCKS5 proxies available to
        /// the client.</returns>
        /// <exception cref="NotSupportedException">The client's XMPP server
        /// does not support querying for proxy information.</exception>
        /// <exception cref="XmppErrorException">The server returned an XMPP error code.
        /// Use the Error property of the XmppErrorException to obtain the specific
        /// error condition.</exception>
        /// <exception cref="XmppException">The server returned invalid data or another
        /// unspecified XMPP error occurred.</exception>
        private IEnumerable<Streamhost> GetProxyList()
        {
            ISet<Streamhost> set = new HashSet<Streamhost>();
            foreach (var item in sdisco.GetItems(im.Jid.Domain))
            {
                // Query each item for its identities and look for a 'proxy' identity.
                foreach (var ident in sdisco.GetIdentities(item.Jid))
                {
                    // It's a SOCKS5 proxy.
                    if (ident.Category == "proxy" && ident.Type == "bytestreams")
                    {
                        // Get the full network address.
                        var address = GetNetworkAddress(item.Jid);
                        set.Add(address);
                    }
                }
            }
            return set;
        }

        /// <summary>
        /// Retrieves an enumerable collection of th XMPP client's external IP
        /// addresses.
        /// </summary>
        /// <returns>An enumerable collection of the XMPP client's external IP
        /// addresses.</returns>
        /// <exception cref="NotSupportedException">The external IP address(es)
        /// could not be determined.</exception>
        /// <remarks>In simple configurations this should usually yield one external
        /// IP address only.</remarks>
        private IEnumerable<IPAddress> GetExternalAddresses()
        {
            // Use a set so we don't get duplicate addresses.
            ISet<IPAddress> set = new HashSet<IPAddress>();
            try
            {
                set.Add(serverIpCheck.GetExternalAddress());
            }
            catch
            {
                // Fall through if server does not support the 'Server IP Check' extension.
            }
            // Next, try to retrieve external IP addresses from UPnP-enabled devices.
            if (UseUPnP)
            {
#if WINDOWSPLATFORM
                try {
					foreach (var address in UPnP.GetExternalAddresses())
						set.Add(address);
				} catch (Exception) {
					// Fall through in case any device querying goes wrong.
				}
#endif
            }
            // Finally, perform a STUN query.
            try
            {
                set.Add(StunClient.Query(StunServer.Host, StunServer.Port, 3000));
            }
            catch
            {
                // Nothing to do here.
            }
            // See if we could gather any external addresses at all.
            if (set.Count == 0)
            {
                throw new NotSupportedException("The external IP address(es) could not " +
                    "be obtained.");
            }
            return set;
        }

        /// <summary>
        /// Determines whether the specified IP address is located behind a NAT.
        /// </summary>
        /// <param name="address">The IP address for which to determine if it's
        /// behind a NAT.</param>
        /// <returns>true if the IP address is possibly located behind a NAT;
        /// Otherwise false.</returns>
        /// <exception cref="ArgumentNullException">The address parameter is
        /// null.</exception>
        /// <remarks>This is a best effort that is probably not completely fail
        /// safe.</remarks>
        private bool BehindNAT(IPAddress address)
        {
            address.ThrowIfNull("address");
            // See if the specified address is assigned to one of the network interfaces
            // of the system. If it isn't, we assume it's behind a NAT.
            try
            {
                return GetIpAddresses().SingleOrDefault(
                    addr => addr.Equals(address)) == null;
            }
            //FIXME
            //If an exception is raised we take foregranted we are behind NAT?
            catch
            {
                return true;
            }
            //END FIXME
        }

        /// <summary>
        /// Returns an enumerable collection of the system's IP addresses.
        /// </summary>
        /// <param name="address">If specified, only IP addresses that are in
        /// the same subnet as the address specified are returned. If this is
        /// null, all of the system's network interfaces IP addresses are
        /// returned.</param>
        /// <returns>An enumerable collection of IP addresses.</returns>
        /// <remarks>This only accounts for IPv4 addresses.</remarks>
        public static IEnumerable<IPAddress> GetIpAddresses(IPAddress address = null)
        {
#if WINDOWSPLATFORM
            return WindowsGetIpAddresses(address);
#else
            return AndroidGetIpAddresses(address);
#endif
        }

        /// <summary>
        /// Returns an enumerable collection of the system's IP addresses for Windows Platform.
        /// </summary>
        /// <param name="address">If specified, only IP addresses that are in
        /// the same subnet as the address specified are returned. If this is
        /// null, all of the system's network interfaces IP addresses are
        /// returned.</param>
        /// <returns>An enumerable collection of IP addresses.</returns>
        /// <remarks>This only accounts for IPv4 addresses.</remarks>
        private static IEnumerable<IPAddress> WindowsGetIpAddresses(IPAddress address = null)
        {
            ISet<IPAddress> set = new HashSet<IPAddress>();
            var netInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            //FIXME
            //NetworkInterface.GetAllNetworkInterfaces does not work in XAMARIN, in Android etc.
            //Thus an exception is raised and we catch it upstream
            //FIXME
            //http://stackoverflow.com/questions/17868420/networkinterface-getallnetworkinterfaces-returns-interfaces-with-operationalst
            //http://developer.xamarin.com/recipes/ios/network/reachability/detect_if_network_is_available/
            if (netInterfaces == null)
            {
                throw new NotImplementedException();
            }
            //END FIXME
            foreach (var ni in netInterfaces)
            {
                // Skip interfaces that aren't up.
                if (ni.OperationalStatus != OperationalStatus.Up)
                    continue;
                foreach (var ip in ni.GetIPProperties().UnicastAddresses)
                {
                    // Skip loopback address.
                    if (IPAddress.IsLoopback(ip.Address))
                        continue;
                    // FIXME: Also return IPv6 addresses?
                    if (ip.Address.AddressFamily != AddressFamily.InterNetwork)
                        continue;
                    if (address != null)
                    {
                        // Check if both IPs are in the same subnet.
                        if (!ip.Address.InSameSubnet(address, ip.IPv4Mask))
                            continue;
                    }
                    set.Add(ip.Address);
                }
            }
            return set;
        }

        /// <summary>
        /// Returns an enumerable collection of the system's IP addresses for Android Platform.
        /// </summary>
        /// <param name="address">If specified, only IP addresses that are in
        /// the same subnet as the address specified are returned. If this is
        /// null, all of the system's network interfaces IP addresses are
        /// returned.</param>
        /// <returns>An enumerable collection of IP addresses.</returns>
        /// <remarks>This only accounts for IPv4 addresses.</remarks>
        private static IEnumerable<IPAddress> AndroidGetIpAddresses(IPAddress address = null)
        {
            ISet<IPAddress> set = new HashSet<IPAddress>();
            //var connectivityManager = (ConnectivityManager)GetSystemService(ConnectivityService);

            var netInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            //FIXME
            //NetworkInterface.GetAllNetworkInterfaces does not work in XAMARIN, in Android etc.
            //Thus an exception is raised and we catch it upstream
            //FIXME
            //http://stackoverflow.com/questions/17868420/networkinterface-getallnetworkinterfaces-returns-interfaces-with-operationalst
            //http://developer.xamarin.com/recipes/ios/network/reachability/detect_if_network_is_available/
            if (netInterfaces == null)
            {
                throw new NotImplementedException();
            }
            return null;
        }

        /// <summary>
        /// Performs a direct transfer, meaning we act as a SOCKS5 server.
        /// </summary>
        /// <param name="session">The SI session whose data to transfer.</param>
        /// <exception cref="Socks5Exception">The SOCKS5 server could not be
        /// instantiated.</exception>
        /// <exception cref="XmppErrorException">The server returned an XMPP error code.
        /// Use the Error property of the XmppErrorException to obtain the specific
        /// error condition.</exception>
        /// <exception cref="XmppException">The server returned invalid data or another
        /// unspecified XMPP error occurred.</exception>
        private void DirectTransfer(SISession session)
        {
            // Create the listening SOCKS5 server.
            Socks5Server socks5Server = null;
            try
            {
                socks5Server = CreateSocks5Server(serverPortFrom, serverPortTo);
            }
            catch (Exception e)
            {
                throw new Socks5Exception("The SOCKS5 server could not be created.", e);
            }
            IEnumerable<IPAddress> externalAddresses = null;
            try
            {
                externalAddresses = GetExternalAddresses();
                // Check if we might need to forward the server port.
#if WINDOWSPLATFORM
                if (externalAddresses.Any(addr => BehindNAT(addr)) && UseUPnP) {
					try {
						UPnP.ForwardPort(socks5Server.Port, ProtocolType.Tcp,
							"XMPP SOCKS5 File-transfer");
					} catch (InvalidOperationException) {
						// If automatic port forwarding failed for whatever reason, just
						// go on normally. The user can still configure forwarding manually.
					}
				}
#endif
            }
            catch (NotSupportedException)
            {
                // Not much we can do.
            }
            // Waiting for a client connection is a blocking call and we need to
            // negotiate the SOCKS5 connection after we send the IQ request but
            // _before_ we wait for the IQ response.
            Task.Run(() =>
            {
                try
                {
                    AcceptClientConnection(session, socks5Server, acceptTimeout);
                    SendData(session, socks5Server.GetStream());
                }
                finally
                {
                    socks5Server.Close();
                }
            });

            // Send target a list of streamhosts, one for each active network interface.
            var xml = Xml.Element("query", "http://jabber.org/protocol/bytestreams")
                .Attr("sid", session.Sid);
            // Compile a set of all our IP addresses that we can advertise.
            ISet<IPAddress> ips = new HashSet<IPAddress>();
            if (externalAddresses != null)
                ips.UnionWith(externalAddresses);
            ips.UnionWith(GetIpAddresses());
            foreach (var ip in ips)
            {
                xml.Child(Xml.Element("streamhost")
                    .Attr("jid", im.Jid.ToString())
                    .Attr("host", ip.ToString())
                    .Attr("port", socks5Server.Port.ToString()));
            }
            // Send IQ with streamhosts to the target.
            var iq = im.IqRequest(IqType.Set, session.To, im.Jid, xml);
            if (iq.Type == IqType.Error)
            {
                throw Util.ExceptionFromError(iq, "The SOCKS5 connection could not " +
                    "be established.");
            }
        }

        /// <summary>
        /// Performs a mediated transfer, meaning we send the data over a proxy server.
        /// </summary>
        /// <param name="session">The SI session whose data to transfer.</param>
        /// <param name="proxies"></param>
        /// <exception cref="Socks5Exception">The SOCKS5 connection to the designated
        /// proxy server could not be established.</exception>
        /// <exception cref="XmppErrorException">The server returned an XMPP error code.
        /// Use the Error property of the XmppErrorException to obtain the specific
        /// error condition.</exception>
        /// <exception cref="XmppException">The server returned invalid data or another
        /// unspecified XMPP error occurred.</exception>
        private void MediatedTransfer(SISession session, IEnumerable<Streamhost> proxies)
        {
            var proxy = NegotiateProxy(session, proxies);
            // Connect to the designated proxy.
            using (var client = new Socks5Client(proxy.Host, proxy.Port))
            {
                // Send the SOCKS5 Connect command.
                string hostname = Sha1(session.Sid + session.From + session.To);
                SocksReply reply = client.Request(SocksCommand.Connect, hostname, 0);
                if (reply.Status != ReplyStatus.Succeeded)
                    throw new Socks5Exception("SOCKS5 Connect request failed.");
                // Activate the bytetream.
                var xml = Xml.Element("query", "http://jabber.org/protocol/bytestreams")
                    .Attr("sid", session.Sid).Child(
                    Xml.Element("activate").Text(session.To.ToString()));
                Iq iq = im.IqRequest(IqType.Set, proxy.Jid, im.Jid, xml);
                if (iq.Type == IqType.Error)
                    throw Util.ExceptionFromError(iq, "Could not activate the bytestream.");
                // Finally, go ahead and send the data to the proxy.
                SendData(session, client.GetStream());
            }
        }

        /// <summary>
        /// Negotiates with the target which of the specified SOCKS5 proxies to use.
        /// </summary>
        /// <param name="session">The SI session whose data to transfer.</param>
        /// <param name="proxies">An enumerable collection of SOCKS5 proxy servers
        /// to advertise to the target.</param>
        /// <returns>The proxy server to use for the data transfer.</returns>
        /// <exception cref="XmppErrorException">The server returned an XMPP error code.
        /// Use the Error property of the XmppErrorException to obtain the specific
        /// error condition.</exception>
        /// <exception cref="XmppException">The server returned invalid data or another
        /// unspecified XMPP error occurred.</exception>
        private Streamhost NegotiateProxy(SISession session, IEnumerable<Streamhost> proxies)
        {
            // Compile XML containing our list of proxy servers.
            var xml = Xml.Element("query", "http://jabber.org/protocol/bytestreams")
                .Attr("sid", session.Sid);
            foreach (var proxy in proxies)
            {
                xml.Child(Xml.Element("streamhost")
                    .Attr("jid", proxy.Jid.ToString())
                    .Attr("host", proxy.Host)
                    .Attr("port", proxy.Port.ToString()));
            }
            // Wait for the other site to tell us which proxy server it selected.
            var iq = im.IqRequest(IqType.Set, session.To, im.Jid, xml);
            if (iq.Type == IqType.Error)
                throw Util.ExceptionFromError(iq, "The SOCKS5 negotiation failed.");
            var query = iq.Data["query"];
            if (query == null || query.NamespaceURI !=
                "http://jabber.org/protocol/bytestreams")
            {
                throw new XmppException("Erroneous response.");
            }
            if (query.GetAttribute("sid") != session.Sid)
                throw new XmppException("Invalid session identifier.");
            var used = query["streamhost-used"];
            if (used == null)
                throw new XmppException("Missing streamhost-used element.");
            var proxyJid = used.GetAttribute("jid");
            var streamhost = proxies.FirstOrDefault(proxy => proxy.Jid == proxyJid);
            if (streamhost == null)
                throw new XmppException("Invalid streamhost JID.");
            return streamhost;
        }

        /// <summary>
        /// Creates a SOCKS5 server bound to a port from the range defined by the
        /// specified values.
        /// </summary>
        /// <param name="portFrom">The lower (inclusive) bound of the port
        /// range.</param>
        /// <param name="portTo">The upper (inclusive) bound of the port
        /// range.</param>
        /// <returns>An initialized instance of the Socks5Server class.</returns>
        /// <exception cref="SocketException">An error occurred while creating or
        /// accessing the SOCKS5 socket.</exception>
        /// <exception cref="ArgumentException">All ports of the specified range
        /// are already in use.</exception>
        private Socks5Server CreateSocks5Server(int portFrom, int portTo)
        {
            for (int port = portFrom; port <= portTo; port++)
            {
                try
                {
                    return new Socks5Server(port);
                }
                catch (SocketException e)
                {
                    if (e.SocketErrorCode != SocketError.AddressAlreadyInUse)
                        throw;
                }
            }
            throw new ArgumentException("All ports of the specified range are " +
                "already in use.");
        }

        /// <summary>
        /// Waits for a client connection to be made and subsequently verifies it.
        /// </summary>
        /// <param name="session">The SI session whose data to transfer.</param>
        /// <param name="server">The instance of the SOCKS5 server to accept
        /// client connections on.</param>
        /// <param name="timeout">The number of milliseconds to wait for a client
        /// connection before returning to the caller, or -1 to wait
        /// indefinitely.</param>
        /// <exception cref="SocketException">An error occurred when accessing the
        /// underlying socket of the SOCKS5 server instance.</exception>
        /// <exception cref="Socks5Exception">The SOCKS5 negotiation with the client
        /// failed.</exception>
        /// <exception cref="TimeoutException">A timeout was specified and it
        /// expired.</exception>
        /// <exception cref="IOException">The stream could not be read, or the
        /// operation timed out.</exception>
        private void AcceptClientConnection(SISession session, Socks5Server server,
            int timeout = -1)
        {
            var request = server.Accept(timeout);
            if (request.Command != SocksCommand.Connect)
            {
                throw new Socks5Exception("Unexpected SOCKS5 command: " +
                    request.Command);
            }
            if (request.ATyp != ATyp.Domain)
                throw new Socks5Exception("Unexpected ATyp: " + request.ATyp);
            string hash = (string)request.Destination;
            // Calculate the SHA-1 hash and compare it with the one in the request.
            string calculated = Sha1(session.Sid + im.Jid + session.To);
            if (calculated != hash)
                throw new Socks5Exception("Hostname hash mismatch.");
            // We're good to go.
            server.Reply(ReplyStatus.Succeeded, hash, request.Port);
        }

        /// <summary>
        /// Sends the actual data associated with the specified session over the
        /// specified stream.
        /// </summary>
        /// <param name="session">The SI session whose data to transfer.</param>
        /// <param name="stream">The (network) stream representing the connection
        /// to the client.</param>
        private void SendData(SISession session, Stream stream)
        {
            long left = session.Size;
            try
            {
                while (left > 0)
                {
                    byte[] buffer = new byte[4096];
                    int read = session.Stream.Read(buffer, 0,
                        (int)Math.Min(left, buffer.Length));
                    if (read > 0)
                        stream.Write(buffer, 0, read);
                    else
                        break;
                    left = left - read;
                    // Update the byte count and raise the 'BytesTransferred' event.
                    session.Count = session.Count + read;
                    BytesTransferred.Raise(this, new BytesTransferredEventArgs(session));
                }
            }
            catch (ObjectDisposedException)
            {
                // This means the IO-stream has been disposed because we cancelled
                // the transfer. Just fall through.
            }
            finally
            {
                // Tear down the SI session.
                siFileTransfer.InvalidateSession(session.Sid);
                // If not all bytes have been transferred, the data-transfer must have
                // been aborted prematurely.
                if (session.Count < session.Size)
                    TransferAborted.Raise(this, new TransferAbortedEventArgs(session));
            }
        }
    }
}