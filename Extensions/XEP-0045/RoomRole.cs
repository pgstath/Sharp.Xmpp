using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharp.Xmpp.Extensions
{
    /// <summary>
    /// A user's role within a multi-user chat room.
    ///
    /// Roles are temporary in that they do not necessarily persist across a user's visits to the room and MAY change during the course of an occupant's visit to the room.
    /// </summary>
    public enum RoomRole
    {
        /// <summary>
        /// No role
        /// </summary>
        None,

        /// <summary>
        /// A participant has fewer privileges than a moderator, although he or she always has the right to speak.
        /// </summary>
        Participant,

        /// <summary>
        /// A moderator is the most powerful role within the context of the room, and can to some extent manage other occupants' roles in the room.
        /// </summary>
        Moderator,

        /// <summary>
        ///  A visitor is a more restricted role within the context of a moderated room, since visitors are not allowed to send messages to all occupants (depending on room configuration, it is even possible that visitors' presence will not be broadcasted to the room).
        /// </summary>
        Visitor
    }
}