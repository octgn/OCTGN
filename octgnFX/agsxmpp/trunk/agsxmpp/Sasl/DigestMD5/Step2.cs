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
using System.Text;

#if CF
using agsXMPP.util;
#endif
using System.Security.Cryptography;

namespace agsXMPP.Sasl.DigestMD5
{
	/// <summary>
	/// Summary description for Step2.
	/// </summary>
	public class Step2 : Step1
	{
		public Step2()
		{
			
		}

		/// <summary>
		/// builds a step2 message reply to the given step1 message
		/// </summary>
		/// <param name="step1"></param>
		public Step2(Step1 step1, string username, string password, string server)
		{
            this.Nonce		= step1.Nonce;
            
            // fixed for SASL n amessage servers (jabberd 1.x)
            if (SupportsAuth(step1.Qop))
                this.Qop        = "auth";
			
            this.Realm		= step1.Realm;
			this.Charset	= step1.Charset;
			this.Algorithm	= step1.Algorithm;

			this.Username	= username;
			this.Password	= password;
			this.Server		= server;

			GenerateCnonce();
			GenerateNc();
			GenerateDigestUri();
			GenerateResponse();
		}
        
        /// <summary>
        /// Does the server support Auth?
        /// </summary>
        /// <param name="qop"></param>
        /// <returns></returns>
        private bool SupportsAuth(string qop)
        {
            string[] auth = qop.Split(',');
            // This overload was not available in the CF, so updated this to the following
            //bool ret = Array.IndexOf(auth, "auth") < 0 ? false : true;
            bool ret = Array.IndexOf(auth, "auth", auth.GetLowerBound(0), auth.Length) < 0 ? false : true;            
            return ret;
        }
        
        /// <summary>
		/// parses a message and returns the step2 object
		/// </summary>
		/// <param name="message"></param>
		public Step2(string message)
		{
			// TODO, important for server stuff
		}

		#region << Properties and member variables >>
		private string m_Cnonce;
		private string m_Nc;
		private string m_DigestUri;
		private string m_Response;
		private string m_Authzid;

		public string Cnonce
		{
			get { return m_Cnonce; }
			set { m_Cnonce = value; }
		}

		public string Nc
		{
			get { return m_Nc; }
			set { m_Nc = value; }
		}
		
		public string DigestUri
		{
			get { return m_DigestUri; }
			set { m_DigestUri = value; }
		}

		public string Response
		{
			get { return m_Response; }
			set { m_Response = value; }
		}

		public string Authzid
		{
			get { return m_Authzid; }
			set { m_Authzid = value; }
		}		
		#endregion


		public override string ToString()
		{
			return GenerateMessage();
		}
	

		private void GenerateCnonce()
		{
			// Lenght of the Session ID on bytes,
			// 32 bytes equaly 64 chars
			// 16^64 possibilites for the session IDs (4.294.967.296)
			// This should be unique enough
			int m_lenght = 32;		

			RandomNumberGenerator RNG = RandomNumberGenerator.Create();

			byte[] buf = new byte[m_lenght];
			RNG.GetBytes(buf);
			
			m_Cnonce = Util.Hash.HexToString(buf).ToLower();

//			m_Cnonce = "e163ceed6cfbf8c1559a9ff373b292c2f926b65719a67a67c69f7f034c50aba3";
		}

		private void GenerateNc()
		{
			int nc = 1;
			m_Nc = nc.ToString().PadLeft(8,'0');
		}

