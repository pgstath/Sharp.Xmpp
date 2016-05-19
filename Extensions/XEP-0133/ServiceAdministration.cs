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
    internal class ServiceAdministration : XmppExtension
    {
        public ServiceAdministration(XmppIm im)
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
                return new string[0];
            }
        }

        public override Extension Xep
        {
            get
            {
                return Extension.ServiceAdministration;
            }
        }

        public void AddUser(Jid userId, string password, string verifiedPassword, string email, string firstName, string lastName)
        {
            // validate
            userId.ThrowIfNull("user ID");
            userId.Node.ThrowIfNullOrEmpty("user node");
            userId.Domain.ThrowIfNullOrEmpty("user domain");
            password.ThrowIfNullOrEmpty("password");
            email.ThrowIfNullOrEmpty("email");
            firstName.ThrowIfNullOrEmpty("first name");
            lastName.ThrowIfNullOrEmpty("last name");

            if (password != verifiedPassword)
            {
                throw new Exception("passwords do not match");
            }

            // declare namespaces
            XNamespace commandNamespace = "http://jabber.org/protocol/commands";
            XNamespace xNamespace = "jabber:x:data";

            // request to add a user
            var command1 = new XElement(commandNamespace + "command",
                new XAttribute("action", "execute"),
                new XAttribute("node", "http://jabber.org/protocol/admin#add-user"));

            var response1 = im.IqRequest(IqType.Set, im.Hostname, im.Jid, command1.ToXmlElement());
            response1.ThrowIfError();

            // add user
            var sessionId = response1.Data["command"].Attributes["sessionid"].Value;
            var command2 = new XElement(commandNamespace + "command",
                new XAttribute("node", "http://jabber.org/protocol/admin#add-user"),
                new XAttribute("sessionid", sessionId),
                new XElement(xNamespace + "x",
                    new XAttribute("type", "submit"),
                    new XElement(xNamespace + "field",
                        new XAttribute("type", "hidden"),
                        new XAttribute("var", "FORM_TYPE"),
                        new XElement(xNamespace + "value", "http://jabber.org/protocol/admin")),
                    new XElement(xNamespace + "field",
                        new XAttribute("var", "accountjid"),
                        new XElement(xNamespace + "value", userId)),
                    new XElement(xNamespace + "field",
                        new XAttribute("var", "password"),
                        new XElement(xNamespace + "value", password)),
                    new XElement(xNamespace + "field",
                        new XAttribute("var", "password-verify"),
                        new XElement(xNamespace + "value", verifiedPassword)),
                    new XElement(xNamespace + "field",
                        new XAttribute("var", "email"),
                        new XElement(xNamespace + "value", email)),
                    new XElement(xNamespace + "field",
                        new XAttribute("var", "given_name"),
                        new XElement(xNamespace + "value", firstName)),
                    new XElement(xNamespace + "field",
                        new XAttribute("var", "surname"),
                        new XElement(xNamespace + "value", lastName))));

            var response2 = im.IqRequest(IqType.Set, im.Hostname, im.Jid, command2.ToXmlElement());
            response2.ThrowIfError();
        }

        public void DeleteUser(Jid userId)
        {
            // declare namespaces
            XNamespace commandNamespace = "http://jabber.org/protocol/commands";
            XNamespace xNamespace = "jabber:x:data";

            // request to delete a user
            var command1 = new XElement(commandNamespace + "command",
                new XAttribute("action", "execute"),
                new XAttribute("node", "http://jabber.org/protocol/admin#delete-user"));

            var response1 = im.IqRequest(IqType.Set, im.Hostname, im.Jid, command1.ToXmlElement());
            response1.ThrowIfError();

            // delete user
            var sessionId = response1.Data["command"].Attributes["sessionid"].Value;
            var command2 = new XElement(commandNamespace + "command",
                new XAttribute("node", "http://jabber.org/protocol/admin#delete-user"),
                new XAttribute("sessionid", sessionId),
                new XElement(xNamespace + "x",
                    new XAttribute("type", "submit"),
                    new XElement(xNamespace + "field",
                        new XAttribute("type", "hidden"),
                        new XAttribute("var", "FORM_TYPE"),
                        new XElement(xNamespace + "value", "http://jabber.org/protocol/admin")),
                    new XElement(xNamespace + "field",
                        new XAttribute("var", "accountjids"),
                        new XElement(xNamespace + "value", userId))));

            var response2 = im.IqRequest(IqType.Set, im.Hostname, im.Jid, command2.ToXmlElement());
            response2.ThrowIfError();

            // validate response
            var noteElement = response2.ToXDocument().Root.Element(commandNamespace + "command").Element(commandNamespace + "note");
            var noteText = noteElement == null ? null : noteElement.Value;
            if (noteText != null)
            {
                // todo: this is weak but no other way to identify
                if (noteText.StartsWith("The following accounts could not be deleted:"))
                {
                    throw new Exception("The user could not be deleted");
                }
            }
        }

        public void EnableUser(Jid userId)
        {
            throw new NotImplementedException();
        }

        public void DisableUser(Jid userId)
        {
            throw new NotImplementedException();

            //// declare namespaces
            //XNamespace commandNamespace = "http://jabber.org/protocol/commands";
            //XNamespace xNamespace = "jabber:x:data";

            //// request to disable a user
            //var command1 = new XElement(commandNamespace + "command",
            //    new XAttribute("action", "execute"),
            //    new XAttribute("node", "http://jabber.org/protocol/admin#disable-user"));

            //var response1 = IM.IqRequest(IqType.Set, IM.Hostname, IM.Jid, command1.ToXmlElement());
            //response1.ThrowIfError();

            //// disable user
            //var sessionId = response1.Data["command"].Attributes["sessionid"].Value;
            //var command2 = new XElement(commandNamespace + "command",
            //    new XAttribute("node", "http://jabber.org/protocol/admin#disable-user"),
            //    new XAttribute("sessionid", sessionId),
            //    new XElement(xNamespace + "x",
            //        new XAttribute("type", "submit"),
            //        new XElement(xNamespace + "field",
            //            new XAttribute("type", "hidden"),
            //            new XAttribute("var", "FORM_TYPE"),
            //            new XElement(xNamespace + "value", "http://jabber.org/protocol/admin")),
            //        new XElement(xNamespace + "field",
            //            new XAttribute("var", "accountjids"),
            //            new XElement(xNamespace + "value", userId))));

            //var response2 = IM.IqRequest(IqType.Set, IM.Hostname, IM.Jid, command2.ToXmlElement());
            //response2.ThrowIfError();
        }
    }
}