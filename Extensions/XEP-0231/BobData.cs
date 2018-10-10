using System;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace XMPPEngineer.Extensions
{
    /// <summary>
    /// Represents a piece of binary data in the context of the 'Bits of Binary'
    /// extension.
    /// </summary>
    [Serializable]
    internal class BobData
    {
        /// <summary>
        /// The actual binary data.
        /// </summary>
        public byte[] Data
        {
            get;
            private set;
        }

        /// <summary>
        /// The content-type of the binary data.
        /// </summary>
        public string Type
        {
            get;
            private set;
        }

        /// <summary>
        /// The content-id of the data.
        /// </summary>
        public string Cid
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns a textual representation of the BobData instance.
        /// </summary>
        /// <returns>A textual XML representation of the BobData instance.</returns>
        public override string ToString()
        {
            return Xml.Element("data", "urn:xmpp:bob")
                .Attr("cid", Cid)
                .Attr("type", Type)
                .Text(Convert.ToBase64String(Data))
                .ToXmlString();
        }

        /// <summary>
        /// Parses a 'data' XML element and creates a new BobData off it.
        /// </summary>
        /// <param name="data">The XML data element to parse.</param>
        /// <returns>An initialized instance of the BobData class representing the
        /// data contained in the specified element.</returns>
        /// <exception cref="ArgumentNullException">The data parameter is
        /// null.</exception>
        /// <exception cref="ArgumentException">The XML namespace of the specified
        /// XML element is not 'urn:xmpp:bob', or the mandatory type attribute is
        /// missing, or the contained data is not a valid Base64-encoded
        /// string.</exception>
        public static BobData Parse(XmlElement data)
        {
            if (data.NamespaceURI != "urn:xmpp:bob")
                throw new ArgumentException("Invalid namespace attribute.");
            string type = data.GetAttribute("type");
            if (String.IsNullOrEmpty(type))
                throw new ArgumentException("The type attribute is missing.");
            string cid = data.GetAttribute("cid");
            if (String.IsNullOrEmpty(cid))
                throw new ArgumentException("The cid attribute is missing.");
            try
            {
                byte[] buffer = Convert.FromBase64String(data.InnerText);
                return new BobData(buffer, type, cid);
            }
            catch (FormatException e)
            {
                throw new ArgumentException("Invalid Base64 data.", e);
            }
        }

        /// <summary>
        /// Initializes a new instance of the BobData class.
        /// </summary>
        /// <param name="data">The binary data.</param>
        /// <param name="type">The content-type of the data.</param>
        /// <exception cref="ArgumentNullException">The data parameter or the
        /// type parameter is null.</exception>
        public BobData(byte[] data, string type)
        {
            data.ThrowIfNull("data");
            type.ThrowIfNull("type");
            Data = data;
            Type = type;
            Cid = "sha1+" + Sha1(Data) + "@bob.xmpp.org";
        }

        /// <summary>
        /// Initializes a new instance of the BobData class.
        /// </summary>
        /// <param name="data">The binary data.</param>
        /// <param name="type">The content-type of the data.</param>
        /// <param name="cid">The CID of the binary-data.</param>
        /// <exception cref="ArgumentNullException">The data parameter or the
        /// type parameter or the cid parameter is null.</exception>
        private BobData(byte[] data, string type, string cid)
        {
            data.ThrowIfNull("data");
            type.ThrowIfNull("type");
            cid.ThrowIfNull("cid");
            Data = data;
            Type = type;
            Cid = cid;
        }

        /// <summary>
        /// Calculates the SHA-1 hash value of the specified data.
        /// </summary>
        /// <param name="data">The data to calculate the SHA-1 hash value of.</param>
        /// <returns>A SHA-1 hash as a hex-string.</returns>
        /// <exception cref="ArgumentNullException">The data parameter is
        /// null.</exception>
        private string Sha1(byte[] data)
        {
            data.ThrowIfNull("data");
            using (var sha1 = new SHA1Managed())
            {
                byte[] hash = sha1.ComputeHash(data);
                StringBuilder builder = new StringBuilder();
                foreach (byte h in hash)
                    builder.Append(h.ToString("x2"));
                return builder.ToString();
            }
        }
    }
}