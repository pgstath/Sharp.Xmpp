# 1.0.2.2

##XMPP Extensions Added
* XEP-0280: Message Carbons, contributed by Ignacio Nicolás Rodríguez, ignacionr 

##Other improvements
* Disconnection detection improvements

#1.0.0.1
First XMPPEngineer Release
##Bugs resolved
* In SaslDigestMd5, replaced `digestUri = "xmpp/" + fields["realm"];` from initial `imap` which caused connection setup failure

##XMPP Extensions Added
* XEP-0153: vCard-Based Avatars
* XEP-0280: Message Carbons, contributed by Ignacio Nicolás Rodríguez, ignacionr 

##Features Added
* An easy way to add extensions and provide a custom XML messaging over IQ messages. 'XmppClient.RequestCustomIq' for GETing custom requests, and delegate CustomIqDelegate in order to call in the event of 
custom IQ message arrived. Xml Element is customiq and namespace is urn:xmppengineer.xmpp:customiq.
* DNS XMPP SRV records lookup is added
* XmppClient.InitiateFileTransfer now returns also the Sid of the file transfer for future reference and management of the transfer process. 
* Improved detection of XmppClient and Xmpp Im connection and disconnection events. With this regard:
** Added XmppDisconnectionException, which is raised when disconnection is detected in core StreamParser
* Added a DefaultTimeOut, in order for the messages to be timed out. 
** In the previous method the program was waiting indefinitely in the event a time out should be generated
XmppClient.DefaultTimeOut & XmppIm.DefaultTimeOut & XmppCore.DefaultTimeOut
** XmppDisconnectionException now is raised if IQ timeout is ellapsed

##Extensions and Protocol Removed
* XEP-0084: User Avatar, is now available only if the '#if WINDOWSPLATFORM' Conditional Compilation Symbol/preprocessor directive is used, since it relies on Windows Imaging packages
* UPNP for XEP-0065: SOCKS5 Bytestreams through UPNPLib. It will be available through a WINDOWSPLATFORM Conditional Compilation Symbol in the future

##Bugs
* UPNPLib is not present and compilation will result in UPNPLib related errors if the WINDOWSPLATFORM Conditional Compilation Symbol is used

##Open Issues and Limitations
* Connection is performed only on first SRV DNS record server. No reconnections on SRV records with less priority are supported for the time being
* Proxy only File Transfer is supported and tested on the build version
* XEP0153 Avatar Update, <x xmlns='vcard-temp:x:update'> message is to be implemented
* __XmppCore.AssertValid should not check for Connection Online and raise InvalidOperationException__
* XmppCore.Close should include again AssertValid?


#1.0.0.0
Original S22.Xmpp forked