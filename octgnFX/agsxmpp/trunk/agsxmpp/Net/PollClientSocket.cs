/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * Copyright (c) 2003-2012 by AG-Software 											 *
 * All Rights Reserved.																 *
 * Contact information for AG-Software is available at http://www.ag-software.de	 *
 *																					 *
 * Licence:																			 *
 * The agsXMPP SDK is released under a dual licence									 *
 * agsXMPP can be used under either of two licences									 *
 * 																					 *
 * A commercial licence which is probably the most appropriate for commercial 		 *
 * corporate use and closed source projects. 										 *
 *																					 *
 * The GNU Public License (GPL) is probably most appropriate for inclusion in		 *
 * other open source projects.														 *
 *																					 *
 * See README.html for details.														 *
 *																					 *
 * For general enquiries visit our website at:										 *
 * http://www.ag-software.de														 *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Collections;
using System.Security.Cryptography;

#if CF
using agsXMPP.util;
#endif

namespace agsXMPP.Net
{
	/// <summary>
	/// JEP-0025 Jabber HTTP Polling Socket
	/// </summary>
	public class PollClientSocket : BaseSocket
	{
		private const	string	CONTENT_TYPE	= "application/x-www-form-urlencoded";
		private const	string	METHOD			= "POST";
		
		private string[]		m_Keys;		
		private int				m_CurrentKeyIdx;
		private	string			m_ID			= null;		
		private Queue			m_SendQueue		= new Queue();

        /// <summary>
        /// Object for synchronizing threads
        /// </summary>
		private Object			m_Lock			= new Object();
#if CF || CF_2
		private DateTime		m_WaitUntil;
#endif
		
		public PollClientSocket()
		{            
		}

		#region << Properties and Members >>
		private bool			m_Connected		= false;
		private int				m_Interval		= 10000;
#if !CF
		private int				m_CountKeys		= 256;
#else
		// set this lower on embedded devices because the key generation is slow there
		private int				m_CountKeys		= 32;
#endif
		private WebProxy		m_Proxy			= null;

		/// <summary>
		/// because the client socket is no presintant socket we return always true
		/// </summary>
		public override bool Connected
		{
			get { return true; }
		}

		/// <summary>
		/// Poll interval in milliseconds.	
		/// The maximum interval recommended for clients between requests is two minutes (120000);
		/// Default is 10 seconds (10000)
		/// </summary>
		public int Interval
		{
			get { return m_Interval; }
			set { m_Interval = value;}
		}

		/// <summary>
		/// count of keys to generate each time. Keys are generated with the Sha1 algoritm.
		/// You can reduce the num,ber of keys to gemerate each time if your device is to slow on generating the keys
		/// or you want to save memory.
		/// 256 is the default value, 32 on CF
		/// </summary>
		public int CountKeys
		{
			get { return m_CountKeys; }
			set { m_CountKeys = value;}
		}

		public WebProxy Proxy
		{
			get { return m_Proxy; }
			set { m_Proxy = value; }
		}
		#endregion

		#region << Public Methods and Functions >>
		public override void Connect()
		{			
			base.Connect();

			FireOnConnect();

			Init();

			m_Connected		= true;

			// Start a new Thread for polling
			Thread m_thread = new Thread(new ThreadStart(PollThread));
#if !CF
			// useful in debug
			m_thread.Name			= "HTTP Polling Thread";
#endif
			m_thread.Start();
		}

		public override void Disconnect()
		{
			base.Disconnect();

			FireOnDisconnect();

			m_Connected = false;
		}

		public override void Send(byte[] bData)
		{
			base.Send (bData);

			Send(Encoding.UTF8.GetString(bData, 0, bData.Length));
		}


		public override void Send(string data)
		{
			base.Send (data);
#if CF || CF_2
            lock (m_Lock)
            {
                m_SendQueue.Enqueue(data);
            }		
#else			
            lock(m_Lock)
			{
				m_SendQueue.Enqueue(data);
				Monitor.Pulse(m_Lock);
			}
#endif
        }
		#endregion

