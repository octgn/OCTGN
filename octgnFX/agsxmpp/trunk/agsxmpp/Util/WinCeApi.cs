using System;
using System.Runtime.InteropServices;

namespace agsXMPP.util
{
	/// <summary>
	/// Crypto API for Windows CE, Pocket PC and Smartphone
	/// will be used for Hashing and the RandomNumberGenerator
	/// </summary>
	internal class WinCeApi
	{
		public enum SecurityProviderType
		{
			RSA_FULL		    = 1,
			HP_HASHVAL		    = 2,
			CALG_MD5		    = 32771,
			CALG_SHA1		    = 32772            
		}        

		[DllImport("coredll.dll")]
		public static extern bool CryptAcquireContext(out IntPtr hProv, string pszContainer, string pszProvider, int dwProvType,int dwFlags);
		
		[DllImport("coredll.dll")]
		public static extern bool CryptCreateHash(IntPtr hProv, int Algid, IntPtr hKey, int dwFlags, out IntPtr phHash);
		
		[DllImport("coredll.dll")]
		public static extern bool CryptHashData(IntPtr hHash, byte [] pbData, int dwDataLen, int dwFlags);
		
		[DllImport("coredll.dll")]
		public static extern bool CryptGetHashParam(IntPtr hHash, int dwParam, byte[] pbData, ref int pdwDataLen, int dwFlags);
		
		[DllImport("coredll.dll")]
		public static extern bool CryptDestroyHash(IntPtr hHash);
		
		[DllImport("coredll.dll")]
		public static extern bool CryptReleaseContext(IntPtr hProv, int dwFlags);

		[DllImport("coredll.dll", EntryPoint="CryptGenRandom", SetLastError=true)]
		public static extern bool CryptGenRandomCe(IntPtr hProv, int dwLen, byte[] pbBuffer);
		
		[DllImport("advapi32.dll", EntryPoint="CryptGenRandom", SetLastError=true)]
		public static extern bool CryptGenRandomXp(IntPtr hProv, int dwLen, byte[] pbBuffer);
		
	}
}
