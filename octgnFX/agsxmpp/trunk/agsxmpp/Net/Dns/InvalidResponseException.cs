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

//
// Bdev.Net.Dns by Rob Philpott, Big Developments Ltd. Please send all bugs/enhancements to
// rob@bigdevelopments.co.uk  This file and the code contained within is freeware and may be
// distributed and edited without restriction.
// 

using System;

namespace agsXMPP.Net.Dns
{
	/// <summary>
	/// Thrown when the server delivers a response we are not expecting to hear
	/// </summary>	
	public class InvalidResponseException : SystemException
	{
		public InvalidResponseException()
		{
			// no implementation
		}

		public InvalidResponseException(Exception innerException) :  base(null, innerException) 
		{
			// no implementation
		}

		public InvalidResponseException(string message, Exception innerException) : base (message, innerException)
		{
			// no implementation
		}
        
        //protected InvalidResponseException(SerializationInfo info, StreamingContext context) : base(info, context)
        //{
        //    // no implementation
        //}
	}
}
