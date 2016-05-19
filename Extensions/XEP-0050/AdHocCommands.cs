using Sharp.Xmpp.Core;
using Sharp.Xmpp.Im;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Sharp.Xmpp.Extensions
{
    internal class AdHocCommands : XmppExtension
    {
        public AdHocCommands(XmppIm im)
            : base(im)
        {
        }

        public override void Initialize()
        {
        }

        public override IEnumerable<string> Namespaces
        {
            get
            {
                return new string[0]; // todo:
            }
        }

        public override Extension Xep
        {
            get
            {
                return Extension.AdHocCommands;
            }
        }

        public List<AdHocCommand> GetAdHocCommands()
        {
            var query = Xml.Element("query", "http://jabber.org/protocol/disco#items").Attr("node", "http://jabber.org/protocol/commands");
            var response = im.IqRequest(IqType.Get, im.Hostname, im.Jid, query);
            var commands = response.Data["query"].GetElementsByTagName("item").Cast<XmlElement>().Select(e => new AdHocCommand(e)).ToList();
            return commands;
        }
    }
}
