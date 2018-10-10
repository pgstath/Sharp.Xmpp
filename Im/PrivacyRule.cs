namespace XMPPEngineer.Im
{
    /// <summary>
    /// Represents a generic privacy rule.
    /// </summary>
    public class PrivacyRule
    {
        /// <summary>
        /// Determines whether entities affected by this rule should be blocked
        /// or allowed.
        /// </summary>
        public bool Allow
        {
            get;
            private set;
        }

        /// <summary>
        /// The order of the privacy rule. Privacy rules are processed in ascending
        /// order determined by this value.
        /// </summary>
        public uint Order
        {
            get;
            set;
        }

        /// <summary>
        /// Specifies which kinds of stanzas are to be blocked (a value of 0 denotes
        /// blocking of all stanzas).
        /// </summary>
        public PrivacyGranularity Granularity
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the PrivacyRule class.
        /// </summary>
        /// <param name="allow">True to allow entities affected by this rule; Otherwise
        /// false.</param>
        /// <param name="order">The order of the privacy rule.</param>
        /// <param name="granularity">Specifies which kinds of stanzas should be
        /// blocked.</param>
        public PrivacyRule(bool allow, uint order, PrivacyGranularity granularity = 0)
        {
            Allow = allow;
            Order = order;
            Granularity = granularity;
        }
    }
}