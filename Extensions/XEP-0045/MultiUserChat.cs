using Sharp.Xmpp.Extensions.Dataforms;
using Sharp.Xmpp.Im;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Sharp.Xmpp.Extensions
{
    internal class MultiUserChat : XmppExtension, IInputFilter<Presence>
    {
        public override IEnumerable<string> Namespaces
        {
            get
            {
                return new[]
                {
                    "http://jabber.org/protocol/muc",
                    "http://jabber.org/protocol/muc#user",
                    "http://jabber.org/protocol/muc#admin",
                    "http://jabber.org/protocol/muc#owner",
                };
            }
        }

        public override Extension Xep
        {
            get { return Extension.MultiUserChat; }
        }

        private ServiceDiscovery disco;
        private ConcurrentDictionary<Jid, TaskCompletionSource<JoinRoomResult>> m_pendingRoomJoins = new ConcurrentDictionary<Jid, TaskCompletionSource<JoinRoomResult>>();
        private ConcurrentDictionary<Jid, TaskCompletionSource<bool>> m_pendingRoomLeaves = new ConcurrentDictionary<Jid, TaskCompletionSource<bool>>();

        public MultiUserChat(XmppIm im)
            : base(im)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            disco = im.GetExtension<ServiceDiscovery>();
        }

        public IList<Jid> GetMucServices()
        {
            List<Jid> result = new List<Jid>();
            var items = disco.GetItems(im.Jid.Domain);

            foreach (var item in items)
            {
                  var extensions = disco.GetExtensions(item.Jid);
               // if (extensions.Contains(Extension.MultiUserChat))
               // {
                  if (!result.Contains(item.Jid)) { result.Add(item.Jid); }
               // }
            }

            return result;
        }

        public IList<XmppItem> GetRooms(Jid mucService)
        {
            mucService.ThrowIfNull("mucService");

            return disco.GetItems(mucService).ToList();
        }

        public RequestForm GetRoomConfiguration(Jid mucService, string roomName)
        {
            mucService.ThrowIfNull("mucService");
            roomName.ThrowIfNull("roomName");

            var response = im.IqRequest(Core.IqType.Get, new Jid(mucService.Domain, roomName), im.Jid, Xml.Element("query", "http://jabber.org/protocol/muc#owner"));

            response.ThrowIfError();

            var queryNode = response.Data["query"];
            if (queryNode != null)
            {
                var formNode = queryNode["x"];

                if (formNode != null)
                {
                    return new RequestForm(formNode);
                }
            }

            throw new XmppException("Failed to fetch room configuration");
        }

        public void SetRoomConfiguration(Jid mucService, string roomName, SubmitForm form)
        {
            mucService.ThrowIfNull("mucService");
            roomName.ThrowIfNull("roomName");
            form.ThrowIfNull("form");

            var payload = Xml.Element("query", "http://jabber.org/protocol/muc#owner");

            if (!form.Fields.Any(f => f.Name == "FORM_TYPE"))
            {
                form.AddUntypedValue("FORM_TYPE", "http://jabber.org/protocol/muc#roomconfig");
            }

            payload.Child(form.ToXmlElement());

            var response = im.IqRequest(Core.IqType.Set, new Jid(mucService.Domain, roomName), im.Jid, payload);

            response.ThrowIfError();
        }

        public Task<JoinRoomResult> JoinRoom(Jid mucService, string roomName, string password = "")
        {
            mucService.ThrowIfNull("mucService");
            roomName.ThrowIfNull("roomName");

            const string ns = "http://jabber.org/protocol/muc";

            var tcs = new TaskCompletionSource<JoinRoomResult>();

            var roomJid = new Jid(mucService.Domain, roomName, im.Jid.Node);
            m_pendingRoomJoins[roomJid.ToString().ToLower()] = tcs;

            var payload = Xml.Element("x", ns);
            if (!string.IsNullOrEmpty(password))
            {
                payload.Child(Xml.Element("password", ns).Text(password));
            }

            im.SendPresence(new Presence(roomJid, im.Jid, PresenceType.Available, null, null, payload));

            return tcs.Task;
        }

        public bool KickUser(Jid mucService, string roomName, string userName, string reason = null)
        {
            mucService.ThrowIfNull("mucService");
            roomName.ThrowIfNullOrEmpty("roomName");
            userName.ThrowIfNullOrEmpty("userName");

            var query = Xml.Element("query", "http://jabber.org/protocol/muc#admin")
                .Child(Xml.Element("item").Attr("nick", userName).Attr("role", "none"));

            if (!string.IsNullOrEmpty(reason))
            {
                query = query.Child(Xml.Element("reason").Text(reason));
            }

            var roomJid = new Jid(mucService.Domain, roomName);
            var response = im.IqRequest(Core.IqType.Set, roomJid, im.Jid, query);

            return response.Type == Core.IqType.Result;
        }

        public Task LeaveRoom(Jid mucService, string roomName, string status = "")
        {
            mucService.ThrowIfNull("mucService");
            roomName.ThrowIfNullOrEmpty("roomName");

            const string ns = "http://jabber.org/protocol/muc";

            var tcs = new TaskCompletionSource<bool>();

            var roomJid = new Jid(mucService.Domain, roomName, im.Jid.Node);
            m_pendingRoomLeaves[roomJid.ToString().ToLower()] = tcs;

            XmlElement payload = null;
            if (!string.IsNullOrEmpty(status))
            {
                payload = Xml.Element("status", ns).Text(status);
            }

            im.SendPresence(new Presence(roomJid, im.Jid, PresenceType.Unavailable, null, null, payload));

            return tcs.Task;
        }

        public bool Input(Presence stanza)
        {
            string canonFrom = stanza.From.ToString().ToLower();

            //Handle error case
            if (stanza.Type == PresenceType.Error)
            {
                //Check if the error relates to a pending room join operation
                TaskCompletionSource<JoinRoomResult> pendingRoomJoin = null;
                if (m_pendingRoomJoins.TryGetValue(canonFrom, out pendingRoomJoin))
                {
                    pendingRoomJoin.SetException(Util.ExceptionFromError(stanza.Data["error"], "Failed to join room " + stanza.From.ToString()));
                    m_pendingRoomJoins.TryRemove(canonFrom, out pendingRoomJoin);

                    return true;
                }

                //Check if the error relates to a pending room leave operation
                TaskCompletionSource<bool> pendingRoomLeave = null;
                if (m_pendingRoomLeaves.TryGetValue(canonFrom, out pendingRoomLeave))
                {
                    pendingRoomLeave.SetException(Util.ExceptionFromError(stanza.Data["error"], "Failed to leave room " + stanza.From.ToString()));
                    m_pendingRoomLeaves.TryRemove(canonFrom, out pendingRoomLeave);

                    return true;
                }
            }

            //Handle success case
            var x = stanza.Data["x"];
            if (x != null && x.NamespaceURI == "http://jabber.org/protocol/muc#user")
            {
                var itemNode = x["item"];
                if (itemNode != null)
                {
                    //See if the result relates to a pending room join operation
                    TaskCompletionSource<JoinRoomResult> pendingRoomJoin = null;
                    if (m_pendingRoomJoins.TryGetValue(canonFrom, out pendingRoomJoin))
                    {
                        //Parse room affiliation and role
                        RoomAffiliation affiliation = RoomAffiliation.None;
                        if (itemNode.HasAttribute("affiliation"))
                        {
                            string a = itemNode.GetAttribute("affiliation");
                            Enum.TryParse<RoomAffiliation>(a, true, out affiliation);
                        }

                        RoomRole role = RoomRole.None;
                        if (itemNode.HasAttribute("role"))
                        {
                            Enum.TryParse<RoomRole>(itemNode.GetAttribute("role"), true, out role);
                        }

                        var result = new JoinRoomResult()
                        {
                            Affiliation = affiliation,
                            Role = role
                        };

                        Exception createException = null;
                        //For rooms that don't exist, the server will create a new room and respond with a role of "owner"
                        if (affiliation == RoomAffiliation.Owner)
                        {
                            //Server should respond with status code 201 to indicate room created. Search for it...
                            //N.B. Prosody doesn't seem to include this node and just auto creates a default room
                            bool created = false;
                            foreach (XmlNode node in x.ChildNodes)
                            {
                                if (node.Name == "status")
                                {
                                    var code = node.Attributes["code"];
                                    if (code != null && code.InnerText == "201")
                                    {
                                        created = true;
                                        break;
                                    }
                                }
                            }

                            if (created)
                            {
                                //If the room was created, the server expects confirmation of room settings
                                //Send off a request to accept default instant room settings
                                Jid roomJid = new Jid(stanza.From.Domain, stanza.From.Node);
                                var response = im.IqRequest(Core.IqType.Set, roomJid, im.Jid, Xml.Element("query", "http://jabber.org/protocol/muc#owner").Child(Xml.Element("x", "jabber:x:data").Attr("type", "submit")));
                                if (response.Type == Core.IqType.Error)
                                {
                                    createException = Util.ExceptionFromError(response, "Failed to join room " + roomJid);
                                }
                            }
                        }

                        if (createException != null)
                        {
                            pendingRoomJoin.SetException(createException);
                        }
                        else
                        {
                            pendingRoomJoin.SetResult(result);
                        }

                        m_pendingRoomJoins.TryRemove(canonFrom, out pendingRoomJoin);

                        return true;
                    }

                    //See if the result relates to a pending room leave operation
                    TaskCompletionSource<bool> pendingRoomLeave = null;
                    if (m_pendingRoomLeaves.TryGetValue(canonFrom, out pendingRoomLeave))
                    {
                        pendingRoomLeave.SetResult(true);

                        m_pendingRoomLeaves.TryRemove(canonFrom, out pendingRoomLeave);

                        return true;
                    }
                }
            }

            return false;
        }
    }
}