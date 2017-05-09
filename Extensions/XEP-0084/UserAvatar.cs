#if WINDOWSPLATFORM
using System.Drawing;
using System.Drawing.Imaging;
#endif

namespace XMPPEngineer.Extensions
{
#if WINDOWSPLATFORM

	/// <summary>
	/// Implements the 'User Avatar' extension as defined in XEP-0084.
	/// </summary>
	/// <remarks>This currently does not account for avatar images hosted on
	/// HTTP servers.</remarks>
	internal class UserAvatar : XmppExtension {
		/// <summary>
		/// A reference to the 'Personal Eventing Protocol' extension instance.
		/// </summary>
		Pep pep;
		/// <summary>
		/// A cache of images indexed by their respective SHA-1 hashes.
		/// </summary>
		IDictionary<string, Image> cachedImages = new Dictionary<string, Image>();

		/// <summary>
		/// An enumerable collection of XMPP namespaces the extension implements.
		/// </summary>
		/// <remarks>This is used for compiling the list of supported extensions
		/// advertised by the 'Service Discovery' extension.</remarks>
		public override IEnumerable<string> Namespaces {
			get {
				return new string[] {
					"urn:xmpp:avatar:data",
					"urn:xmpp:avatar:metadata",
					"urn:xmpp:avatar:metadata+notify"
				};
			}
		}

		/// <summary>
		/// The named constant of the Extension enumeration that corresponds to this
		/// extension.
		/// </summary>
		public override Extension Xep {
			get {
				return Extension.UserAvatar;
			}
		}

		/// <summary>
		/// Determines whether our server supports personal eventing and thusly
		/// the user avatar extension.
		/// </summary>
		public bool Supported {
			get {
				return pep.Supported;
			}
		}

		/// <summary>
		/// The event that is raised when another XMPP entity has changed its
		/// avatar image.
		/// </summary>
		public event EventHandler<AvatarChangedEventArgs> AvatarChanged;

		/// <summary>
		/// Invoked after all extensions have been loaded.
		/// </summary>
		public override void Initialize() {
			pep = im.GetExtension<Pep>();
			pep.Subscribe("urn:xmpp:avatar:metadata", onMetadata);
		}

		/// <summary>
		/// Publishes the image data provided by the specified stream as the user's
		/// avatar.
		/// </summary>
		/// <param name="stream">A stream containing the image data to publish as
		/// the user's avatar.</param>
		/// <exception cref="ArgumentNullException">The stream parameter is
		/// null.</exception>
		/// <exception cref="ArgumentException">The stream does not have a valid
		/// image format.</exception>
		/// <exception cref="NotSupportedException">The server does not support
		/// the 'Personal Eventing Protocol' extension.</exception>
		/// <exception cref="XmppErrorException">The server returned an XMPP error code.
		/// Use the Error property of the XmppErrorException to obtain the specific
		/// error condition.</exception>
		/// <exception cref="XmppException">The server returned invalid data or another
		/// unspecified XMPP error occurred.</exception>
		public void Publish(Stream stream) {
			stream.ThrowIfNull("stream");
			using (Image image = Image.FromStream(stream)) {
				string mimeType = GetMimeType(image);
				int width = image.Width;
				int height = image.Height;
				long size = 0;
				string hash = String.Empty, base64Data = String.Empty;
				using (var ms = new MemoryStream()) {
					image.Save(ms, image.RawFormat);
					size = ms.Length;
					// Calculate the SHA-1 hash of the image data.
					byte[] data = ms.ToArray();
					hash = Hash(data);
					// Convert the binary data into a BASE64-string.
					base64Data = Convert.ToBase64String(data);
				}
				// Publish the image- and meta data.
				pep.Publish("urn:xmpp:avatar:data", hash,
					Xml.Element("data", "urn:xmpp:avatar:data").Text(base64Data));
				pep.Publish("urn:xmpp:avatar:metadata", hash,
					Xml.Element("metadata", "urn:xmpp:avatar:metadata").Child(
					Xml.Element("info")
						.Attr("bytes", size.ToString())
						.Attr("height", height.ToString())
						.Attr("width", width.ToString())
						.Attr("id", hash)
						.Attr("type", mimeType))
				);
			}
		}

		/// <summary>
		/// Publishes the image located at the specified path as the user's avatar.
		/// </summary>
		/// <param name="filePath">The path to the image to publish as the user's
		/// avatar.</param>
		/// <exception cref="ArgumentNullException">The filePath parameter is
		/// null.</exception>
		/// <exception cref="ArgumentException">filePath is a zero-length string,
		/// contains only white space, or contains one or more invalid
		/// characters.</exception>
		/// <exception cref="PathTooLongException">The specified path, file name,
		/// or both exceed the system-defined maximum length. For example, on
		/// Windows-based platforms, paths must be less than 248 characters, and
		/// file names must be less than 260 characters.</exception>
		/// <exception cref="DirectoryNotFoundException">The specified path is
		/// invalid, (for example, it is on an unmapped drive).</exception>
		/// <exception cref="UnauthorizedAccessException">The path specified is
		/// a directory, or the caller does not have the required
		/// permission.</exception>
		/// <exception cref="FileNotFoundException">The file specified in
		/// filePath was not found.</exception>
		/// <exception cref="NotSupportedException">filePath is in an invalid
		/// format, or the server does not support the 'Personal Eventing
		/// Protocol' extension.</exception>
		/// <exception cref="XmppErrorException">The server returned an XMPP error code.
		/// Use the Error property of the XmppErrorException to obtain the specific
		/// error condition.</exception>
		/// <exception cref="XmppException">The server returned invalid data or another
		/// unspecified XMPP error occurred.</exception>
		public void Publish(string filePath) {
			using (Stream s = File.OpenRead(filePath)) {
				Publish(s);
			}
		}

