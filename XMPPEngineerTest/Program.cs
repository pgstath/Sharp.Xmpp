using System;
using XMPPEngineer;
using XMPPEngineer.Client;
using XMPPEngineer.Im;

namespace XMPPEngineerTest
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            // basic
            /*
            using (XmppClient client = new XmppClient("domain", "user", "password"))
            {
                client.Connect();

                Message message = new Message(new Jid("user@domain"), "Hello, World.");
                client.SendMessage(message);
            }*/

            // with stream management
            using (XmppClient clientsm = new XmppClient(
                "alchemy.local",
                "admin",
                "PASSWORD",
                "alchemy.local",
                XmppClient.AvailableExtensions.Default | XmppClient.AvailableExtensions.Ping))
			//XmppClient.AvailableExtensions.DataForms
			//XmppClient.AvailableExtensions.MessageCarbons
			/*using (XMPPEngineer.Core.XmppCore clientsm = new XMPPEngineer.Core.XmppCore(
                "alchemy.local",
                "steven",
                "test",
                "dev-lite-citym-access.westeurope.cloudapp.azure.com"))*/
			{
                clientsm.Error += (sender, e) =>
                {
                    Console.WriteLine(e.ToString());
                };
                clientsm.Message += (sender, e) =>
                {
                    Console.WriteLine(e.Message);
                };

                clientsm.RetrieveRoster = false;
                clientsm.Connect("mobile");

                clientsm.StreamManagementEnabled += (sdr, evt) =>
                {
                    // normal send
                    Message messagesm = new Message(new Jid("admin@alchemy.local"), "ok - " + DateTime.Now.ToLongTimeString());
                    clientsm.SendMessage(messagesm);

                    // xep-0033 - multicast can send to a jid that can then route to multiple users
                    /*
                    System.Collections.Generic.List<Jid> jids = new System.Collections.Generic.List<Jid>();
                    jids.Add(new Jid("admin@alchemy.local"));
                    jids.Add(new Jid("test@alchemy.local"));
                    jids.Add(new Jid("steven@alchemy.local"));

                    Message multimessagesm = new Message(new Jid("multicast.alchemy.local"), "ok5 - " + DateTime.Now.ToLongTimeString(), null, jids);
                    clientsm.SendMessage(multimessagesm);
                    */
                };

                // enable stream management and recovery mode - // xep-0198
                clientsm.EnableStreamManagement(true);

                Console.ReadKey();
            }
		}
    }
}
