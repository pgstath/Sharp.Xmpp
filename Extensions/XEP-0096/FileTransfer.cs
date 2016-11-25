using System;

namespace Sharp.Xmpp.Extensions
{
    /// <summary>
    /// Represents an on-going file-transfer operation.
    /// </summary>
    [Serializable]
    public class FileTransfer : IFileTransfer
    {
        /// <summary>
        /// The JID of the XMPP entity that is sending the file.
        /// </summary>
        public Jid From
        {
            get;
            private set;
        }

        /// <summary>
        /// The JID of the XMPP entity that is receiving the file.
        /// </summary>
        public Jid To
        {
            get;
            private set;
        }

        /// <summary>
        /// The name of the file being transfered.
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// The size of the file being transfered, in bytes.
        /// </summary>
        public long Size
        {
            get;
            private set;
        }

        /// <summary>
        /// The number of bytes transferred.
        /// </summary>
        public long Transferred
        {
            get;
            private set;
        }

        /// <summary>
        /// A description of the file provided by the sender so that the receiver
        /// can better understand what is being sent.
        /// </summary>
        /// <remarks>This may be null.</remarks>
        public string Description
        {
            get;
            private set;
        }

        /// <summary>
        /// An opaque identifier uniquely identifying the file-transfer operation.
        /// </summary>
        public string SessionId
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the FileTransfer class.
        /// </summary>
        /// <param name="from">The JID of the XMPP entity that is sending the
        /// file.</param>
        /// <param name="to">The JID of the XMPP entity that is receiving the
        /// file.</param>
        /// <param name="name">The name of the file.</param>
        /// <param name="size">The size of the file, in bytes.</param>
        /// <param name="sessionId">The session id associated with the
        /// file-transfer.</param>
        /// <param name="description">A description of the file.</param>
        /// <param name="transferred">The number of bytes transferred.</param>
        /// <exception cref="ArgumentNullException">The from parameter or the to
        /// parameter or the name parameter is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The size parameter
        /// is negative.</exception>
        internal FileTransfer(Jid from, Jid to, string name, long size,
            string sessionId = null, string description = null, long transferred = 0)
        {
            from.ThrowIfNull("from");
            to.ThrowIfNull("to");
            name.ThrowIfNull("name");
            size.ThrowIfOutOfRange("size", 0, Int64.MaxValue);
            transferred.ThrowIfOutOfRange("transferred", 0, size);
            From = from;
            To = to;
            Name = name;
            Size = size;
            SessionId = sessionId;
            Description = description;
            Transferred = transferred;
        }

        /// <summary>
        /// Initializes a new instance of the FileTransfer class.
        /// </summary>
        /// <param name="session">The SISession instance to initialize this
        /// instance from.</param>
        /// <param name="name">The name of the file.</param>
        /// <param name="description">A description of the file.</param>
        /// <exception cref="ArgumentNullException">The session parameter or
        /// the name parameter is null.</exception>
        internal FileTransfer(SISession session, string name, string description) :
            this(session.From, session.To, name, session.Size, session.Sid,
            description, session.Count)
        {
        }
    }
}