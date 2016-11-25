using Sharp.Xmpp.Extensions;
using Sharp.Xmpp.Im;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Security;

namespace Sharp.Xmpp.Client
{

    /// <summary>
    /// Implements an XMPP client providing basic instant messaging (IM) and
    /// presence functionality as well as various XMPP extension functionality.
    /// </summary>
    /// <remarks>
    /// This provides most of the functionality exposed by the XmppIm class but
    /// simplifies some of the more complicated aspects such as privacy lists and
    /// roster management. It also implements various XMPP protocol extensions.
    /// </remarks>
    public interface IXmppClient : IDisposable
    {
        /// <summary>
        /// The hostname of the XMPP server to connect to.
        /// </summary>
        string Hostname { get; set; }

        /// <summary>
        /// The port number of the XMPP service of the server.
        /// </summary>
        int Port { get; set; }

        /// <summary>
        /// The username with which to authenticate. In XMPP jargon this is known
        /// as the 'node' part of the JID.
        /// </summary>
        string Username { get; set; }

        /// <summary>
        /// The password with which to authenticate.
        /// </summary>
        string Password { get; set; }

        /// <summary>
        /// If true the session will be TLS/SSL-encrypted if the server supports it.
        /// </summary>
        bool Tls { get; set; }

        /// <summary>
        /// A delegate used for verifying the remote Secure Sockets Layer (SSL)
        /// certificate which is used for authentication.
        /// </summary>
        RemoteCertificateValidationCallback Validate { get; set; }

        /// <summary>
        /// Determines whether the session with the server is TLS/SSL encrypted.
        /// </summary>
        bool IsEncrypted { get; }

        /// <summary>
        /// The address of the Xmpp entity.
        /// </summary>
        Jid Jid { get; }

        /// <summary>
        /// Determines whether the instance is connected to the XMPP server.
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// Determines whether the instance has been authenticated.
        /// </summary>
        bool Authenticated { get; }

        /// <summary>
        /// The default IQ Set Time out in Milliseconds. -1 means no timeout
        /// </summary>
        int DefaultTimeOut { get; set; }

        /// <summary>
        /// If true prints XML stanzas
        /// </summary>
        bool DebugStanzas { get; set; }

        /// <summary>
        /// Contains settings for configuring file-transfer options.
        /// </summary>
        IFileTransferSettings FileTransferSettings { get; }

        /// <summary>
        /// The underlying XmppIm instance.
        /// </summary>
        IXmppIm Im { get; }

        /// <summary>
        /// A callback method to invoke when a request for a subscription is received
        /// from another XMPP user.
        /// </summary>
        /// <include file='Examples.xml' path='S22/Xmpp/Client/XmppClient[@name="SubscriptionRequest"]/*'/>
        SubscriptionRequest SubscriptionRequest { get; set; }

        /// <summary>
        /// The event that is raised when a status notification has been received.
        /// </summary>
        event EventHandler<StatusEventArgs> StatusChanged;

        /// <summary>
        /// The event that is raised when a mood notification has been received.
        /// </summary>
        event EventHandler<MoodChangedEventArgs> MoodChanged;

        /// <summary>
        /// The event that is raised when an activity notification has been received.
        /// </summary>
        event EventHandler<ActivityChangedEventArgs> ActivityChanged;

#if WINDOWSPLATFORM
		/// <summary>
		/// The event that is raised when a contact has updated his or her avatar.
		/// </summary>
		event EventHandler<AvatarChangedEventArgs> AvatarChanged { add; remove; }
#endif

        /// <summary>
        /// The event that is raised when a contact has published tune information.
        /// </summary>
        event EventHandler<TuneEventArgs> Tune;

        /// <summary>
        /// The event that is raised when a chat message is received.
        /// </summary>
        event EventHandler<MessageEventArgs> Message;

        /// <summary>
        /// The event that is raised periodically for every file-transfer operation to
        /// inform subscribers of the progress of the operation.
        /// </summary>
        event EventHandler<FileTransferProgressEventArgs> FileTransferProgress;

        /// <summary>
        /// The event that is raised when an on-going file-transfer has been aborted
        /// prematurely, either due to cancellation or error.
        /// </summary>
        event EventHandler<FileTransferAbortedEventArgs> FileTransferAborted;

        /// <summary>
        /// The event that is raised when the chat-state of an XMPP entity has
        /// changed.
        /// </summary>
        event EventHandler<ChatStateChangedEventArgs> ChatStateChanged;

        /// <summary>
        /// The event that is raised when the roster of the user has been updated,
        /// i.e. a contact has been added, removed or updated.
        /// </summary>
        event EventHandler<RosterUpdatedEventArgs> RosterUpdated;

        /// <summary>
        /// The event that is raised when a user or resource has unsubscribed from
        /// receiving presence notifications of the JID associated with this instance.
        /// </summary>
        event EventHandler<UnsubscribedEventArgs> Unsubscribed;

        /// <summary>
        /// The event that is raised when a subscription request made by the JID
        /// associated with this instance has been approved.
        /// </summary>
        event EventHandler<SubscriptionApprovedEventArgs> SubscriptionApproved;

        /// <summary>
        /// The event that is raised when a subscription request made by the JID
        /// associated with this instance has been refused.
        /// </summary>
        event EventHandler<SubscriptionRefusedEventArgs> SubscriptionRefused;

