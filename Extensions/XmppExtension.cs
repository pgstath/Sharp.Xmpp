using XMPPEngineer.Im;
using System.Collections.Generic;

namespace XMPPEngineer.Extensions
{
    /// <summary>
    /// The base class from which all implementations of XMPP extensions must
    /// derive.
    /// </summary>
    internal abstract class XmppExtension
    {
        /// <summary>
        /// A reference to the instance of the XmppIm class.
        /// </summary>
        protected XmppIm im;

        /// <summary>
        /// An enumerable collection of XMPP namespaces the extension implements.
        /// </summary>
        /// <remarks>This is used for compiling the list of supported extensions
        /// advertised by the 'Service Discovery' extension.</remarks>
        public abstract IEnumerable<string> Namespaces
        {
            get;
        }

        /// <summary>
        /// The named constant of the Extension enumeration that corresponds to this
        /// extension.
        /// </summary>
        public abstract Extension Xep
        {
            get;
        }

        /// <summary>
        /// Initializes a new instance of the XmppExtension class.
        /// </summary>
        /// <param name="im">A reference to the XmppIm instance on whose behalf the
        /// extension is being created.</param>
        public XmppExtension(XmppIm im)
        {
            im.ThrowIfNull("im");
            this.im = im;
        }

        /// <summary>
        /// Invoked after all extensions have been loaded.
        /// </summary>
        public virtual void Initialize()
        {
        }
    }
}