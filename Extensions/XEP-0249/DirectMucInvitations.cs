using Sharp.Xmpp.Core;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Sharp.Xmpp.Extensions
{
    internal class DirectMucInvitations : XmppExtension, IInputFilter<Sharp.Xmpp.Im.Message>
    {
        public event EventHandler<DirectMucInvitation> DirectMucInvitationReceived;

        private const string xmlns = "jabber:x:conference";

        public override IEnumerable<string> Namespaces
        {
            get { return new string[] { xmlns }; }
        }

        public override Extension Xep
        {
            get { return Extension.DirectMucInvitations; }
        }

        public DirectMucInvitations(Sharp.Xmpp.Im.XmppIm im)
            : base(im)
        {
        }

        public void InviteUserToMuc(Jid mucService, string roomName, Jid userId, string reason = null, string password = null)
        {
            mucService.ThrowIfNull();
            roomName.ThrowIfNull();
            userId.ThrowIfNull();

            XmlElement inviteNode = Xml.Element("x", xmlns).Attr("jid", roomName + "@" + mucService.ToString());

            if (!string.IsNullOrEmpty(reason))
            {
                inviteNode.Attr("reason", reason);
            }

            if (!string.IsNullOrEmpty(password))
            {
                inviteNode.Attr("password", password);
            }

            im.SendMessage(new Im.Message(new Message(userId, im.Jid, inviteNode)));
        }

        public bool Input(Sharp.Xmpp.Im.Message stanza)
        {
            var xNode = stanza.Data["x"];
            if (xNode != null && xNode.NamespaceURI == xmlns)
            {
                if (DirectMucInvitationReceived != null)
                {
                    string jid = xNode.GetAttribute("jid");
                    string reason = xNode.GetAttribute("reason");
                    string password = xNode.GetAttribute("password");

                    DirectMucInvitationReceived(im, new DirectMucInvitation()
                    {
                        From = stanza.From,
                        Password = password,
                        Reason = reason,
                        RoomJid = new Jid(jid)
                    });
                }

                return true;
            }

            return false;
        }
    }
}