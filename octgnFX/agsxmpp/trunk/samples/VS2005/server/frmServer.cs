using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Net;
using System.Net.Sockets;

using System.Threading;

namespace server
{
	/// <summary>
	/// This is a _very simple_ XMPP/Jabber Server example
    /// this example will give you an idea how you can build a server using our SDK and classes,
    /// but it's far away from beeing a complete and working server.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button cmdStart;
		private System.Windows.Forms.Button cmdStop;
		private System.Windows.Forms.Label label1;
		
		private System.ComponentModel.Container components = null;

		public Form1()
		{			
			InitializeComponent();			
		}

		/// <summary>
		/// 
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Vom Windows Form-Designer generierter Code
		/// <summary>
		///		
		/// </summary>
		private void InitializeComponent()
		{
            this.cmdStart = new System.Windows.Forms.Button();
            this.cmdStop = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cmdStart
            // 
            this.cmdStart.Location = new System.Drawing.Point(16, 16);
            this.cmdStart.Name = "cmdStart";
            this.cmdStart.Size = new System.Drawing.Size(80, 24);
            this.cmdStart.TabIndex = 0;
            this.cmdStart.Text = "Start Server";
            this.cmdStart.Click += new System.EventHandler(this.cmdStart_Click);
            // 
            // cmdStop
            // 
            this.cmdStop.Location = new System.Drawing.Point(144, 16);
            this.cmdStop.Name = "cmdStop";
            this.cmdStop.Size = new System.Drawing.Size(80, 24);
            this.cmdStop.TabIndex = 1;
            this.cmdStop.Text = "Stop Server";
            this.cmdStop.Click += new System.EventHandler(this.cmdStop_Click);
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Location = new System.Drawing.Point(16, 56);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(208, 64);
            this.label1.TabIndex = 2;
            this.label1.Text = "Use your favorite client and connect to the server with a random username and pas" +
                "sword. The server is running on localhost.";
            // 
            // Form1
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(248, 128);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmdStop);
            this.Controls.Add(this.cmdStart);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.Text = "Server Example";
            this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// Main entry point of the application
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new Form1());
		}

		
		

		// Thread signal.
		private ManualResetEvent allDone = new ManualResetEvent(false);
		private Socket listener;
		private bool m_Listening;

		private void cmdStart_Click(object sender, System.EventArgs e)
		{
			ThreadStart myThreadDelegate = new ThreadStart(Listen);
			Thread myThread = new Thread(myThreadDelegate);
			myThread.Start();		
		}

		private void Listen()
		{
			IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 5222);

			// Create a TCP/IP socket.
			listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );

			
			// Bind the socket to the local endpoint and listen for incoming connections.
			try 
			{
				listener.Bind(localEndPoint);
				listener.Listen(10);
				
				m_Listening = true;
				
				while (m_Listening) 
				{
					// Set the event to nonsignaled state.
					allDone.Reset();

					// Start an asynchronous socket to listen for connections.
					Console.WriteLine("Waiting for a connection...");
					listener.BeginAccept( new AsyncCallback(AcceptCallback), null );

					// Wait until a connection is made before continuing.
					allDone.WaitOne();
				}

			} 
			catch (Exception ex) 
			{
				Console.WriteLine(ex.ToString());
			}

		}

		public void AcceptCallback(IAsyncResult ar) 
		{
			// Signal the main thread to continue.
			allDone.Set();
			// Get the socket that handles the client request.
			Socket newSock = listener.EndAccept(ar);
			
			agsXMPP.XmppSeverConnection con = new agsXMPP.XmppSeverConnection(newSock);
			//listener.BeginReceive(buffer, 0, BUFFERSIZE, 0, new AsyncCallback(ReadCallback), null);
		}


		private void cmdStop_Click(object sender, System.EventArgs e)
		{            
			m_Listening = false;
            allDone.Set();
            //allDone.Reset();
		}
	}
}
