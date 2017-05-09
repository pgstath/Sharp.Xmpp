using System;

namespace XMPPEngineer.Extensions
{
    /// <summary>
    /// Represents a group presence error event in a group chat. Ref XEP-0045
    /// </summary>
    public class GroupErrorEventArgs : EventArgs
    {
        /// <summary>The full error object.</summary>
        internal MucError Data { get; private set; }

        /// <summary>The error condition.</summary>
        public ErrorCondition ErrorCondition { get { return Data.ErrorCondition; } }

        /// <summary>The type of error.</summary>
        public ErrorType ErrorType { get { return Data.ErrorType; } }

        /// <summary>Jid of the chat room.</summary>
        public Jid By { get { return Data.By; } }

        /// <summary>Your group chat jid.</summary>
        public Jid From { get { return Data.From; } }

        /// <summary>Your public jid.</summary>
        public Jid To { get { return Data.To; } }

        /// <summary>
        /// Constructs a GroupErrorEventArgs.
        /// </summary>
        /// <param name="data">Group Chat Error.</param>
        internal GroupErrorEventArgs(MucError data)
        {
            data.ThrowIfNull("invite");
            Data = data;
        }
    }
}
