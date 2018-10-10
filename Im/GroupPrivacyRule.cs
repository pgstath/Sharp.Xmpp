using System;

namespace XMPPEngineer.Im
{
    /// <summary>
    /// Represents a privacy rule pertaining to a group.
    /// </summary>
    public class GroupPrivacyRule : PrivacyRule
    {
        /// <summary>
        /// The name of the group the privacy rule applies to.
        /// </summary>
        public string Group
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the PrivacyRule class.
        /// </summary>
        /// <param name="group">The group the privacy rule applies to.</param>
        /// <param name="allow">True to allow entities affected by this rule; Otherwise
        /// false.</param>
        /// <param name="order">The order of the privacy rule.</param>
        /// <param name="granularity">Specifies which kinds of stanzas should be
        /// allowed or blocked, respectively.</param>
        /// <exception cref="ArgumentNullException">The group parameter is null.</exception>
        public GroupPrivacyRule(string group, bool allow, uint order,
            PrivacyGranularity granularity = 0)
            : base(allow, order, granularity)
        {
            group.ThrowIfNull("group");
            Group = group;
        }
    }
}