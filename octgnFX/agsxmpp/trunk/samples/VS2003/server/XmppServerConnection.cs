using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

using agsXMPP.protocol;
using agsXMPP.protocol.iq;
using agsXMPP.protocol.iq.auth;
using agsXMPP.protocol.iq.roster;
using agsXMPP.protocol.client;

using agsXMPP.Xml;
using agsXMPP.Xml.Dom;

namespace agsXMPP
{
	/// <summary>
	/// Zusammenfassung für XMPPSeverConnection.
	/// </summary>
	public class XmppSeverConnection
	{
		#region << Constructors >>
		public XmppSeverConnection()
		{		
			streamParser = new StreamParser();
			streamParser.OnStreamStart		+= new StreamHandler(streamParser_OnStreamStart);
			streamParser.OnStreamEnd		+= new StreamHandler(streamParser_OnStreamEnd);
			streamParser.OnStreamElement	+= new StreamHandler(streamParser_OnStreamElement);	
            
		}

		public XmppSeverConnection(Socket sock) : this()
		{	
			m_Sock = sock;
			m_Sock.BeginReceive(buffer, 0, BUFFERSIZE, 0, new AsyncCallback(ReadCallback), null);		
		}
		#endregion
			
		private StreamParser			streamParser;
		private Socket					m_Sock;
        private const int BUFFERSIZE = 1024;
        private byte[] buffer = new byte[BUFFERSIZE];
                
	
		public void ReadCallback(IAsyncResult ar) 
		{        
			// Retrieve the state object and the handler socket
			// from the asynchronous state object

			// Read data from the client socket. 
			int bytesRead = m_Sock.EndReceive(ar);

			if (bytesRead > 0) 
			{				
				streamParser.Push(buffer, 0, bytesRead);
				
				// Not all data received. Get more.
				m_Sock.BeginReceive(buffer, 0, BUFFERSIZE, 0, new AsyncCallback(ReadCallback), null);
			}
			else
			{
				m_Sock.Shutdown(SocketShutdown.Both);
				m_Sock.Close();
			}
		}

		private void Send(string data) 
		{
			// Convert the string data to byte data using ASCII encoding.
			byte[] byteData = Encoding.UTF8.GetBytes(data);

			// Begin sending the data to the remote device.
			m_Sock.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), null);
		}

		private void SendCallback(IAsyncResult ar) 
		{
			try 
			{
				// Complete sending the data to the remote device.
				int bytesSent = m_Sock.EndSend(ar);
				Console.WriteLine("Sent {0} bytes to client.", bytesSent);

			} 
			catch (Exception e) 
			{
				Console.WriteLine(e.ToString());
			}
		}
	
		
		public void Stop()
		{
			Send("</stream:stream>");
//			client.Close();
//			_TcpServer.Stop();

			m_Sock.Shutdown(SocketShutdown.Both);
			m_Sock.Close();
		}
			
		
		#region << Properties and Member Variables >>
//		private int			m_Port			= 5222;		
		private string		m_SessionId		= null;

		public string SessionId
		{
			get
			{
				return m_SessionId;
			}
			set
			{
				m_SessionId = value;
			}
		}
		#endregion

		private void streamParser_OnStreamStart(object sender, Node e)
		{
			SendOpenStream();
		}

		private void streamParser_OnStreamEnd(object sender, Node e)
		{

		}

		private void streamParser_OnStreamElement(object sender, Node e)
		{
            Console.WriteLine("OnStreamElement: " + e.ToString());
			if (e.GetType() == typeof(Presence))
			{
				// route presences here and handle all subscription stuff
			}
			else if (e.GetType() == typeof(Message))
			{
				// route the messages here

			}
			else if (e.GetType() == typeof(IQ))
			{
				ProcessIQ(e as IQ);
			}
		}

		private void ProcessIQ(IQ iq)
		{
			if(iq.Query.GetType() == typeof(Auth))
			{
				Auth auth = iq.Query as Auth;
				switch(iq.Type)
				{
					case IqType.get:
						iq.SwitchDirection();
						iq.Type = IqType.result;
						auth.AddChild(new Element("password"));
						auth.AddChild(new Element("digest"));
						Send(iq);
						break;
					case IqType.set:
						// Here we should verify the authentication credentials
						iq.SwitchDirection();
						iq.Type = IqType.result;
						iq.Query = null;
						Send(iq);
						break;
				}
				
			}
			else if(iq.Query.GetType() == typeof(Roster))
			{
				ProcessRosterIQ(iq);
				
			}
			
		}

		private void ProcessRosterIQ(IQ iq)
		{
			if (iq.Type == IqType.get)
			{
				// Send the roster
				// we send a dummy roster here, you should retrieve it from a
				// database or some kind of directory (LDAP, AD etc...)
				iq.SwitchDirection();
				iq.Type = IqType.result;
				for (int i=1; i<11;i++)
				{
					RosterItem ri = new RosterItem();
					ri.Name = "Item " + i.ToString();
					ri.Subscription = SubscriptionType.both;
					ri.Jid = new Jid("item" + i.ToString() + "@localhost");
					ri.AddGroup("localhost");
					iq.Query.AddChild(ri);
				}
				for (int i=1; i<11;i++)
				{
					RosterItem ri = new RosterItem();
					ri.Name = "Item JO " + i.ToString();
					ri.Subscription = SubscriptionType.both;
					ri.Jid = new Jid("item" + i.ToString() + "@jabber.org");
					ri.AddGroup("JO");
					iq.Query.AddChild(ri);
				}
				Send(iq);
			}
		}

		private void SendOpenStream()
		{
			
			// Recv:<stream:stream xmlns='jabber:client' xmlns:stream='http://etherx.jabber.org/streams' from='myjabber.net' id='1075705237'>
			
			// Send the Opening Strem to the client
			string ServerDomain = "localhost";
			
			this.SessionId = agsXMPP.SessionId.CreateNewId();
			
			
			StringBuilder sb = new StringBuilder();

			sb.Append( "<stream:stream from='");
			sb.Append( ServerDomain );
			
			sb.Append( "' xmlns='" );
			sb.Append( Uri.CLIENT );
			
			sb.Append( "' xmlns:stream='" );
			sb.Append( Uri.STREAM );
			
			sb.Append( "' id='" );
			sb.Append( this.SessionId );
			
			sb.Append( "'>" );

			Send( sb.ToString() );
		}

		private void Send(Element el)
		{
			Send(el.ToString());
		}

	}
}