using XMPPEngineer.Im;
using System.Collections.Generic;

namespace XMPPEngineer.Extensions
{
    /// <summary>
    /// Implements the 'Data Forms' extension as defined in XEP-0004.
    /// </summary>
    internal class DataForms : XmppExtension
    {
        /// <summary>
        /// An enumerable collection of XMPP namespaces the extension implements.
        /// </summary>
        /// <remarks>This is used for compiling the list of supported extensions
        /// advertised by the 'Service Discovery' extension.</remarks>
        public override IEnumerable<string> Namespaces
        {
            get
            {
                return new string[] { "jabber:x:data" };
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
                return Extension.DataForms;
            }
        }

        /// <summary>
        /// Initializes a new instance of the DataForms class.
        /// </summary>
        /// <param name="im">A reference to the XmppIm instance on whose behalf this
        /// instance is created.</param>
        public DataForms(XmppIm im)
            : base(im)
        {
        }
    }
}