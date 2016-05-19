using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharp.Xmpp.Extensions
{
    /// <summary>
    /// Result of joining a multi-user chat room
    /// </summary>
    public class JoinRoomResult
    {
        /// <summary>
        /// Affiliation with the joined room
        /// </summary>
        public RoomAffiliation Affiliation { get; set; }

        /// <summary>
        /// Role within the joined room
        /// </summary>
        public RoomRole Role { get; set; }
    }
}