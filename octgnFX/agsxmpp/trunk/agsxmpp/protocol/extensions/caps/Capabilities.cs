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
using System.Collections;
using System.Text;

using agsXMPP.protocol.iq.disco;

using agsXMPP.Xml.Dom;

namespace agsXMPP.protocol.extensions.caps
{
    /*
        Example 1. Annotated presence sent

        <presence>
          <c xmlns='http://jabber.org/protocol/caps'
             node='http://exodus.jabberstudio.org/caps'
             ver='0.9'/>
        </presence>

        Example 2. Annotated presence sent, with feature extensions

        <presence>
          <c xmlns='http://jabber.org/protocol/caps'
             node='http://exodus.jabberstudio.org/caps'
             ver='0.9'
             ext='jingle ftrans xhtml'/>
        </presence>
        
    */

    /// <summary>
    /// <para>
    /// It is often desirable for a Jabber/XMPP application (commonly but not necessarily a client) to take different actions 
    /// depending on the capabilities of another application from which it receives presence information. Examples include: 
    /// </para> 
    /// <list type="bullet">
    /// <item>
    ///     <term>Showing a different set of icons depending on the capabilities of other clients.</term>
    /// </item>
    /// <item>
    ///     <term>Not sending XHTML-IM content to plaintext clients such as cell phones.</term>
    /// </item>
    /// <item>
    ///     <term>Allowing the initiation of Voice over IP (VoIP) sessions only to clients that support VoIP.</term>
    /// </item>
    /// <item>
    ///     <term>Not showing a "Send a File" button if another user's client does not support File Transfer.</term>
    /// </item>
    /// </list>
    /// <para>
    /// Recently, some existing Jabber clients have begun sending Software Version requests to each entity from which they 
    /// receive presence. That solution is impractical on a larger scale, particularly for users or applications with large rosters. 
    /// This document proposes a more robust and scalable solution: namely, a presence-based mechanism for exchanging information 
    /// about entity capabilities.
    /// </para>
    /// </summary>
    public class Capabilities : Element
    {
        /// <summary>
        /// 
        /// </summary>
        public Capabilities()
        {
            this.TagName    = "c";
            this.Namespace  = Uri.CAPS;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="version"></param>
        /// <param name="node"></param>
        public Capabilities(string version, string node) : this()
        {
            Version = version;
            Node    = node;
        }

        /// <summary>
        /// Required node attribute
        /// </summary>
        public string Node
        {
            get { return GetAttribute("node"); }
            set { SetAttribute("node", value); }
        }

        /// <summary>
        /// Required version attribute
        /// </summary>
        public string Version
        {
            get { return GetAttribute("ver"); }
            set { SetAttribute("ver", value); }
        }

        /// <summary>
        /// Required Hash-Type.
        /// </summary>
        public string Hash
        {
            get { return GetAttribute("hash"); }
            set { SetAttribute("hash", value); }
        }

        [Obsolete("This property is deprecated with version 1.4 of XEP-0115. You shouldn't use this propety anymore.")]
        public string[] Extensions
        {
            get 
            {
                string ext = GetAttribute("ext");
                if (ext != null)
                {
                    string[] ret = ext.Split(' ');
                    return ret;
                }
                else
                {
                    return null;
                }
            }
            set 
            {
                if (value != null)
                {
                    string temp = null;
                    for (int i = 0; i < value.Length; i++)
                    {
                        temp += value[i];
                        if (i < value.Length - 1)
                            temp += " ";                            
                    }                    
                    SetAttribute("ext", temp);
                }
            }
        }

        /// <summary>
        /// Builds and sets the caps ver attribute from a DiscoInfo object
        /// </summary>
        /// <param name="di"></param>
        public void SetVersion(DiscoInfo di)
        {
            Hash = "sha-1";
            Version = BuildCapsVersion(di);
        }

        private string BuildCapsVersion(DiscoInfo di)
        {
            /*
                1.  Initialize an empty string S.
                2. Sort the service discovery identities by category and then by type (if it exists), formatted as 'category' '/' 'type'.
                3. For each identity, append the 'category/type' to S, followed by the '<' character.
                4. Sort the supported features.
                5. For each feature, append the feature to S, followed by the '<' character.
                6. Compute ver by hashing S using the SHA-1 algorithm as specified in RFC 3174 [17] (with binary output) and 
                   encoding the hash using Base64 as specified in Section 4 of RFC 4648 [18] 
                   (note: the Base64 output MUST NOT include whitespace and MUST set padding bits to zero). [19]
             */
            ArrayList features      = new ArrayList();
            ArrayList identities    = new ArrayList();

            foreach (DiscoIdentity did in di.GetIdentities())
                identities.Add(did.Type == null ? did.Category : did.Category + "/" + did.Type);

            foreach (DiscoFeature df in di.GetFeatures())
                features.Add(df.Var);
            
            identities.Sort();
            features.Sort();            

            StringBuilder S = new StringBuilder();

            foreach (string s in identities)
                S.Append(s + "<");

            foreach (string s in features)
                S.Append(s + "<");

            byte[] sha1 = Util.Hash.Sha1HashBytes(S.ToString());

#if CF
            return Convert.ToBase64String(sha1, 0, sha1.Length);
#else
            return Convert.ToBase64String(sha1);
#endif
        }

        #region << Extension Helpers >>
        public void AddExtension(string ext)
        {
            string[] extensions = this.Extensions;
            // check if the extension already exists
            if (extensions != null && Array.IndexOf(extensions, ext, extensions.GetLowerBound(0), extensions.Length) >= 0)
                return;

            int size = extensions == null ? 1 : extensions.Length + 1;
            string[] tmpExtensions = new string[size];
            if (size > 1)
                extensions.CopyTo(tmpExtensions, 0);           
            
            tmpExtensions[size - 1] = ext;            
            this.Extensions = tmpExtensions;
        }

        public void RemoveExtension(string ext)
        {
            string[] extensions = this.Extensions;
            if (extensions != null)
            {
                if (Array.IndexOf(extensions, ext, extensions.GetLowerBound(0), extensions.Length) >= 0)
                {
                    int i = 0;
                    string[] tmpExtensions = new string[extensions.Length -1];
                    foreach (string s in extensions)
                    {
                        if (s != ext)
                            tmpExtensions[i++] = s;
                    }
                    this.Extensions = tmpExtensions;
                }
            }            
        }

        public bool ContainsExtension(string ext)
        {
            string[] extensions = this.Extensions;
            if (extensions == null)
                return false;
            
            if (Array.IndexOf(extensions, ext, extensions.GetLowerBound(0), extensions.Length) >= 0)            
                return true;
            else
                return false;
        }
        #endregion
    }
}