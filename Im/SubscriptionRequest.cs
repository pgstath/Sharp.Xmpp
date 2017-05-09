namespace XMPPEngineer.Im
{
    /// <summary>
    /// Represents a request for subscription.
    /// </summary>
    /// <param name="from">The JID of the XMPP user that is requesting
    /// authorization to subscribe to presence notifications.</param>
    /// <returns>true to approve the subscription request, or false to refuse
    /// the request.</returns>
    public delegate bool SubscriptionRequest(Jid from);
}