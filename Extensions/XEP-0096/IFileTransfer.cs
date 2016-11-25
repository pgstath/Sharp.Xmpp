namespace Sharp.Xmpp.Extensions
{
    /// <summary>
    /// Represents an on-going file-transfer operation.
    /// </summary>
    public interface IFileTransfer
    {
        /// <summary>
        /// A description of the file provided by the sender so that the receiver
        /// can better understand what is being sent.
        /// </summary>
        /// <remarks>This may be null.</remarks>
        string Description { get; }

        /// <summary>
        /// The JID of the XMPP entity that is sending the file.
        /// </summary>
        Jid From { get; }

        /// <summary>
        /// The name of the file being transferred.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// An opaque identifier uniquely identifying the file-transfer operation.
        /// </summary>
        string SessionId { get; }

        /// <summary>
        /// The size of the file being transferred, in bytes.
        /// </summary>
        long Size { get; }

        /// <summary>
        /// The JID of the XMPP entity that is receiving the file.
        /// </summary>
        Jid To { get; }

        /// <summary>
        /// The number of bytes transferred.
        /// </summary>
        long Transferred { get; }
    }
}