		/// <summary>
		/// Initializes a new instance of the UserAvatar class.
		/// </summary>
		/// <param name="im">A reference to the XmppIm instance on whose behalf this
		/// instance is created.</param>
		public UserAvatar(XmppIm im)
			: base(im) {
		}

		/// <summary>
		/// Invoked when a contact has published avatar metadata information.
		/// </summary>
		/// <param name="jid">The JID of the XMPP entity that published the tune
		/// information.</param>
		/// <param name="item">The 'item' Xml element of the pubsub publish
		/// event.</param>
		void onMetadata(Jid jid, XmlElement item) {
			if (item == null || item["metadata"] == null)
				return;
			// An empty 'metadata' element means the XMPP entity wishes to temporarily
			// disable avatar publishing.
			if (item["metadata"].IsEmpty) {
				// Raise the 'AvatarChanged' event with the 'Cleared' property set to true.
				AvatarChanged.Raise(this, new AvatarChangedEventArgs(jid));
				return;
			}
			var info = item["metadata"]["info"];
			if (info == null)
				return;
			// Parse meta data attributes.
			string hash = info.GetAttribute("id");
			if (String.IsNullOrEmpty(hash))
				return;
			// Request image unless we already have a cached copy.
			if (!cachedImages.ContainsKey(hash))
				cachedImages.Add(hash, RequestImage(jid, hash));
			Image image = cachedImages[hash];
			// Raise 'AvatarChanged' event.
			AvatarChanged.Raise(this, new AvatarChangedEventArgs(jid, hash, image));
		}

		/// <summary>
		/// Returns the mime-type of the specified image.
		/// </summary>
		/// <param name="image">The image to retrieve the mime-type for.</param>
		/// <returns>The mime-type of the specified image.</returns>
		/// <exception cref="ArgumentNullException">The image parameter is
		/// null.</exception>
		/// <exception cref="ArgumentException">The mime-type of the specified
		/// image could not be determined.</exception>
		string GetMimeType(Image image) {
			image.ThrowIfNull("image");
			foreach(var codec in ImageCodecInfo.GetImageEncoders()) {
				if (codec.FormatID == image.RawFormat.Guid)
					return codec.MimeType;
			}
			throw new ArgumentException("The mime-type could not be determined.");
		}

		/// <summary>
		/// Generates a BASE64-encoded SHA-1 hash from the specified input data.
		/// </summary>
		/// <param name="data">The input data to hash.</param>
		/// <returns>The hashed input data as a BASE64-encoded string.</returns>
		/// <exception cref="ArgumentNullException">The data parameter is
		/// null.</exception>
		string Hash(byte[] data) {
			data.ThrowIfNull("data");
			using (var sha1 = new SHA1Managed()) {
				return Convert.ToBase64String(sha1.ComputeHash(data));
			}
		}

		/// <summary>
		/// Requests the avatar image with the specified hash from the node service
		/// running at the specified JID.
		/// </summary>
		/// <param name="jid">The JID of the node service to request the avatar
		/// image from.</param>
		/// <param name="hash">The hash of the avatar image to retrieve.</param>
		/// <returns>An Image instance representing the retrieved avatar image.</returns>
		/// <exception cref="ArgumentNullException">The jid parameter or the
		/// hash parameter is null.</exception>
		/// <exception cref="XmppErrorException">The server returned an XMPP error code.
		/// Use the Error property of the XmppErrorException to obtain the specific
		/// error condition.</exception>
		/// <exception cref="XmppException">The server returned invalid data or another
		/// unspecified XMPP error occurred.</exception>
		Image RequestImage(Jid jid, string hash) {
			jid.ThrowIfNull("jid");
			hash.ThrowIfNull("hash");
			XmlElement item = pep.RetrieveItem(jid, "urn:xmpp:avatar:data", hash);
			if (item["data"] == null || item["data"].NamespaceURI != "urn:xmpp:avatar:data")
				throw new XmppException("Erroneous avatar data: " + item);
			string b64 = item["data"].InnerText;
			// Try to decode the base64-string and create an Image instance from the
			// decoded binary data.
			try {
				byte[] data = Convert.FromBase64String(b64);
				using (var ms = new MemoryStream(data)) {
					return Image.FromStream(ms);
				}
			} catch (Exception e) {
				throw new XmppException("Invalid image data.", e);
			}
		}
	}
#endif
}