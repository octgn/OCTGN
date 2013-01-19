/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * Copyright (c) 2003-2008 by AG-Software 											 *
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

#if BCCRYPTO
using System;

using Org.BouncyCastle.Crypto.Tls;

namespace agsXMPP.net
{
    internal class CertificateVerifier : ICertificateVerifyer
    {
        internal event BaseSocket.CertificateValidationCallback OnVerifyCertificate;
        #region ICertificateVerifyer Members
        
        public bool IsValid(Org.BouncyCastle.Asn1.X509.X509CertificateStructure[] certs)
        {
            if (OnVerifyCertificate != null)
                return OnVerifyCertificate(certs);
            else           
                return true;
        }

        #endregion
    }
}
#endif