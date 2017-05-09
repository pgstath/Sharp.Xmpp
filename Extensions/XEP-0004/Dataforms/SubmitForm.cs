using System;
using System.Xml;

namespace XMPPEngineer.Extensions.Dataforms
{
    /// <summary>
    /// Represents a data-form for data submission.
    /// </summary>
    /// <remarks>The form-submitting entity uses this to submit data to the
    /// form-processing entity.</remarks>
    public class SubmitForm : DataForm
    {
        /// <summary>
        /// Initializes a new instance of the SubmitForm class.
        /// </summary>
        /// <param name="fields">One or several data-fields to add to the
        /// form.</param>
        public SubmitForm(params DataField[] fields)
            : base(null, null, false, fields)
        {
            Type = DataFormType.Submit;
        }

        /// <summary>
        /// Initializes a new instance of the SubmitForm class from the specified
        /// XML element.
        /// </summary>
        /// <param name="element">The XML 'field' element to initialize the instance
        /// with.</param>
        /// <exception cref="ArgumentNullException">The element parameter is
        /// null.</exception>
        /// <exception cref="ArgumentException">The specified XML element is not a
        /// valid data-form element of type 'submit'.</exception>
        internal SubmitForm(XmlElement element)
            : base(element)
        {
            AssertType(DataFormType.Submit);
        }
    }
}