namespace XMPPEngineer.Core
{
    /// <summary>
    /// Defines the possible types for IQ stanzas.
    /// </summary>
    public enum IqType
    {
        /// <summary>
        /// The stanza provides required data, sets new values, or replaces
        /// existing values.
        /// </summary>
        Set,

        /// <summary>
        /// The stanza is a request for information or requirements.
        /// </summary>
        Get,

        /// <summary>
        /// The stanza is a response to a successful get or set request.
        /// </summary>
        Result,

        /// <summary>
        /// An error has occurred regarding processing or delivery of a
        /// previously-sent get or set.
        /// </summary>
        Error
    }
}