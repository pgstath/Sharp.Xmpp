namespace XMPPEngineer.Extensions
{
    /// <summary>
    /// Identifiers used to describe objects in objects in the MUC Namespace.
    /// Really this wants to be layers classes of constants to simulate namespaces with strings,
    /// But so far this only covers what needs to be used in MUC.
    /// </summary>
    internal class MucNs
    {
        private const string NsProtocol = "http://jabber.org/protocol/";

        // Data Namespace

        public const string NsXData = "jabber:x:data";

        // Service Discovery Namespaces

        public const string NsRequestItems = NsProtocol + "disco#items";

        public const string NsRequestInfo = NsProtocol + "disco#info";

        // MUC Namespaces

        public const string NsMain = NsProtocol + "muc";

        public const string NsAdmin = NsMain + "#admin";

        public const string NsOwner = NsMain + "#owner";

        public const string NsUser = NsMain + "#user";

        public const string NsRegister = NsMain + "#register";

        public const string NsRequest = NsMain + "#request";

        public const string NsRoomConfig = NsMain + "#roomconfig";

        public const string NsRoomInfo = NsMain + "#roominfo";

        // Room Features

        /// <summary>
        /// Unsecured Room
        /// A room that anyone is allowed to enter without first providing the correct password; 
        /// antonym: Password-Protected Room.
        /// </summary>
        public const string FeatureProtectionUnsecured = "muc_unsecured";

        /// <summary>
        /// Password-Protected Room
        /// A room that a user cannot enter without first providing the correct password; 
        /// antonym: Unsecured Room.
        /// </summary>
        public const string FeatureProtectionPassword = "muc_passwordprotected";

        /// <summary>
        /// Public Room
        /// A room that can be found by any user through normal means such as searching and service discovery;
        /// antonym: Hidden Room.
        /// </summary>
        public const string FeatureVisiblityPublic = "muc_public";

        /// <summary>
        /// Hidden Room
        /// A room that cannot be found by any user through normal means such as searching and service discovery; 
        /// antonym: Public Room.
        /// </summary>
        public const string FeatureVisiblityHidden = "muc_hidden";

        /// <summary>
        /// Temporary Room
        /// A room that is destroyed if the last occupant exits; 
        /// antonym: Persistent Room.
        /// </summary>
        public const string FeaturePersistTemporary = "muc_temporary";

        /// <summary>
        /// Persistent Room
        /// A room that is not destroyed if the last occupant exits; 
        /// antonym: Temporary Room.
        /// </summary>
        public const string FeaturePersistPersistent = "muc_persistent";

        /// <summary>
        /// Open Room
        /// A room that non-banned entities are allowed to enter without being on the member list; 
        /// antonym: Members-Only Room.
        /// </summary>
        public const string FeaturePrivacyOpen = "muc_open";

        /// <summary>
        /// Members-Only Room
        /// A room that a user cannot enter without being on the member list; 
        /// antonym: Open Room.
        /// </summary>
        public const string FeaturePrivacyMembersOnly = "muc_membersonly";

        /// <summary>
        /// Unmoderated Room
        /// A room in which any occupant is allowed to send messages to all occupants;
        /// antonym: Moderated Room.
        /// </summary>
        public const string FeatureUnmoderated = "muc_unmoderated";

        /// <summary>
        /// Moderated Room
        /// A room in which only those with "voice" are allowed to send messages to all occupants; 
        /// antonym: Unmoderated Room.
        /// </summary>
        public const string FeatureModerated = "muc_moderated";

        /// <summary>
        /// Non-Anonymous Room
        /// A room in which an occupant's full JID is exposed to all other occupants, although the occupant can request any desired room nickname; 
        /// contrast with Semi-Anonymous Room.
        /// </summary>
        public const string FeatureNonAnonymous = "muc_nonanonymous";

        /// <summary>
        /// Semi-Anonymous Room
        /// A room in which an occupant's full JID can be discovered by room admins only; 
        /// contrast with Non-Anonymous Room.
        /// </summary>
        public const string FeatureSemiAnonymous = "muc_semianonymous";

        // Room Info

        /// <summary>
        /// Room Description.
        /// </summary>
        public const string InfoDescription = "muc#roominfo_description";

        /// <summary>
        /// Occupants May Change the Subject.
        /// </summary>
        public const string InfoChangeSubject = "muc#roominfo_changesubject";

        /// <summary>
        /// Contact Addresses.
        /// </summary>
        public const string InfoContactJid = "muc#roominfo_contactjid";

        /// <summary>
        /// Current Discussion Topic.
        /// </summary>
        public const string InfoSubject = "muc#roominfo_subject";

        /// <summary>
        /// The room subject can be modified by participants.
        /// </summary>
        public const string InfoSubjectMod = "muc#roominfo_subjectmod";

        /// <summary>
        /// Number of occupants.
        /// </summary>
        public const string InfoOccupants = "muc#roominfo_occupants";

        /// <summary>
        /// Associated LDAP Group.
        /// </summary>
        public const string InfoLdapGroup = "muc#roominfo_ldapgroup";

        /// <summary>
        /// Language of discussion.
        /// </summary>
        public const string InfoLanguage = "muc#roominfo_lang";

        /// <summary>
        /// URL for discussion logs.
        /// </summary>
        public const string InfoLogs = "muc#roominfo_logs";

        /// <summary>
        /// Associated pubsub node.
        /// </summary>
        public const string InfoPubSub = "muc#roominfo_pubsub";

        /// <summary>
        /// Date the room was created.
        /// </summary>
        public const string InfoCreationDate = "x-muc#roominfo_creationdate";

        /// <summary>
        /// Maximum Number of History Messages Returned by Room.
        /// </summary>
        public const string MaxHistoryFetch = "muc#maxhistoryfetch";

        // Room Config

        /// <summary>
        /// Natural-Language Room Name.
        /// </summary>
        public const string ConfigRoomName = "muc#roomconfig_roomname";

        /// <summary>
        /// Short Description of Room
        /// </summary>
        public const string ConfigRoomDescription = "muc#roomconfig_roomdesc";

        /// <summary>
        /// 'Natural Language for Room Discussions.
        /// </summary>
        public const string ConfigLanguage = "muc#roomconfig_lang";

        /// <summary>
        /// Enable Public Logging?
        /// </summary>
        public const string ConfigEnableLogging = "muc#roomconfig_enablelogging";

        /// <summary>
        /// Allow Occupants to Change Subject?
        /// </summary>
        public const string ConfigChangeSubject = "muc#roomconfig_changesubject";

        /// <summary>
        /// Allow Occupants to Invite Others?
        /// </summary>
        public const string ConfigAllowInvites = "muc#roomconfig_allowinvites";

        /// <summary>
        /// Who Can Send Private Messages?
        /// </summary>
        public const string ConfigAllowPrivateMessages = "muc#roomconfig_allowpm";

        /// <summary>
        /// Maximum Number of Occupants.
        /// </summary>
        public const string ConfigMaxUsers = "muc#roomconfig_maxusers";

        /// <summary>
        /// Roles for which Presence is Broadcasted.
        /// </summary>
        public const string ConfigPresenceBroadcast = "muc#roomconfig_presencebroadcast";

        /// <summary>
        /// Roles and Affiliations that May Retrieve Member List.
        /// </summary>
        public const string ConfigGetMemberList = "muc#roomconfig_getmemberlist";

        /// <summary>
        /// Make Room Publicly Searchable?
        /// </summary>
        public const string ConfigPublicRoom = "muc#roomconfig_publicroom";

        /// <summary>
        /// Make Room Persistent?
        /// </summary>
        public const string ConfigPersistentRoom = "muc#roomconfig_persistentroom";

        /// <summary>
        /// Make Room Moderated?
        /// </summary>
        public const string ConfigModeratedRoom = "muc#roomconfig_moderatedroom";

        /// <summary>
        /// Make Room Members-Only?
        /// </summary>
        public const string ConfigMembersOnly = "muc#roomconfig_membersonly";

        /// <summary>
        /// Password Required to Enter?
        /// </summary>
        public const string ConfigPasswordProtectedRoom = "muc#roomconfig_passwordprotectedroom";

        /// <summary>
        /// Room Password.
        /// </summary>
        public const string ConfigRoomSecret = "muc#roomconfig_roomsecret";

        /// <summary>
        /// Who May Discover Real JIDs?
        /// </summary>
        public const string ConfigWhoIs = "muc#roomconfig_whois";

        /// <summary>
        /// Room Admins.
        /// </summary>
        public const string ConfigRoomAdmins = "muc#roomconfig_roomadmins";

        /// <summary>
        /// Room Owners.
        /// </summary>
        public const string ConfigRoomOwners = "muc#roomconfig_roomowners";

        // Registration Form

        /// <summary>
        /// First Name used for registration.
        /// </summary>
        public const string RegisterFirstName = "muc#register_first";

        /// <summary>
        /// Last Name used for registration.
        /// </summary>
        public const string RegisterLastName = "muc#register_last";

        /// <summary>
        /// Desired Nickname used for registration.
        /// </summary>
        public const string RegisterNickname = "muc#register_roomnick";

        /// <summary>
        /// A Web Page...
        /// </summary>
        public const string RegisterUrl = "muc#register_url";

        /// <summary>
        /// Email Address used for registration.
        /// </summary>
        public const string RegisterEmail = "muc#register_email";

        /// <summary>
        /// FAQ Entry.
        /// </summary>
        public const string RegisterFAQ = "muc#register_faqentry";

        /// <summary>
        /// Allow this person to register with the room?
        /// </summary>
        public const string RegisterAllow = "muc#register_allow";

        // Request Form

        /// <summary>
        /// Requested role.
        /// </summary>
        public const string Role = "muc#role";

        /// <summary>
        /// User ID.
        /// </summary>
        public const string Jid = "muc#jid";

        /// <summary>
        /// Room Nickname.
        /// </summary>
        public const string RoomNick = "muc#roomnick";

        /// <summary>
        /// Whether to grant voice.
        /// </summary>
        public const string RequestAllow = "muc#request_allow";
    }
}