        /// <summary>
        /// The event that is raised when an unrecoverable error condition occurs.
        /// </summary>
        event EventHandler<Im.ErrorEventArgs> Error;

        /// <summary>
        /// Establishes a connection to the XMPP server.
        /// </summary>
        /// <param name="resource">The resource identifier to bind with. If this is null,
        /// a resource identifier will be assigned by the server.</param>
        /// <returns>The user's roster (contact list).</returns>
        /// <exception cref="System.Security.Authentication.AuthenticationException">An
        /// authentication error occured while trying to establish a secure connection, or
        /// the provided credentials were rejected by the server, or the server requires
        /// TLS/SSL and the Tls property has been set to false.</exception>
        /// <exception cref="System.IO.IOException">There was a failure while writing to or
        /// reading from the network. If the InnerException is of type SocketExcption, use
        /// the ErrorCode property to obtain the specific socket error code.</exception>
        /// <exception cref="ObjectDisposedException">The XmppClient object has been
        /// disposed.</exception>
        /// <exception cref="XmppException">An XMPP error occurred while negotiating the
        /// XML stream with the server, or resource binding failed, or the initialization
        /// of an XMPP extension failed.</exception>
        void Connect(string resource = null);

        /// <summary>
        /// Authenticates with the XMPP server using the specified username and
        /// password.
        /// </summary>
        /// <param name="username">The username to authenticate with.</param>
        /// <param name="password">The password to authenticate with.</param>
        /// <exception cref="ArgumentNullException">The username parameter or the
        /// password parameter is null.</exception>
        /// <exception cref="System.Security.Authentication.AuthenticationException">
        /// An authentication error occurred while trying to establish a secure connection,
        /// or the provided credentials were rejected by the server, or the server requires
        /// TLS/SSL and the Tls property has been set to false.</exception>
        /// <exception cref="IOException">There was a failure while writing to or reading
        /// from the network. If the InnerException is of type SocketExcption, use the
        /// ErrorCode property to obtain the specific socket error code.</exception>
        /// <exception cref="ObjectDisposedException">The XmppIm object has been
        /// disposed.</exception>
        /// <exception cref="XmppException">An XMPP error occurred while negotiating the
        /// XML stream with the server, or resource binding failed, or the initialization
        /// of an XMPP extension failed.</exception>
        void Authenticate(string username, string password);

        /// <summary>
        /// Sends a chat message with the specified content to the specified JID.
        /// </summary>
        /// <param name="to">The JID of the intended recipient.</param>
        /// <param name="body">The content of the message.</param>
        /// <param name="subject">The subject of the message.</param>
        /// <param name="thread">The conversation thread the message belongs to.</param>
        /// <param name="type">The type of the message. Can be one of the values from
        /// the MessagType enumeration.</param>
        /// <param name="language">The language of the XML character data of
        /// the stanza.</param>
        /// <exception cref="ArgumentNullException">The to parameter or the body parameter
        /// is null.</exception>
        /// <exception cref="ArgumentException">The body parameter is the empty
        /// string.</exception>
        /// <exception cref="IOException">There was a failure while writing to or reading
        /// from the network.</exception>
        /// <exception cref="InvalidOperationException">The XmppClient instance is not
        /// connected to a remote host, or the XmppClient instance has not authenticated with
        /// the XMPP server.</exception>
        /// <exception cref="ObjectDisposedException">The XmppClient object has been
        /// disposed.</exception>
        /// <include file='Examples.xml' path='S22/Xmpp/Client/XmppClient[@name="SendMessage-1"]/*'/>
        void SendMessage(Jid to, string body, string subject = null,
            string thread = null, MessageType type = MessageType.Normal,
            CultureInfo language = null);

        /// <summary>
        /// Sends a chat message with the specified content to the specified JID.
        /// </summary>
        /// <param name="to">The JID of the intended recipient.</param>
        /// <param name="bodies">A dictionary of message bodies. The dictionary
        /// keys denote the languages of the message bodies and must be valid
        /// ISO 2 letter language codes.</param>
        /// <param name="subjects">A dictionary of message subjects. The dictionary
        /// keys denote the languages of the message subjects and must be valid
        /// ISO 2 letter language codes.</param>
        /// <param name="thread">The conversation thread the message belongs to.</param>
        /// <param name="type">The type of the message. Can be one of the values from
        /// the MessagType enumeration.</param>
        /// <param name="language">The language of the XML character data of
        /// the stanza.</param>
        /// <exception cref="ArgumentNullException">The to parameter or the bodies
        /// parameter is null.</exception>
        /// <exception cref="IOException">There was a failure while writing to or reading
        /// from the network.</exception>
        /// <exception cref="InvalidOperationException">The XmppClient instance is not
        /// connected to a remote host, or the XmppClient instance has not authenticated with
        /// the XMPP server.</exception>
        /// <exception cref="ObjectDisposedException">The XmppClient object has been
        /// disposed.</exception>
        /// <remarks>
        /// An XMPP chat-message may contain multiple subjects and bodies in different
        /// languages. Use this method in order to send a message that contains copies of the
        /// message content in several distinct languages.
        /// </remarks>
        /// <include file='Examples.xml' path='S22/Xmpp/Client/XmppClient[@name="SendMessage-2"]/*'/>
        void SendMessage(Jid to, IDictionary<string, string> bodies,
            IDictionary<string, string> subjects = null, string thread = null,
            MessageType type = MessageType.Normal, CultureInfo language = null);

