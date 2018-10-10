using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace XMPPEngineer.Im
{
    /// <summary>
    /// Provides status information for a contact.
    /// </summary>
    [Serializable]
    public class Status
    {
        /// <summary>
        /// The contact's availability.
        /// </summary>
        public Availability Availability
        {
            get;
            private set;
        }

        /// <summary>
        /// A user-provided description of his or her availability status.
        /// </summary>
        /// <remarks>If the Status instance contains multiple messages, this
        /// field returns the value of the first message in the 'Messages'
        /// dictionary.</remarks>
        public string Message
        {
            get
            {
                return Messages.Values.FirstOrDefault();
            }
        }

        /// <summary>
        /// A dictionary of user-provided descriptions of his or her availability
        /// status.
        /// </summary>
        /// <remarks>The keys of the dictionary are ISO 2 letter language
        /// Codes denoting the language of the respective value.</remarks>
        public Dictionary<string, string> Messages
        {
            get;
            private set;
        }

        /// <summary>
        /// The priority level of the resource.
        /// </summary>
        /// <remarks>The priority value serves as a hint for the server
        /// in stanza routing within instant messaging and presence
        /// applications.</remarks>
        public sbyte Priority
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the Status class.
        /// </summary>
        /// <param name="availability">The availability state.</param>
        /// <param name="message">An optional message providing a detailed
        /// description of the availability state.</param>
        /// <param name="priority">Provides a hint for stanza routing.</param>
        /// <param name="language">The language of the description of the
        /// availability state.</param>
        public Status(Availability availability, string message = null,
            sbyte priority = 0, CultureInfo language = null)
        {
            Availability = availability;
            Priority = priority;
            Messages = new Dictionary<string, string>();
            if (language == null)
                language = CultureInfo.CurrentCulture;
            if (message != null)
                Messages.Add(language.Name, message);
        }

        /// <summary>
        /// Initializes a new instance of the Status class.
        /// </summary>
        /// <param name="availability">The availability state.</param>
        /// <param name="messages">A dictionary of messages providing detailed
        /// descriptions of the availability state. The dictionary keys denote
        /// the languages of the messages and must be valid ISO 2 letter language
        /// codes.</param>
        /// <param name="priority">Provides a hint for stanza routing.</param>
        public Status(Availability availability, Dictionary<string, string> messages,
            sbyte priority = 0)
        {
            Availability = availability;
            Priority = priority;
            Messages = new Dictionary<string, string>();
            if (messages != null)
            {
                foreach (KeyValuePair<string, string> pair in messages)
                    Messages.Add(pair.Key, pair.Value);
            }
        }
    }
}