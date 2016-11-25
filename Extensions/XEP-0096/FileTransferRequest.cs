namespace Sharp.Xmpp.Extensions
{
    /// <summary>
    /// Represents a request for a file-transfer.
    /// </summary>
    /// <param name="transfer">A FileTransfer object containing information about
    /// the file that is being offered.</param>
    /// <returns>The path where the file will be saved to or null to reject
    /// the file request.</returns>
    public delegate string FileTransferRequest(IFileTransfer transfer);
}