        /// <summary>
        /// Sends the specified chat message.
        /// </summary>
        /// <param name="message">The chat message to send.</param>
        /// <exception cref="ArgumentNullException">The message parameter is null.</exception>
        /// <exception cref="IOException">There was a failure while writing to or reading
        /// from the network.</exception>
        /// <exception cref="InvalidOperationException">The XmppClient instance is not
        /// connected to a remote host, or the XmppClient instance has not authenticated with
        /// the XMPP server.</exception>
        /// <exception cref="ObjectDisposedException">The XmppClient object has been
        /// disposed.</exception>
        void SendMessage(IMessage message);

        /// <summary>
        /// Sets the availability status.
        /// </summary>
        /// <param name="availability">The availability state. Can be one of the
        /// values from the Availability enumeration, however not
        /// Availability.Offline.</param>
        /// <param name="message">An optional message providing a detailed
        /// description of the availability state.</param>
        /// <param name="priority">Provides a hint for stanza routing.</param>
        /// <param name="language">The language of the description of the
        /// availability state.</param>
        /// <exception cref="ArgumentException">The availability parameter has a
        /// value of Availability.Offline.</exception>
        /// <exception cref="IOException">There was a failure while writing to or reading
        /// from the network.</exception>
        /// <exception cref="InvalidOperationException">The XmppClient instance is not
        /// connected to a remote host, or the XmppClient instance has not authenticated with
        /// the XMPP server.</exception>
        /// <exception cref="ObjectDisposedException">The XmppClient object has been
        /// disposed.</exception>
        void SetStatus(Availability availability, string message = null,
            sbyte priority = 0, CultureInfo language = null);

        /// <summary>
        /// Sets the availability status.
        /// </summary>
        /// <param name="availability">The availability state. Can be one of the
        /// values from the Availability enumeration, however not
        /// Availability.Offline.</param>
        /// <param name="messages">A dictionary of messages providing detailed
        /// descriptions of the availability state. The dictionary keys denote
        /// the languages of the messages and must be valid ISO 2 letter language
        /// codes.</param>
        /// <param name="priority">Provides a hint for stanza routing.</param>
        /// <exception cref="ArgumentException">The availability parameter has a
        /// value of Availability.Offline.</exception>
        /// <exception cref="IOException">There was a failure while writing to or reading
        /// from the network.</exception>
        /// <exception cref="InvalidOperationException">The XmppClient instance is not
        /// connected to a remote host, or the XmppClient instance has not authenticated with
        /// the XMPP server.</exception>
        /// <exception cref="ObjectDisposedException">The XmppClient object has been
        /// disposed.</exception>
        void SetStatus(Availability availability,
            Dictionary<string, string> messages, sbyte priority = 0);

        /// <summary>
        /// Sets the availability status.
        /// </summary>
        /// <param name="status">An instance of the Status class.</param>
        /// <exception cref="ArgumentNullException">The status parameter is null.</exception>
        /// <exception cref="ArgumentException">The Availability property of the status
        /// parameter has a value of Availability.Offline.</exception>
        /// <exception cref="IOException">There was a failure while writing to or reading
        /// from the network.</exception>
        /// <exception cref="InvalidOperationException">The XmppClient instance is not
        /// connected to a remote host, or the XmppClient instance has not authenticated with
        /// the XMPP server.</exception>
        /// <exception cref="ObjectDisposedException">The XmppClient object has been
        /// disposed.</exception>
        void SetStatus(Status status);

        /// <summary>
        /// Retrieves the user's roster (contact list).
        /// </summary>
        /// <returns>The user's roster.</returns>
        /// <remarks>In XMPP jargon, the user's contact list is called a
        /// 'roster'.</remarks>
        /// <exception cref="IOException">There was a failure while writing to or reading
        /// from the network.</exception>
        /// <exception cref="InvalidOperationException">The XmppClient instance is not
        /// connected to a remote host, or the XmppClient instance has not authenticated with
        /// the XMPP server.</exception>
        /// <exception cref="ObjectDisposedException">The XmppClient object has been
        /// disposed.</exception>
        /// <exception cref="XmppErrorException">The server returned an XMPP error code.
        /// Use the Error property of the XmppErrorException to obtain the specific
        /// error condition.</exception>
        /// <exception cref="XmppException">The server returned invalid data or another
        /// unspecified XMPP error occurred.</exception>
        /// <include file='Examples.xml' path='S22/Xmpp/Client/XmppClient[@name="GetRoster"]/*'/>
        IRoster GetRoster();

