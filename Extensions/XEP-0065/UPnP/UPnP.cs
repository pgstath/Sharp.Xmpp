#if WINDOWSPLATFORM
using UPNPLib;
#endif

namespace XMPPEngineer.Extensions.Upnp
{
#if WINDOWSPLATFORM

	/// <summary>
	/// Provides static methods for retrieving the external IP address from UPnP-enabled
	/// routers as well as methods for managing automatic port-forwarding.
	/// </summary>
	/// <remarks>
	/// This leverages the "UPnP Control Point" COM Component which is part of Windows.
	/// For more details on UPnP, refer to the "UPnP Device Architecture" document and
	/// the "Internet Gateway Device" descriptions, specifically the "WANIPConnection:1"
	/// service template.
	/// </remarks>
	internal static class UPnP {
		/// <summary>
		/// Search target for devices that expose the WANIPConnection service.
		/// </summary>
		static string stIpConnection = "urn:schemas-upnp-org:service:WANIPConnection:1";
		/// <summary>
		/// A collection of cached UPnP-devices.
		/// </summary>
		/// <remarks>
		/// Some routers don't like too frequent SSDP multicasts so we only perform one
		/// every once in a while and cache the result.
		/// </remarks>
		static IEnumerable<UPnPDevice> cachedDevices;
		/// <summary>
		/// The last time we performed an SSDP multicast.
		/// </summary>
		static DateTime lastUPnPFind = DateTime.MinValue;

		/// <summary>
		/// Retrieves an enumerable collection of external IP addresses from
		/// UPnP-enabled routers.
		/// </summary>
		/// <returns>A collection of external IP addresses.</returns>
		/// <exception cref="InvalidOperationException">Querying for UPnP-enabled
		/// devices failed, or an error occurred while obtaining an external IP
		/// address.</exception>
		public static IEnumerable<IPAddress> GetExternalAddresses() {
			ISet<IPAddress> set = new HashSet<IPAddress>();
			VerifyCache();
			foreach (UPnPDevice wanDevice in cachedDevices) {
				string ip = wanDevice.InvokeAction<string>(stIpConnection,
					"GetExternalIPAddress");
				try {
					set.Add(IPAddress.Parse(ip));
				} catch (Exception e) {
					throw new InvalidOperationException("Invalid IP address.", e);
				}
			}
			return set;
		}

		/// <summary>
		/// Forwards the specified port, optionally for the specified protocol only.
		/// </summary>
		/// <param name="port">The port to forward.</param>
		/// <param name="protocol">If specified, must be either ProcotolType.Tcp or
		/// ProtocolType.Udp to forward the port only for the TCP or the UDP protocol,
		/// respectively. If this is null, the port will be forwarded for both
		/// protocols.</param>
		/// <param name="description">A description of the port-forwarding rule that
		/// may be displayed in the UPnP-device's user interface to help the user
		/// better understand what the port-forwarding rule is for.</param>
		/// <exception cref="ArgumentOutOfRangeException">The port parameter is not
		/// between 0 and 65535.</exception>
		/// <exception cref="ArgumentException">The protocol parameter has a value
		/// other than ProtocolType.Tcp or ProtocolType.Udp.</exception>
		/// <exception cref="InvalidOperationException">Querying for UPnP-enabled
		/// devices failed, or an error occurred while trying to forward the
		/// specified port.</exception>
		public static void ForwardPort(int port,
			ProtocolType? protocol = null, string description = null) {
			port.ThrowIfOutOfRange("port", 0, 65535);
			if (protocol.HasValue && protocol != ProtocolType.Tcp &&
				protocol != ProtocolType.Udp) {
				throw new ArgumentException("The argument parameter must be either " +
					"ProtocolType.Tcp or ProtocolType.Udp");
			}
			VerifyCache();
			foreach (UPnPDevice wanDevice in cachedDevices) {
				// Get local IP addresses of WAN device.
				if (!(wanDevice is IUPnPDeviceDocumentAccess))
					continue;
				var uPnPDoc = wanDevice as IUPnPDeviceDocumentAccess;
				IEnumerable<IPAddress> localIPs = uPnPDoc.GetLocalIpAddresses();
				// Check if any of the WAN device's addresses are in the same subnet
				// as any of the host's addresses.
				foreach (IPAddress local in localIPs) {
					var ips = Socks5Bytestreams.GetIpAddresses(local);
					// If so, perform port forwarding.
					foreach (var ip in ips)
						ForwardPort(wanDevice, ip, port, protocol, description);
				}
			}
		}

