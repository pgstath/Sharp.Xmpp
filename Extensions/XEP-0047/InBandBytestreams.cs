using XMPPEngineer.Core;
using XMPPEngineer.Im;
using System;
using System.Collections.Generic;
using System.IO;

namespace XMPPEngineer.Extensions
{
    /// <summary>
    /// Implements the 'In-Band Bytestreams' extension as defined in XEP-0047.
    /// </summary>
    /// <remarks>
    /// The optional data-transfer via message stanzas is not supported.
    /// </remarks>
    internal class InBandBytestreams : XmppExtension, IInputFilter<Iq>, IDataStream
    {
        /// <summary>
        /// A reference to the 'SI File Transfer' XMPP extension.
        /// </summary>
        private SIFileTransfer siFileTransfer;

        /// <summary>
        /// A reference to the 'Entity Capabilities' extension instance.
        /// </summary>
        private EntityCapabilities ecapa;

        /// <summary>
        /// The block-size defines the maximum size in bytes of each data chunk.
        /// </summary>
        private const int blockSize = 4096;

        /// <summary>
        /// An enumerable collection of XMPP namespaces the extension implements.
        /// </summary>
        /// <remarks>This is used for compiling the list of supported extensions
        /// advertised by the 'Service Discovery' extension.</remarks>
        public override IEnumerable<string> Namespaces
        {
            get
            {
                return new string[] {
					"http://jabber.org/protocol/ibb"
				};
            }
        }

        /// <summary>
        /// The named constant of the Extension enumeration that corresponds to this
        /// extension.
        /// </summary>
        public override Extension Xep
        {
            get
            {
                return Extension.InBandBytestreams;
            }
        }

        /// <summary>
        /// The event that is raised whenever bytes have been transferred.
        /// </summary>
        public event EventHandler<BytesTransferredEventArgs> BytesTransferred;

        /// <summary>
        /// The event that is raised when an on-going transfer was aborted
        /// before it completed.
        /// </summary>
        public event EventHandler<TransferAbortedEventArgs> TransferAborted;

        /// <summary>
        /// Invoked after all extensions have been loaded.
        /// </summary>
        public override void Initialize()
        {
            siFileTransfer = im.GetExtension<SIFileTransfer>();
            ecapa = im.GetExtension<EntityCapabilities>();
        }

        /// <summary>
        /// Invoked when an IQ stanza has been received.
        /// </summary>
        /// <param name="stanza">The stanza which has been received.</param>
        /// <returns>true to intercept the stanza or false to pass the stanza
        /// on to the next handler.</returns>
        public bool Input(Iq stanza)
        {
            if (stanza.Type != IqType.Set)
                return false;
            var e = stanza.Data["open"];
            if (e == null)
                e = stanza.Data["close"];
            if (e == null)
                e = stanza.Data["data"];
            if (e == null || e.NamespaceURI != "http://jabber.org/protocol/ibb")
                return false;
            string sessionId = e.GetAttribute("sid");
            // Dispatch to the proper handler method.
            try
            {
                switch (e.Name)
                {
                    case "open":
                        Open(sessionId, stanza);
                        break;

                    case "data":
                        Data(sessionId, stanza);
                        break;

                    case "close":
                        Close(sessionId, stanza);
                        break;

                    default:
                        throw new ArgumentException("Invalid stanza element.");
                }
                // Acknowledge the IQ request.
                im.IqResult(stanza);
            }
            catch (Exception ex)
            {
                // Send an error response.
                im.IqError(stanza, ErrorType.Cancel, ErrorCondition.ServiceUnavailable,
                    ex.Message);
                // If there is an open stream associated with the session id, we should
                // dispose of it.
                siFileTransfer.InvalidateSession(sessionId);
            }
            // We took care of this IQ request, so intercept it and don't pass it
            // on to other handlers.
            return true;
        }

