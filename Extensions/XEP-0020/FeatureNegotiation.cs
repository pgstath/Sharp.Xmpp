using XMPPEngineer.Extensions.Dataforms;
using XMPPEngineer.Im;
using System;
using System.Collections.Generic;
using System.Xml;

namespace XMPPEngineer.Extensions
{
    /// <summary>
    /// Implements the 'Feature Negotiation' extension as defined in XEP-0020.
    /// </summary>
    internal class FeatureNegotiation : XmppExtension
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
                return new string[] { "http://jabber.org/protocol/feature-neg" };
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
                return Extension.FeatureNegotiation;
            }
        }

        /// <summary>
        /// Creates an offer or result from the specified data-form.
        /// </summary>
        /// <param name="form">The data-form to include in the feature negotiation
        /// offer or result.</param>
        /// <returns>An XML element representing the feature negotiation
        /// offer or result.</returns>
        /// <exception cref="ArgumentNullException">The form parameter is
        /// null.</exception>
        public static XmlElement Create(DataForm form)
        {
            form.ThrowIfNull("form");
            return Xml.Element("feature",
                "http://jabber.org/protocol/feature-neg").Child(form.ToXmlElement());
        }

        /// <summary>
        /// Parses the the specified negotiation offer or result.
        /// </summary>
        /// <param name="feature">The 'feature' element containing the data-form</param>
        /// <returns>An initialized data-form instance.</returns>
        /// <exception cref="ArgumentNullException">The feature parameter is
        /// null.</exception>
        /// <exception cref="ArgumentException">The feature parameter is not a
        /// valid 'feature' XML element, or the feature element contains invalid
        /// data.</exception>
        public static DataForm Parse(XmlElement feature)
        {
            feature.ThrowIfNull("feature");
            if (feature.Name != "feature" || feature.NamespaceURI !=
                "http://jabber.org/protocol/feature-neg" || feature["x"] == null)
            {
                throw new ArgumentException("Invalid XML 'feature' element.");
            }
            return DataFormFactory.Create(feature["x"]);
        }

        /// <summary>
        /// Initializes a new instance of the FeatureNegotiation class.
        /// </summary>
        /// <param name="im">A reference to the XmppIm instance on whose behalf this
        /// instance is created.</param>
        public FeatureNegotiation(XmppIm im)
            : base(im)
        {
        }
    }
}