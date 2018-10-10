using System;
using System.IO;
using System.Runtime.Serialization;

namespace XMPPEngineer.Extensions.Socks5
{
    /// <summary>
    /// Represents a SOCKS5 Username/Password auth-response message.
    /// </summary>
    [Serializable]
    internal class AuthResponse
    {
        /// <summary>
        /// The version of the protocol.
        /// </summary>
        private const byte version = 1;

        /// <summary>
        /// Determines whether authentication has been successful.
        /// </summary>
        public bool Success
        {
            get;
            private set;
        }

        /// <summary>
        /// Serializes the instance into an array of bytes.
        /// </summary>
        /// <returns>An array of bytes representing the instance of the
        /// AuthResponse class.</returns>
        public byte[] Serialize()
        {
            return new ByteBuilder()
                .Append(version)
                .Append((byte)(Success ? 0x00 : 0xFF))
                .ToArray();
        }

        /// <summary>
        /// Deserializes a new instance of the AuthResponse class from the
        /// specified byte array.
        /// </summary>
        /// <param name="buffer">An array of bytes to deserialize a new instance
        /// of the AuthResponse class from.</param>
        /// <returns>An initialized instance of the AuthResponse class.</returns>
        /// <exception cref="ArgumentNullException">The buffer parameter is
        /// null.</exception>
        /// <exception cref="SerializationException">The specified buffer does
        /// not contain a valid SOCKS5 auth-response message.</exception>
        public static AuthResponse Deserialize(byte[] buffer)
        {
            buffer.ThrowIfNull("buffer");
            using (var ms = new MemoryStream(buffer))
            {
                using (BinaryReader r = new BinaryReader(ms))
                {
                    if (r.ReadByte() != version)
                        throw new SerializationException("Invalid auth response.");
                    bool success = r.ReadByte() == 0;
                    return new AuthResponse(success);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the AuthResponse class.
        /// </summary>
        /// <param name="success">true to create an auth-response indicating success;
        /// Otherwise false.</param>
        public AuthResponse(bool success)
        {
            Success = success;
        }
    }
}