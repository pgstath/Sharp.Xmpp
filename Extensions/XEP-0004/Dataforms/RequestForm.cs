using System;
using System.Xml;

namespace XMPPEngineer.Extensions.Dataforms
{
    /// <summary>
    /// Represents a requesting data-form.
    /// </summary>
    /// <remarks>The form-processing entity uses this to ask the form-submitting
    /// entity to provide data.</remarks>
    public class RequestForm : DataForm
    {
        /// <summary>
        /// Initializes a new instance of the RequestForm class.
        /// </summary>
        /// <param name="title">The title of the data-form.</param>
        /// <param name="instructions">The natural-language instructions to be
        /// followed by the form-submitting entity.</param>
        /// <param name="fields">One or several data-fields to add to the
        /// form.</param>
        public RequestForm(string title = null, string instructions = null,
            params DataField[] fields)
            : base(title, instructions, false, fields)
        {
            Type = DataFormType.Form;
        }

        /// <summary>
        /// Initializes a new instance of the RequestForm class from the specified
        /// XML element.
        /// </summary>
        /// <param name="element">The XML 'field' element to initialize the instance
        /// with.</param>
        /// <exception cref="ArgumentNullException">The element parameter is
        /// null.</exception>
        /// <exception cref="ArgumentException">The specified XML element is not a
        /// valid data-form element of type 'form'.</exception>
        public RequestForm(XmlElement element)
            : base(element)
        {
            AssertType(DataFormType.Form);
        }
    }
}