		/// <summary>
		/// Removes existing port-mappings of the specified port.
		/// </summary>
		/// <param name="port">The port to "unforward".</param>
		/// <param name="protocol">If specified, must be either ProcotolType.Tcp or
		/// ProtocolType.Udp to unforward the port only for the TCP or the UDP protocol,
		/// respectively. If this is null, the port will be unforwarded for both
		/// protocols.</param>
		/// <exception cref="ArgumentOutOfRangeException">The port parameter is not
		/// between 0 and 65535.</exception>
		/// <exception cref="ArgumentException">The protocol parameter has a value
		/// other than ProtocolType.Tcp or ProtocolType.Udp.</exception>
		public static void UnforwardPort(int port, ProtocolType? protocol = null) {
			port.ThrowIfOutOfRange("port", 0, 65535);
			if (protocol.HasValue && protocol != ProtocolType.Tcp &&
				protocol != ProtocolType.Udp) {
				throw new ArgumentException("The argument parameter must be either " +
					"ProtocolType.Tcp or ProtocolType.Udp");
			}
			VerifyCache();
			foreach (UPnPDevice wanDevice in cachedDevices) {
				// Get local IP addresses of WAN device.
				if (!(wanDevice is IUPnPDeviceDocumentAccess))
					continue;
				var uPnPDoc = wanDevice as IUPnPDeviceDocumentAccess;
				IEnumerable<IPAddress> localIPs = uPnPDoc.GetLocalIpAddresses();
				// Check if any of the WAN device's addresses are in the same subnet
				// as one of the host's addresses.
				foreach (IPAddress local in localIPs) {
					var ips = Socks5Bytestreams.GetIpAddresses(local);
					// If so, try to remove port-mapping.
					foreach (var ip in ips) {
						try {
							UnforwardPort(wanDevice, port, protocol);
						} catch {
							// Fall through.
						}
					}
				}
			}
		}

		/// <summary>
		/// Forwards the specified port on the specified WAN device, optionally for
		/// the specified protocol only.
		/// </summary>
		/// <param name="wanDevice">The UPnP WAN device to forward the port on.</param>
		/// <param name="targetAddress">The IP address to forward the port to.</param>
		/// <param name="port">The port to forward.</param>
		/// <param name="protocol">If specified, must be either ProcotolType.Tcp or
		/// ProtocolType.Udp to forward the port only for the TCP or the UDP protocol,
		/// respectively. If this is null, the port will be forwarded for both
		/// protocols.</param>
		/// <param name="description">A description of the port-forwarding rule that
		/// may be displayed in the UPnP-device's user interface to help the user
		/// better understand what the port-forwarding rule is for.</param>
		/// <exception cref="ArgumentNullException">The wanDevice parameter or the
		/// targetAddress parameter is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">The port parameter is not
		/// between 0 and 65535.</exception>
		/// <exception cref="ArgumentException">The protocol parameter has a value
		/// other than ProtocolType.Tcp or ProtocolType.Udp.</exception>
		/// <exception cref="InvalidOperationException">An error occurred while
		/// trying to forward the specified port.</exception>
		static void ForwardPort(UPnPDevice wanDevice, IPAddress targetAddress, int port,
			ProtocolType? protocol = null, string description = null) {
			wanDevice.ThrowIfNull("wanDevice");
			targetAddress.ThrowIfNull("targetAddress");
			port.ThrowIfOutOfRange("port", 0, 65535);
			if (protocol.HasValue && protocol != ProtocolType.Tcp &&
				protocol != ProtocolType.Udp) {
				throw new ArgumentException("The argument parameter must be either " +
					"ProtocolType.Tcp or ProtocolType.Udp");
			}
			string[] p = protocol.HasValue ? new string[] { protocol.Value.ToString()
				.ToUpperInvariant() } : new string[] { "TCP", "UDP" };
			foreach (string proto in p) {
				var result = wanDevice.InvokeAction(stIpConnection, "AddPortMapping",
					new object[] { null, port, proto, port, targetAddress.ToString(), 1,
						description, 0 }
				);
				if (result.Length > 0) {
					throw new InvalidOperationException("The port-forwarding could not be " +
						"performed: " + result[0]);
				}
			}
		}

