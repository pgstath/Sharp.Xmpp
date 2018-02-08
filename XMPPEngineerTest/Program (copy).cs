//using System;

//namespace XMPPEngineerTest
//{
//    class MainClass
//    {
//        public static void Main(string[] args)
//        {
//			/*
//            XMPPEngineer.Core.XmppCore client = new XMPPEngineer.Core.XmppCore(
//				"alchemy.local",
//				"test",
//				"eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJPbmxpbmUgSldUIEJ1aWxkZXIiLCJpYXQiOjE0OTI2Mzc1NzMsImV4cCI6MTUyNDE3MzU3MywiYXVkIjoid3d3LmV4YW1wbGUuY29tIiwic3ViIjoianJvY2tldEBleGFtcGxlLmNvbSIsInVzZXJuYW1lIjoidGVzdCIsIm5ldHdvcmsiOiJhbGNoZW15LmxvY2FsIn0.0IYkS8KH22kai_j6hRcjy5x7Lvwb6TG5XyRY8n9fym0",
//				"dev-lite-citym-access.westeurope.cloudapp.azure.com"
//            );*/

//			XMPPEngineer.Client.XmppClient client = new XMPPEngineer.Client.XmppClient(
//				"alchemy.local",
//				"steven",
//				"test",
//				"dev-lite-citym-access.westeurope.cloudapp.azure.com"
//			);

//            client.Connect("mobile");

//            client.Error += (sender, e) => {                
//				Console.WriteLine(e.ToString());
//			};
//            client.Message += (sender, e) => { 
//                Console.WriteLine(e.ToString()); 
//            };
//            //client.Iq += (sender, e) => {
//			//	Console.WriteLine(e.ToString());
//			//};
//			//client.Presence += (sender, e) =>
//			//{
//			//	Console.WriteLine(e.ToString());
//			//};
//			///client.A += (sender, e) =>
//			///{
//			///Console.WriteLine(e.ToString());
//			///};

//			//RunBasic(client);
//			//RunStreamManagement(client);
//            RunStreamManagement2(client);

//			Console.ReadKey();
//		}

//		private static void RunBasic(XMPPEngineer.Core.XmppCore client)
//		{
//			XMPPEngineer.Im.Message message = new XMPPEngineer.Im.Message(new XMPPEngineer.Jid("admin@alchemy.local"), "ok - " + DateTime.Now.ToLongTimeString());
//			client.SendMessage(message);
//		}

//        private static void RunStreamManagement(XMPPEngineer.Core.XmppCore client)
//        {
//			client.StreamManagementEnabled += (sdr, evt) =>
//			{
//		    	//XMPPEngineer.Im.Message message = new XMPPEngineer.Im.Message(new XMPPEngineer.Jid("admin@alchemy.local"), "ok - " + DateTime.Now.ToLongTimeString());
//			    //client.SendMessage(message);

//			// send a message in a loop
				
//                System.Timers.Timer timer = new System.Timers.Timer(10000);
//                timer.Elapsed += (sender, e) => {
//                    XMPPEngineer.Im.Message messagex = new XMPPEngineer.Im.Message(new XMPPEngineer.Jid("admin@alchemy.local"), "ok - " + DateTime.Now.ToLongTimeString());
//                    client.SendMessage(messagex);
//                };
//                timer.Enabled = true;
//                timer.Start();

//			//	// Resumes a stream that was disconnected for some reason - this will only work within the time window allows by the server to stream resumption - which defaults to 20 seconds
//			//	// We only need to do this if we detect we have lost a connection to the server - probably from no acks.
//			//	///client.StreamManagementResumed += (sender, e) => {
//			//	// continue in here  
//			//	///};
//			//	//client.ResumeStream();

//			//	///XMPPEngineer.Im.Message message = new XMPPEngineer.Im.Message(new XMPPEngineer.Jid("admin@alchemy.local"), "ok - " + DateTime.Now.ToLongTimeString());
//			//	///client.SendMessage(message);                
//			};

//			// enable stream management and recovery mode
//			///client.NumberOfItemsBeforeServerAck = 3;
//			///client.NumberOfSecondsBeforeServerAck = 15;//30;
//			client.EnableStreamManagement(true);
//		}

//        private static void RunStreamManagement2(XMPPEngineer.Client.XmppClient client)
//		{
//			client.StreamManagementEnabled += (sdr, evt) =>
//			{
//				// send a message in a loop

//				System.Timers.Timer timer = new System.Timers.Timer(10000);
//				timer.Elapsed += (sender, e) =>
//				{
//					XMPPEngineer.Im.Message messagex = new XMPPEngineer.Im.Message(new XMPPEngineer.Jid("admin@alchemy.local"), "ok - " + DateTime.Now.ToLongTimeString());
//					client.SendMessage(messagex);
//				};
//				timer.Enabled = true;
//				timer.Start();            
//			};

//			// enable stream management and recovery mode
//			client.EnableStreamManagement(true);
//		}
//    }
//}
