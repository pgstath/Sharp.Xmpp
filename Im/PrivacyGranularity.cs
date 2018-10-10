using System;

namespace XMPPEngineer.Im
{
    /// <summary>
    /// Defines the possible values for specifying more granular control over
    /// which kinds of stanzas are to be blocked.
    /// </summary>
    [Flags]
    public enum PrivacyGranularity
    {
        /// <summary>
        /// Blocks incoming message stanzas.
        /// </summary>
        Message = 0x01,

        /// <summary>
        /// Blocks incoming IQ stanzas.
        /// </summary>
        Iq = 0x02,

        /// <summary>
        /// Blocks incoming presence notifications.
        /// </summary>
        PresenceIn = 0x04,

        /// <summary>
        /// Blocks outgoing presence notifications.
        /// </summary>
        PresenceOut = 0x08
    }
}