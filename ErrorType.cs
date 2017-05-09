namespace XMPPEngineer
{
    /// <summary>
    /// Defines possible values for the type of XMPP errors.
    /// </summary>
    public enum ErrorType
    {
        /// <summary>
        /// Do not retry (the error is unrecoverable).
        /// </summary>
        Cancel,

        /// <summary>
        /// Proceed (the condition was only a warning).
        /// </summary>
        Continue,

        /// <summary>
        /// Retry after changing the data sent.
        /// </summary>
        Modify,

        /// <summary>
        /// Retry after providing credentials.
        /// </summary>
        Auth,

        /// <summary>
        /// Retry after waiting (the error is temporary).
        /// </summary>
        Wait
    }
}