        /// <summary>
        /// Performs the actual data-transfer implied by the specified
        /// SI session.
        /// </summary>
        /// <param name="session">The SI session whose data to transfer.</param>
        /// <exception cref="ArgumentNullException">The session parameter is
        /// null.</exception>
        /// <exception cref="NotSupportedException">The XMPP extension
        /// implementing this method is not supported by the intended recipient's
        /// XMPP client.</exception>
        /// <exception cref="XmppErrorException">The server or the XMPP entity
        /// with the specified JID returned an XMPP error code. Use the Error
        /// property of the XmppErrorException to obtain the specific error
        /// condition.</exception>
        /// <exception cref="XmppException">The server returned invalid data or
        /// another unspecified XMPP error occurred.</exception>
        public void Transfer(SISession session)
        {
            session.ThrowIfNull("session");
            // Open the negotiated IBB.
            OpenStream(session.To, session.Sid);
            byte[] buf = new byte[blockSize];
            // 'seq' is defined as 16-bit unsigned short value that wraps around.
            ushort seq = 0;
            long left = session.Size;
            try
            {
                while (left > 0)
                {
                    int read = session.Stream.Read(buf, 0, blockSize);
                    left = left - read;
                    if (read <= 0)
                        break;
                    string b64 = Convert.ToBase64String(buf, 0, read);
                    var data = Xml.Element("data", "http://jabber.org/protocol/ibb")
                        .Attr("sid", session.Sid)
                        .Attr("seq", seq.ToString())
                        .Text(b64);
                    seq++;
                    Iq response = im.IqRequest(IqType.Set, session.To, im.Jid, data);
                    if (response.Type == IqType.Error)
                        throw Util.ExceptionFromError(response);
                    session.Count = session.Count + read;
                    // Raise the 'BytesTransferred' event.
                    BytesTransferred.Raise(this, new BytesTransferredEventArgs(session));
                }
            }
            catch (ObjectDisposedException)
            {
                // This means the IO-stream has been disposed because we cancelled
                // the transfer. Just fall through.
            }
            catch
            {
                // The IQ response is of type 'error', the other site has cancelled
                // the transfer.
                TransferAborted.Raise(this, new TransferAbortedEventArgs(session));
                // Rethrow.
                throw;
            }
            finally
            {
                // Gracefully close the IBB.
                CloseStream(session.To, session.Sid);
            }
        }

        /// <summary>
        /// Cancels the data-transfer implied by the specified SI session.
        /// </summary>
        /// <param name="session">The SI session whose data-transfer to
        /// cancel.</param>
        /// <exception cref="ArgumentNullException">The session parameter is
        /// null.</exception>
        public void CancelTransfer(SISession session)
        {
            session.ThrowIfNull("session");
            siFileTransfer.InvalidateSession(session.Sid);
            TransferAborted.Raise(this, new TransferAbortedEventArgs(session));
        }

        /// <summary>
        /// Initializes a new instance of the InBandBytestreams class.
        /// </summary>
        /// <param name="im">A reference to the XmppIm instance on whose behalf this
        /// instance is created.</param>
        public InBandBytestreams(XmppIm im)
            : base(im)
        {
        }

        /// <summary>
        /// Processes an IBB 'open' request.
        /// </summary>
        /// <param name="sessionId">The mandatory session id attribute of the 'open'
        /// element.</param>
        /// <param name="stanza">The IQ stanza containing the request.</param>
        /// <exception cref="ArgumentNullException">The sessionId parameter or the
        /// stanza parameter is null.</exception>
        /// <exception cref="ArgumentException">The specified session id is not
        /// valid, or the initiator requested a data-transfer other than IQ
        /// stanzas (i.e. message stanzas).</exception>
        private void Open(string sessionId, Iq stanza)
        {
            sessionId.ThrowIfNull("sessionId");
            stanza.ThrowIfNull("stanza");
            if (siFileTransfer.GetSession(sessionId, stanza.From, im.Jid) == null)
                throw new XmppException("Invalid session-id.");
            string s = stanza.Data["open"].GetAttribute("stanza");
            if (!String.IsNullOrEmpty(s) && s != "iq")
                throw new XmppException("Only IQ stanzas are supported.");
        }

        /// <summary>
        /// Processes an IBB 'close' request.
        /// </summary>
        /// <param name="sessionId">The mandatory session id attribute of the 'close'
        /// element.</param>
        /// <param name="stanza">The IQ stanza containing the request.</param>
        /// <exception cref="ArgumentNullException">The sessionId parameter or the
        /// stanza parameter is null.</exception>
        private void Close(string sessionId, Iq stanza)
        {
            sessionId.ThrowIfNull("sessionId");
            stanza.ThrowIfNull("stanza");
            SISession session = siFileTransfer.GetSession(sessionId,
                stanza.From, stanza.To);
            // We don't allow the other site to close a session that we opened.
            if (session != null)
            {
                siFileTransfer.InvalidateSession(sessionId);
                // Had all bytes been received when we got the 'close' request?
                // Otherwise, the other site cancelled the transfer prematurely.
                if (session.Count < session.Size)
                    TransferAborted.Raise(this, new TransferAbortedEventArgs(session));
            }
        }

