namespace XMPPEngineer.Im
{
    /// <summary>
    /// Represents a privacy rule pertaining to a subscription state.
    /// </summary>
    public class SubscriptionPrivacyRule : PrivacyRule
    {
        /// <summary>
        /// The subscription state the privacy rule applies to.
        /// </summary>
        public SubscriptionState SubscriptionState
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the PrivacyRule class.
        /// </summary>
        /// <param name="state">The subscription state the privacy rule applies
        /// to.</param>
        /// <param name="allow">True to block entities affected by this rule; Otherwise
        /// false.</param>
        /// <param name="order">The order of the privacy rule.</param>
        /// <param name="granularity">Specifies which kinds of stanzas are to be
        /// blocked.</param>
        public SubscriptionPrivacyRule(SubscriptionState state, bool allow, uint order,
            PrivacyGranularity granularity = 0)
            : base(allow, order, granularity)
        {
            SubscriptionState = state;
        }
    }
}