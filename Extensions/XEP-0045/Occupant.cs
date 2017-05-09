using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMPPEngineer.Extensions
{
    /// <summary>
    /// Represents an participant in a group chat.
    /// </summary>
    public class Occupant
    {
        /// <summary>
        /// The real identifier of the participant.
        /// </summary>
        public Jid RealJid { get; set; }

        /// <summary>
        /// The real identifier of the participant.
        /// </summary>
        public Jid GroupJid { get; set; }

        /// <summary>
        /// The participants nickname.
        /// </summary>
        public string Nickname
        {
            get
            {
                return GroupJid == null ? null : GroupJid.Resource;
            }
        }

        /// <summary>
        /// Member level of the participant.
        /// </summary>
        public Role Role { get; set; }

        /// <summary>
        /// Permission level of the participant.
        /// </summary>
        public Affiliation Affiliation { get; set; }

        /// <summary>
        /// Constructs a Occupant object.
        /// </summary>
        /// <param name="groupJid"></param>
        /// <param name="affiliation"></param>
        /// <param name="role"></param>
        public Occupant(Jid groupJid, Affiliation affiliation, Role role)
        {
            GroupJid = groupJid;
            Affiliation = affiliation;
            Role = role;
        }

        /// <summary>
        /// Constructs a Occupant object.
        /// </summary>
        /// <param name="groupJid"></param>
        /// <param name="affiliation"></param>
        /// <param name="role"></param>
        public Occupant(Jid groupJid, string affiliation, string role)
        {
            GroupJid = groupJid;
            Affiliation = (Affiliation) Enum.Parse(typeof(Affiliation), affiliation, true);
            Role = (Role)Enum.Parse(typeof(Role), role, true);
        }

        /// <summary>
        /// Constructs an empty Occupant object.
        /// </summary>
        public Occupant()
        {
            GroupJid = null;
            RealJid = null;
            Role = Role.None;
            Affiliation = Affiliation.None;
        }
    }
}
