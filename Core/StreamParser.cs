using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Sharp.Xmpp.Core
{
    /// <summary>
    /// Implements a parser for parsing XMPP XML-streams as defined per XMPP:Core
    /// Section 4 ('XML Streams').
    /// </summary>
    internal class StreamParser : IDisposable
    {
        /// <summary>
        /// The reader that provides the fast-forward access to the XML stream.
        /// </summary>
        private XmlReader reader;

        /// <summary>
        /// If true, the stream is not closed when the StreamParser instance is
        /// disposed of.
        /// </summary>
        private bool leaveOpen;

        /// <summary>
        /// The stream on which the reader operates.
        /// </summary>
        private Stream stream;

        /// <summary>
        /// The default language of any human-readable XML character send over
        /// that stream.
        /// </summary>
        public CultureInfo Language
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the StreamParser class for the specified
        /// stream.
        /// </summary>
        /// <param name="stream">The stream to read the XML data from.</param>
        /// <param name="leaveOpen">true to leave the stream open when the StreamParser
        /// instance is closed, otherwise false.</param>
        /// <exception cref="ArgumentNullException">The stream parameter is
        /// null.</exception>
        /// <exception cref="XmlException">The parser has encountered invalid
        /// or unexpected XML data.</exception>
        /// <exception cref="CultureNotFoundException">The culture specified by the
        /// XML-stream in it's 'xml:lang' attribute could not be found.</exception>
        public StreamParser(Stream stream, bool leaveOpen = false)
        {
            
            stream.ThrowIfNull("stream");
            this.leaveOpen = leaveOpen;
            this.stream = stream;
            reader = XmlReader.Create(stream, new XmlReaderSettings()
            {
                // Ignore restricted XML data (Refer to RFC 3920, 11.1 Restrictions).
                IgnoreProcessingInstructions = true,
                IgnoreComments = true,
                IgnoreWhitespace = true
            });
            // Read up to the opening stream tag.
            ReadRootElement();
        }

        /// <summary>
        /// Reads the next XML element from the input stream.
        /// </summary>
        /// <param name="expected">A list of element names, that are expected. If
        /// provided, and the read element does not match any of the provided names,
        /// an XmlException is thrown.</param>
        /// <returns>The XML element read from the stream.</returns>
        /// <exception cref="XmlException">The input stream contains invalid XML, or
        /// the read element is not an XML node of type XmlElement, or the read element
        /// is not a start element, or the read element is not one of the expected
        /// elements.</exception>
        /// <exception cref="IOException">An unrecoverable stream error condition
        /// has been encountered and the server has closed the connection.</exception>
        public XmlElement NextElement(params string[] expected)
        {
            // Advance reader to next node.
            reader.Read();
            if (reader.NodeType == XmlNodeType.EndElement && reader.Name ==
                "stream:stream")
                throw new IOException("The server has closed the XML stream.");
            if (reader.NodeType != XmlNodeType.Element)
                throw new XmlException("Unexpected node: '" + reader.Name +
                    "' of type " + reader.NodeType);
            if (!reader.IsStartElement())
                throw new XmlException("Not a start element: " + reader.Name);
            // We can't use the ReadOuterXml method of reader directly as it places
            // the cursor on the next element which may result in a blocking read
            // on the underlying network stream.
            using (XmlReader inner = reader.ReadSubtree())
            {
                inner.Read();
                string xml = inner.ReadOuterXml();
                XmlDocument doc = new XmlDocument();
                using (var sr = new StringReader(xml))
                using (var xtr = new XmlTextReader(sr))
                    doc.Load(xtr);
                XmlElement elem = (XmlElement)doc.FirstChild;
                // Handle unrecoverable stream errors.
                if (elem.Name == "stream:error")
                {
                    string condition = elem.FirstChild != null ?
                        elem.FirstChild.Name : "undefined";
                    //throw new IOException("Unrecoverable stream error: " + condition);
                    //This indicates a disconnection event
                    throw new XmppDisconnectionException("Unrecoverable stream error: " + condition);
                }
                if (expected.Length > 0 && !expected.Contains(elem.Name))
                    throw new XmlException("Unexpected XML element: " + elem.Name);
                return elem;
            }
        }

        /// <summary>
        /// Closes the stream parser.
        /// </summary>
        public void Close()
        {
            Dispose();
        }

        /// <summary>
        /// Releases all resources used by the current instance of the XmppParser class.
        /// </summary>
        public void Dispose()
        {
            reader.Close();
            if (!leaveOpen)
                stream.Close();
        }

        /// <summary>
        /// Reads the XML stream up to the 'stream:stream' opening tag.
        /// </summary>
        /// <exception cref="XmlException">The parser has encountered invalid
        /// or unexpected XML data.</exception>
        /// <exception cref="CultureNotFoundException">The culture specified by the
        /// XML-stream in it's 'xml:lang' attribute could not be found.</exception>
        private void ReadRootElement()
        {
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    // Skip optional XML declaration.
                    case XmlNodeType.XmlDeclaration:
                        break;

                    case XmlNodeType.Element:
                        if (reader.Name == "stream:stream")
                        {
                            // Remember the default language communicated by the server.
                            string lang = reader.GetAttribute("xml:lang");
                            if (!String.IsNullOrEmpty(lang))
                                Language = new CultureInfo(lang);
                            return;
                        }
                        throw new XmlException("Unexpected document root: " + reader.Name);
                    default:
                        throw new XmlException("Unexpected node: " + reader.Name);
                }
            }
        }
    }
}