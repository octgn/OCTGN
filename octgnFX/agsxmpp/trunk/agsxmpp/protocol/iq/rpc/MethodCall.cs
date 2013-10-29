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

using System.Collections;
using agsXMPP.Xml.Dom;

namespace agsXMPP.protocol.iq.rpc
{    

    /// <summary>
    /// The methodCall element.     
    /// </summary>
    public class MethodCall : Element
    {
        /*
        
         <methodCall>
            <methodName>examples.getStateName</methodName>
            <params>
                <param><value><i4>41</i4></value></param>
            </params>
         </methodCall>        
         
        */

        /// <summary>
        /// 
        /// </summary>
        public MethodCall()
        {
            TagName    = "methodCall";
            Namespace  = Uri.IQ_RPC;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="Params"></param>
        public MethodCall(string methodName, ArrayList Params) : this()
        {
            WriteCall(methodName, Params);            
        }

        /// <summary>
        /// 
        /// </summary>
        public string MethodName
		{
			set	{ SetTag("methodName", value); }
            get { return GetTag("methodName"); }
		}

        /// <summary>
        /// Write the functions call with params to this Element
        /// </summary>
        /// <param name="name"></param>
        /// <param name="Params"></param>
        public void WriteCall(string name, ArrayList Params)
        {
            MethodName = name;

            // remove this tag if exists, in case this function gets
            // calles multiple times by some guys
            RemoveTag("params");
            
            var elParams = RpcHelper.WriteParams(Params);

            if (elParams != null)
                AddChild(elParams);
        }
        
    }
}
