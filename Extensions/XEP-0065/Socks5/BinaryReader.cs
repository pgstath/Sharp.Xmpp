using System.IO;

namespace XMPPEngineer.Extensions.Socks5
{
    /// <summary>
    /// Adds extension methods to the BinaryReader class to simplify the
    /// deserialization of SOCKS5 messages.
    /// </summary>
    internal static class BinaryReaderExtensions
    {
        /// <summary>
        /// Reads an unsigned integer value from the underlying stream,
        /// optionally using big endian byte ordering.
        /// </summary>
        /// <param name="reader">Extension method for BinaryReader.</param>
        /// <param name="bigEndian">Set to true to interpret the integer value
        /// as big endian value.</param>
        /// <returns>The 32-byte unsigned integer value read from the underlying
        /// stream.</returns>
        public static uint ReadUInt32(this BinaryReader reader, bool bigEndian)
        {
            if (!bigEndian)
                return reader.ReadUInt32();
            int ret = 0;
            ret |= (reader.ReadByte() << 24);
            ret |= (reader.ReadByte() << 16);
            ret |= (reader.ReadByte() << 8);
            ret |= (reader.ReadByte() << 0);
            return (uint)ret;
        }

        /// <summary>
        /// Reads an unsigned short value from the underlying stream, optionally
        /// using big endian byte ordering.
        /// </summary>
        /// <param name="reader">Extension method for BinaryReader.</param>
        /// <param name="bigEndian">Set to true to interpret the short value
        /// as big endian value.</param>
        /// <returns>The 16-byte unsigned short value read from the underlying
        /// stream.</returns>
        public static ushort ReadUInt16(this BinaryReader reader, bool bigEndian)
        {
            if (!bigEndian)
                return reader.ReadUInt16();
            int ret = 0;
            ret |= (reader.ReadByte() << 8);
            ret |= (reader.ReadByte() << 0);
            return (ushort)ret;
        }

        /// <summary>
        /// Reads an short value from the underlying stream, optionally using
        /// big endian byte ordering.
        /// </summary>
        /// <param name="reader">Extension method for BinaryReader.</param>
        /// <param name="bigEndian">Set to true to interpret the short value
        /// as big endian value.</param>
        /// <returns>The 16-byte short value read from the underlying
        /// stream.</returns>
        public static short ReadInt16(this BinaryReader reader, bool bigEndian)
        {
            if (!bigEndian)
                return reader.ReadInt16();
            int ret = 0;
            ret |= (reader.ReadByte() << 8);
            ret |= (reader.ReadByte() << 0);
            return (short)ret;
        }
    }
}