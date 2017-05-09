namespace XMPPEngineer.Extensions.Socks5
{
    /// <summary>
    /// Defines possible values for the reply field of a SOCKS5 reply.
    /// </summary>
    internal enum ReplyStatus : byte
    {
        /// <summary>
        /// The requested operation succeeded.
        /// </summary>
        Succeeded = 0,

        /// <summary>
        /// A general SOCKS server failure occurred.
        /// </summary>
        GeneralServerFailure = 1,

        /// <summary>
        /// The connection is not allowed by ruleset.
        /// </summary>
        ConnectionNotAllowed = 2,

        /// <summary>
        /// The network is unreachable.
        /// </summary>
        NetworkUnreachable = 3,

        /// <summary>
        /// The host is unreachable.
        /// </summary>
        HostUnreachable = 4,

        /// <summary>
        /// The remote host refused the connection.
        /// </summary>
        ConnectionRefused = 5,

        /// <summary>
        /// The time-to-live count expired.
        /// </summary>
        TtlExpired = 6,

        /// <summary>
        /// The command is not supported.
        /// </summary>
        CommandNotSupported = 7,

        /// <summary>
        /// The address type is not supported.
        /// </summary>
        AddressTypeNotSupported = 8
    }
}