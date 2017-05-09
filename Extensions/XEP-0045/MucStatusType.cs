using System;
namespace XMPPEngineer.Extensions
{
    /// <summary>
    /// Defines an extensible format for status conditions in MUC.
    /// </summary>
    public enum MucStatusType : int
    {
        /// <summary> Real Jid Public </summary>
        RealJidPublic = 100,

        /// <summary> User Affiliation Changed </summary>
        AffiliationChanged = 101,

        /// <summary> Unavailable Shown </summary>
        UnavailableShown = 102,

        /// <summary> Unavailable Not Shown </summary>
        UnavailableNotShown = 103,

        /// <summary> Configuration Changed </summary>
        ConfigurationChanged = 104,

        /// <summary> Self Presence </summary>
        SelfPresence = 110,

        /// <summary> Chat Logging Enabled </summary>
        LoggingEnabled = 170,

        /// <summary> Chat Logging Disabled </summary>
        LoggingDisabled = 171,

        /// <summary> Non Anonymous </summary>
        NonAnonymous = 172,

        /// <summary> Semi Anonymous </summary>
        SemiAnonymous = 173,

        /// <summary> Fully Anonymous </summary>
        FullyAnonymous = 174,

        /// <summary> Room Created </summary>
        RoomCreated = 201,

        /// <summary> Nickname Assigned </summary>
        NicknameAssigned = 210,

        /// <summary> Banned </summary>
        Banned = 301,

        /// <summary> New Nickname </summary>
        NewNickname = 303,

        /// <summary> Kicked </summary>
        Kicked = 307,

        /// <summary> Removed Affiliation </summary>
        RemovedAffiliation = 321,

        /// <summary> Removed Membership </summary>
        RemovedMembership = 322,

        /// <summary> Removed Shutdown </summary>
        RemovedShutdown = 332
    }
}
