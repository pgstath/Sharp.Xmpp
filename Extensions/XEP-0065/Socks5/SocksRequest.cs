using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;

namespace XMPPEngineer.Extensions.Socks5
{
    /// <summary>
    /// Represents a SOCKS5 request message.
    /// </summary>
    [Serializable]
    internal class SocksRequest
    {
        /// <summary>
        /// The version of the protocol.
        /// </summary>
        private const byte version = 5;

        /// <summary>
        /// The command of the request.
        /// </summary>
        public SocksCommand Command
        {
            get;
            private set;
        }

        /// <summary>
        /// The type of address contained within the request.
        /// </summary>
        public ATyp ATyp
        {
            get;
            private set;
        }

        /// <summary>
        /// The destination address. The type is either IPAddress or
        /// string, depending on the value of the ATyp field.
        /// </summary>
        public object Destination
        {
            get;
            private set;
        }

        /// <summary>
        /// The port of the destination host.
        /// </summary>
        public ushort Port
        {
            get;
            private set;
        }

        /// <summary>
        /// Serializes the instance into an array of bytes.
        /// </summary>
        /// <returns>An array of bytes representing the instance of the
        /// SocksRequest class.</returns>
        public byte[] Serialize()
        {
            byte[] dest;
            if (Destination is IPAddress)
                dest = (Destination as IPAddress).GetAddressBytes();
            else
            {
                byte[] domainBytes = Encoding.ASCII.GetBytes((string)Destination);
                dest = new byte[domainBytes.Length + 1];
                dest[0] = Convert.ToByte(domainBytes.Length);
                for (int i = 0; i < domainBytes.Length; i++)
                    dest[1 + i] = domainBytes[i];
            }
            return new ByteBuilder()
                .Append(version)
                .Append((byte)Command)
                .Append((byte)0x00)
                .Append((byte)ATyp)
                .Append(dest)
                .Append(Port, bigEndian: true)
                .ToArray();
        }

        /// <summary>
        /// Deserializes a new instance of the SocksRequest class from the
        /// specified byte array.
        /// </summary>
        /// <param name="buffer">An array of bytes to deserialize a new instance
        /// of the SocksRequest class from.</param>
        /// <returns>An initialized instance of the SocksRequest class.</returns>
        /// <exception cref="ArgumentNullException">The buffer parameter is
        /// null.</exception>
        /// <exception cref="SerializationException">The specified buffer does
        /// not contain a valid SOCKS5 request message.</exception>
        public static SocksRequest Deserialize(byte[] buffer)
        {
            using (var ms = new MemoryStream(buffer))
            {
                using (BinaryReader r = new BinaryReader(ms))
                {
                    if (r.ReadByte() != version)
                        throw new SerializationException("Invalid SOCKS5 request.");
                    SocksCommand command = (SocksCommand)r.ReadByte();
                    // Skip reserved octet.
                    r.ReadByte();
                    ATyp atyp = (ATyp)r.ReadByte();
                    IPAddress addr = null;
                    string domain = null;
                    switch (atyp)
                    {
                        case ATyp.IPv4:
                        case ATyp.IPv6:
                            addr = new IPAddress(r.ReadBytes(atyp == ATyp.IPv4 ? 4 : 16));
                            break;

                        case ATyp.Domain:
                            byte len = r.ReadByte();
                            domain = Encoding.ASCII.GetString(r.ReadBytes(len));
                            break;
                    }
                    ushort port = r.ReadUInt16(true);
                    if (atyp == ATyp.Domain)
                        return new SocksRequest(command, domain, port);
                    return new SocksRequest(command, addr, port);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the SocksRequest class.
        /// </summary>
        /// <param name="command">The command of the request.</param>
        /// <param name="destination">The IP address of the destination host.</param>
        /// <param name="port">The port of the destination host.</param>
        /// <exception cref="ArgumentNullException">The destination parameter
        /// is null.</exception>
        public SocksRequest(SocksCommand command, IPAddress destination,
            ushort port)
        {
            destination.ThrowIfNull("destination");
            Command = command;
            ATyp = destination.AddressFamily == AddressFamily.InterNetworkV6 ?
                ATyp.IPv6 : ATyp.IPv4;
            Destination = destination;
            Port = port;
        }

        /// <summary>
        /// Initializes a new instance of the SocksRequest class.
        /// </summary>
        /// <param name="command">The command of the request.</param>
        /// <param name="domain">The fully-qualified domain name of the
        /// destination host.</param>
        /// <param name="port">The port of the destination host.</param>
        /// <exception cref="ArgumentNullException">The domain parameter
        /// is null.</exception>
        /// <exception cref="ArgumentException">The length of the domain string
        /// exceeds 255 characters.</exception>
        public SocksRequest(SocksCommand command, string domain, ushort port)
        {
            domain.ThrowIfNull("domain");
            if (domain.Length > 255)
            {
                throw new ArgumentException("The length of the domain string must " +
                    "not exceed 255 characters.");
            }
            Command = command;
            ATyp = ATyp.Domain;
            Destination = domain;
            Port = port;
        }
    }
}