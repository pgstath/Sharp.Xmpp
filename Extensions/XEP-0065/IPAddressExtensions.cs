using System;
using System.Net;

namespace XMPPEngineer.Extensions
{
    /// <summary>
    /// Contains extension methods for the IPAddress class.
    /// </summary>
    internal static class IPAddressExtensions
    {
        /// <summary>
        /// Bitwise ANDs the IP address with the specified netmask.
        /// </summary>
        /// <param name="address">The IPAddress instance to AND with the specified
        /// netmask.</param>
        /// <param name="netmask">The netmask to bitwise AND with the IP
        /// address.</param>
        /// <returns>The result of the bitwise AND.</returns>
        /// <exception cref="ArgumentNullException">The netmask parameter is
        /// null.</exception>
        /// <exception cref="ArgumentException">The netmask and IP address must be
        /// of the same address family.</exception>
        /// <remarks>This is an extension method for the IPAddress class.</remarks>
        private static IPAddress And(this IPAddress address, IPAddress netmask)
        {
            netmask.ThrowIfNull("netmask");
            if (address.AddressFamily != netmask.AddressFamily)
                throw new ArgumentException("The address family of the specified netmask " +
                    "is different from the address family of the IP address.");
            byte[] buffer = address.GetAddressBytes();
            byte[] other = netmask.GetAddressBytes();
            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = (byte)(buffer[i] & other[i]);
            return new IPAddress(buffer);
        }

        /// <summary>
        /// Determines whether the IP address is in the same subnet as the
        /// specified IP address.
        /// </summary>
        /// <param name="address">The IPAddress instance this is being called
        /// for.</param>
        /// <param name="other">The IP address to determine whether it is in the same
        /// subnet.</param>
        /// <param name="netmask">The subnetmask to apply.</param>
        /// <returns>true if both IP address are located in the same subnet; Otherwise
        /// false.</returns>
        /// <exception cref="ArgumentNullException">The other parameter or the netmask
        /// parameter is null.</exception>
        /// <exception cref="ArgumentException">The netmask and IP address must be
        /// of the same address family.</exception>
        /// <remarks>This is an extension method for the IPAddress class.</remarks>
        public static bool InSameSubnet(this IPAddress address, IPAddress other,
            IPAddress netmask)
        {
            other.ThrowIfNull("other");
            netmask.ThrowIfNull("netmask");

            return address.And(netmask).Equals(other.And(netmask));
        }
    }
}