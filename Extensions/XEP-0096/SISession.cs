using System;
using System.IO;

namespace XMPPEngineer.Extensions
{
    /// <summary>
    /// Represents an SI session as is negotiated during stream initiation.
    /// </summary>
    [Serializable]
    internal class SISession
    {
        /// <summary>
        /// The identifier of the session.
        /// </summary>
        public string Sid
        {
            get;
            private set;
        }

        /// <summary>
        /// The IO-stream from which data is read, if we are sending the data or
        /// to which data is written, if we are receiving the data.
        /// </summary>
        public Stream Stream
        {
            get;
            private set;
        }

        /// <summary>
        /// The number of bytes to read from the IO-stream, if we are sending the
        /// data, or the number of bytes to write to the IO-stream, if we are
        /// receiving the data.
        /// </summary>
        public long Size
        {
            get;
            private set;
        }

        /// <summary>
        /// The number of bytes read, if we are receiving the data, or the number
        /// of bytes sent, if we are sending the data.
        /// </summary>
        public long Count
        {
            get;
            set;
        }

        /// <summary>
        /// Determines whether data is being received or data is being sent.
        /// </summary>
        public bool Receiving
        {
            get;
            private set;
        }

        /// <summary>
        /// The JID of the XMPP entity that is sending the data.
        /// </summary>
        public Jid From
        {
            get;
            private set;
        }

        /// <summary>
        /// The JID of the XMPP entity that is receiving the data.
        /// </summary>
        public Jid To
        {
            get;
            private set;
        }

        /// <summary>
        /// A reference to the XMPP extension performing the actual transfer.
        /// </summary>
        public IDataStream Extension
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the SISession class.
        /// </summary>
        /// <param name="sid">The identifier of the session.</param>
        /// <param name="stream">The IO-stream from which data is read, or to
        /// which data is written.</param>
        /// <param name="size">The total number of bytes to read from, or to
        /// write to the stream.</param>
        /// <param name="receiving">true if data is being received over the session;
        /// Otherwise false.</param>
        /// <param name="from">The JID of the XMPP entity that wishes to send data.</param>
        /// <param name="to">The JID of the XMPP entity that wishes to receive
        /// data.</param>
        /// <param name="extension">The instance of the data-stream extension
        /// negotiated during session-initiation.</param>
        /// <exception cref="ArgumentNullException">The sid parameter or the stream
        /// parameter or the from parameter or the to parameter or the extension
        /// parameter is null.</exception>
        /// <exception cref="ArgumentException">The receiving parameter is true, but
        /// the specified stream cannot be written, or the receiving parameter is
        /// false, but the specified stream cannot be read.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The size parameter is
        /// negative.</exception>
        public SISession(string sid, Stream stream, long size, bool receiving,
            Jid from, Jid to, IDataStream extension)
        {
            sid.ThrowIfNull("sid");
            stream.ThrowIfNull("stream");
            size.ThrowIfOutOfRange(0, Int64.MaxValue);
            from.ThrowIfNull("from");
            to.ThrowIfNull("to");
            extension.ThrowIfNull("extension");
            if (receiving && !stream.CanWrite)
                throw new ArgumentException("The specified stream cannot be written.");
            if (!receiving && !stream.CanRead)
                throw new ArgumentException("The specified stream cannot be read.");
            Sid = sid;
            Stream = stream;
            Size = size;
            Count = 0;
            Receiving = receiving;
            From = from;
            To = to;
            Extension = extension;
        }
    }
}