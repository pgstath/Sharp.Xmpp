using System;
using System.Collections.Generic;

namespace XMPPEngineer.Extensions
{
    /// <summary>
    /// Represents a presence change event in a group chat. Ref XEP-0045
    /// </summary>
    public class GroupPresenceEventArgs : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public Occupant Person { get; set; }

        public Jid Group { get; }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<MucStatusType> Statuses { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="person"></param>
        /// <param name="group"></param>
        /// <param name="statuses"></param>
        public GroupPresenceEventArgs(Occupant person, Jid group, IEnumerable<MucStatusType> statuses) : base()
        {
            Person = person;
            Group = group;
            Statuses = statuses;
        }
    }
}
