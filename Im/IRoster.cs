using System.Collections.Generic;

namespace Sharp.Xmpp.Im
{
    /// <summary>
    /// Represents the user's roster.
    /// </summary>
    /// <remarks>In XMPP jargon, the user's contact list is called a 'roster'.</remarks>
    public interface IRoster
    {
        /// <summary>
        /// The number of items on the roster.
        /// </summary>
        int Count { get; }
        
        /// <summary>
        /// Returns an enumerator that iterates through the collection of
        /// roster items.
        /// </summary>
        /// <returns>An enumerator that iterates through the collection of
        /// roster items.</returns>
        IEnumerator<IRosterItem> GetEnumerator();
    }
}