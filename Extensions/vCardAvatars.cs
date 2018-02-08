using XMPPEngineer;
using XMPPEngineer.Core;
using XMPPEngineer.Im;
using XMPPEngineer.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Linq;
using System.Linq;

namespace S22.Xmpp.Extensions
{
       
	/// <summary>
	/// Implements the 'vCard based Avatars' extension as defined in XEP-0153.
	/// </summary>
	internal class vCardAvatars : XmppExtension, IInputFilter<Iq> {
		/// <summary>
		/// A reference to the 'Entity Capabilities' extension instance.
		/// </summary>
		EntityCapabilities ecapa;

        /// <summary>
		/// A cache of images indexed by their respective SHA-1 hashes.
		/// </summary>
		IDictionary<string, string> cachedImages = new Dictionary<string, string>();

        
		/// <summary>
		/// An enumerable collection of XMPP namespaces the extension implements.
		/// </summary>
		/// <remarks>This is used for compiling the list of supported extensions
		/// advertised by the 'Service Discovery' extension.</remarks>
		public override IEnumerable<string> Namespaces {
			get {
				return new string[] {
					 "vcard-temp:x:update" ,
					 "vcard-temp"
				};
			}
		}

		/// <summary>
		/// The named constant of the Extension enumeration that corresponds to this
		/// extension.
		/// </summary>
		public override Extension Xep {
			get {
				return Extension.vCardsAvatars;
			}
		}

		/// <summary>
		/// Invoked after all extensions have been loaded.
		/// </summary>
		public override void Initialize() {
			ecapa = im.GetExtension<EntityCapabilities>();
		}

		/// <summary>
		/// Invoked when an IQ stanza is being received.
		/// </summary>
		/// <param name="stanza">The stanza which is being received.</param>
		/// <returns>true to intercept the stanza or false to pass the stanza
		/// on to the next handler.</returns>
		public bool Input(Iq stanza) {
			if (stanza.Type != IqType.Get)
				return false;
			var vcard = stanza.Data["vCard "];
			if (vcard == null || vcard.NamespaceURI != "vcard-temp")
				return false;
			im.IqResult(stanza);
			// We took care of this IQ request, so intercept it and don't pass it
			// on to other handlers.
			return true;
		}

        //http://www.xmpp.org/extensions/xep-0153.html
        public void SetAvatar(Stream stream)
        {
            stream.ThrowIfNull("stream");

            string mimeType = "image/png";

            string hash = String.Empty, base64Data = String.Empty;
            MemoryStream ms = new MemoryStream();
            stream.CopyTo(ms);
            using (ms)
            {
                //                    await image.Save(Splat.CompressedBitmapFormat.Jpeg,1, ms);
                //					// Calculate the SHA-1 hash of the image data.
                byte[] data = ms.ToArray();
                hash = Hash(data);
                //					// Convert the binary data into a BASE64-string.
                base64Data = Convert.ToBase64String(data);
            }
            var xml = Xml.Element("vCard", "vcard-temp").Child(Xml.Element("Photo").Child(Xml.Element("Type").Text(mimeType)).Child(Xml.Element("BINVAL").Text(base64Data)));
            im.IqRequestAsync(IqType.Set, null, im.Jid, xml,null,(id, iq) => {

				if (iq.Type == IqType.Result) {
					// Result must contain a 'feature' element.
                    im.SendPresence(new XMPPEngineer.Im.Presence(null, null, PresenceType.Available, null, null, Xml.Element("x", "vcard-temp:x:update").Child(Xml.Element("photo").Text(hash))));
				}
	
			});

            //var result = im.IqRequest(IqType.Set, null, im.Jid, xml);
            //            

        }
            // Publish the image- and meta data.

            //pep.Publish("urn:xmpp:avatar:data", hash,
            //    Xml.Element("data", "urn:xmpp:avatar:data").Text(base64Data));
            //pep.Publish("urn:xmpp:avatar:metadata", hash,
            //    Xml.Element("metadata", "urn:xmpp:avatar:metadata").Child(
            //    Xml.Element("info")
            //        .Attr("bytes", size.ToString())
            //        .Attr("height", height.ToString())
            //        .Attr("width", width.ToString())
            //        .Attr("id", hash)
            //        .Attr("type", mimeType))


        string Hash(byte[] data)
        {
            data.ThrowIfNull("data");
            using (var sha1 = new SHA1Managed())
            {
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
        public void RequestAvatar(Jid jid, string filepath, Action callback)
        {
            jid.ThrowIfNull("jid");
            //Make the request
            var xml = Xml.Element("vCard", "vcard-temp"); 
    
            //var result = im.IqRequest(IqType.Get, jid, im.Jid, xml);
            im.IqRequestAsync(IqType.Get, jid, im.Jid, xml,null, (id, iq) => {
                
                XmlElement query = iq.Data["vCard"];
                if (iq.Data["vCard"].NamespaceURI == "vcard-temp")
                {

                    XElement root = XElement.Parse(iq.Data.OuterXml);
                    XNamespace aw = "vcard-temp"; //SOS the correct namespace
                    IEnumerable<string> b64collection = (from el in root.Descendants(aw + "BINVAL")
                                                         select (string)el);
                    string b64 = null;
                    if (b64collection != null)
                    {
                        b64 = b64collection.FirstOrDefault();

                        if (b64 != null)
                        {
                            try
                            {
                                byte[] data = Convert.FromBase64String(b64);
                                //string hashvalue = Hash(data);
                                if (data != null)
                                {
                                    string dir = Path.GetDirectoryName(filepath);
                                    if (!Directory.Exists(dir))
                                    {
                                        Directory.CreateDirectory(dir);
                                    }

                                    using (var file = new FileStream(filepath, FileMode.Create, System.IO.FileAccess.Write))
                                    {
                                        file.Write(data, 0, data.Length);
                                    }
                                    if (callback != null)
                                    {
                                        callback.Invoke();
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Error downloading and writing avatar file" + e);
                            }
                        }
                    }
                }
	
			});
            //Check the result
            
    
        }

		
		/// <summary>
		/// Initializes a new instance of the vCard-Avatar class.
		/// </summary>
		/// <param name="im">A reference to the XmppIm instance on whose behalf this
		/// instance is created.</param>
		public vCardAvatars(XmppIm im)
			: base(im) {
		}
	}

}
