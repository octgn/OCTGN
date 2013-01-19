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

using System.Text;
#if !CF
using System.Security.Cryptography;
#endif

namespace agsXMPP.Util
{
	/// <summary>
	/// Helper class for hashing.
	/// </summary>
	public class Hash
	{		

		#region << SHA1 Hash Desktop Framework and Mono >>		
#if !CF
		public static string Sha1Hash(string pass)
		{			
			SHA1 sha = SHA1.Create();
			byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(pass));            
			return HexToString(hash);
		}

		public static byte[] Sha1HashBytes(string pass)
		{			
			SHA1 sha = SHA1.Create();
			return sha.ComputeHash(Encoding.UTF8.GetBytes(pass));
		}

        public static byte[] Sha1HashBytes(byte[] pass)
        {
            using (var sha = new SHA1Managed())
            {
                return sha.ComputeHash(pass);
            }
        }
#endif

		/// <summary>
		/// Converts all bytes in the Array to a string representation.
		/// </summary>
		/// <param name="buf"></param>
		/// <returns>string representation</returns>
		public static string HexToString(byte[] buf)
		{			
			StringBuilder sb = new StringBuilder();
			foreach (byte b in buf)
			{
				sb.Append(b.ToString("x2"));
			}
			return sb.ToString();            
		}
		
		#endregion


		#region << SHA1 Hash Compact Framework >>
#if CF
		

		/// <summary>
		/// return a SHA1 Hash on PPC and Smartphone
		/// </summary>
		/// <param name="pass"></param>
		/// <returns></returns>
		public static byte[] Sha1Hash(byte[] pass)
		{
			IntPtr hProv;
			bool retVal = WinCeApi.CryptAcquireContext( out hProv, null, null, (int) WinCeApi.SecurityProviderType.RSA_FULL, 0 );
			IntPtr hHash;
			retVal = WinCeApi.CryptCreateHash( hProv, (int) WinCeApi.SecurityProviderType.CALG_SHA1, IntPtr.Zero, 0, out hHash );
			
			byte [] publicKey = pass;
			int publicKeyLen = publicKey.Length;
			retVal = WinCeApi.CryptHashData( hHash, publicKey, publicKeyLen, 0 );
			int bufferLen = 20; //SHA1 size
			byte [] buffer = new byte[bufferLen];
			retVal = WinCeApi.CryptGetHashParam( hHash, (int) WinCeApi.SecurityProviderType.HP_HASHVAL, buffer, ref bufferLen, 0 );
			retVal = WinCeApi.CryptDestroyHash( hHash );
			retVal = WinCeApi.CryptReleaseContext( hProv, 0 );
			
			return buffer;
		}

		/// <summary>
		/// return a SHA1 Hash on PPC and Smartphone
		/// </summary>
		/// <param name="pass"></param>
		/// <returns></returns>
		public static string Sha1Hash(string pass)
		{
			return HexToString(Sha1Hash(System.Text.Encoding.ASCII.GetBytes(pass)));		
		}

		/// <summary>
		/// return a SHA1 Hash on PPC and Smartphone
		/// </summary>
		/// <param name="pass"></param>
		/// <returns></returns>
		public static byte[] Sha1HashBytes(string pass)
		{
			return Sha1Hash(System.Text.Encoding.UTF8.GetBytes(pass));
		}

		/// <summary>
		/// omputes the MD5 hash value for the specified byte array.		
		/// </summary>
		/// <param name="pass">The input for which to compute the hash code.</param>
		/// <returns>The computed hash code.</returns>
		public static byte[] MD5Hash(byte[] pass)
		{
			IntPtr hProv;
			bool retVal = WinCeApi.CryptAcquireContext( out hProv, null, null, (int) WinCeApi.SecurityProviderType.RSA_FULL, 0 );
			IntPtr hHash;
			retVal = WinCeApi.CryptCreateHash( hProv, (int) WinCeApi.SecurityProviderType.CALG_MD5, IntPtr.Zero, 0, out hHash );
			
			byte [] publicKey = pass;
			int publicKeyLen = publicKey.Length;
			retVal = WinCeApi.CryptHashData( hHash, publicKey, publicKeyLen, 0 );
			int bufferLen = 16; //SHA1 size
			byte [] buffer = new byte[bufferLen];
			retVal = WinCeApi.CryptGetHashParam( hHash, (int) WinCeApi.SecurityProviderType.HP_HASHVAL, buffer, ref bufferLen, 0 );
			retVal = WinCeApi.CryptDestroyHash( hHash );
			retVal = WinCeApi.CryptReleaseContext( hProv, 0 );

			return buffer;
		}

		#endif
		#endregion

#if !(CF || CF_2)
        public static byte[] HMAC(byte[] key, byte[] data)
        {
            using (var hmacsha1 = new HMACSHA1(key, true))
            {
                byte[] bytes = hmacsha1.ComputeHash(data);
                return bytes;
            }
        }

        public static byte[] HMAC(string key, byte[] data)
        {
            return HMAC(Encoding.UTF8.GetBytes(key), data);
        }

        public static byte[] HMAC(byte[] key, string data)
        {
            return HMAC(key, Encoding.UTF8.GetBytes(data));
        }

        public static byte[] HMAC(string key, string data)
        {
            return HMAC(Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(data));
        }

#endif

    }
}