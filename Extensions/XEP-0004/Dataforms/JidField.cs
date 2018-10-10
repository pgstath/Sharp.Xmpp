using System;
using System.Xml;

namespace XMPPEngineer.Extensions.Dataforms
{
    /// <summary>
    /// Represents a field for gathering or providing a single Jabber ID.
    /// </summary>
    /// <remarks>
    /// This corresponds to a Winforms TextField control with the added requirement
    /// that the entered text be a valid JID.
    /// </remarks>
    public class JidField : DataField
    {
        /// <summary>
        /// The gathered or provided JID.
        /// </summary>
        /// <exception cref="XmlException">The value of the underlying XML element
        /// is not a valid JID.</exception>
        public Jid Jid
        {
            get
            {
                return GetJid();
            }

            private set
            {
                if (element["value"] != null)
                {
                    if (value == null)
                        element.RemoveChild(element["value"]);
                    else
                        element["value"].InnerText = value.ToString();
                }
                else
                {
                    if (value != null)
                        element.Child(Xml.Element("value").Text(value.ToString()));
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the JidField class for use in a
        /// requesting dataform.
        /// </summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="required">Determines whether the field is required or
        /// optional.</param>
        /// <param name="label">A human-readable name for the field.</param>
        /// <param name="description">A natural-language description of the field,
        /// intended for presentation in a user-agent.</param>
        /// <param name="jid">The default value of the field.</param>
        /// <exception cref="ArgumentNullException">The name parameter is
        /// null.</exception>
        public JidField(string name, bool required = false, string label = null,
            string description = null, Jid jid = null)
            : base(DataFieldType.JidSingle, name, required, label, description)
        {
            name.ThrowIfNull("name");
            Jid = jid;
        }

        /// <summary>
        /// Initializes a new instance of the JidField class for use in a
        /// submitting dataform.
        /// </summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="jid">The value of the field.</param>
        /// <exception cref="ArgumentNullException">The name parameter is
        /// null.</exception>
        public JidField(string name, Jid jid)
            : this(name, false, null, null, jid)
        {
        }

        /// <summary>
        /// Initializes a new instance of the JidField class from the specified
        /// XML element.
        /// </summary>
        /// <param name="element">The XML 'field' element to initialize the instance
        /// with.</param>
        /// <exception cref="ArgumentNullException">The element parameter is
        /// null.</exception>
        /// <exception cref="ArgumentException">The specified XML element is not a
        /// valid data-field element, or the element is not a data-field of type
        /// 'jid-single'.</exception>
        internal JidField(XmlElement element)
            : base(element)
        {
            AssertType(DataFieldType.JidSingle);
            // Assert the provided JID is valid.
            try
            {
                GetJid();
            }
            catch (Exception e)
            {
                throw new ArgumentException("The value is not a valid JID.", e);
            }
        }

        /// <summary>
        /// Retrieves the gathered or provided JID.
        /// </summary>
        /// <returns>The gathered or provided JID.</returns>
        /// <exception cref="XmlException">The value of the underlying XML element
        /// is not a valid JID.</exception>
        private Jid GetJid()
        {
            XmlElement v = element["value"];
            try
            {
                return v != null ? new Jid(v.InnerText) : null;
            }
            catch (Exception e)
            {
                throw new XmlException("Invalid value for JidField.", e);
            }
        }
    }
}