		private void GenerateDigestUri()
		{
			m_DigestUri = "xmpp/" + base.Server;
		}

		
		//	HEX( KD ( HEX(H(A1)),
		//	{
		//		nonce-value, ":" nc-value, ":",
		//		cnonce-value, ":", qop-value, ":", HEX(H(A2)) }))
		//
		//	If authzid is specified, then A1 is
		//
		//	A1 = { H( { username-value, ":", realm-value, ":", passwd } ),
		//	":", nonce-value, ":", cnonce-value, ":", authzid-value }
		//
		//	If authzid is not specified, then A1 is
		//
		//	A1 = { H( { username-value, ":", realm-value, ":", passwd } ),
		//	":", nonce-value, ":", cnonce-value }
		//
		//	where
		//
		//	passwd   = *OCTET
		public void GenerateResponse()
		{
			byte[] H1;
			byte[] H2;
			byte[] H3;
			//byte[] temp;
			string A1;
			string A2;
			string A3;
			string p1;
			string p2;			

			StringBuilder sb = new StringBuilder();
			sb.Append(this.Username);
			sb.Append(":");
			sb.Append(this.Realm);
			sb.Append(":");
			sb.Append(this.Password);
			
#if !CF
			H1 =  new MD5CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
#else
			//H1 = Encoding.Default.GetBytes(util.Hash.MD5Hash(sb.ToString()));
			H1 = util.Hash.MD5Hash(Encoding.UTF8.GetBytes(sb.ToString()));
#endif

			sb.Remove(0, sb.Length);
			sb.Append(":");
			sb.Append(this.Nonce);
			sb.Append(":");
			sb.Append(this.Cnonce);

			if (m_Authzid != null)
			{				
				sb.Append(":");
				sb.Append(m_Authzid);
			}
			A1 = sb.ToString();


//			sb.Remove(0, sb.Length);			
//			sb.Append(Encoding.Default.GetChars(H1));
//			//sb.Append(Encoding.ASCII.GetChars(H1));
//			
//			sb.Append(A1);			
			byte[] bA1 = Encoding.ASCII.GetBytes(A1);
			byte[] bH1A1 = new byte[H1.Length + bA1.Length];
			
			//Array.Copy(H1, bH1A1, H1.Length);
			Array.Copy(H1, 0, bH1A1, 0, H1.Length);
			Array.Copy(bA1, 0, bH1A1, H1.Length, bA1.Length);
#if !CF
			H1 =  new MD5CryptoServiceProvider().ComputeHash(bH1A1);			
			//Console.WriteLine(util.Hash.HexToString(H1));
#else
			//H1 = Encoding.Default.GetBytes(util.Hash.MD5Hash(sb.ToString()));
			//H1 =util.Hash.MD5Hash(Encoding.Default.GetBytes(sb.ToString()));
			H1 =util.Hash.MD5Hash(bH1A1);
#endif
			sb.Remove(0,sb.Length);
			sb.Append("AUTHENTICATE:");
			sb.Append(m_DigestUri);
			if (this.Qop.CompareTo("auth") != 0)
			{
				sb.Append(":00000000000000000000000000000000");
			}
			A2 = sb.ToString();
			H2 = Encoding.ASCII.GetBytes(A2);
			
#if !CF
			H2 = new MD5CryptoServiceProvider().ComputeHash(H2);
#else
			//H2 = Encoding.Default.GetBytes(util.Hash.MD5Hash(H2));
			H2 =util.Hash.MD5Hash(H2);
#endif            
			// create p1 and p2 as the hex representation of H1 and H2
			p1 = Util.Hash.HexToString(H1).ToLower();
			p2 = Util.Hash.HexToString(H2).ToLower();
            
			sb.Remove(0, sb.Length);
			sb.Append(p1);
			sb.Append(":");
			sb.Append(this.Nonce);
			sb.Append(":");
			sb.Append(m_Nc);
			sb.Append(":");
			sb.Append(this.m_Cnonce);
			sb.Append(":");
			sb.Append(base.Qop);
			sb.Append(":");
			sb.Append(p2);
            
			A3 = sb.ToString();
#if !CF
			H3 = new MD5CryptoServiceProvider().ComputeHash(Encoding.ASCII.GetBytes(A3));
#else
			//H3 = Encoding.Default.GetBytes(util.Hash.MD5Hash(A3));
			H3 =util.Hash.MD5Hash(Encoding.ASCII.GetBytes(A3));
#endif
			m_Response = Util.Hash.HexToString(H3).ToLower(); 
		}

		private string GenerateMessage()
		{			
			StringBuilder sb = new StringBuilder();
			sb.Append("username=");
			sb.Append(AddQuotes(this.Username));
			sb.Append(",");
			sb.Append("realm=");
			sb.Append(AddQuotes(this.Realm));
			sb.Append(",");
			sb.Append("nonce=");
			sb.Append(AddQuotes(this.Nonce));
			sb.Append(",");
			sb.Append("cnonce=");
			sb.Append(AddQuotes(this.Cnonce));
			sb.Append(",");
			sb.Append("nc=");
			sb.Append(this.Nc);
			sb.Append(",");
			sb.Append("qop=");
			sb.Append(base.Qop);
			sb.Append(",");
			sb.Append("digest-uri=");
			sb.Append(AddQuotes(this.DigestUri));			
			sb.Append(",");
			sb.Append("charset=");
			sb.Append(this.Charset);
			sb.Append(",");
			sb.Append("response=");
			sb.Append(this.Response);
			
			return sb.ToString();			
		}

		/// <summary>
		/// return the given string with quotes
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		private string AddQuotes(string s)
		{
            // fixed, s can be null (eg. for realm in ejabberd)
            if (s != null && s.Length > 0)
                s = s.Replace(@"\", @"\\");
			
            string quote = "\"";
			return quote + s + quote;
		}
	}
}
