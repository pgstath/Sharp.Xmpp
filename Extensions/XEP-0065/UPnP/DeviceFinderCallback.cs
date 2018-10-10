#if WINDOWSPLATFORM
using UPNPLib;
#endif

namespace XMPPEngineer.Extensions.Upnp
{
#if WINDOWSPLATFORM
	/// <summary>
	/// Represents a class for collecting UPnP-Discovery results.
	/// </summary>
	/// <remarks>An instance of this class is passed to the "UPnP Control Point"
	/// COM Component for providing callback methods to invoke when a device has
	/// been discovered or removed.</remarks>
	internal class DeviceFinderCallback : IUPnPDeviceFinderCallback {
		/// <summary>
		/// A set of devices discovered during UPnP-Discovery.
		/// </summary>
		ISet<UPnPDevice> devices = new HashSet<UPnPDevice>();
		/// <summary>
		/// Set once UPnP-Discovery has been completed.
		/// </summary>
		ManualResetEvent searchCompleted = new ManualResetEvent(false);

		/// <summary>
		/// Determines whether the UPnP-Discovery has been completed.
		/// </summary>
		public WaitHandle SearchCompleted {
			get {
				return searchCompleted;
			}
		}

		/// <summary>
		/// An enumerable collection of UPnP devices that were discovered during
		/// UPnP-Discovery.
		/// </summary>
		public IEnumerable<UPnPDevice> Devices {
			get {
				return devices;
			}
		}

		/// <summary>
		/// Callback method invoked when a new UPnP device has been discovered.
		/// </summary>
		/// <param name="lFindData">The handle of the search-operation for which
		/// the method is being invoked.</param>
		/// <param name="pDevice">The UPnP-device that has been discovered.</param>
		public void DeviceAdded(int lFindData, UPnPDevice pDevice) {
			devices.Add(pDevice);
		}

		/// <summary>
		/// Callback method invoked when a UPnP device has been removed.
		/// </summary>
		/// <param name="lFindData">The handle of the search-operation for which
		/// the method is being invoked.</param>
		/// <param name="bstrUDN">The UDN of the device that has been removed.</param>
		public void DeviceRemoved(int lFindData, string bstrUDN) {
		}

		/// <summary>
		/// Callback method invoked when UPnP-Discovery has been completed.
		/// </summary>
		/// <param name="lFindData">The handle of the search-operation for which
		/// the method is being invoked.</param>
		public void SearchComplete(int lFindData) {
			// Signal the event.
			searchCompleted.Set();
		}
	}
#endif
}