        /// <summary>
        /// Adds the contact with the specified JID to the user's roster.
        /// </summary>
        /// <param name="jid">The JID of the contact to add to the user's roster.</param>
        /// <param name="name">The nickname with which to associate the contact.</param>
        /// <param name="groups">An array of groups or categories the new contact
        /// will be added to.</param>
        /// <remarks>This method creates a new item on the user's roster and requests
        /// a subscription from the contact with the specified JID.</remarks>
        /// <exception cref="ArgumentNullException">The jid parameter is null.</exception>
        /// <exception cref="IOException">There was a failure while writing to or reading
        /// from the network.</exception>
        /// <exception cref="InvalidOperationException">The XmppClient instance is not
        /// connected to a remote host, or the XmppClient instance has not authenticated with
        /// the XMPP server.</exception>
        /// <exception cref="ObjectDisposedException">The XmppClient object has been
        /// disposed.</exception>
        /// <exception cref="XmppErrorException">The server returned an XMPP error code.
        /// Use the Error property of the XmppErrorException to obtain the specific
        /// error condition.</exception>
        /// <exception cref="XmppException">The server returned invalid data or another
        /// unspecified XMPP error occurred.</exception>
        void AddContact(Jid jid, string name = null, params string[] groups);

        /// <summary>
        /// Removes the item with the specified JID from the user's roster.
        /// </summary>
        /// <param name="jid">The JID of the roster item to remove.</param>
        /// <exception cref="ArgumentNullException">The jid parameter is null.</exception>
        /// <exception cref="IOException">There was a failure while writing to or reading
        /// from the network.</exception>
        /// <exception cref="InvalidOperationException">The XmppClient instance is not
        /// connected to a remote host, or the XmppClient instance has not authenticated with
        /// the XMPP server.</exception>
        /// <exception cref="ObjectDisposedException">The XmppClient object has been
        /// disposed.</exception>
        /// <exception cref="XmppErrorException">The server returned an XMPP error code.
        /// Use the Error property of the XmppErrorException to obtain the specific
        /// error condition.</exception>
        /// <exception cref="XmppException">The server returned invalid data or another
        /// unspecified XMPP error occurred.</exception>
        void RemoveContact(Jid jid);

        /// <summary>
        /// Removes the specified item from the user's roster.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <exception cref="ArgumentNullException">The item parameter is null.</exception>
        /// <exception cref="IOException">There was a failure while writing to or reading
        /// from the network.</exception>
        /// <exception cref="InvalidOperationException">The XmppClient instance is not
        /// connected to a remote host, or the XmppClient instance has not authenticated with
        /// the XMPP server.</exception>
        /// <exception cref="ObjectDisposedException">The XmppClient object has been
        /// disposed.</exception>
        /// <exception cref="XmppErrorException">The server returned an XMPP error code.
        /// Use the Error property of the XmppErrorException to obtain the specific
        /// error condition.</exception>
        /// <exception cref="XmppException">The server returned invalid data or another
        /// unspecified XMPP error occurred.</exception>
        void RemoveContact(IRosterItem item);

#if WINDOWSPLATFORM
        /// <summary>
        /// Publishes the image located at the specified path as the user's avatar.
        /// </summary>
        /// <param name="filePath">The path to the image to publish as the user's
        /// avatar.</param>
        /// <exception cref="ArgumentNullException">The filePath parameter is
        /// null.</exception>
        /// <exception cref="ArgumentException">filePath is a zero-length string,
        /// contains only white space, or contains one or more invalid
        /// characters.</exception>
        /// <exception cref="PathTooLongException">The specified path, file name,
        /// or both exceed the system-defined maximum length. For example, on
        /// Windows-based platforms, paths must be less than 248 characters, and
        /// file names must be less than 260 characters.</exception>
        /// <exception cref="DirectoryNotFoundException">The specified path is
        /// invalid, (for example, it is on an unmapped drive).</exception>
        /// <exception cref="UnauthorizedAccessException">The path specified is
        /// a directory, or the caller does not have the required
        /// permission.</exception>
        /// <exception cref="FileNotFoundException">The file specified in
        /// filePath was not found.</exception>
        /// <exception cref="NotSupportedException">filePath is in an invalid
        /// format, or the server does not support the 'Personal Eventing
        /// Protocol' extension.</exception>
        /// <exception cref="XmppErrorException">The server returned an XMPP error code.
        /// Use the Error property of the XmppErrorException to obtain the specific
        /// error condition.</exception>
        /// <exception cref="XmppException">The server returned invalid data or another
        /// unspecified XMPP error occurred.</exception>
        /// <exception cref="InvalidOperationException">The XmppClient instance is not
        /// connected to a remote host, or the XmppClient instance has not authenticated with
        /// the XMPP server.</exception>
        /// <exception cref="ObjectDisposedException">The XmppClient object has been
        /// disposed.</exception>
        /// <remarks>
        /// The following file types are supported:
        ///  BMP, GIF, JPEG, PNG and TIFF.
        /// </remarks>
		void SetAvatar(string filePath);
#endif

        /// <summary>
        /// Publishes the image located at the specified path as the user's avatar using vcard based Avatars
        /// </summary>
        /// <param name="filePath">The path to the image to publish as the user's avatar.</param>
        void SetvCardAvatar(string filePath);

        /// <summary>
        /// Get the vcard based Avatar of user with Jid
        /// </summary>
        /// <param name="jid">The string jid of the user</param>
        /// <param name="filepath">The filepath where the avatar will be stored</param>
        /// <param name="callback">The action that will be executed after the file has been downloaded</param>
        void GetvCardAvatar(string jid, string filepath, Action callback);

