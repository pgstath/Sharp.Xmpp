using System;

namespace XMPPEngineer.Extensions
{
    /// <summary>
    /// Provides data for the Tune event.
    /// </summary>
    [Serializable]
    public class TuneEventArgs : EventArgs
    {
        /// <summary>
        /// The JID of the XMPP entity that published the tune information.
        /// </summary>
        public Jid Jid
        {
            get;
            private set;
        }

        /// <summary>
        /// Determines whether the XMPP entity stopped play back.
        /// </summary>
        public bool Stop
        {
            get
            {
                return Information == null;
            }
        }

        /// <summary>
        /// Contains information about the music to which a user is listening.
        /// </summary>
        public TuneInformation Information
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the TuneEventArgs class.
        /// </summary>
        /// <param name="jid">The JID of the XMPP entity that published the
        /// tune information.</param>
        /// <param name="information">The tune information to include as part of
        /// the event.</param>
        /// <exception cref="ArgumentNullException">The jid parameter is
        /// null.</exception>
        public TuneEventArgs(Jid jid, TuneInformation information = null)
        {
            jid.ThrowIfNull("jid");
            Jid = jid;
            Information = information;
        }
    }
}