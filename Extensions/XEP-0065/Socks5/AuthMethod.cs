namespace XMPPEngineer.Extensions.Socks5
{
    /// <summary>
    /// Defines possible values for the different authentication methods
    /// supported by SOCKS5.
    /// </summary>
    internal enum AuthMethod : byte
    {
        /// <summary>
        /// No authentication.
        /// </summary>
        None = 0,

        /// <summary>
        /// GSSAPI authentication.
        /// </summary>
        Gssapi = 1,

        /// <summary>
        /// Username/Password authentication.
        /// </summary>
        Username = 2
    }
}