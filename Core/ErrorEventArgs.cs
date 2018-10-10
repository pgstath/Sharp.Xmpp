using System;

namespace XMPPEngineer.Core
{
    /// <summary>
    /// Provides data for the Error event.
    /// </summary>
    public class ErrorEventArgs : EventArgs
    {
        /// <summary>
        /// The reason why the error event was raised.
        /// </summary>
        public string Reason
        {
            get
            {
                return Exception.Message;
            }
        }

        /// <summary>
        /// The exception that caused the error event.
        /// </summary>
        public Exception Exception
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the ErrorEventArgs class.
        /// </summary>
        /// <param name="e">The exception that causes the error event.</param>
        /// <exception cref="ArgumentNullException">The e parameter is null.</exception>
        public ErrorEventArgs(Exception e)
        {
            e.ThrowIfNull("e");
            Exception = e;
        }
    }
}