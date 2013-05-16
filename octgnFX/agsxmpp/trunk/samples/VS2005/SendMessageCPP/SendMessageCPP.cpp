#include "stdafx.h"

using namespace System;
using namespace System::Threading;

using namespace agsXMPP;
using namespace agsXMPP::protocol::client;


int main(array<System::String ^> ^args)
{	
	// create new XmppConnection
	XmppClientConnection ^ xmpp = gcnew  XmppClientConnection();
	
	// set required properties for login
	xmpp->AutoResolveConnectServer = true;
	xmpp->Username = "user1";	// Add your username here
	xmpp->Password = "secret";
	xmpp->Server = "jabber.org";

	xmpp->Open();

	Threading::Thread::Sleep(5000);

	// Send our presence to teh server
	xmpp->SendMyPresence();

	Threading::Thread::Sleep(5000);

	// Send a message
	Message ^ msg = gcnew Message;
	msg->To = gcnew Jid("user2@jabber.org");	// The receiver of the Message
	msg->Type = MessageType::chat;
	msg->Body = "Test Message";

	xmpp->Send(msg);

	Threading::Thread::Sleep(5000);

	// Close the XmppConnection properly
	xmpp->Close();

	Threading::Thread::Sleep(5000);
    
	return 0;
}