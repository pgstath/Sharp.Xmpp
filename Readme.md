### Introduction

This repository contains an easy-to-use and well-documented .NET assembly for communicating with
an XMPP server. It supports basic Instant Messaging and Presence funtionality as well as a variety
of XMPP extensions.


### Supported XMPP Features

The library fully implements the [XMPP Core](http://xmpp.org/rfcs/rfc3920.html) and 
[XMPP IM](http://xmpp.org/rfcs/rfc3921.html) specifications and thusly provides the basic XMPP instant
messaging (IM) and presence functionality. In addition, the library offers support for most of the
optional procotol extensions. More specifically, the following features are supported:

+ SASL Authentication (PLAIN, DIGEST-MD5, and SCRAM-SHA-1)
+ User Avatars
+ SOCKS5 and In-Band File-Transfer
+ In-Band Registration
+ XEP 0198 Stream Management (alpha)
+ User Mood
+ User Tune
+ User Activity
+ Simplified Blocking
+ API designed to be very easy to use
+ Well documented with lots of example code
+ Free to use in commercial and personal projects (MIT License)

### Usage & Examples

To use the library add the XMPPEngineer.dll assembly to your project references in Visual Studio. Here's
a simple example that initializes a new instance of the XmppClient class and connects to an XMPP
server:

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
	            using (XmppClient client = new XmppClient("domain", "user", "password"))
	            {
	                client.Connect();

	                Message message = new Message(new Jid("user@domain"), "Hello, World.");
	                client.SendMessage(message);
	            }

	            // with stream management
	            using (XmppClient clientsm = new XmppClient("domain", "user", "password"))
	            {
	                clientsm.Connect();

	                clientsm.StreamManagementEnabled += (sdr, evt) =>
	                {
	                    Message messagesm = new Message(new Jid("user@domain"), "Hello, World.");
	                    clientsm.SendMessage(messagesm);
	                };

	                // enable stream management and recovery mode
	                clientsm.EnableStreamManagement();
	            }
	        }
	    }
	}


### Documention (to be updated)
Please see the [documentation](http://smiley22.github.com/S22.Xmpp/Documentation/) for a getting started
guide, examples and details on using the classes and methods exposed by the S22.Xmpp assembly.

I will update this shortly.


### Credits
The XMPPEngineer library is copyright © 2017 Steven Livingstone.
The Sharp.Xmpp library is copyright © 2015 Panagiotis Georgiou Stathopoulos.
The initial S22.Xmpp library is copyright © 2013-2014 Torben Könke.


### License

This library is released under the [MIT license](https://github.com/pgstath/XMPPEngineer/blob/master/License.md).


### Bug reports

Please create a new issue on the GitHub project homepage.