        /// <summary>
        /// Requests a Custom Iq from the XMPP entity Jid
        /// </summary>
        /// <param name="jid">The XMPP entity to request the custom IQ</param>
        /// <param name="str">The payload string to provide to the Request</param>
        /// <param name="callback">The callback method to call after the Request Result has being received. Included the serialised dat
        /// of the answer to the request</param>
        void RequestCustomIq(Jid jid, string str, Action callback = null);

        /// <summary>
        /// Sets the user's mood to the specified mood value.
        /// </summary>
        /// <param name="mood">A value from the Mood enumeration to set the user's
        /// mood to.</param>
        /// <param name="description">A natural-language description of, or reason
        /// for, the mood.</param>
        /// <exception cref="InvalidOperationException">The XmppClient instance is not
        /// connected to a remote host, or the XmppClient instance has not authenticated with
        /// the XMPP server.</exception>
        /// <exception cref="ObjectDisposedException">The XmppClient object has been
        /// disposed.</exception>
        void SetMood(Mood mood, string description = null);

        /// <summary>
        /// Sets the user's activity to the specified activity value(s).
        /// </summary>
        /// <param name="activity">A value from the GeneralActivity enumeration to
        /// set the user's general activity to.</param>
        /// <param name="specific">A value from the SpecificActivity enumeration
        /// best describing the user's activity in more detail.</param>
        /// <param name="description">A natural-language description of, or reason
        /// for, the activity.</param>
        /// <exception cref="InvalidOperationException">The XmppClient instance is not
        /// connected to a remote host, or the XmppClient instance has not authenticated with
        /// the XMPP server.</exception>
        /// <exception cref="ObjectDisposedException">The XmppClient object has been
        /// disposed.</exception>
        /// <include file='Examples.xml' path='S22/Xmpp/Client/XmppClient[@name="SetActivity"]/*'/>
        void SetActivity(GeneralActivity activity, SpecificActivity specific =
            SpecificActivity.Other, string description = null);

        /// <summary>
        /// Publishes the specified music information to contacts on the user's
        /// roster.
        /// </summary>
        /// <param name="title">The title of the song or piece.</param>
        /// <param name="artist">The artist or performer of the song or piece.</param>
        /// <param name="track">A unique identifier for the tune; e.g., the track number
        /// within a collection or the specific URI for the object (e.g., a
        /// stream or audio file).</param>
        /// <param name="length">The duration of the song or piece in seconds.</param>
        /// <param name="rating">The user's rating of the song or piece, from 1
        /// (lowest) to 10 (highest).</param>
        /// <param name="source">The collection (e.g., album) or other source
        /// (e.g., a band website that hosts streams or audio files).</param>
        /// <param name="uri">A URI or URL pointing to information about the song,
        /// collection, or artist</param>
        /// <exception cref="NotSupportedException">The server does not support the
        /// 'Personal Eventing Protocol' extension.</exception>
        /// <exception cref="XmppErrorException">The server returned an XMPP error code.
        /// Use the Error property of the XmppErrorException to obtain the specific
        /// error condition.</exception>
        /// <exception cref="XmppException">The server returned invalid data or another
        /// unspecified XMPP error occurred.</exception>
        /// <exception cref="InvalidOperationException">The XmppClient instance is not
        /// connected to a remote host, or the XmppClient instance has not authenticated with
        /// the XMPP server.</exception>
        /// <exception cref="ObjectDisposedException">The XmppClient object has been
        /// disposed.</exception>
        /// <remarks>Publishing no information (i.e. calling Publish without any parameters
        /// is considered a "stop command" to disable publishing).</remarks>
        void SetTune(string title = null, string artist = null, string track = null,
            int length = 0, int rating = 0, string source = null, string uri = null);

        /// <summary>
        /// Publishes the specified music information to contacts on the user's
        /// roster.
        /// </summary>
        /// <param name="tune">The tune information to publish.</param>
        /// <exception cref="ArgumentNullException">The tune parameter is
        /// null.</exception>
        /// <exception cref="NotSupportedException">The server does not support the
        /// 'Personal Eventing Protocol' extension.</exception>
        /// <exception cref="XmppErrorException">The server returned an XMPP error code.
        /// Use the Error property of the XmppErrorException to obtain the specific
        /// error condition.</exception>
        /// <exception cref="XmppException">The server returned invalid data or another
        /// unspecified XMPP error occurred.</exception>
        /// <exception cref="InvalidOperationException">The XmppClient instance is not
        /// connected to a remote host, or the XmppClient instance has not authenticated with
        /// the XMPP server.</exception>
        /// <exception cref="ObjectDisposedException">The XmppClient object has been
        /// disposed.</exception>
        /// <include file='Examples.xml' path='S22/Xmpp/Client/XmppClient[@name="SetTune"]/*'/>
        void SetTune(TuneInformation tune);

        /// <summary>
        /// A callback method to invoke when a request for a file-transfer is received
        /// from another XMPP user.
        /// </summary>
        FileTransferRequest FileTransferRequest { get; set; }

        /// <summary>
        /// A callback method to invoke when a Custom Iq Request is received
        /// from another XMPP user.
        /// </summary>
        CustomIqRequestDelegate CustomIqDelegate { get; set; }

