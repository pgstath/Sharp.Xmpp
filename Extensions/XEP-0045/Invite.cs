using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using XMPPEngineer.Core;

namespace XMPPEngineer.Extensions
{
    /// <summary>
    /// Implements MUC Mediated Invitation as described in XEP-0045.
    /// </summary>
    public class Invite : Message
    {
        private const string rootTag = "message",
            xTag = "x",
            inviteTag = "invite",
            reasonTag = "reason",
            passwordTag = "password",
            toAttribute = "to",
            fromAttribute = "from";

        /// <summary>
        /// Initialises a group chat invite.
        /// </summary>
        /// <param name="to">User you intend to invite to chat room.</param>
        /// /// <param name="from">User sending the invitation.</param>
        /// <param name="reason">Message included with the invitation.</param>
        /// <param name="room">Jid of the chat room.</param>
        /// <param name="password">(optional) Password.</param>
        public Invite(Jid to, Jid from, Jid room, string reason, string password = null)
            : base(room, from, Xml.Element(xTag, MucNs.NsUser))
        {
            XElement.Child(Xml.Element(inviteTag).Child(Xml.Element(reasonTag)));
            SendTo = to;
            Reason = reason;
            Password = password;
        }

        internal Invite(Core.Message message)
            : base(message.Data)
        {
        }

        /// <summary>
        /// JID of the user the invite is intended to be send to.
        /// </summary>
        public Jid SendTo
        {
            get
            {
                XmlElement node = InviteElement;
                string v = node == null ? null : node.GetAttribute(toAttribute);

                return String.IsNullOrEmpty(v) ? null : new Jid(v);
            }

            set
            {
                if (value == null)
                    InviteElement.RemoveAttribute(toAttribute);
                else
                    InviteElement.SetAttribute(toAttribute, value.ToString());
            }
        }

        /// <summary>
        /// JID of the user the invite has been sent from.
        /// </summary>
        public Jid ReceivedFrom
        {
            get
            {
                XmlElement node = InviteElement;
                string v = node == null ? null : node.GetAttribute(fromAttribute);

                return String.IsNullOrEmpty(v) ? null : new Jid(v);
            }

            private set
            {
                if (value == null)
                    InviteElement.RemoveAttribute(fromAttribute);
                else
                    InviteElement.SetAttribute(fromAttribute, value.ToString());
            }
        }

        /// <summary>
        /// Custom message that may be sent with the invitation.
        /// </summary>
        public string Reason
        {
            get
            {
                XmlElement invite = ReasonElement;
                return invite == null ? null : invite.InnerText;
            }

            set
            {
                if (!string.IsNullOrEmpty(value))
                    ReasonElement.Text(value);
            }
        }

        /// <summary>
        /// (Optional) password of the chat room in the invitation.
        /// </summary>
        public string Password
        {
            get
            {
                XmlElement node = PasswordElement;
                return node == null ? null : node.InnerText;
            }

            set
            {
                XmlElement node = PasswordElement;

                if(node != null)
                    XElement.RemoveChild(node);

                if (!string.IsNullOrEmpty(value))
                    XElement.Child(Xml.Element(passwordTag).Text(value));
            }
        }

        /// <summary>
        /// The tag name of the stanza's root element
        /// </summary>
        protected override string RootElementName { get { return rootTag; } }

        private XmlElement XElement { get { return element[xTag]; } }

        private XmlElement InviteElement { get { return GetNode(xTag, inviteTag); } }

        private XmlElement ReasonElement { get { return GetNode(xTag, inviteTag, reasonTag); } }

        private XmlElement PasswordElement { get { return GetNode(xTag, passwordTag); } }

        internal static bool IsElement(Core.Message message)
        {
            Invite temp = new Invite(message);
            return temp?.XElement?.NamespaceURI == MucNs.NsUser && temp?.InviteElement != null;
        }
    }
}
