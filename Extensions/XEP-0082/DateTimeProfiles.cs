using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharp.Xmpp.Extensions
{
    /// <summary>
    /// Extension methods to converting date time objects to XEP-0082 compliant strings
    /// </summary>
    public static class DateTimeProfiles
    {
        /// <summary>
        /// Convert a DateTime to an XEP-0082 compliant date string
        /// </summary>
        public static string ToXmppDateString(this DateTime dt)
        {
            if (dt.Kind == DateTimeKind.Unspecified)
            {
                throw new ArgumentException("DateTime Kind cannot be unspecified");
            }

            if (dt.Kind == DateTimeKind.Local)
            {
                dt = dt.ToUniversalTime();
            }

            return dt.ToString("yyyy-MM-dd");
        }

        /// <summary>
        /// Convert a DateTime to an XEP-0082 compliant time string
        /// </summary>
        public static string ToXmppTimeString(this DateTime dt)
        {
            if (dt.Kind == DateTimeKind.Unspecified)
            {
                throw new ArgumentException("DateTime Kind cannot be unspecified");
            }

            if (dt.Kind == DateTimeKind.Local)
            {
                dt = dt.ToUniversalTime();
            }

            return dt.ToString("HH:mm:ss.fffZ");
        }

        /// <summary>
        /// Convert a DateTime to an XEP-0082 compliant date time string
        /// </summary>
        public static string ToXmppDateTimeString(this DateTime dt)
        {
            if (dt.Kind == DateTimeKind.Unspecified)
            {
                throw new ArgumentException("DateTime Kind cannot be unspecified");
            }

            if (dt.Kind == DateTimeKind.Local)
            {
                dt = dt.ToUniversalTime();
            }

            return dt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        }

        /// <summary>
        /// Convert a date time to an XEP-0082 compliant date string
        /// </summary>
        public static string ToXmppDateString(this DateTimeOffset dt)
        {
            return dt.ToUniversalTime().ToString("yyyy-MM-dd");
        }

        /// <summary>
        /// Convert a date time to an XEP-0082 compliant time string
        /// </summary>
        public static string ToXmppTimeString(this DateTimeOffset dt)
        {
            return dt.ToUniversalTime().ToString("HH:mm:ss.fff");
        }

        /// <summary>
        /// Convert a date time to an XEP-0082 compliant date time string
        /// </summary>
        public static string ToXmppDateTimeString(this DateTimeOffset dt)
        {
            return dt.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        }

        /// <summary>
        /// Create a DateTimeOffset from an XEP-0082 string
        /// </summary>
        public static DateTimeOffset FromXmppString(string dt)
        {
            dt.ThrowIfNullOrEmpty("dt");

            return DateTimeOffset.Parse(dt, null, DateTimeStyles.AdjustToUniversal | DateTimeStyles.RoundtripKind);
        }
    }
}