		private void Init()
		{
			m_ID			= null;
			m_Keys			= null;
			m_CurrentKeyIdx	= 0;
		}

		/// <summary>
		/// Simple algotithm for generating a random key
		/// </summary>
		/// <returns></returns>
		private string GenerateRandomKey()
		{
			// Lenght of the Session ID on bytes,
			// 16 bytes equaly 32 chars
			// This should be unique enough
			int m_lenght = 16;		

			RandomNumberGenerator RNG = RandomNumberGenerator.Create();

			byte[] buf = new byte[m_lenght];
			RNG.GetBytes(buf);
			
			return Util.Hash.HexToString(buf).ToLower();
		}

		/// <summary>
		/// Generates a bunch of keys
		/// </summary>
		private void GenerateKeys()
		{			
			string prev = GenerateRandomKey();		
			m_Keys = new string[m_CountKeys];
			
			for (int i=0; i < m_CountKeys; i++)
			{
				byte[] hash = Util.Hash.Sha1HashBytes(prev);
				m_Keys[i] = Convert.ToBase64String(hash, 0, hash.Length);
				prev = m_Keys[i];
			}
			m_CurrentKeyIdx = m_CountKeys - 1;
		}

		private void PollThread()
		{
			/*
			Example 7. Initial request (with keys)

			POST /wc12/webclient HTTP/1.1
			Content-Type: application/x-www-form-urlencoded
			Host: webim.jabber.com

			0;VvxEk07IFy6hUmG/PPBlTLE2fiA=,<stream:stream to="jabber.com"
			xmlns="jabber:client"
			xmlns:stream="http://etherx.jabber.org/streams">
			
			
			POST /wc12/webclient HTTP/1.1
			Content-Type: application/x-www-form-urlencoded
			Host: webim.jabber.com

			7776:2054;moPFsvHytDGiJQOjp186AMXAeP0=,<iq type="get" id="WEBCLIENT3">
			<query xmlns="jabber:iq:auth">
				<username>hildjj</username>
			</query>
			</iq>
			*/		
			
			while(m_Connected)
			{
				string content;
				string data;

				//				lock(m_lock)
				//				{
				if (m_SendQueue.Count > 0)
					data = m_SendQueue.Dequeue() as string;
				else
					data = "";
				//				}
			
				if (m_ID == null)
				{
					GenerateKeys();
					content = string.Format("{0};{1},{2}", "0", m_Keys[m_CurrentKeyIdx], data);
				}
				else
				{
					if (m_CurrentKeyIdx == 0)
					{
						// only 1 Key left
						string key = m_Keys[0];
						// generate new keys
						GenerateKeys();
						/*
						Example 9. Changing key

						POST /wc12/webclient HTTP/1.1
						Content-Type: application/x-www-form-urlencoded
						Host: webim.jabber.com

						7776:2054;C+7Hteo/D9vJXQ3UfzxbwnXaijM=;Tr697Eff02+32FZp38Xaq2+3Bv4=,<presence/>    
						*/
						content = string.Format("{0};{1};{2},{3}", m_ID, key, m_Keys[m_CurrentKeyIdx], data);

					}
					else
					{
						// m_CurrentKey = CreateNextKey(m_CurrentKey);
						content = string.Format("{0};{1},{2}", m_ID, m_Keys[m_CurrentKeyIdx], data);
					}
				}
				//Console.WriteLine("used Key index: " + m_CurrentKeyIdx.ToString());
				m_CurrentKeyIdx--;				

				byte[] bytes = Encoding.UTF8.GetBytes(content);

				FireOnSend(bytes, bytes.Length);
			
				HttpWebRequest req	= (HttpWebRequest) WebRequest.Create(Address);
				
				// Set Proxy Information
				if (m_Proxy != null)
					req.Proxy = m_Proxy;
		
				req.Method          = METHOD;
				//req.KeepAlive		= true;
				req.ContentType     = CONTENT_TYPE;
				req.ContentLength	= bytes.Length;
				req.Timeout         = 5000;

                Stream outputStream;
                try
                {
                    outputStream = req.GetRequestStream();
                }
                catch (Exception ex)
                {
                    base.FireOnError(ex);
                    Disconnect();
                    return;
                }
				

				outputStream.Write (bytes, 0, bytes.Length);
		
				outputStream.Close ();			
		
				// This does the Webrequest. So catch errors here
				HttpWebResponse resp;
				try
				{
					 resp = (HttpWebResponse) req.GetResponse();
				}
				catch(Exception ex)
				{
					FireOnError(ex);
					return;
				}

				// The server must always return a 200 response code,
				// sending any session errors as specially-formatted identifiers.
				if (resp.StatusCode != HttpStatusCode.OK)
				{
					FireOnError(new PollSocketException("unexpected status code " + resp.StatusCode.ToString()));
					return;
				}

				Stream rs = resp.GetResponseStream();

				int readlen;
				byte[] readbuf = new byte[1024];
				MemoryStream ms = new MemoryStream();
				while ((readlen = rs.Read(readbuf, 0, readbuf.Length)) > 0)
				{
					ms.Write(readbuf, 0, readlen);
				}
		
				byte[] recv = ms.ToArray();

				// Read Cookies from Header
				// Set-Cookie: ID=7776:2054; path=/webclient/; expires=-1
				WebHeaderCollection headers = resp.Headers;
            
				// Check for any cookies
				// Didnt get the .NET CookieCollection classes working correct
				// So read it by hand, i cookie is only another simple header
				if (headers["Set-Cookie"] != null)
				{
					string header = headers["Set-Cookie"];
					string[] cookies = header.Split( (char) ';');
			
					Hashtable htCookies = new Hashtable();
					foreach(string cookie in cookies)
					{
						string[] vals = cookie.Split( (char) '=');
						if (vals.Length == 2)					
							htCookies.Add(vals[0], vals[1]);				
					}

					if (htCookies.ContainsKey("ID"))
					{
						string id = htCookies["ID"] as string;
						// if ID ends with its an error message
						if ( !id.EndsWith(":0"))
						{
							// if me dont have the ID yet cache it
							if (m_ID == null)
								m_ID = id;
						}
						else
						{
							// Handle Errors
							switch (id)
							{
								case "0:0":
									// 3.1.1 Unknown Error
									// Server returns ID=0:0.
									// The response body can contain a textual error message.									
									return;
								case "-1:0":
									// 3.1.2 Server Error
									// Server returns ID=-1:0
									return;
								case "-2:0":
									// 3.1.3 Bad Request
									// Server returns ID=-2:0
									return;
								case "-3:0":
									// 3.1.4 Key Sequence Error
									// Server returns ID=-3:0
									return;
							}
						}
					}
				}

                // cleanup webrequest resources
                ms.Close();
                rs.Close();
                resp.Close();

				if (recv.Length > 0)
				{
					//Console.WriteLine("RECV: " + Encoding.UTF8.GetString(recv));
					FireOnReceive(recv, recv.Length);
				}
				else
				{
					// We received nothing in the response, 
					// so sleep until next poll
#if CF || CF_2
					if (m_SendQueue.Count == 0)
					{							
						m_WaitUntil = DateTime.Now.AddMilliseconds(m_Interval);
						while (m_SendQueue.Count == 0 && DateTime.Compare(m_WaitUntil, DateTime.Now) > 0)
						{							
							Thread.Sleep(100);
						}
					}					
#else
					lock(m_Lock)
					{
						if (m_SendQueue.Count == 0)
						{
                            // Left for debugging
							//Console.WriteLine("Start Wait: " + m_Interval.ToString());							
							Monitor.Wait(m_Lock, m_Interval);							
							//Console.WriteLine("End Wait:");
						}
					}
#endif
				}
								
			}			
		}
	}
}