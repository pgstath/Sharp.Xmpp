using XMPPEngineer.Core;

namespace XMPPEngineer.Extensions
{
    /// <summary>
    /// Represents an extension that filters incoming stanzas.
    /// </summary>
    /// <typeparam name="T">The type of the stanzas to filter.</typeparam>
    internal interface IInputFilter<T> where T : Stanza
    {
        /// <summary>
        /// Invoked when a stanza is being received.
        /// </summary>
        /// <param name="stanza">The stanza which is being received.</param>
        /// <returns>true to intercept the stanza or false to pass the stanza
        /// on to the next handler.</returns>
        bool Input(T stanza);
    }
}