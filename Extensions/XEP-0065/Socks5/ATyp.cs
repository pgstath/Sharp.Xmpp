namespace XMPPEngineer.Extensions.Socks5
{
    /// <summary>
    /// Defines possible values for the address type field.
    /// </summary>
    internal enum ATyp : byte
    {
        /// <summary>
        /// The address is an IPv4 address.
        /// </summary>
        IPv4 = 1,

        /// <summary>
        /// The address is a fully-qualified domain name.
        /// </summary>
        Domain = 3,

        /// <summary>
        /// The address is an IPv6 address.
        /// </summary>
        IPv6 = 4
    }
}