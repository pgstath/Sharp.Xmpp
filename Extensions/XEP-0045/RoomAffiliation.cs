using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharp.Xmpp.Extensions
{
    /// <summary>
    /// A users affiliation with a multi-user chat room.
    ///
    /// These affiliations are long-lived in that they persist across a user's visits to the room and are not affected by happenings in the room.
    /// </summary>
    public enum RoomAffiliation
    {
        /// <summary>
        /// No affiliation
        /// </summary>
        None,

        /// <summary>
        /// Can appoint admins and edit a room's configuration.
        /// </summary>
        Owner,

        /// <summary>
        /// Can appoint moderators and members within a room.
        /// </summary>
        Admin,

        /// <summary>
        /// The member affiliation provides a way for a room owner or admin to specify a "whitelist" of users who are allowed to enter a members-only room.
        /// When a member enters a members-only room, his or her affiliation does not change, no matter what his or her role is.
        /// The member affiliation also provides a way for users to register with an open room and thus be lastingly associated with that room in some way (one result might be that the service could reserve the user's nickname in the room).
        /// </summary>
        Member,

        /// <summary>
        /// An outcast is a user who has been banned from a room and who is not allowed to enter the room.
        /// </summary>
        Outcast
    }
}