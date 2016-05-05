namespace Sharp.Xmpp.Extensions
{
    /// <summary>
    /// An enumeration of supported XMPP extensions.
    /// </summary>
    public enum Extension
    {
        /// <summary>
        /// An extension for discovering information about other XMPP entities.
        /// </summary>
        /// <remarks>Defined in XEP-0030.</remarks>
        ServiceDiscovery,

        /// <summary>
        /// An extension for retrieving information about the software application
        /// associated with an XMPP entity.
        /// </summary>
        SoftwareVersion,

        /// <summary>
        /// An extension for broadcasting and dynamically discovering client,
        /// device, or generic entity capabilities.
        /// </summary>
        EntityCapabilities,

        /// <summary>
        /// An extension for sending application-level pings over XML streams.
        /// </summary>
        Ping,

        /// <summary>
        /// An extension for getting the attention of another user.
        /// </summary>
        Attention,

        /// <summary>
        /// An extension for communicating the local time of an entity.
        /// </summary>
        EntityTime,

        /// <summary>
        /// An extension for communications blocking that is intended to be
        /// simpler than privacy lists.
        /// </summary>
        BlockingCommand,

        /// <summary>
        /// An extension for publishing and subscribing to broadcast state change
        /// events associated with an instant messaging and presence account.
        /// </summary>
        PersonalEventingProcotol,

        /// <summary>
        /// An extension for communicating information about music to which a user
        /// is listening, including the title, track number, length and others.
        /// </summary>
        UserTune,

        /// <summary>
        /// An extension for exchanging user avatars, which are small images or
        /// icons associated with human users.
        /// </summary>
        UserAvatar,

        /// <summary>
        /// An extension for for communicating information about user moods, such
        /// as whether a person is currently happy, sad, angy, or annoyed.
        /// </summary>
        UserMood,

        /// <summary>
        /// An extension extension for data forms that can be used in workflows
        /// such as service configuration as well as for application-specific
        /// data description and reporting.
        /// </summary>
        DataForms,

        /// <summary>
        /// An extension that enables two entities to mutually negotiate feature
        /// options, such as parameters related to a file transfer or a
        /// communications session.
        /// </summary>
        FeatureNegotiation,

        /// <summary>
        /// An extension for initiating a data stream between any two XMPP
        /// entities.
        /// </summary>
        StreamInitiation,

        /// <summary>
        /// An extension for transferring files between two entities.
        /// </summary>
        SIFileTransfer,

        /// <summary>
        /// An extension that enables any two entities to establish a one-to-one
        /// bytestream between themselves, where the data is broken down into
        /// smaller chunks and transported in-band over XMPP.
        /// </summary>
        InBandBytestreams,

        /// <summary>
        /// An extension for communicating information about user activities,
        /// such as whether a person is currently working, travelling, or
        /// relaxing.
        /// </summary>
        UserActivity,

        /// <summary>
        /// An extension for establishing an out-of-band bytestream between any
        /// two XMPP users, mainly for the purpose of file transfer.
        /// </summary>
        Socks5Bytestreams,

        /// <summary>
        /// An extension that enables a client to discover its external IP
        /// address.
        /// </summary>
        ServerIpCheck,

        /// <summary>
        /// An extension for in-band registration with XMPP-based instant messaging
        /// servers and other services hosted on an XMPP network.
        /// </summary>
        InBandRegistration,

        /// <summary>
        /// An extension for including or referring to small bits of binary data in
        /// an XML stanza.
        /// </summary>
        BitsOfBinary,

        /// <summary>
        /// An extension for communicating the status of a user in a chat session,
        /// thus indicating whether a chat partner is actively engaged in the chat,
        /// composing a message, temporarily paused, inactive, or gone.
        /// </summary>
        ChatStateNotifications,

        /// <summary>
        /// An extestion for downloading-uploading avatar vcard data
        /// </summary>
        vCardsAvatars,

        /// <summary>
        /// An extestion for downloading-uploading vcard data
        /// </summary>
        vCards,

        /// <summary>
        /// Wrapper for providing Custom IQ Extensions
        /// </summary>
        CustomIqExtension,

        /// <summary>
        /// An extension for receiving messages directed to the bare Jid
        /// on multiple resources that have opted-in
        /// </summary>
        MessageCarbons
    }
}