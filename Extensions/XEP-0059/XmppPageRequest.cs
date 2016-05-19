using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Sharp.Xmpp.Extensions
{
    /// <summary>
    /// Represents the parameters necessary for requesting a page of items from a collection, as specified in XEP-0059
    /// </summary>
    public class XmppPageRequest
    {
        private const string xmlns = "http://jabber.org/protocol/rsm";

        /// <summary>
        /// Request items that immediately follow the given 'After' id
        /// </summary>
        public string After { get; set; }

        /// <summary>
        /// Request items that immediately precede the given 'Before' id
        /// </summary>
        public string Before { get; set; }

        /// <summary>
        /// The maximum number of items to return
        /// </summary>
        public int FetchCount { get; private set; }

        /// <summary>
        /// Create a page request
        /// </summary>
        public XmppPageRequest(int fetchCount)
        {
            FetchCount = fetchCount;
        }

        /// <summary>
        /// Create an XML element that represents this request
        /// </summary>
        public XmlElement ToXmlElement()
        {
            var setNode = Xml.Element("set", xmlns);
            var maxNode = Xml.Element("max", xmlns).Text(FetchCount.ToString());

            setNode.Child(maxNode);

            if (After != null)
            {
                var afterNode = Xml.Element("after", xmlns);

                if (After.Length > 0)
                {
                    afterNode.Text(After);
                }

                setNode.Child(afterNode);
            }

            if (Before != null)
            {
                var beforeNode = Xml.Element("before", xmlns);

                if (Before.Length > 0)
                {
                    beforeNode.Text(Before);
                }

                setNode.Child(beforeNode);
            }

            return setNode;
        }
    }
}