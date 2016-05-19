using Sharp.Xmpp.Core;
using Sharp.Xmpp.Im;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Linq;

namespace Sharp.Xmpp.Extensions
{
 
    internal class VCards : XmppExtension, IInputFilter<Iq>
    {
        /// <summary>
        /// A reference to the 'Entity Capabilities' extension instance.
        /// </summary>
        private EntityCapabilities ecapa;

        /// <summary>
        /// An enumerable collection of XMPP namespaces the extension implements.
        /// </summary>
        /// <remarks>This is used for compiling the list of supported extensions
        /// advertised by the 'Service Discovery' extension.</remarks>
        public override IEnumerable<string> Namespaces
        {
            get
            {
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
        public override Extension Xep
        {
            get
            {
                return Extension.vCards;
            }
        }

        /// <summary>
        /// Invoked after all extensions have been loaded.
        /// </summary>
        public override void Initialize()
        {
            ecapa = im.GetExtension<EntityCapabilities>();
        }

        /// <summary>
        /// Invoked when an IQ stanza is being received.
        /// </summary>
        /// <param name="stanza">The stanza which is being received.</param>
        /// <returns>true to intercept the stanza or false to pass the stanza
        /// on to the next handler.</returns>
        public bool Input(Iq stanza)
        {
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
        /// <summary>
        /// Set the Avatar based on the stream
        /// </summary>
        /// <param name="stream">Avatar stream</param>
        public void SetAvatar(Stream stream)
        {
            stream.ThrowIfNull("stream");

            string mimeType = "image/png";

            string hash = String.Empty, base64Data = String.Empty;
            MemoryStream ms = new MemoryStream();
            stream.CopyTo(ms);
            using (ms)
            {
                //					// Calculate the SHA-1 hash of the image data.
                byte[] data = ms.ToArray();
                hash = Hash(data);
                //					// Convert the binary data into a BASE64-string.
                base64Data = Convert.ToBase64String(data);
            }
            var xml = Xml.Element("vCard", "vcard-temp").Child(Xml.Element("Photo").Child(Xml.Element("Type").Text(mimeType)).Child(Xml.Element("BINVAL").Text(base64Data)));
            im.IqRequestAsync(IqType.Set, null, im.Jid, xml, null, (id, iq) =>
            {
                if (iq.Type == IqType.Result)
                {
                    // Result must contain a 'feature' element.
                    im.SendPresence(new Sharp.Xmpp.Im.Presence(null, null, PresenceType.Available, null, null, Xml.Element("x", "vcard-temp:x:update").Child(Xml.Element("photo").Text(hash))));
                }
            });
        }

        /// <summary>
        /// Convert the Image to the appropriate format for XEP-0153
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private string Hash(byte[] data)
        {
            data.ThrowIfNull("data");
            using (var sha1 = new SHA1Managed())
            {
                return Convert.ToBase64String(sha1.ComputeHash(data));
            }
        }

        public void RequestvCards(Jid jid, Action<VCardsData, Jid> callback)
        {
            jid.ThrowIfNull("jid");
            VCardsData vCD = new VCardsData(); 

            //Make the request
            var xml = Xml.Element("vCard", "vcard-temp");

            //The Request is Async
            im.IqRequestAsync(IqType.Get, jid, im.Jid, xml, null, (id, iq) =>
            {
                XmlElement query = iq.Data["vCard"];
                if (iq.Data["vCard"].NamespaceURI == "vcard-temp")
                {
                    XElement root = XElement.Parse(iq.Data.OuterXml); 
                    XNamespace aw = "vcard-temp"; //SOS the correct namespace
                    IEnumerable<string> b64collection = (from el in root.Descendants(aw + "BINVAL") select (string)el);
                    IEnumerable<string> nicknamecollection = (from el in root.Descendants(aw + "NICKNAME") select (string)el);
                    IEnumerable<string> fullnamecollection = (from el in root.Descendants(aw + "FN") select (string)el);
                    IEnumerable<string> familynamecollection = (from el in root.Descendants(aw + "FAMILY") select (string)el);
                    IEnumerable<string> firstnamecollection = (from el in root.Descendants(aw + "GIVEN") select (string)el);
                    IEnumerable<string> urlcollection = (from el in root.Descendants(aw + "URL") select (string)el);
                    IEnumerable<string> birthdaycollection = (from el in root.Descendants(aw + "BDAY") select (string)el);
                    IEnumerable<string> orgnamecollection = (from el in root.Descendants(aw + "ORGNAME") select (string)el);
                    IEnumerable<string> titlecollection = (from el in root.Descendants(aw + "TITLE") select (string)el);
                    IEnumerable<string> rolecollection = (from el in root.Descendants(aw + "ROLE") select (string)el);

                    vCD.NickName = nicknamecollection.FirstOrDefault();
                    vCD.FullName = fullnamecollection.FirstOrDefault();
                    vCD.FamilyName = familynamecollection.FirstOrDefault();
                    vCD.FirstName = firstnamecollection.FirstOrDefault();
                    vCD.URL = urlcollection.FirstOrDefault();
                    vCD.Birthday = birthdaycollection.FirstOrDefault();
                    vCD.OrgName = orgnamecollection.FirstOrDefault();
                    vCD.Title = titlecollection.FirstOrDefault();
                    vCD.Role = rolecollection.FirstOrDefault();

                    string b64 = null;
                    if (b64collection != null)
                    {
                        b64 = b64collection.FirstOrDefault();

                        if (b64 != null)
                        {
                            try
                            {
                                byte[] data = Convert.FromBase64String(b64);
                                if (data != null)
                                {
                                    vCD.Avatar = data;
                                }
                            }
                            catch (Exception e)
                            {
                                System.Diagnostics.Debug.WriteLine("Error downloading vcard data" + e.StackTrace + e.ToString());
                                //Exception is not contained here. Fix?
                            }
                        }
                    }

                    callback.Invoke(vCD, jid);
                }
            });
        }

        /// <summary>
        /// Initializes a new instance of the vCard class.
        /// </summary>
        /// <param name="im">A reference to the XmppIm instance on whose behalf this
        /// instance is created.</param>
        public VCards(XmppIm im)
            : base(im)
        {
        }
    }

    /// <summary>
    /// Represents the VCards Data.
    /// </summary>
    [Serializable]
    public sealed class VCardsData
    {

        /// <summary>
        /// The FN from vCard.
        /// </summary>
        public string FullName
        {
            get;
            set;
        }

        /// <summary>
        /// The FAMILY from vCard.
        /// </summary>
        public string FamilyName
        {
            get;
            set;
        }

        /// <summary>
        /// The GIVEN from vCard.
        /// </summary>
        public string FirstName
        {
            get;
            set;
        }

        /// <summary>
        /// The nickname from vCard.
        /// </summary>
        public string NickName
        {
            get;
            set;
        }

        /// <summary>
        /// The URL from vCard.
        /// </summary>
        public string URL
        {
            get;
            set;
        }

        /// <summary>
        /// The avatar from vCard.
        /// </summary>
        public byte[] Avatar
        {
            get;
            set;
        }

        /// <summary>
        /// The BDAY from vCard.
        /// </summary>
        public string Birthday
        {
            get;
            set;
        }

        /// <summary>
        /// The ORGNAME from vCard.
        /// </summary>
        public string OrgName
        {
            get;
            set;
        }

        /// <summary>
        /// The TITLE from vCard.
        /// </summary>
        public string Title
        {
            get;
            set;
        }

        /// <summary>
        /// The ROLE from vCard.
        /// </summary>
        public string Role
        {
            get;
            set;
        }






    }
}