		/// <summary>
		/// Removes existing port-mappings of the specified port from the specified
		/// UPnP WAN device.
		/// </summary>
		/// <param name="wanDevice">The UPnP WAN device to unforward the port on.</param>
		/// <param name="port">The port to "unforward".</param>
		/// <param name="protocol">If specified, must be either ProcotolType.Tcp or
		/// ProtocolType.Udp to unforward the port only for the TCP or the UDP protocol,
		/// respectively. If this is null, the port will be unforwarded for both
		/// protocols.</param>
		/// <exception cref="ArgumentNullException">The wanDevice parameter is
		/// null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">The port parameter is not
		/// between 0 and 65535.</exception>
		/// <exception cref="ArgumentException">The protocol parameter has a value
		/// other than ProtocolType.Tcp or ProtocolType.Udp.</exception>
		/// <exception cref="InvalidOperationException">An error occurred while
		/// trying to unforward the specified port.</exception>
		/// <remarks>Trying to remove a non-existing port-mapping will cause an
		/// InvalidOperationException.</remarks>
		static void UnforwardPort(UPnPDevice wanDevice, int port,
			ProtocolType? protocol = null) {
			wanDevice.ThrowIfNull("wanDevice");
			port.ThrowIfOutOfRange("port", 0, 65535);
			if (protocol.HasValue && protocol != ProtocolType.Tcp &&
				protocol != ProtocolType.Udp) {
				throw new ArgumentException("The argument parameter must be either " +
					"ProtocolType.Tcp or ProtocolType.Udp");
			}
			string[] p = protocol.HasValue ? new string[] { protocol.Value.ToString()
				.ToUpperInvariant() } : new string[] { "TCP", "UDP" };
			foreach (string proto in p) {
				var result = wanDevice.InvokeAction(stIpConnection, "DeletePortMapping",
					new object[] { null, port, proto });
				if (result.Length > 0) {
					throw new InvalidOperationException("The port-mapping could not be " +
						"removed: " + result[0]);
				}
			}
		}

		/// <summary>
		/// Invokes the specified action of the specified service.
		/// </summary>
		/// <param name="device">The UPnP device the method is being called for.</param>
		/// <param name="serviceType">The type of the service which exposes the
		/// action to invoke.</param>
		/// <param name="action">The action to invoke.</param>
		/// <param name="args">The input parameters to pass along when invoking
		/// the specified action.</param>
		/// <returns>An array of values representing the result of the invoked
		/// action.</returns>
		/// <exception cref="ArgumentNullException">The serviceType parameter or
		/// the action parameter is null.</exception>
		/// <exception cref="InvalidOperationException">The specified device does not
		/// expose the requested service, or an error occurred while invoking the
		/// specified action of the requested service.</exception>
		/// <remarks>This is an extension method for the UPnPDevice class.</remarks>
		static object[] InvokeAction(this UPnPDevice device, string serviceType,
			string action, params object[] args) {
			serviceType.ThrowIfNull("serviceType");
			action.ThrowIfNull("action");
			try {
				foreach (IUPnPService srv in device.Services) {
					if (srv.ServiceTypeIdentifier != serviceType)
						continue;
					object outParams = null, ret = null;
					if (args == null)
						args = new object[] { };
					ret = srv.InvokeAction(action, args, ref outParams);
					if (ret != null) {
						throw new InvalidOperationException("The invoked action returned " +
							"an error: " + ret);
					}
					return (object[])outParams;
				}
				throw new InvalidOperationException("The specified device does not " +
					"expose the requested service.");
			} catch (COMException e) {
				throw new InvalidOperationException("The specified action could " +
					"not be invoked.", e);
			}
		}

