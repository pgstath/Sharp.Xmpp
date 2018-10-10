using System;
using System.Xml;

namespace XMPPEngineer.Extensions.Dataforms
{
    /// <summary>
    /// Represents a data-form that signals cancellation of submission of data.
    /// </summary>
    internal class CancelForm : DataForm
    {
        /// <summary>
        /// Initializes a new instance of the CancelForm class.
        /// </summary>
        /// <remarks>This creates a read-only data-form to which no fields must
        /// be added.</remarks>
        public CancelForm()
            : base(null, null, true)
        {
            Type = DataFormType.Cancel;
        }

        /// <summary>
        /// Initializes a new instance of the CancelForm class from the specified
        /// XML element.
        /// </summary>
        /// <param name="element">The XML 'field' element to initialize the instance
        /// with.</param>
        /// <exception cref="ArgumentNullException">The element parameter is
        /// null.</exception>
        /// <exception cref="ArgumentException">The specified XML element is not a
        /// valid data-form element of type 'cancel'.</exception>
        public CancelForm(XmlElement element)
            : base(element)
        {
            AssertType(DataFormType.Cancel);
        }
    }
}