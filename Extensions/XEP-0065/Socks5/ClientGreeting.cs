using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;

namespace XMPPEngineer.Extensions.Socks5
{
    /// <summary>
    /// Represents a SOCKS5 client-greeting message.
    /// </summary>
    [Serializable]
    internal class ClientGreeting
    {
        /// <summary>
        /// The version of the protocol.
        /// </summary>
        private const byte version = 5;

        /// <summary>
        /// A set of authentication methods advertised to the server.
        /// </summary>
        private HashSet<AuthMethod> methods = new HashSet<AuthMethod>();

        /// <summary>
        /// An enumerable collection of authentication methods.
        /// </summary>
        public IEnumerable<AuthMethod> Methods
        {
            get
            {
                return methods;
            }
        }

        /// <summary>
        /// Serializes the instance into an array of bytes.
        /// </summary>
        /// <returns>An array of bytes representing the instance of the
        /// ClientGreeting class.</returns>
        public byte[] Serialize()
        {
            ByteBuilder b = new ByteBuilder()
                .Append(version)
                .Append((byte)methods.Count);
            foreach (AuthMethod m in Methods)
                b.Append((byte)m);
            return b.ToArray();
        }

        /// <summary>
        /// Deserializes a new instance of the ClientGreeting class from the
        /// specified byte array.
        /// </summary>
        /// <param name="buffer">An array of bytes to deserialize a new instance
        /// of the ClientGreeting class from.</param>
        /// <returns>An initialized instance of the ClientGreeting class.</returns>
        /// <exception cref="ArgumentNullException">The buffer parameter is
        /// null.</exception>
        /// <exception cref="SerializationException">The specified buffer does
        /// not contain a valid SOCKS5 client-greeting message.</exception>
        public static ClientGreeting Deserialize(byte[] buffer)
        {
            buffer.ThrowIfNull("buffer");
            using (var ms = new MemoryStream(buffer))
            {
                using (BinaryReader r = new BinaryReader(ms))
                {
                    if (r.ReadByte() != version)
                        throw new SerializationException("Invalid SOCKS5 greeting.");
                    byte count = r.ReadByte();
                    AuthMethod[] methods = new AuthMethod[count];
                    for (int i = 0; i < count; i++)
                        methods[i] = (AuthMethod)r.ReadByte();
                    return new ClientGreeting(methods);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the ClientGreeting class.
        /// </summary>
        /// <param name="methods">The authentication methods the client wishes
        /// to advertise.</param>
        public ClientGreeting(params AuthMethod[] methods)
        {
            if (methods != null)
            {
                foreach (var m in methods)
                    this.methods.Add(m);
            }
        }

        /// <summary>
        /// Initializes a new instance of the ClientGreeting class.
        /// </summary>
        /// <param name="methods">An enumerable collection of authentication
        /// methods the client wishes to advertise.</param>
        /// <exception cref="ArgumentNullException">The methods parameter is
        /// null.</exception>
        public ClientGreeting(IEnumerable<AuthMethod> methods)
        {
            methods.ThrowIfNull("methods");
            foreach (var m in methods)
                this.methods.Add(m);
        }
    }
}