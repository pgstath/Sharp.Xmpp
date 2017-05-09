using System;

namespace XMPPEngineer.Extensions
{
    /// <summary>
    /// Provides data for the 'TransferAborted' event which is raised by
    /// IDataStream implementations.
    /// </summary>
    [Serializable]
    internal class TransferAbortedEventArgs : EventArgs
    {
        /// <summary>
        /// The session for which the event is raised.
        /// </summary>
        public SISession Session
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the BytesTransferredEventArgs class.
        /// </summary>
        /// <param name="session">The session for which the event is raised.</param>
        /// <exception cref="ArgumentNullException">The session parameter
        /// is null.</exception>
        public TransferAbortedEventArgs(SISession session)
        {
            session.ThrowIfNull("session");
            Session = session;
        }
    }
}