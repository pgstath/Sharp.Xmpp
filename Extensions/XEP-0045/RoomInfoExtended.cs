using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMPPEngineer.Extensions.Dataforms;

namespace XMPPEngineer.Extensions
{
    /// <summary>
    /// Room information provided upon inspection.
    /// </summary>
    public class RoomInfoExtended : RoomInfoBasic
    {
        private string description;
        private string subject;
        private string ldapGroup;
        private string logUrl;
        private string pubSubNode;
        private int occupantsCount;
        private int maxHistoryFetch;
        private bool canChangeSubject;
        private DateTime? creationDate;
        private CultureInfo language;

        private IList<Jid> contactAddresses;
        private ISet<Jid> occupants;

        private RoomVisibility visibility;
        private RoomPersistence persistence;
        private RoomProtection protection;
        private RoomPrivacy privacy;
        private RoomModeration moderation;
        private RoomAnonymity anonymity;

        internal RoomInfoExtended(Jid jid, string name, string description, string subject)
            : base(jid, name)
        {
            Visibility = RoomVisibility.Public;
            Persistence = RoomPersistence.Temporary;
            Protection = RoomProtection.Unsecured;
            Privacy = RoomPrivacy.Open;
            Moderation = RoomModeration.Unmoderated;
            Anonymity = RoomAnonymity.NonAnonymous;
            Description = string.Empty;
            Subject = string.Empty;
            LDAPGroup = string.Empty;
            LogUrl = string.Empty;
            pubSubNode = string.Empty;
            canChangeSubject = false;
            MaxHistoryFetch = 0;
            NumberOfOccupants = 0;
            CreationDate = DateTime.UtcNow;
            contactAddresses = new List<Jid>();
            occupants = new HashSet<Jid>();
        }

        internal RoomInfoExtended(Jid jid, string name, IEnumerable<DataField> features, IEnumerable<DataField> fields)
             : base(jid, name)
        {
            Visibility = RoomVisibility.Undefined;
            Persistence = RoomPersistence.Undefined;
            Protection = RoomProtection.Undefined;
            Privacy = RoomPrivacy.Undefined;
            Moderation = RoomModeration.Undefined;
            Anonymity = RoomAnonymity.Undefined;
            Description = string.Empty;
            Subject = string.Empty;
            LDAPGroup = string.Empty;
            LogUrl = string.Empty;
            pubSubNode = string.Empty;
            canChangeSubject = false;
            MaxHistoryFetch = 0;
            NumberOfOccupants = 0;
            CreationDate = null;
            contactAddresses = new List<Jid>();
            occupants = new HashSet<Jid>();

            IntialiseRoomFeatures(features);
            IntialiseRoomSettings(fields);
        }

        /// <summary>
        /// The visibility of the room.
        /// </summary>
        public RoomVisibility Visibility
        {
            get { return visibility; }
            protected set { visibility = value; }
        }

        /// <summary>
        /// The persistence level of the room.
        /// </summary>
        public RoomPersistence Persistence
        {
            get { return persistence; }
            protected set { persistence = value; }
        }

        /// <summary>
        /// The protection level of the room.
        /// </summary>
        public RoomProtection Protection
        {
            get { return protection; }
            protected set { protection = value; }
        }

        /// <summary>
        /// The privacy level of the room.
        /// </summary>
        public RoomPrivacy Privacy
        {
            get { return privacy; }
            protected set { privacy = value; }
        }

        /// <summary>
        /// The moderation level of the room.
        /// </summary>
        public RoomModeration Moderation
        {
            get { return moderation; }
            protected set { moderation = value; }
        }

        /// <summary>
        /// The anonymity level of the room.
        /// </summary>
        public RoomAnonymity Anonymity
        {
            get { return anonymity; }
            protected set { anonymity = value; }
        }

        /// <summary>
        /// The description of the room.
        /// </summary>
        public string Description
        {
            get { return description; }
            protected set { description = value; }
        }

        /// <summary>
        /// The subject of the room.
        /// </summary>
        public string Subject
        {
            get { return subject; }
            protected set { subject = value; }
        }

        /// <summary>
        /// The number of occupants in the room.
        /// </summary>
        public int NumberOfOccupants
        {
            get { return occupantsCount; }
            protected set { occupantsCount = value; }
        }

        /// <summary>
        /// Datetime the room was created.
        /// </summary>
        public DateTime? CreationDate
        {
            get { return creationDate; }
            protected set { creationDate = value; }
        }

        /// <summary>
        /// The owner or owners of the room.
        /// </summary>
        public IEnumerable<Jid> ContactAddresses
        {
            get { return contactAddresses; }
        }

        /// <summary>
        /// The participants in the room.
        /// </summary>
        public IEnumerable<Jid> Occupants
        {
            get { return occupants; }
        }

        /// <summary>
        /// An associated LDAP group that defines room membership.
        /// </summary>
        public string LDAPGroup
        {
            get { return ldapGroup; }
            protected set { ldapGroup = value; }
        }

        /// <summary>
        /// The language of the room.
        /// </summary>
        public CultureInfo Language
        {
            get { return language; }
            protected set { language = value; }
        }

