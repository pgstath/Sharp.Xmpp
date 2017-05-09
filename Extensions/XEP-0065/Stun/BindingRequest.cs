using XMPPEngineer.Extensions.Socks5;
using System;
using System.Security.Cryptography;

namespace XMPPEngineer.Extensions.Stun
{
    /// <summary>
    /// Represents a STUN 'Binding Request' message.
    /// </summary>
    /// <remarks>For details, refer to RFC 5389.</remarks>
    internal class BindingRequest
    {
        /// <summary>
        /// The STUN message type for a 'Binding Request' message.
        /// </summary>
        private const short stunMessageType = 0x0100;

        /// <summary>
        /// The 'Magic Cookie' value as defined in RFC 5389.
        /// </summary>
        private readonly byte[] magicCookie = new byte[] { 0x21, 0x12, 0xA4, 0x42 };

        /// <summary>
        /// The crypto provider for generating random transaction ids.
        /// </summary>
        private static RNGCryptoServiceProvider cryptoProvider =
            new RNGCryptoServiceProvider();

        /// <summary>
        /// The transaction id of the binding request.
        /// </summary>
        public byte[] Id
        {
            get;
            private set;
        }

        /// <summary>
        /// Serializes this instance into an array of bytes.
        /// </summary>
        /// <returns>An array of bytes representing this instance of the
        /// BindingRequest class.</returns>
        public byte[] Serialize()
        {
            return new ByteBuilder()
                .Append(stunMessageType)
                // The length of the attributes as a 16-bit short value.
                .Append(0x00, 0x00)
                .Append(magicCookie)
                .Append(Id)
                .ToArray();
        }

        /// <summary>
        /// Initializes a new instance of the BindingRequest class.
        /// </summary>
        /// <param name="id">The transaction id to use for the binding
        /// request.</param>
        /// <remarks>If specified, the id parameter must have a length of exactly
        /// 12 bytes.</remarks>
        /// <exception cref="ArgumentException">The length of the id parameter is
        /// not equal to 12.</exception>
        public BindingRequest(byte[] id = null)
        {
            if (id != null)
            {
                if (id.Length != 12)
                {
                    throw new ArgumentException("The id parameter must have a length " +
                        "of 12.");
                }
                Id = id;
            }
            else
            {
                Id = new byte[12];
                cryptoProvider.GetBytes(Id);
            }
        }
    }
}