        /// <summary>
        /// Processes an IBB 'data' request.
        /// </summary>
        /// <param name="sessionId">The mandatory session id attribute of the 'data'
        /// element.</param>
        /// <param name="stanza">The IQ stanza containing the request.</param>
        /// <exception cref="ArgumentNullException">The sessionId parameter or the
        /// stanza parameter is null.</exception>
        /// <exception cref="ArgumentException">The IQ stanza is missing the
        /// mandatory 'data' element, or the specified session id is not associated
        /// with a file-stream.</exception>
        /// <exception cref="FormatException">The data contained in the 'data' element
        /// is not a valid BASE64-encoded string.</exception>
        /// <exception cref="IOException">The data could not be written to the
        /// file-stream. Consult the InnerException property of the IOException object
        /// to obtain the specific reason.</exception>
        private void Data(string sessionId, Iq stanza)
        {
            sessionId.ThrowIfNull("sessionId");
            stanza.ThrowIfNull("stanza");
            var data = stanza.Data["data"];
            if (data == null)
                throw new ArgumentException("Invalid stanza, missing data element.");
            SISession session = siFileTransfer.GetSession(sessionId, stanza.From, im.Jid);
            if (session == null)
                throw new ArgumentException("Invalid session-id.");
            string base64 = data.InnerText;
            // Decode base64 string and write decoded binary data to file.
            byte[] bytes = Convert.FromBase64String(base64);
            try
            {
                session.Stream.Write(bytes, 0, bytes.Length);
            }
            catch (Exception e)
            {
                throw new IOException("The stream could not be written.", e);
            }
            // Update the byte count and raise the 'BytesTransferred' event.
            session.Count = session.Count + bytes.Length;
            BytesTransferred.Raise(this, new BytesTransferredEventArgs(session));
        }

        /// <summary>
        /// Opens an in-band bytestream with the XMPP entity with the specified JID.
        /// </summary>
        /// <param name="to">The JID of the XMPP entity to open an in-band bytestream
        /// with.</param>
        /// <param name="sessionId">The session id obtained during stream
        /// initiation.</param>
        /// <remarks>This method expects that stream initiation has been previously
        /// performed.</remarks>
        /// <exception cref="NotSupportedException">The XMPP entity with
        /// the specified JID does not support the 'In-Band Bytestreams' XMPP
        /// extension.</exception>
        /// <exception cref="XmppErrorException">The server returned an XMPP error code.
        /// Use the Error property of the XmppErrorException to obtain the specific
        /// error condition.</exception>
        private void OpenStream(Jid to, string sessionId)
        {
            if (!ecapa.Supports(to, Extension.InBandBytestreams))
            {
                throw new NotSupportedException("The XMPP entity does not support the " +
                    "'In-Band Bytestreams' extension.");
            }
            // Send the 'open' request.
            var open = Xml.Element("open", "http://jabber.org/protocol/ibb")
                .Attr("block-size", blockSize.ToString())
                .Attr("sid", sessionId)
                .Attr("stanza", "iq");
            Iq response = im.IqRequest(IqType.Set, to, im.Jid, open);
            if (response.Type == IqType.Error)
            {
                throw Util.ExceptionFromError(response, "The in-band bytestream could " +
                    "not be opened.");
            }
        }

        /// <summary>
        /// Closes a previously established in-band bytestream with the XMPP entity
        /// with the specified JID.
        /// </summary>
        /// <param name="to">The JID of the XMPP entity to close the in-band bytestream
        /// with.</param>
        /// <param name="sessionId">The session id obtained during stream
        /// initiation.</param>
        /// <remarks>This method expects that stream initiation has been previously
        /// performed.</remarks>
        /// <exception cref="NotSupportedException">The XMPP entity with
        /// the specified JID does not support the 'In-Band Bytestreams' XMPP
        /// extension.</exception>
        private void CloseStream(Jid to, string sessionId)
        {
            siFileTransfer.InvalidateSession(sessionId);
            if (!ecapa.Supports(to, Extension.InBandBytestreams))
            {
                throw new NotSupportedException("The XMPP entity does not support the " +
                    "'In-Band Bytestreams' extension.");
            }
            // Send the 'close' request.
            var close = Xml.Element("close", "http://jabber.org/protocol/ibb")
                .Attr("sid", sessionId);
            // We don't care about the other site's response to this.
            im.IqRequestAsync(IqType.Set, to, im.Jid, close);
        }
    }
}