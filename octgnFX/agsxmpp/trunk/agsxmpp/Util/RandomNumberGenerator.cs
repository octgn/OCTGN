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
	/// Represents the abstract class from which all implementations of cryptographic random number generators derive.
	/// Its a replacement for System.Security.RandomNumberGenerator
	/// which is not abailavle on the compact framework
	/// </summary>
	public abstract class RandomNumberGenerator
	{
		public RandomNumberGenerator()
		{

		}

		/// <summary>
		/// Creates an instance of an implementation of a cryptographic random number generator.
		/// </summary>
		/// <returns>a new instance of a cryptographic random number generator.</returns>
		public static RandomNumberGenerator Create()
		{
			return new RNGCryptoServiceProvider();	
		}
		
		/// <summary>
		/// When overridden in a derived class, fills an array of bytes with a cryptographically strong random sequence of values.
		/// </summary>
		/// <param name="data">The array to fill with cryptographically strong random bytes.</param>
		public abstract void GetBytes(byte[] data);

		/// <summary>
		/// When overridden in a derived class, fills an array of bytes with a cryptographically strong random sequence of nonzero values.
		/// </summary>
		/// <param name="data">The array to fill with cryptographically strong random nonzero bytes.</param>
		public abstract void GetNonZeroBytes(byte[] data);
	}
}
#endif