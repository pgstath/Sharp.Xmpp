using System;

namespace XMPPEngineer.Im
{
    /// <summary>
    /// Provides data for the Error event.
    /// </summary>
    public class ErrorEventArgs : Core.ErrorEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the ErrorEventArgs class.
        /// </summary>
        /// <param name="e">The exception that causes the error event.</param>
        /// <exception cref="ArgumentNullException">The e parameter is null.</exception>
        public ErrorEventArgs(Exception e)
            : base(e)
        {
        }
    }
}