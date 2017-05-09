using XMPPEngineer.Extensions.Socks5;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;

namespace XMPPEngineer.Extensions.Stun
{
    /// <summary>
    /// Represents a STUN 'Binding Response' message.
    /// </summary>
    /// <remarks>For details, refer to RFC 5389.</remarks>
    internal class BindingResponse
    {
        /// <summary>
        /// The STUN message type for a 'Binding Response' message.
        /// </summary>
        private const short stunMessageType = 0x0101;

        /// <summary>
        /// The size of a STUN message header.
        /// </summary>
        private const int headerSize = 20;

        /// <summary>
        /// The type of a 'MAPPED-ADDRESS' attribute.
        /// </summary>
        private const short mappedAddress = 0x0100;

        /// <summary>
        /// The type of a 'XOR-MAPPED-ADDRESS' attribute.
        /// </summary>
        private const short xorMappedAddress = 0x2000;

        /// <summary>
        /// The value of the IPv4 address family in a 'MAPPED-ADDRESS' attribute.
        /// </summary>
        private const byte IPv4 = 1;

        /// <summary>
        /// The value of the IPv6 address family in a 'MAPPED-ADDRESS' attribute.
        /// </summary>
        private const byte IPv6 = 2;

        /// <summary>
        /// The 'Magic Cookie' value as defined in RFC 5389.
        /// </summary>
        private static byte[] magicCookie = new byte[] { 0x21, 0x12, 0xA4, 0x42 };

        /// <summary>
        /// The transaction id of the binding response.
        /// </summary>
        public byte[] Id
        {
            get;
            private set;
        }

        /// <summary>
        /// The IP address contained in the 'Binding Response' message.
        /// </summary>
        public IPAddress Address
        {
            get;
            private set;
        }

        /// <summary>
        /// Deserializes a BindingResponse instance from the specified byte
        /// array.
        /// </summary>
        /// <param name="buffer">The byte array to deserialize a BindingResponse
        /// instance from.</param>
        /// <returns>A BindingResponse instance deserialized from the specified
        /// byte buffer.</returns>
        /// <exception cref="ArgumentNullException">The buffer parameter is
        /// null.</exception>
        /// <exception cref="SerializationException">The buffer parameter does not
        /// contain a valid STUN 'Binding Response' message.</exception>
        public static BindingResponse Deserialize(byte[] buffer)
        {
            buffer.ThrowIfNull("buffer");
            if (buffer.Length < headerSize)
                throw new SerializationException("The buffer does not contain a " +
                    "valid STUN message header.");
            using (var ms = new MemoryStream(buffer))
            {
                using (var r = new BinaryReader(ms))
                {
                    if (r.ReadInt16() != stunMessageType)
                        throw new SerializationException("Unexpected STUN message type.");
                    int length = r.ReadInt16(bigEndian: true);
                    if (!r.ReadBytes(4).SequenceEqual(magicCookie))
                        throw new SerializationException("Invalid 'Magic Cookie' value.");
                    byte[] id = r.ReadBytes(12);
                    // Read the attributes.
                    try
                    {
                        while (length > 0)
                        {
                            short type = r.ReadInt16(), size = r.ReadInt16(bigEndian: true);
                            if (type == mappedAddress || type == xorMappedAddress)
                            {
                                r.ReadByte();
                                byte family = r.ReadByte();
                                if (family != IPv4 && family != IPv6)
                                    throw new SerializationException("Invalid address-family.");
                                short port = r.ReadInt16();
                                byte[] address = r.ReadBytes(family == IPv4 ? 4 : 16);
                                if (type == xorMappedAddress)
                                {
                                    address = Xor(address, family == IPv4 ? magicCookie :
                                        new ByteBuilder().Append(magicCookie).Append(id).ToArray());
                                }
                                return new BindingResponse(id, new IPAddress(address));
                            }
                            // If it's any other attribute, skip it.
                            r.ReadBytes(size);
                            length = length - 4 - size;
                        }
                    }
                    catch (Exception e)
                    {
                        throw new SerializationException("The format of the STUN binding " +
                            "response is invalid.", e);
                    }
                    throw new SerializationException("+The binding response does not contain " +
                        " a MAPPED-ADDRESS attribute.");
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the BindingResponse class.
        /// </summary>
        /// <param name="id">The transaction id of the binding response.</param>
        /// <param name="address">The IP address containd in the binding
        /// response message.</param>
        /// <exception cref="ArgumentNullException">The id parameter or the
        /// address parameter is null.</exception>
        internal BindingResponse(byte[] id, IPAddress address)
        {
            id.ThrowIfNull("id");
            address.ThrowIfNull("address");
            Id = id;
            Address = address;
        }

        /// <summary>
        /// Returns a new array made up of the bitwise XOR of each of the
        /// specified input arrays elements.
        /// </summary>
        /// <param name="a">The first array.</param>
        /// <param name="b">The second array.</param>
        /// <returns>A new array made up of the bitwise XOR result of
        /// each of the input arrays elements.</returns>
        /// <exception cref="ArgumentNullException">The a parameter or the
        /// b parameter is null.</exception>
        /// <exception cref="ArgumentException">The number of elements of the
        /// input arrays is not equal.</exception>
        private static byte[] Xor(byte[] a, byte[] b)
        {
            a.ThrowIfNull("a");
            b.ThrowIfNull("b");
            if (a.Length != b.Length)
            {
                throw new ArgumentException("The input arrays must have the same " +
                    "number of elements.");
            }
            byte[] ret = new byte[a.Length];
            for (int i = 0; i < a.Length; i++)
                ret[i] = (byte)(a[i] ^ b[i]);
            return ret;
        }
    }
}