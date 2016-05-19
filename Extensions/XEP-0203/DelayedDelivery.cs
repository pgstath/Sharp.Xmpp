using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Sharp.Xmpp.Extensions
{
    /// <summary>
    /// Helper class for extracting delayed delivery timestamps as specified in XEP-0203
    /// </summary>
    public static class DelayedDelivery
    {
        /// <summary>
        /// Try to get a timestamp from a delay node. If this fails, return the current UTC time.
        /// </summary>
        /// <param name="xml">The xml node that contains a delay node</param>
        public static DateTimeOffset GetDelayedTimestampOrNow(XmlElement xml)
        {
            DateTimeOffset timestamp = DateTimeOffset.UtcNow;

            // Refer to XEP-0203.
            var delay = xml["delay"];

            if (delay != null && delay.NamespaceURI == "urn:xmpp:delay")
            {
                var stampAttribute = delay.GetAttribute("stamp");
                if (stampAttribute != null)
                {
                    timestamp = DateTimeProfiles.FromXmppString(stampAttribute);
                }
            }

            return timestamp;
        }
    }
}