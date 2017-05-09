namespace XMPPEngineer.Extensions
{
    /// <summary>
    /// Invoked when a CustomIqRequest is made.
    /// </summary>
    /// <param name="jid">The jid</param>
    /// <param name="str">The serialised data stream</param>
    /// <returns>The serialised anwser string</returns>
    public delegate string CustomIqRequestDelegate(Jid jid, string str);
}