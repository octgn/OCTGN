using System;
using System.Collections.Generic;
using System.Threading;
using System.Net.Sockets;

namespace Octgn.Server
{
	public sealed class Server
	{
		#region Private fields

		private TcpListener tcp;            // Underlying windows socket
		private Handler handler;            // Message handler
		private List<Connection> clients = new List<Connection>();  // List of all the connected clients		

		#endregion

		#region Public interface

		// Creates and starts a new server
		public Server(int port, bool useIPv6, Guid gameId, Version gameVersion)
		{
      if (useIPv6)
      {
        tcp = new TcpListener(System.Net.IPAddress.IPv6Any, port);
        tcp.AllowNatTraversal(true);
      }
      else
        tcp = new TcpListener(System.Net.IPAddress.Any, port);
			this.handler = new Handler(gameId, gameVersion);
			Start();
		}

		// Stop the server
		public void Stop()
		{
			// Stop the server and release resources
			try
			{ tcp.Server.Close(); tcp.Stop(); }
			catch
			{ }
			// Close all open connections
			while (true)
			{
				Connection client = null;
				lock (clients)
				{
					if (clients.Count > 0)
						client = clients[0];
				}
				if (client != null)
					client.Disconnect();
				else
					break;
			}
		}

		#endregion

		#region Private implementation

		// Start the server
		private void Start()
		{
			// Creates a new thread for the server
			Thread thread = new Thread(Listen);
			thread.Name = "OCTGN.net Server";
			// Flag used to wait until the server is really started
			ManualResetEvent started = new ManualResetEvent(false);
			// Start the server
			thread.Start(started);
			started.WaitOne();
		}

		// Called when a client gets disconnected
		public bool Disconnected(TcpClient lost)
		{
			lock (clients)
			{
				// Search the client
				for (int i = 0; i < clients.Count; i++)
					if (clients[i].client == lost)
					{
						// Remove it
						clients[i].Disconnected();
						return true;
					}
			}
			// Client is not found
			return false;
		}

		// Main thread function: waits and accept incoming connections
		private void Listen(object o)
		{
			// Retrieve the parameter
			ManualResetEvent started = (ManualResetEvent)o;
			// Start the server and signal it
			tcp.Start();
			started.Set();
			try
			{
				while (true)
				{
					// Accept new connections
					Connection sc = new Connection(this, tcp.AcceptTcpClient());
					lock (clients) clients.Add(sc);
				}
			}
			catch (SocketException e)
			{
				// Interrupted is expected when the server gets stopped
				if (e.SocketErrorCode != SocketError.Interrupted) throw;
			}
		}

		#endregion

		public class Connection
		{
			private Server server;                  // The containing server
			internal readonly TcpClient client;     // The underlying Windows socket            
			private byte[] buffer = new byte[512];  // Buffer to receive data
			private byte[] packet = new byte[512];  // Buffer where received data is processed in packets
			private int packetPos = 0;              // Current position in the packet buffer
			private bool binary = false;            // Receives binary data ?
			private bool disposed = false;          // Indicates if the connection has already been disposed
		    private DateTime lastPing = DateTime.Now;

		    private Thread PingThread;

			// C'tor
			internal Connection(Server server, TcpClient client)
			{
				// Init fields
				this.server = server; this.client = client;
                //Start ping thread
                PingThread = new Thread(DoPing);
                PingThread.Start();
				// Start reading
				client.GetStream().BeginRead(buffer, 0, 512, Receive, null);
			}

            public void PingReceived()
            {
                lastPing = DateTime.Now;
            }

            private void DoPing()
            {
                while (!disposed)
                {
                    lock (this)
                    {
                        TimeSpan ts = new TimeSpan(DateTime.Now.Ticks - lastPing.Ticks);
                        if (ts.TotalSeconds > 5)
                            Disconnect(); //TODO We want to disconnect, but we also want to inform the server to lock the game until a rejoin, or a vote to kick happens.
                        if (disposed) return;
                    }
                    Thread.Sleep(1000);
                }
            }

