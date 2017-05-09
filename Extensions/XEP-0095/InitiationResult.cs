using System;
using System.Xml;

namespace XMPPEngineer.Extensions
{
    /// <summary>
    /// Represents the result of a stream initiation.
    /// </summary>
    internal class InitiationResult
    {
        /// <summary>
        /// The session identifier which must be used when talking to the XMPP entity
        /// with which the session has been negotiated.
        /// </summary>
        public string SessionId
        {
            get;
            private set;
        }

        /// <summary>
        /// The negotiated stream-method.
        /// </summary>
        public string Method
        {
            get;
            private set;
        }

        /// <summary>
        /// The 'si' data element with any additional data which may be required by
        /// an SI profile.
        /// </summary>
        public XmlElement Data
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the InitiationResult class.
        /// </summary>
        /// <param name="sessionId">The session identifier negotiated during
        /// stream initiation.</param>
        /// <param name="method">The stream method negotiated during stream
        /// negotiation.</param>
        /// <param name="data">The 'si' element as received in the SI
        /// response.</param>
        /// <exception cref="ArgumentNullException">The sessionId parameter or
        /// the method parameter is null.</exception>
        public InitiationResult(string sessionId, string method,
            XmlElement data = null)
        {
            sessionId.ThrowIfNull("sessionId");
            method.ThrowIfNull("method");
            SessionId = sessionId;
            Method = method;
            Data = data;
        }
    }
}