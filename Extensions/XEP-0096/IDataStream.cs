using System;

namespace XMPPEngineer.Extensions
{
    /// <summary>
    /// Represents an XMPP extension that implements a data-stream for transferring
    /// arbitrary binary data.
    /// </summary>
    internal interface IDataStream
    {
        /// <summary>
        /// Performs the actual data-transfer implied by the specified
        /// SI session.
        /// </summary>
        /// <param name="session">The SI session whose data to transfer.</param>
        /// <exception cref="ArgumentNullException">The session parameter is
        /// null.</exception>
        /// <exception cref="NotSupportedException">The XMPP extension
        /// implementing this method is not supported by the intended recipient's
        /// XMPP client.</exception>
        /// <exception cref="XmppErrorException">The server or the XMPP entity
        /// with the specified JID returned an XMPP error code. Use the Error
        /// property of the XmppErrorException to obtain the specific error
        /// condition.</exception>
        /// <exception cref="XmppException">The server returned invalid data or
        /// another unspecified XMPP error occurred.</exception>
        void Transfer(SISession session);

        /// <summary>
        /// Cancels the data-transfer implied by the specified SI session.
        /// </summary>
        /// <param name="session">The SI session whose data-transfer to
        /// cancel.</param>
        /// <exception cref="ArgumentNullException">The session parameter is
        /// null.</exception>
        void CancelTransfer(SISession session);

        /// <summary>
        /// The event that is raised whenever bytes have been transferred.
        /// </summary>
        event EventHandler<BytesTransferredEventArgs> BytesTransferred;

        /// <summary>
        /// The event that is raised when an on-going transfer was aborted
        /// before it completed.
        /// </summary>
        event EventHandler<TransferAbortedEventArgs> TransferAborted;
    }
}