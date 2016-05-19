using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Sharp.Xmpp.Extensions
{
    /// <summary>
    /// A command that an XMPP server supports
    /// </summary>
    public class AdHocCommand
    {
        /// <summary>
        /// The Jabber ID of the user who can call this command
        /// </summary>
        public Jid Id { get; set; }

        /// <summary>
        /// The friendly name of the command
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A value that is used to identify the command
        /// </summary>
        public string Node { get; set; }

        /// <summary>
        /// The default constructor
        /// </summary>
        public AdHocCommand()
        {
        }

        /// <summary>
        /// A constructor that maps the XML returned from the XMPP server after discovering what commands it supports
        /// </summary>
        /// <param name="element"></param>
        public AdHocCommand(XmlElement element)
        {
            Id = new Jid(element.Attributes["jid"].Value);
            Name = element.Attributes["name"].Value;
            Node = element.Attributes["node"].Value;
        }
    }
}
