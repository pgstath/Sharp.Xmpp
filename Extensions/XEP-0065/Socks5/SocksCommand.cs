namespace XMPPEngineer.Extensions.Socks5
{
    /// <summary>
    /// Defines possible values for the command field of a SOCKS5 request.
    /// </summary>
    internal enum SocksCommand : byte
    {
        /// <summary>
        /// Connect to a remote host.
        /// </summary>
        Connect = 1,

        /// <summary>
        /// Bind to accept connections from the server.
        /// </summary>
        Bind = 2,

        /// <summary>
        /// Establish an association within the UDP relay process to handle
        /// UDP datagrams.
        /// </summary>
        UdpAssociate = 3
    }
}