using System;
using System.IO;
using System.Runtime.Serialization;

namespace XMPPEngineer.Extensions.Socks5
{
    /// <summary>
    /// Represents a SOCKS5 server-greeting message.
    /// </summary>
    [Serializable]
    internal class ServerGreeting
    {
        /// <summary>
        /// The version of the protocol.
        /// </summary>
        private const byte version = 5;

        /// <summary>
        /// The authentication method selected by the server.
        /// </summary>
        public AuthMethod Method
        {
            get;
            private set;
        }

        /// <summary>
        /// Serializes the instance into an array of bytes.
        /// </summary>
        /// <returns>An array of bytes representing the instance of the
        /// ServerGreeting class.</returns>
        public byte[] Serialize()
        {
            return new ByteBuilder()
                .Append(version)
                .Append((byte)Method)
                .ToArray();
        }

        /// <summary>
        /// Deserializes a new instance of the ServerGreeting class from the
        /// specified byte array.
        /// </summary>
        /// <param name="buffer">An array of bytes to deserialize a new instance
        /// of the ServerGreeting class from.</param>
        /// <returns>An initialized instance of the ServerGreeting class.</returns>
        /// <exception cref="ArgumentNullException">The buffer parameter is
        /// null.</exception>
        /// <exception cref="SerializationException">The specified buffer does
        /// not contain a valid SOCKS5 server-greeting message.</exception>
        public static ServerGreeting Deserialize(byte[] buffer)
        {
            buffer.ThrowIfNull("buffer");
            using (var ms = new MemoryStream(buffer))
            {
                using (BinaryReader r = new BinaryReader(ms))
                {
                    if (r.ReadByte() != version)
                        throw new SerializationException("Invalid SOCKS5 greeting.");
                    AuthMethod method = (AuthMethod)r.ReadByte();
                    return new ServerGreeting(method);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the ServerGreeting class.
        /// </summary>
        /// <param name="method">The authentication method to include as part of
        /// the greeting message.</param>
        public ServerGreeting(AuthMethod method)
        {
            Method = method;
        }
    }
}