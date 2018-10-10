using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;

namespace XMPPEngineer.Extensions.Socks5
{
    /// <summary>
    /// Represents a SOCKS5 reply message.
    /// </summary>
    [Serializable]
    internal class SocksReply
    {
        /// <summary>
        /// The version of the protocol.
        /// </summary>
        private const byte version = 5;

        /// <summary>
        /// The status of the reply.
        /// </summary>
        public ReplyStatus Status
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
        public object Address
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
        /// SocksReply class.</returns>
        public byte[] Serialize()
        {
            byte[] addr;
            if (Address is IPAddress)
                addr = (Address as IPAddress).GetAddressBytes();
            else
            {
                byte[] domainBytes = Encoding.ASCII.GetBytes((string)Address);
                addr = new byte[domainBytes.Length + 1];
                addr[0] = Convert.ToByte(domainBytes.Length);
                for (int i = 0; i < domainBytes.Length; i++)
                    addr[1 + i] = domainBytes[i];
            }
            return new ByteBuilder()
                .Append(version)
                .Append((byte)Status)
                .Append((byte)0x00)
                .Append((byte)ATyp)
                .Append(addr)
                .Append(Port, bigEndian: true)
                .ToArray();
        }

        /// <summary>
        /// Deserializes a new instance of the SocksReply class from the
        /// specified byte array.
        /// </summary>
        /// <param name="buffer">An array of bytes to deserialize a new instance
        /// of the SocksReply class from.</param>
        /// <returns>An initialized instance of the SocksReply class.</returns>
        /// <exception cref="ArgumentNullException">The buffer parameter is
        /// null.</exception>
        /// <exception cref="SerializationException">The specified buffer does
        /// not contain a valid SOCKS5 reply message.</exception>
        public static SocksReply Deserialize(byte[] buffer)
        {
            buffer.ThrowIfNull("buffer");
            using (var ms = new MemoryStream(buffer))
            {
                using (BinaryReader r = new BinaryReader(ms))
                {
                    if (r.ReadByte() != version)
                        throw new SerializationException("Invalid SOCKS5 reply.");
                    ReplyStatus status = (ReplyStatus)r.ReadByte();
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
                        return new SocksReply(status, domain, port);
                    return new SocksReply(status, addr, port);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the SocksReply class.
        /// </summary>
        /// <param name="status">The status of the response.</param>
        /// <param name="address">The server bound IP address.</param>
        /// <param name="port">The server bound port.</param>
        /// <exception cref="ArgumentNullException">The address parameter
        /// is null.</exception>
        public SocksReply(ReplyStatus status, IPAddress address, ushort port)
        {
            address.ThrowIfNull("address");
            Status = status;
            ATyp = address.AddressFamily == AddressFamily.InterNetworkV6 ?
                ATyp.IPv6 : ATyp.IPv4;
            Address = address;
            Port = port;
        }

        /// <summary>
        /// Initializes a new instance of the SocksReply class.
        /// </summary>
        /// <param name="status">The status of the response.</param>
        /// <param name="domain">The server bound fully-qualified domain
        /// name.</param>
        /// <param name="port">The server bound port.</param>
        /// <exception cref="ArgumentNullException">The domain parameter
        /// is null.</exception>
        /// <exception cref="ArgumentException">The length of the domain string
        /// exceeds 255 characters.</exception>
        public SocksReply(ReplyStatus status, string domain, ushort port)
        {
            domain.ThrowIfNull("domain");
            if (domain.Length > 255)
            {
                throw new ArgumentException("The length of the domain string must " +
                    "not exceed 255 characters.");
            }
            Status = status;
            ATyp = ATyp.Domain;
            Address = domain;
            Port = port;
        }
    }
}