			// Callback when data is received
			private void Receive(IAsyncResult ar)
			{
				try
				{
					// Get how many bytes were received
					int count = client.GetStream().EndRead(ar);
					// 0 or less mean we were disconnected, or an error happened
					if (count < 1)
					{ Disconnected(); return; }
					// Copy the new data in the packet buffer. Make the buffer larger if necessary.
					if ((packetPos + count) > packet.Length)
					{
						byte[] newPacket = new byte[packetPos + count];
						Array.Copy(packet, newPacket, packetPos);
						packet = newPacket;
					}
					Array.Copy(buffer, 0, packet, packetPos, count);
					// Handle the received data, either as binary or xml, depending on current status
					if (binary)
						BinaryReceive(count);
					else
						XmlReceive(count);
					// Check if the connection is still alive (might be refused by handler)
					if (client.Connected)
					{
						// Wait for new data
						client.GetStream().BeginRead(buffer, 0, 512, Receive, null);
					}
					else
					{
						lock (server.clients) server.clients.Remove(this);
					}
				}
				catch (Exception e)
				{
					// If an unexpected error arose during processing, log it
					if (!(e is SocketException) && !(e is ObjectDisposedException))
					{
						System.Diagnostics.Debug.WriteLine("Unexpected exception in Server.Receive:");
						System.Diagnostics.Debug.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
					}
					// Disconnect the client
					Disconnected();
				}
			}

			// Handle the received data as a binary packet 
			private void BinaryReceive(int count)
			{
				// Adjust the current packet position with the new data
				packetPos += count;
				// Packet starts with size as a 32 bits int.
				while (packetPos > 4)
				{
					int length = packet[0] | (int)packet[1] << 8 | (int)packet[2] << 16 | (int)packet[3] << 24;
					if (packetPos >= length)
					{
						// Copy the packet data in an array
						byte[] data = new byte[length - 4]; Array.Copy(packet, 4, data, 0, length - 4);
						// Lock the handler, because it is not thread-safe
						lock (server.handler)
							server.handler.ReceiveMessage(data, client,this);
						// Adjust the packet pos and contents
						packetPos -= length;
						Array.Copy(packet, length, packet, 0, packetPos);
					}
					else
						break;
				}
			}

			// Handle the received data as an xml packet
			private void XmlReceive(int count)
			{
				// Look for a 0 at the end of the packet
				for (int i = packetPos; i < packetPos + count; i++)
				{
					if (packet[i] == 0)
					{
						// Get the message as xml
						string xml = System.Text.Encoding.UTF8.GetString(packet, 0, i);
						// Check if it's a request to switch to binary mode
						if (xml == "<Binary />")
							binary = true;
						else
							// Lock the handler, because it is not thread-safe
							lock (server.handler) server.handler.ReceiveMessage(xml, client);
						// Adjust the packet position and contents
						count += packetPos - i - 1; packetPos = 0;
						Array.Copy(packet, i + 1, packet, 0, count);
						// Continue the loop
						i = -1; continue;
					}
				}
				// Ajust packet position
				packetPos += count;
			}

			// Disconnect the client
			internal void Disconnect()
			{
				// Lock the disposed field
				lock (this)
				{
					// Quit if this client is already disposed
					if (disposed) return;
					// Mark as disposed
					disposed = true;
				}
				// If it is connected, close it
				if (client.Connected)
					try
					{
						client.GetStream().Close();
						client.Close();
					}
					catch
					{ }
				// Remove it from the list
				lock (server.clients)
					server.clients.Remove(this);
			}

			// Notify that the client was unexpectedly disconnected
			internal void Disconnected()
			{
				// Lock the disposed field
				lock (this)
				{
					// Quit if the client is already disposed
					if (disposed) return;
					// Disconnect the client
					Disconnect();
					// Notify the event
					server.handler.Disconnected(client);
				}
			}
		}
	}
}