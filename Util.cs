using XMPPEngineer.Core;
using System;
using System.Xml;

namespace XMPPEngineer
{
    /// <summary>
    /// Contains utility and extension methods.
    /// </summary>
    internal static class Util
    {
        /// <summary>
        /// Creates an exception from the specified Iq stanza.
        /// </summary>
        /// <param name="errorIq">The Iq stanza to create the exception from. The
        /// stanza must be of type IqType.Error.</param>
        /// <param name="message">A description of the error. The content of message
        /// is intended to be understood by humans.</param>
        /// <returns>An exception of type XmppErrorException if an XmppError instance
        /// could be created from the specified Iq stanza, or an exception of type
        /// XmppException denoting an unrecoverable error.</returns>
        /// <exception cref="ArgumentNullException">The errorIq parameter is null.</exception>
        /// <exception cref="ArgumentException">The errorIq parameter is not
        /// of type IqType.Error.</exception>
        internal static Exception ExceptionFromError(Iq errorIq, string message = null)
        {
            errorIq.ThrowIfNull("errorIq");
            if (errorIq.Type != IqType.Error)
            {
                throw new ArgumentException("The specified Iq stanza is not of " +
                    "type 'error'.");
            }
            return ExceptionFromError(errorIq.Data["error"], message);
        }

        /// <summary>
        /// Creates an exception from the specified XML error element.
        /// </summary>
        /// <param name="error">An XML XMPP error element.</param>
        /// <param name="message">A description of the error. The content of message
        /// is intended to be understood by humans.</param>
        /// <returns>An exception of type XmppErrorException if an XmppError instance
        /// could be created from the specified Iq stanza, or an exception of type
        /// XmppException denoting an unrecoverable error.</returns>
        internal static Exception ExceptionFromError(XmlElement error,
            string message = null)
        {
            try
            {
                return new XmppErrorException(new XmppError(error), message);
            }
            catch
            {
                if (error == null)
                    return new XmppException("Unspecified error.");
                return new XmppException("Invalid XML error-stanza: " +
                    error.ToXmlString());
            }
        }

        /// <summary>
        /// Raises the event. Ensures the event is only raised, if it is not null.
        /// </summary>
        /// <typeparam name="T">Extends System.EventHandler class</typeparam>
        /// <param name="event">Extends System.EventHandler class</param>
        /// <param name="sender">The sender of the event</param>
        /// <param name="args">The event arguments associated with this event</param>
        internal static void Raise<T>(this EventHandler<T> @event, object sender, T args)
            where T : EventArgs
        {
            EventHandler<T> handler = @event;
            if (handler != null)
                handler(sender, args);
        }

        /// <summary>
        /// Throws an ArgumentNullException if the given data item is null.
        /// </summary>
        /// <param name="data">The item to check for nullity.</param>
        /// <param name="name">The name to use when throwing an
        /// exception, if necessary.</param>
        /// <remarks>Courtesy of Jon Skeet.</remarks>
        internal static void ThrowIfNull<T>(this T data, string name)
            where T : class
        {
            if (data == null)
                throw new ArgumentNullException(name);
        }

        /// <summary>
        /// Throws an ArgumentNullException if the given data item is null.
        /// </summary>
        /// <param name="data">The item to check for nullity.</param>
        /// <remarks>Courtesy of Jon Skeet.</remarks>
        internal static void ThrowIfNull<T>(this T data)
            where T : class
        {
            if (data == null)
                throw new ArgumentNullException();
        }

        /// <summary>
        /// Throws an ArgumentOufOfRangeExcption if the given value is not within
        /// the specified range.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="from">The minimum value (including).</param>
        /// <param name="to">The maximum value (including).</param>
        internal static void ThrowIfOutOfRange(this int value, int from, int to)
        {
            if (value < from || value > to)
                throw new ArgumentOutOfRangeException();
        }

        /// <summary>
        /// Throws an ArgumentOufOfRangeExcption if the given value is not within
        /// the specified range.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="name">The name to use when throwing an
        /// exception, if necessary.</param>
        /// <param name="from">The minimum value (including).</param>
        /// <param name="to">The maximum value (including).</param>
        internal static void ThrowIfOutOfRange(this int value, string name,
            int from, int to)
        {
            if (value < from || value > to)
                throw new ArgumentOutOfRangeException(name);
        }

        /// <summary>
        /// Throws an ArgumentOufOfRangeExcption if the given value is not within
        /// the specified range.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="from">The minimum value (including).</param>
        /// <param name="to">The maximum value (including).</param>
        internal static void ThrowIfOutOfRange(this long value, long from, long to)
        {
            if (value < from || value > to)
                throw new ArgumentOutOfRangeException();
        }

        /// <summary>
        /// Throws an ArgumentOufOfRangeExcption if the given value is not within
        /// the specified range.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="name">The name to use when throwing an
        /// exception, if necessary.</param>
        /// <param name="from">The minimum value (including).</param>
        /// <param name="to">The maximum value (including).</param>
        internal static void ThrowIfOutOfRange(this long value, string name,
            long from, long to)
        {
            if (value < from || value > to)
                throw new ArgumentOutOfRangeException(name);
        }

        /// <summary>
        /// Throws an ArgumentNullException if the given string is null and
        /// throws an ArgumentException if the given string is empty.
        /// </summary>
        /// <param name="s">The string to check for nullity and emptiness.</param>
        internal static void ThrowIfNullOrEmpty(this string s)
        {
            if (s == null)
                throw new ArgumentNullException();
            if (s == String.Empty)
                throw new ArgumentException();
        }

        /// <summary>
        /// Throws an ArgumentNullException if the given string is null and
        /// throws an ArgumentException if the given string is empty.
        /// </summary>
        /// <param name="s">The string to check for nullity and emptiness.</param>
        /// <param name="name">The name to use when throwing an
        /// exception, if necessary.</param>
        internal static void ThrowIfNullOrEmpty(this string s, string name)
        {
            if (s == null)
                throw new ArgumentNullException(name);
            if (s == String.Empty)
                throw new ArgumentException(name + " must not be empty.");
        }

        /// <summary>
        /// Capitalizes the first character of the string.
        /// </summary>
        /// <param name="s">The string to capitalize.</param>
        /// <returns>A new string with the first character capitalized.</returns>
        internal static string Capitalize(this string s)
        {
            return char.ToUpperInvariant(s[0]) + s.Substring(1);
        }

        /// <summary>
        /// Converts the string representation of the name or numeric value of one
        /// or more enumerated constants to an equivalent enumerated object.
        /// </summary>
        /// <typeparam name="T">An enumeration type.</typeparam>
        /// <param name="value">A string containing the name or value to
        /// convert.</param>
        /// <param name="ignoreCase">true to ignore case; false to regard
        /// case.</param>
        /// <returns>An object of the specified enumeration type whose value is
        /// represented by value.</returns>
        /// <exception cref="ArgumentNullException">The value parameter is
        /// null.</exception>
        /// <exception cref="ArgumentException">The specified type is not an
        /// enumeration type, or value is either an empty string or only contains
        /// white space, or value is a name, but not one of the named
        /// constants.</exception>
        internal static T ParseEnum<T>(string value, bool ignoreCase = true) where T :
            struct, IComparable, IFormattable, IConvertible
        {
            value.ThrowIfNull("value");
            if (!typeof(T).IsEnum)
                throw new ArgumentException("T must be an enumerated type.");
            return (T)Enum.Parse(typeof(T), value, ignoreCase);
        }
    }
}