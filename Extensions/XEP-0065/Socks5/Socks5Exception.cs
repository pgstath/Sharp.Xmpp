using System;
using System.Runtime.Serialization;

namespace XMPPEngineer.Extensions.Socks5
{
    /// <summary>
    /// The exception that is thrown when a SOCKS5 error has been encountered.
    /// </summary>
    [Serializable()]
    internal class Socks5Exception : Exception
    {
        /// <summary>
        /// Initializes a new instance of the Socks5Exception class
        /// </summary>
        public Socks5Exception() : base() { }

        /// <summary>
        /// Initializes a new instance of the Socks5Exception class with its message
        /// string set to <paramref name="message"/>.
        /// </summary>
        /// <param name="message">A description of the error. The content of message is intended
        /// to be understood by humans.</param>
        public Socks5Exception(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the Socks5Exception class with its message
        /// string set to <paramref name="message"/> and a reference to the inner exception that
        /// is the cause of this exception.
        /// </summary>
        /// <param name="message">A description of the error. The content of message is intended
        /// to be understood by humans.</param>
        /// <param name="inner">The exception that is the cause of the current exception.</param>
        public Socks5Exception(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// Initializes a new instance of the Socks5Exception class with the specified
        /// serialization and context information.
        /// </summary>
        /// <param name="info">An object that holds the serialized object data about the exception
        /// being thrown. </param>
        /// <param name="context">An object that contains contextual information about the source
        /// or destination. </param>
        protected Socks5Exception(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}