/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * Copyright (c) 2003-2007 by AG-Software 											 *
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

#if CF
using System;

namespace agsXMPP.util
{
	/// <summary>
	/// Implements a cryptographic Random Number Generator (RNG) using the implementation 
	/// provided by the cryptographic service provider (CSP).
	/// Its a replacement for System.Security.Cryptography.RandomNumberGenerator which
	/// is not available in the compact framework.
	/// </summary>
	public class RNGCryptoServiceProvider : RandomNumberGenerator
	{
		public RNGCryptoServiceProvider()
		{

		}

		/// <summary>
		/// Fills an array of bytes with a cryptographically strong random sequence of values.
		/// </summary>
		/// <param name="seed">The array to fill with cryptographically strong random bytes.</param>
		public override void GetBytes(byte[] seed)
		{
			seed = _GetRandomBytes(seed);
		}

		/// <summary>
		/// Fills an array of bytes with a cryptographically strong random sequence of nonzero values.
		/// </summary>
		/// <param name="seed">The array to fill with cryptographically strong random nonzero bytes.</param>
		public override void GetNonZeroBytes(byte[] seed)
		{
			seed = _GetNonZeroBytes(seed);
		}
		
		#region << private functions >>
		private byte [] _GetRandomBytes(byte[] seed)
		{			
			IntPtr prov;
			bool retVal = WinCeApi.CryptAcquireContext(out prov, null, null, (int) WinCeApi.SecurityProviderType.RSA_FULL, 0);
			retVal = _CryptGenRandom(prov, seed.Length, seed);			
			WinCeApi.CryptReleaseContext(prov, 0);
			return seed;
		}

		private bool _CryptGenRandom(IntPtr hProv, int dwLen, byte[] pbBuffer)
		{
			if(System.Environment.OSVersion.Platform == PlatformID.WinCE)
				return WinCeApi.CryptGenRandomCe(hProv, dwLen, pbBuffer);
			else
				return WinCeApi.CryptGenRandomXp(hProv, dwLen, pbBuffer);
		}

		private byte [] _GetNonZeroBytes(byte[] seed)
		{
			byte [] buf = _GetRandomBytes(seed);
			for(int i=0; i<buf.Length; i++)
			{
				if(buf[i] == 0)
					buf[i] = 1;
			}
			return buf;
		}
		#endregion

	}
}
#endif