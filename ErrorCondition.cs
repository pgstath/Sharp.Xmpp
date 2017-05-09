namespace XMPPEngineer
{
    /// <summary>
    /// Defines possible values for the condition of XMPP errors.
    /// </summary>
    public enum ErrorCondition
    {
        /// <summary>
        /// The sender has sent XML that is malformed or that cannot be processed.
        /// </summary>
        BadRequest,

        /// <summary>
        /// Access cannot be granted because an existing resource or session exists
        /// with the same name or address.
        /// </summary>
        Conflict,

        /// <summary>
        /// The feature requested is not implemented by the recipient or server and
        /// therefore cannot be processed.
        /// </summary>
        FeatureNotImplemented,

        /// <summary>
        /// The requesting entity does not possess the required permissions to
        /// perform the action.
        /// </summary>
        Forbidden,

        /// <summary>
        /// The recipient or server can no longer be contacted at this address.
        /// </summary>
        Gone,

        /// <summary>
        /// The server could not process the stanza because of a misconfiguration or
        /// an otherwise-undefined internal server error.
        /// </summary>
        InternalServerError,

        /// <summary>
        /// The addressed JID or item requested cannot be found.
        /// </summary>
        ItemNotFound,

        /// <summary>
        /// The sending entity has provided or communicated an XMPP address or aspect
        /// thereof that does not adhere to the XMPP syntax.
        /// </summary>
        JidMalformed,

        /// <summary>
        /// The recipient or server understands the request but is refusing to process
        /// it.
        /// </summary>
        NotAcceptable,

        /// <summary>
        /// The recipient or server does not allow any entity to perform the action.
        /// </summary>
        NotAllowed,

        /// <summary>
        /// The sender must provide proper credentials before being allowed to perform
        /// the action, or has provided improper credentials.
        /// </summary>
        NotAuthorized,

        /// <summary>
        /// The requesting entity is not authorized to access the requested service
        /// because payment is required.
        /// </summary>
        PaymentRequired,

        /// <summary>
        /// The intended recipient is temporarily unavailable.
        /// </summary>
        RecipientUnavailable,

        /// <summary>
        /// The recipient or server is redirecting requests for this information to
        /// another entity, usually temporarily.
        /// </summary>
        Redirect,

        /// <summary>
        /// The requesting entity is not authorized to access the requested service
        /// because registration is required.
        /// </summary>
        RegistrationRequired,

        /// <summary>
        /// A remote server or service specified as part or all of the JID of the
        /// intended recipient does not exist.
        /// </summary>
        RemoteServerNotFound,

        /// <summary>
        /// A remote server or service specified as part or all of the JID of the
        /// intended recipient could not be contacted within a reasonable amount
        /// of time.
        /// </summary>
        RemoteServerTimeout,

        /// <summary>
        /// The server or recipient lacks the system resources necessary to service
        /// the request.
        /// </summary>
        ResourceConstraint,

        /// <summary>
        /// The server or recipient does not currently provide the requested service.
        /// </summary>
        ServiceUnavailable,

        /// <summary>
        /// The requesting entity is not authorized to access the requested service
        /// because a subscription is required.
        /// </summary>
        SubscriptionRequired,

        /// <summary>
        /// The error condition is not one of those defined by the other conditions in
        /// this enumeration.
        /// </summary>
        UndefinedCondition,

        /// <summary>
        /// The recipient or server understood the request but was not expecting it at
        /// this time (e.g., the request was out of order).
        /// </summary>
        UnexpectedRequest
    }
}