        /// <summary>
        /// The language of the room.
        /// </summary>
        public string LogUrl
        {
            get { return logUrl; }
            protected set { logUrl = value; }
        }

        /// <summary>
        /// The maximum number historic of messages a room will display.
        /// </summary>
        public int MaxHistoryFetch
        {
            get { return maxHistoryFetch; }
            protected set { maxHistoryFetch = value; }
        }

        private void InvalidateOccupantsCount(bool shouldInvalidate = true)
        {
            if (shouldInvalidate)
            {
                NumberOfOccupants = Occupants.Count();
            }
        }

        /// <summary>
        /// Initialises the room settings using the provided features.
        /// </summary>
        /// <param name="features">Room features</param>
        private void IntialiseRoomFeatures(IEnumerable<DataField> features)
        {
            foreach (DataField f in features)
            {
                switch (f.Name)
                {
                    case MucNs.FeatureProtectionUnsecured:
                        Protection = RoomProtection.Unsecured;
                        break;
                    case MucNs.FeatureProtectionPassword:
                        Protection = RoomProtection.PasswordProtected;
                        break;
                    case MucNs.FeatureVisiblityPublic:
                        Visibility = RoomVisibility.Public;
                        break;
                    case MucNs.FeatureVisiblityHidden:
                        Visibility = RoomVisibility.Hidden;
                        break;
                    case MucNs.FeaturePersistTemporary:
                        Persistence = RoomPersistence.Temporary;
                        break;
                    case MucNs.FeaturePersistPersistent:
                        Persistence = RoomPersistence.Persistent;
                        break;
                    case MucNs.FeaturePrivacyOpen:
                        Privacy = RoomPrivacy.Open;
                        break;
                    case MucNs.FeaturePrivacyMembersOnly:
                        Privacy = RoomPrivacy.MembersOnly;
                        break;
                    case MucNs.FeatureUnmoderated:
                        Moderation = RoomModeration.Unmoderated;
                        break;
                    case MucNs.FeatureModerated:
                        Moderation = RoomModeration.Moderated;
                        break;
                    case MucNs.FeatureNonAnonymous:
                        Anonymity = RoomAnonymity.NonAnonymous;
                        break;
                    case MucNs.FeatureSemiAnonymous:
                        Anonymity = RoomAnonymity.SemiAnonymous;
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Initialises the room settings using the provided fields.
        /// </summary>
        /// <param name="fields">Room fields</param>
        private void IntialiseRoomSettings(IEnumerable<DataField> fields)
        {
            foreach (DataField f in fields)
                switch (f.Name)
                {
                    case MucNs.InfoDescription:
                        Description = f.Values.FirstOrDefault();
                        break;
                    case MucNs.InfoChangeSubject:
                        canChangeSubject = ConvertToBoolean(f.Values.FirstOrDefault());
                        break;
                    case MucNs.InfoContactJid:
                        contactAddresses.Add(new Jid(f.Values.FirstOrDefault()));
                        break;
                    case MucNs.InfoCreationDate:
                        CreationDate = ConvertToDateTime(f.Values.FirstOrDefault());
                        break;
                    case MucNs.InfoSubject:
                        Subject = f.Values.FirstOrDefault();
                        break;
                    case MucNs.InfoSubjectMod:
                        canChangeSubject = ConvertToBoolean(f.Values.FirstOrDefault());
                        break;
                    case MucNs.InfoOccupants:
                        NumberOfOccupants = ConvertToInteger(f.Values.FirstOrDefault());
                        break;
                    case MucNs.InfoLdapGroup:
                        LDAPGroup = f.Values.FirstOrDefault();
                        break;
                    case MucNs.InfoLanguage:
                        Language = ConvertToCultureInfo(f.Values.FirstOrDefault());
                        break;
                    case MucNs.InfoLogs:
                        LogUrl = f.Values.FirstOrDefault();
                        break;
                    case MucNs.InfoPubSub:
                        pubSubNode = f.Values.FirstOrDefault();
                        break;
                    case MucNs.MaxHistoryFetch:
                        maxHistoryFetch = ConvertToInteger(f.Values.FirstOrDefault());
                        break;
                    default:
                        break;
                }

            InvalidateOccupantsCount(NumberOfOccupants != Occupants.Count());
        }

        private bool ConvertToBoolean(string value)
        {
            bool tmp;

            if (bool.TryParse(value, out tmp))
            {
                return tmp;
            }

            return false;
        }

        private CultureInfo ConvertToCultureInfo(string value)
        {
            CultureInfo tmp = null;

            try
            {
                tmp = CultureInfo.GetCultureInfo(value);
            }
            catch (CultureNotFoundException)
            {
                // Suppress missing cultures
            }

            return tmp;
        }

        private DateTime? ConvertToDateTime(string value)
        {
            DateTime tmp;

            if (DateTime.TryParse(value, out tmp))
            {
                return tmp;
            }

            return null;
        }

        private int ConvertToInteger(string value)
        {
            int tmp;

            if (int.TryParse(value, out tmp))
            {
                return tmp;
            }

            return 0;
        }
    }
}
