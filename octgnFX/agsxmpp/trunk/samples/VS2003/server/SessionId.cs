#if !CF
using System;
using System.Text;
using System.Collections;
using System.Security.Cryptography;

namespace agsXMPP
{
	/// <summary>
	/// Summary description for SessionId.
	/// </summary>
	public class SessionId
	{
		// Lenght of the Session ID on bytes,
		// 4 bytes equaly 8 chars
		// 16^8 possibilites for the session IDs (4.294.967.296)
		// This should be unique enough
		private static int m_lenght = 4;
	
		public static string CreateNewId()
		{			
			RandomNumberGenerator RNG = RandomNumberGenerator.Create();
			byte[] buf = new byte[m_lenght];
			RNG.GetBytes(buf);
			
			return util.Hash.HexToString(buf);
		}
	}
}
#endif