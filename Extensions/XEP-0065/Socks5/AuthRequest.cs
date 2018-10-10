using System;
using System.Text;

namespace XMPPEngineer.Extensions.Socks5
{
    /// <summary>
    /// Represents a SOCKS5 Username/Password auth-request message.
    /// </summary>
    [Serializable]
    internal class AuthRequest
    {
        /// <summary>
        /// The version of the protocol.
        /// </summary>
        private const byte version = 1;

        /// <summary>
        /// The username to authenticate with.
        /// </summary>
        public string Username
        {
            get;
            private set;
        }

        /// <summary>
        /// The password to authenticate with.
        /// </summary>
        public string Password
        {
            get;
            private set;
        }

        /// <summary>
        /// Serializes the instance into an array of bytes.
        /// </summary>
        /// <returns>An array of bytes representing the instance of the
        /// AuthRequest class.</returns>
        public byte[] Serialize()
        {
            byte[] usernameBytes = Encoding.ASCII.GetBytes(Username);
            byte[] passwordBytes = Encoding.ASCII.GetBytes(Password);
            return new ByteBuilder()
                .Append(version)
                .Append(Convert.ToByte(usernameBytes.Length))
                .Append(usernameBytes)
                .Append(Convert.ToByte(passwordBytes.Length))
                .Append(passwordBytes)
                .ToArray();
        }

        /// <summary>
        /// Initializes a new instance of the AuthRequest class.
        /// </summary>
        /// <param name="username">The username to authenticate with.</param>
        /// <param name="password">The password to authenticate with.</param>
        /// <exception cref="ArgumentNullException">The username parameter or
        /// the password parameter is null.</exception>
        public AuthRequest(string username, string password)
        {
            username.ThrowIfNull("username");
            password.ThrowIfNull("password");
            Username = username;
            Password = password;
        }
    }
}