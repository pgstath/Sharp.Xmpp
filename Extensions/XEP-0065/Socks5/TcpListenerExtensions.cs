using System;
using System.Net.Sockets;

namespace XMPPEngineer.Extensions.Socks5
{
    /// <summary>
    /// Provides extension methods for the TcpListener class.
    /// </summary>
    internal static class TcpListenerExtensions
    {
        /// <summary>
        /// Accepts a pending connection request.
        /// </summary>
        /// <param name="listener">TcpListener extension.</param>
        /// <param name="timeout">The number of milliseconds to wait for a
        /// connection, or -1 to wait indefinitely.</param>
        /// <returns>A TcpClient instance used to send and receive data.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The timeout parameter
        /// is a negative value other than -1.</exception>
        /// <exception cref="ObjectDisposedException">The socket has been
        /// closed.</exception>
        /// <exception cref="SocketException">An error occurred while attempting to
        /// access the socket.</exception>
        /// <exception cref="TimeoutException">A timeout was specified and it
        /// expired.</exception>
        public static TcpClient AcceptTcpClient(this TcpListener listener, int timeout)
        {
            if (timeout == -1)
                return listener.AcceptTcpClient();
            timeout.ThrowIfOutOfRange("timeout", 0, int.MaxValue);
            IAsyncResult ar = listener.BeginAcceptTcpClient(null, null);
            bool signalled = ar.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(timeout));
            if (signalled)
                return listener.EndAcceptTcpClient(ar);
            throw new TimeoutException("The operation timed out.");
        }
    }
}