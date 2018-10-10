using System;
using System.Collections.Generic;
using System.Xml;

namespace XMPPEngineer.Extensions.Dataforms
{
    /// <summary>
    /// Represents a result form containing returned data or a generic data set.
    /// </summary>
    internal class ResultForm : DataForm
    {
        /// <summary>
        /// Gets an enumerable collection of fields contained in the table header of
        /// the data set.
        /// </summary>
        /// <remarks>The table header describes the data to follow. Refer to XEP-0004
        /// "3.4 Multiple Items in Form Results" for more details.</remarks>
        public IEnumerable<DataField> Header
        {
            get
            {
                var e = element["reported"];
                if (e == null)
                    return new List<DataField>();
                return new FieldList(e);
            }
        }

        /// <summary>
        /// Gets an enumerable list of table cells of the data set.
        /// </summary>
        /// <remarks>The table cells contain the data described in the table header.
        /// Refer to XEP-0004 "3.4 Multiple Items in Form Results" for more
        /// details.</remarks>
        public IList<IEnumerable<DataField>> Cells
        {
            get
            {
                var list = new List<IEnumerable<DataField>>();
                foreach (XmlElement item in element.GetElementsByTagName("item"))
                {
                    list.Add(new FieldList(item));
                }
                return list;
            }
        }

        /// <summary>
        /// Initializes a new instance of the ResultForm class.
        /// </summary>
        /// <param name="title">The title of the data-form.</param>
        /// <param name="instructions">The natural-language instructions to be
        /// followed by the form-submitting entity.</param>
        /// <param name="header">An enumerable collection of fields to add to the
        /// table header of the result form.</param>
        /// <param name="cells">enumerable list of table cells to add to the
        /// data set.</param>
        public ResultForm(string title = null, string instructions = null,
            IEnumerable<DataField> header = null,
            IList<IEnumerable<DataField>> cells = null)
            : base(title, instructions, false)
        {
            Type = DataFormType.Result;
            if (header != null)
            {
                var reported = Xml.Element("reported");
                foreach (var field in header)
                    reported.Child(field.ToXmlElement());
                element.Child(reported);
            }
            if (cells != null)
            {
                foreach (var cell in cells)
                {
                    var item = Xml.Element("item");
                    foreach (var field in cell)
                        item.Child(field.ToXmlElement());
                    element.Child(item);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the ResultForm class from the specified
        /// XML element.
        /// </summary>
        /// <param name="element">The XML 'field' element to initialize the instance
        /// with.</param>
        /// <exception cref="ArgumentNullException">The element parameter is
        /// null.</exception>
        /// <exception cref="ArgumentException">The specified XML element is not a
        /// valid data-form element of type 'result'.</exception>
        public ResultForm(XmlElement element)
            : base(element)
        {
            AssertType(DataFormType.Result);
        }
    }
}