        /// <summary>
        /// Offers the specified file to the XMPP user with the specified JID and, if
        /// accepted by the user, transfers the file.
        /// </summary>
        /// <param name="to">The JID of the XMPP user to offer the file to.</param>
        /// <param name="path">The path of the file to transfer.</param>
        /// <param name="cb">a callback method invoked once the other site has
        /// accepted or rejected the file-transfer request.</param>
        /// <param name="description">A description of the file so the receiver can
        /// better understand what is being sent.</param>
        /// <returns>Sid of the file transfer</returns>
        /// <exception cref="ArgumentNullException">The to parameter or the path
        /// parameter is null.</exception>
        /// <exception cref="ArgumentException">path is a zero-length string,
        /// contains only white space, or contains one or more invalid
        /// characters.</exception>
        /// <exception cref="PathTooLongException">The specified path, file name,
        /// or both exceed the system-defined maximum length.</exception>
        /// <exception cref="DirectoryNotFoundException">The specified path is
        /// invalid, (for example, it is on an unmapped drive).</exception>
        /// <exception cref="UnauthorizedAccessException">path specified a
        /// directory, or the caller does not have the required
        /// permission.</exception>
        /// <exception cref="FileNotFoundException">The file specified in path
        /// was not found.</exception>
        /// <exception cref="NotSupportedException">path is in an invalid
        /// format, or the XMPP entity with the specified JID does not support
        /// the 'SI File Transfer' XMPP extension.</exception>
        /// <exception cref="XmppErrorException">The server or the XMPP entity
        /// with the specified JID returned an XMPP error code. Use the Error
        /// property of the XmppErrorException to obtain the specific error
        /// condition.</exception>
        /// <exception cref="XmppException">The server returned invalid data or
        /// another unspecified XMPP error occurred.</exception>
        /// <exception cref="InvalidOperationException">The XmppClient instance is not
        /// connected to a remote host, or the XmppClient instance has not authenticated with
        /// the XMPP server.</exception>
        /// <exception cref="ObjectDisposedException">The XmppClient object has been
        /// disposed.</exception>
        string InitiateFileTransfer(Jid to, string path,
            string description = null, Action<bool, IFileTransfer> cb = null);

        /// <summary>
        /// Offers the XMPP user with the specified JID the file with the specified
        /// name and, if accepted by the user, transfers the file using the supplied
        /// stream.
        /// </summary>
        /// <param name="to">The JID of the XMPP user to offer the file to.</param>
        /// <param name="stream">The stream to read the file-data from.</param>
        /// <param name="name">The name of the file, as offered to the XMPP user
        /// with the specified JID.</param>
        /// <param name="size">The number of bytes to transfer.</param>
        /// <param name="cb">A callback method invoked once the other site has
        /// accepted or rejected the file-transfer request.</param>
        /// <param name="description">A description of the file so the receiver can
        /// better understand what is being sent.</param>
        /// <returns>The Sid of the file transfer</returns>
        /// <exception cref="ArgumentNullException">The to parameter or the stream
        /// parameter or the name parameter is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The value of the size
        /// parameter is negative.</exception>
        /// <exception cref="NotSupportedException">The XMPP entity with the
        /// specified JID does not support the 'SI File Transfer' XMPP
        /// extension.</exception>
        /// <exception cref="XmppErrorException">The server or the XMPP entity
        /// with the specified JID returned an XMPP error code. Use the Error
        /// property of the XmppErrorException to obtain the specific error
        /// condition.</exception>
        /// <exception cref="XmppException">The server returned invalid data or
        /// another unspecified XMPP error occurred.</exception>
        /// <exception cref="InvalidOperationException">The XmppClient instance is not
        /// connected to a remote host, or the XmppClient instance has not authenticated with
        /// the XMPP server.</exception>
        /// <exception cref="ObjectDisposedException">The XmppClient object has been
        /// disposed.</exception>
        string InitiateFileTransfer(Jid to, Stream stream, string name, long size,
            string description = null, Action<bool, IFileTransfer> cb = null);

        /// <summary>
        /// Cancels the specified file-transfer.
        /// </summary>
        /// <param name="transfer">The file-transfer to cancel.</param>
        /// <exception cref="ArgumentNullException">The transfer parameter is
        /// null.</exception>
        /// <exception cref="ArgumentException">The specified transfer instance does
        /// not represent an active data-transfer operation.</exception>
        /// <exception cref="InvalidOperationException">The XmppClient instance is not
        /// connected to a remote host, or the XmppClient instance has not authenticated with
        /// the XMPP server.</exception>
        /// <exception cref="ObjectDisposedException">The XmppClient object has been
        /// disposed.</exception>
        void CancelFileTransfer(IFileTransfer transfer);

        /// <summary>
        /// Cancels the specified file-transfer.
        /// </summary>
        /// <param name="from">From Jid</param>
        /// <param name="sid">Sid</param>
        /// <param name="to">To Jid</param>
        /// <exception cref="ArgumentNullException">The transfer parameter is
        /// null.</exception>
        /// <exception cref="ArgumentException">The specified transfer instance does
        /// not represent an active data-transfer operation.</exception>
        /// <exception cref="InvalidOperationException">The XmppClient instance is not
        /// connected to a remote host, or the XmppClient instance has not authenticated with
        /// the XMPP server.</exception>
        /// <exception cref="ObjectDisposedException">The XmppClient object has been
        /// disposed.</exception>
        void CancelFileTransfer(string sid, Jid from, Jid to);