		/// <summary>
		/// Invokes the specified action of the specified service.
		/// </summary>
		/// <typeparam name="T">The type of the return value of the specified
		/// action.</typeparam>
		/// <param name="device">The UPnP device the method is being called for.</param>
		/// <param name="serviceType">The type of the service which exposes the
		/// action to invoke.</param>
		/// <param name="action">The action to invoke.</param>
		/// <param name="args">The input parameters to pass along when invoking
		/// the specified action.</param>
		/// <returns>The result of the invoked action.</returns>
		/// <exception cref="ArgumentNullException">The serviceType parameter or
		/// the action parameter is null.</exception>
		/// <exception cref="InvalidOperationException">The specified device does not
		/// expose the requested service, or an error occurred while invoking the
		/// specified action of the requested service.</exception>
		/// <remarks>This is an extension method for the UPnPDevice class.</remarks>
		static T InvokeAction<T>(this UPnPDevice device, string serviceType,
			string action, params object[] args) {
				return (T) InvokeAction(device, serviceType, action, args)[0];
		}

		/// <summary>
		/// Retrieves the local IP addresses of the UPnP-device.
		/// </summary>
		/// <param name="device">The UPnP device the method is being called for.</param>
		/// <returns>An enumerable collection of local IP addresses under which
		/// the UPnP-device can be reached.</returns>
		/// <remarks>This is an extension method for the IUPnPDeviceDocumentAccess
		/// class.</remarks>
		static IEnumerable<IPAddress> GetLocalIpAddresses(
			this IUPnPDeviceDocumentAccess device) {
				try {
					// Get the devices URL and extract the host component of it.
					Uri uri = new Uri(device.GetDocumentURL());

					return Dns.GetHostAddresses(uri.Host);
				} catch (Exception e) {
					throw new InvalidOperationException("The local IP addresses could not " +
						"be obtained.", e);
				}
		}

		/// <summary>
		/// Ensures the cached items are still valid and if necessary refreshes
		/// the cache.
		/// </summary>
		/// <exception cref="InvalidOperationException">An error occurred while
		/// performing an SSDP search-operation.</exception>
		static void VerifyCache() {
			TimeSpan span = DateTime.Now - lastUPnPFind;
			// Cache entries are still recent enough.
			if (span < TimeSpan.FromMinutes(5))
				return;
			// Perform a new search for UPnP devices with a 5-second timeout.
			cachedDevices = FindDevices(stIpConnection, TimeSpan.FromSeconds(5));
			// Remember the last time we searched.
			lastUPnPFind = DateTime.Now;
		}

		/// <summary>
		/// Performs a search for UPnP devices on all active network interfaces.
		/// </summary>
		/// <param name="searchTarget">The search target. If this is null, a search
		/// for all UPnP devices will be performed.</param>
		/// <param name="timeout">The timespan after which to cancel the search
		/// and return the results to the caller. If this is null, the method blocks
		/// until the search has been completed.</param>
		/// <returns>An enumerable collection of discovered UPnP devices.</returns>
		/// <exception cref="InvalidOperationException">An error occurred while
		/// performing an SSDP search-operation.</exception>
		/// <remarks>
		/// A full UPnP search can take 9 seconds or longer;
		///
		/// Possible values for the searchTarget parameter include:
		///  * ssdp:all (searches for all devices and services)
		///  * ssdp:rootdevice (searches for root devices only)
		/// For details on all possible values for the searchTarget parameter, refer
		/// to the 'UPnP Device Architecture 1.1' document, page 33.
		/// </remarks>
        static IEnumerable<UPnPDevice> FindDevices(string searchTarget = null,
			TimeSpan? timeout = null) {
			UPnPDeviceFinder deviceFinder = new UPnPDeviceFinder();
			DeviceFinderCallback dfCallback = new DeviceFinderCallback();
			if (searchTarget == null)
				searchTarget = "ssdp:all";
			int findHandle = deviceFinder.CreateAsyncFind(searchTarget, 0, dfCallback);
			if (findHandle == 0)
				throw new InvalidOperationException("Asynchronous search operation could " +
					"not be created.");
			// Start the asynchronous search.
			deviceFinder.StartAsyncFind(findHandle);
			if (timeout == null)
				timeout = TimeSpan.FromMilliseconds(-1);
			// Wait until the search has been completed or the specified timeout has
			// expired.
			if (!dfCallback.SearchCompleted.WaitOne(timeout.Value))
				deviceFinder.CancelAsyncFind(findHandle);
			// The Devices property of the DeviceFinderCallback contains the UPnP devices
			// that were discovered.
			return dfCallback.Devices;
		}
	}
#endif
}