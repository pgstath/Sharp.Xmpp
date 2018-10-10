using System;

namespace XMPPEngineer.Extensions
{
    /// <summary>
    /// Provides data for the ActivityChanged event.
    /// </summary>
    [Serializable]
    public class ActivityChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The JID of the XMPP entity that published the activity information.
        /// </summary>
        public Jid Jid
        {
            get;
            private set;
        }

        /// <summary>
        /// The general activity of the XMPP entity.
        /// </summary>
        public GeneralActivity Activity
        {
            get;
            private set;
        }

        /// <summary>
        /// The specific activity of the XMPP entity.
        /// </summary>
        public SpecificActivity Specific
        {
            get;
            private set;
        }

        /// <summary>
        /// a natural-language description of, or reason for, the activity. This
        /// may be null.
        /// </summary>
        public string Description
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the ActivityChangedEventArgs class.
        /// </summary>
        /// <param name="jid">The JID of the XMPP entity that published the
        /// activity information.</param>
        /// <param name="activity">One of the values from the GeneralActivity
        /// enumeration.</param>
        /// <param name="specific">A value from the SpecificActivity enumeration
        /// best describing the user's activity in more detail.</param>
        /// <param name="description">A natural-language description of, or
        /// reason for, the activity.</param>
        /// <exception cref="ArgumentNullException">The jid parameter is
        /// null.</exception>
        public ActivityChangedEventArgs(Jid jid, GeneralActivity activity,
            SpecificActivity specific = SpecificActivity.Other,
            string description = null)
        {
            jid.ThrowIfNull("jid");
            Jid = jid;
            Activity = activity;
            Specific = specific;
            Description = description;
        }
    }
}