        /// <summary>
        /// Initiates in-band registration with the XMPP server in order to register
        /// a new XMPP account.
        /// </summary>
        /// <param name="callback">A callback method invoked to let the user
        /// enter any information required by the server in order to complete the
        /// registration.</param>
        /// <exception cref="ArgumentNullException">The callback parameter is
        /// null.</exception>
        /// <exception cref="NotSupportedException">The XMPP server with does not
        /// support the 'In-Band Registration' XMPP extension.</exception>
        /// <exception cref="XmppErrorException">The server returned an XMPP error code.
        /// Use the Error property of the XmppErrorException to obtain the specific
        /// error condition.</exception>
        /// <exception cref="XmppException">The server returned invalid data or another
        /// unspecified XMPP error occurred.</exception>
        /// <remarks>
        /// See the "Howto: Register an account" guide for a walkthrough on how to
        /// register an XMPP account through the in-band registration process.
        /// </remarks>
        void Register(RegistrationCallback callback);

        /// <summary>
        /// Retrieves the current time of the XMPP client with the specified JID.
        /// </summary>
        /// <param name="jid">The JID of the user to retrieve the current time
        /// for.</param>
        /// <returns>The current time of the XMPP client with the specified JID.</returns>
        /// <exception cref="ArgumentNullException">The jid parameter is null.</exception>
        /// <exception cref="InvalidOperationException">The XmppClient instance is
        /// not connected to a remote host.</exception>
        /// <exception cref="System.IO.IOException">There was a failure while writing to or
        /// reading from the network.</exception>
        /// <exception cref="NotSupportedException">The XMPP client of the
        /// user with the specified JID does not support the retrieval of the
        /// current time.</exception>
        /// <exception cref="ObjectDisposedException">The XmppClient object
        /// has been disposed.</exception>
        /// <exception cref="XmppErrorException">The server or the XMPP client of
        /// the user with the specified JID returned an XMPP error code. Use the
        /// Error property of the XmppErrorException to obtain the specific error
        /// condition.</exception>
        /// <exception cref="XmppException">The server returned invalid data or another
        /// unspecified XMPP error occurred.</exception>
        DateTime GetTime(Jid jid);

        /// <summary>
        /// Retrieves the software version of the XMPP client with the specified JID.
        /// </summary>
        /// <param name="jid">The JID of the user to retrieve version information
        /// for.</param>
        /// <returns>An initialized instance of the VersionInformation class providing
        /// the name and version of the XMPP client used by the user with the specified
        /// JID.</returns>
        /// <exception cref="ArgumentNullException">The jid parameter is null.</exception>
        /// <exception cref="InvalidOperationException">The XmppClient instance is not
        /// connected to a remote host, or the XmppCleint instance has not authenticated
        /// with the XMPP server.</exception>
        /// <exception cref="System.IO.IOException">There was a failure while writing to or
        /// reading from the network.</exception>
        /// <exception cref="NotSupportedException">The XMPP client of the
        /// user with the specified JID does not support the retrieval of version
        /// information.</exception>
        /// <exception cref="ObjectDisposedException">The XmppClient object
        /// has been disposed.</exception>
        /// <exception cref="XmppErrorException">The server or the XMPP client of
        /// the user with the specified JID returned an XMPP error code. Use the
        /// Error property of the XmppErrorException to obtain the specific error
        /// condition.</exception>
        /// <exception cref="XmppException">The server returned invalid data or another
        /// unspecified XMPP error occurred.</exception>
        VersionInformation GetVersion(Jid jid);

        /// <summary>
        /// Returns an enumerable collection of XMPP features supported by the XMPP
        /// client with the specified JID.
        /// </summary>
        /// <param name="jid">The JID of the XMPP client to retrieve a collection of
        /// supported features for.</param>
        /// <returns>An enumerable collection of XMPP extensions supported by the
        /// XMPP client with the specified JID.</returns>
        /// <exception cref="ArgumentNullException">The jid parameter is null.</exception>
        /// <exception cref="InvalidOperationException">The XmppClient instance is
        /// not connected to a remote host.</exception>
        /// <exception cref="System.IO.IOException">There was a failure while writing to or
        /// reading from the network.</exception>
        /// <exception cref="NotSupportedException">The XMPP client of the
        /// user with the specified JID does not support the retrieval of feature
        /// information.</exception>
        /// <exception cref="ObjectDisposedException">The XmppClient object
        /// has been disposed.</exception>
        /// <exception cref="XmppErrorException">The server or the XMPP client of
        /// the user with the specified JID returned an XMPP error code. Use the
        /// Error property of the XmppErrorException to obtain the specific error
        /// condition.</exception>
        /// <exception cref="XmppException">The server returned invalid data or another
        /// unspecified XMPP error occurred.</exception>
        /// <include file='Examples.xml' path='S22/Xmpp/Client/XmppClient[@name="GetFeatures"]/*'/>
        IEnumerable<Extension> GetFeatures(Jid jid);

