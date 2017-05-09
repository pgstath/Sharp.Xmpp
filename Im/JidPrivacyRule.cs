using System;

namespace XMPPEngineer.Im
{
    /// <summary>
    /// Represents a privacy rule pertaining to a JID.
    /// </summary>
    public class JidPrivacyRule : PrivacyRule
    {
        /// <summary>
        /// The JID the privacy rule applies to.
        /// </summary>
        public Jid Jid
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the PrivacyRule class.
        /// </summary>
        /// <param name="jid">The JID the privacy rule applies to.</param>
        /// <param name="allow">True to allow entities affected by this rule; Otherwise
        /// false.</param>
        /// <param name="order">The order of the privacy rule.</param>
        /// <param name="granularity">Specifies which kinds of stanzas should be
        /// allowed or blocked, respectively.</param>
        /// <exception cref="ArgumentNullException">The jid parameter is null.</exception>
        public JidPrivacyRule(Jid jid, bool allow, uint order,
            PrivacyGranularity granularity = 0)
            : base(allow, order, granularity)
        {
            jid.ThrowIfNull("jid");
            Jid = jid;
        }
    }
}