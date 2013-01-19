/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * Copyright (c) 2003-2009 by AG-Software 											 *
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
using System.Security.Principal;
using agsXMPP.Xml.Dom;
using agsXMPP.protocol.sasl;

namespace agsXMPP.Sasl.Gssapi
{
    /// <summary>
    /// Handels the SASL Digest MD5 authentication
    /// </summary>
    public class GssapiMechanism : Mechanism
    {
        SSPIHelper sspiHelper;
        
        public GssapiMechanism()
        {
        }

        public override void Init(XmppClientConnection con)
        {
            XmppClientConnection = con;

            string kerbPrinc = XmppClientConnection.KerberosPrincipal;
            
            /*
             * try to build the kerberos principal if none is sent by the server or provided by the user.
             * XCP send the kerberos pricipal, Openfire doesnt.
             */
            if (kerbPrinc == null)
                kerbPrinc = string.Format("xmpp/{0}@{1}", XmppClientConnection.Server, GetNtDomain());   
            
            //if (XmppClientConnection.KerberosPrincipal != null)
            //    sspiHelper = new SSPIHelper(XmppClientConnection.KerberosPrincipal);
            //else
            //    sspiHelper = new SSPIHelper();

            sspiHelper = new SSPIHelper(kerbPrinc);

            Auth auth = new Auth(MechanismType.GSSAPI);
            
            byte[]  clientToken;

            sspiHelper.Process(null, out clientToken);
           
            auth.Value = Convert.ToBase64String(clientToken);
            
            XmppClientConnection.Send(auth);
        }

        public override void Parse(Node e)
        {
            if (e is Challenge)
            {
                Challenge c = e as Challenge;
                Response resp;

                byte[] outBytes;
                byte[] inBytes = Convert.FromBase64String(c.Value);
               
                sspiHelper.Process(inBytes, out outBytes);

                if (outBytes == null)
                {
                    resp = new Response();
                }
                else
                {
                    resp = new Response();
                    resp.Value = Convert.ToBase64String(outBytes);
                }

                XmppClientConnection.Send(resp);
            }
        }


        /// <summary>
        /// returns the NT domain, tis is used for building the kerberos principal when none is provided.
        /// </summary>
        /// <returns></returns>
        internal string GetNtDomain()
        {
            var curName = WindowsIdentity.GetCurrent().Name;
            var domain = curName.Substring(0, curName.IndexOf('\\'));

            return domain.ToUpper();
        }
    }
}