        /// <summary>
        /// Buzzes the user with the specified JID in order to get his or her attention.
        /// </summary>
        /// <param name="jid">The JID of the user to buzz.</param>
        /// <param name="message">An optional message to send along with the buzz
        /// notification.</param>
        /// <exception cref="ArgumentNullException">The jid parameter is null.</exception>
        /// <exception cref="InvalidOperationException">The XmppClient instance is
        /// not connected to a remote host.</exception>
        /// <exception cref="System.IO.IOException">There was a failure while writing to or
        /// reading from the network.</exception>
        /// <exception cref="NotSupportedException">The XMPP client of the
        /// user with the specified JID does not support buzzing.</exception>
        /// <exception cref="ObjectDisposedException">The XmppClient object
        /// has been disposed.</exception>
        /// <exception cref="XmppErrorException">The server or the XMPP client of
        /// the user with the specified JID returned an XMPP error code. Use the
        /// Error property of the XmppErrorException to obtain the specific error
        /// condition.</exception>
        /// <exception cref="XmppException">The server returned invalid data or another
        /// unspecified XMPP error occurred.</exception>
        void Buzz(Jid jid, string message = null);

        /// <summary>
        /// Pings the user with the specified JID.
        /// </summary>
        /// <param name="jid">The JID of the user to ping.</param>
        /// <returns>The time it took to ping the user with the specified
        /// JID.</returns>
        /// <exception cref="ArgumentNullException">The jid parameter is null.</exception>
        /// <exception cref="InvalidOperationException">The XmppClient instance is
        /// not connected to a remote host.</exception>
        /// <exception cref="System.IO.IOException">There was a failure while writing to or
        /// reading from the network.</exception>
        /// <exception cref="NotSupportedException">The XMPP client of the
        /// user with the specified JID does not support the 'Ping' XMPP protocol
        /// extension.</exception>
        /// <exception cref="ObjectDisposedException">The XmppClient object
        /// has been disposed.</exception>
        /// <exception cref="XmppErrorException">The server or the XMPP client of
        /// the user with the specified JID returned an XMPP error code. Use the
        /// Error property of the XmppErrorException to obtain the specific error
        /// condition.</exception>
        /// <exception cref="XmppException">The server returned invalid data or another
        /// unspecified XMPP error occurred.</exception>
        TimeSpan Ping(Jid jid);

        /// <summary>
        /// Blocks all communication to and from the XMPP entity with the specified JID.
        /// </summary>
        /// <param name="jid">The JID of the XMPP entity to block.</param>
        /// <exception cref="ArgumentNullException">The jid parameter is
        /// null.</exception>
        /// <exception cref="NotSupportedException">The server does not support the
        /// 'Blocking Command' extension and does not support privacy-list management.
        /// </exception>
        /// <exception cref="XmppErrorException">The server returned an XMPP error code.
        /// Use the Error property of the XmppErrorException to obtain the specific
        /// error condition.</exception>
        /// <exception cref="XmppException">The server returned invalid data or another
        /// unspecified XMPP error occurred.</exception>
        /// <exception cref="InvalidOperationException">The XmppClient instance is
        /// not connected to a remote host.</exception>
        /// <exception cref="ObjectDisposedException">The XmppClient object
        /// has been disposed.</exception>
        void Block(Jid jid);

        /// <summary>
        /// Unblocks all communication to and from the XMPP entity with the specified
        /// JID.
        /// </summary>
        /// <param name="jid">The JID of the XMPP entity to unblock.</param>
        /// <exception cref="ArgumentNullException">The jid parameter is
        /// null.</exception>
        /// <exception cref="NotSupportedException">The server does not support the
        /// 'Blocking Command' extension and does not support privacy-list management.
        /// </exception>
        /// <exception cref="XmppErrorException">The server returned an XMPP error code.
        /// Use the Error property of the XmppErrorException to obtain the specific
        /// error condition.</exception>
        /// <exception cref="XmppException">The server returned invalid data or another
        /// unspecified XMPP error occurred.</exception>
        /// <exception cref="InvalidOperationException">The XmppClient instance is
        /// not connected to a remote host.</exception>
        /// <exception cref="InvalidOperationException">The XmppClient instance is
        /// not connected to a remote host.</exception>
        /// <exception cref="ObjectDisposedException">The XmppClient object
        /// has been disposed.</exception>
        void Unblock(Jid jid);

        /// <summary>
        /// Returns an enumerable collection of blocked contacts.
        /// </summary>
        /// <returns>An enumerable collection of JIDs which are on the client's
        /// blocklist.</returns>
        /// <exception cref="NotSupportedException">The server does not support the
        /// 'Blocking Command' extension and does not support privacy-list management.
        /// </exception>
        /// <exception cref="XmppErrorException">The server returned an XMPP error code.
        /// Use the Error property of the XmppErrorException to obtain the specific
        /// error condition.</exception>
        /// <exception cref="XmppException">The server returned invalid data or another
        /// unspecified XMPP error occurred.</exception>
        /// <exception cref="InvalidOperationException">The XmppClient instance is
        /// not connected to a remote host.</exception>
        /// <exception cref="ObjectDisposedException">The XmppClient object
        /// has been disposed.</exception>
        IEnumerable<Jid> GetBlocklist();

        /// <summary>
        /// Closes the connection with the XMPP server. This automatically disposes
        /// of the object.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The XmppClient object has been
        /// disposed.</exception>
        void Close();

    }
}
