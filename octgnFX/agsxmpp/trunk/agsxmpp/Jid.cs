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
using System.Text;
using System.Collections;

using agsXMPP.Exceptions;
using agsXMPP.Collections;
#if STRINGPREP
using agsXMPP.Idn;
#endif

namespace agsXMPP
{	
	/// <summary>
	/// Class for building and handling XMPP Id's (JID's)
	/// </summary>  
    public class Jid : IComparable, IEquatable<Jid>

	{
        /*		
        14 possible invalid forms of JIDs and some variations on valid JIDs with invalid lengths, viz:

        jidforms = [
            "",
            "@",
            "@/resource",
            "@domain",
            "@domain/",
            "@domain/resource",
            "nodename@",
            "/",
            "nodename@domain/",
            "nodename@/",
            "@/",
            "nodename/",
            "/resource",
            "nodename@/resource",
        ]
        

        TODO
        Each allowable portion of a JID (node identifier, domain identifier, and resource identifier) MUST NOT
        be more than 1023 bytes in length, resulting in a maximum total size
        (including the '@' and '/' separators) of 3071 bytes.
            
        stringprep with libIDN        
        m_User      ==> nodeprep
        m_Server    ==> nameprep
        m_Resource  ==> resourceprep
        */
        
        // !!! 
        // use this internal variables only if you know what you are doing
        // !!!
        internal string m_Jid      = null;
		internal string m_User     = null;
		internal string m_Server   = null;
		internal string m_Resource = null;

		/// <summary>
		/// Create a new JID object from a string. The input string must be a valid jabberId and already prepared with stringprep.
        /// Otherwise use one of the other constructors with escapes the node and prepares the gives balues with the stringprep
        /// profiles
		/// </summary>
		/// <param name="jid">XMPP ID, in string form examples: user@server/Resource, user@server</param>
		public Jid(string jid)
		{			
			m_Jid = jid;
			Parse(jid);
		}

        /// <summary>
        /// builds a new Jid object
        /// </summary>
        /// <param name="user">XMPP User part</param>
        /// <param name="server">XMPP Domain part</param>
        /// <param name="resource">XMPP Resource part</param>        
        public Jid(string user, string server, string resource)
        {
#if !STRINGPREP
            if (user != null)
            {
                user = EscapeNode(user);
                
                m_User = user.ToLower();
            }

            if (server != null)
                m_Server = server.ToLower();

            if (resource != null)
                m_Resource = resource;
#else
            if (user != null)
            {             
                user = EscapeNode(user);

                m_User = Stringprep.NodePrep(user);
            }

            if (server != null)
                m_Server = Stringprep.NamePrep(server);

            if (resource != null)
                m_Resource = Stringprep.ResourcePrep(resource);
#endif
            BuildJid();            
        }
                
        /// <summary>
        /// Parses a JabberId from a string. If we parse a jid we assume it's correct and already prepared via stringprep.
        /// </summary>
        /// <param name="fullJid">jis to parse as string</param>
        /// <returns>true if the jid could be parsed, false if an error occured</returns>
		public bool Parse(string fullJid)
		{
			string user		= null;
			string server	= null;
			string resource = null;

            try
            {
                if (fullJid == null || fullJid.Length == 0)
                {
                    return false;
                }

                m_Jid = fullJid;

                int atPos = m_Jid.IndexOf('@');
                int slashPos = m_Jid.IndexOf('/');

                // some more validations
                // @... or /...
                if (atPos == 0 || slashPos == 0)
                    return false;

                // nodename@
                if (atPos + 1 == fullJid.Length)
                    return false;

                // @/ at followed by resource separator
                if (atPos + 1 == slashPos)
                    return false;

                if (atPos == -1)
                {
                    user = null;
                    if (slashPos == -1)
                    {
                        // JID Contains only the Server
                        server = m_Jid;
                    }
                    else
                    {
                        // JID Contains only the Server and Resource
                        server = m_Jid.Substring(0, slashPos);
                        resource = m_Jid.Substring(slashPos + 1);
                    }
                }
                else
                {
                    if (slashPos == -1)
                    {
                        // We have no resource
                        // Devide User and Server (user@server)
                        server = m_Jid.Substring(atPos + 1);
                        user = m_Jid.Substring(0, atPos);
                    }
                    else
                    {
                        // We have all
                        user = m_Jid.Substring(0, atPos);
                        server = m_Jid.Substring(atPos + 1, slashPos - atPos - 1);
                        resource = m_Jid.Substring(slashPos + 1);
                    }
                }

                if (user != null)
                    this.m_User = user;
                if (server != null)
                    this.m_Server = server;
                if (resource != null)
                    this.m_Resource = resource;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
		}               
        
        internal void BuildJid()
        {
            m_Jid = BuildJid(m_User, m_Server, m_Resource);
        }

        private string BuildJid(string user, string server, string resource)
		{			
			StringBuilder sb = new StringBuilder();
			if (user != null)
			{
				sb.Append(user);
				sb.Append("@");
			}
			sb.Append(server);
			if (resource != null)
			{
				sb.Append("/");
				sb.Append(resource);
			}
			return sb.ToString();
		}
        
		public override string ToString()
		{
			return m_Jid;
		}
				
		/// <summary>
		/// the user part of the JabberId.
		/// </summary>
        public string User
		{
			get
			{				
				return m_User;
			}
			set
			{
                // first Encode the user/node
                string tmpUser = EscapeNode(value);
#if !STRINGPREP
                if (value != null)
				    m_User = tmpUser.ToLower();
                else
                    m_User = null;
#else
                if (value != null)
                    m_User = Stringprep.NodePrep(tmpUser);
                else
                    m_User = null;
#endif
                BuildJid();				
			}
		}

		/// <summary>
		/// Only Server
		/// </summary>
        public string Server
		{
			get
			{
				return m_Server;
			}
			set 
			{			
#if !STRINGPREP
                if (value != null)
				    m_Server = value.ToLower();
                else
                    m_Server = null;
#else
                if (value != null)
                    m_Server = Stringprep.NamePrep(value);
                else
                    m_Server = null;
#endif
                BuildJid();                
			}
		}

		/// <summary>
		/// Only the Resource field.
		/// null for none
		/// </summary>        
        public string Resource
		{
			get
			{				
				return m_Resource;
			}
			set 
			{			
#if !STRINGPREP
                if (value != null)
				    m_Resource = value;
                else
                    m_Resource = null;
#else
                if (value != null)
                    m_Resource = Stringprep.ResourcePrep(value);
                else
                    m_Resource = null;
#endif
                BuildJid();				
			}
		}

		/// <summary>
		/// The Bare Jid only (user@server).
		/// </summary>		
        public string Bare
		{
			get
			{				
				return BuildJid(m_User, m_Server, null);
			}
        }

        #region << Overrides >>
        /// <summary>
        /// This compares the full Jid by default
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return Equals(obj, new FullJidComparer());
        }

        public override int GetHashCode()
        {
            int hcode = 0;
            if (m_User  !=null)
                hcode ^= m_User.GetHashCode();

            if (m_Server != null)
                hcode ^= m_Server.GetHashCode();

            if (m_Resource != null)
                hcode ^= m_Resource.GetHashCode();
            
            return hcode;
        }
        #endregion

        public bool Equals(object other, System.Collections.IComparer comparer)
		{
			if (comparer.Compare(other, this) == 0) 
				return true;
			else
				return false;
        }

        /*
        public static bool operator !=(Jid jid1, Jid jid2)
        {
            return !jid1.Equals(jid2, new FullJidComparer());
        }

        public static bool operator ==(Jid jid1, Jid jid2)
        {
            return jid1.Equals(jid2, new FullJidComparer());
        }
        */

        #region << implicit operators >>
        static public implicit operator Jid(string value)
        {
            return new Jid(value);
        }

        static public implicit operator string(Jid jid)
        {
            return jid.ToString();
        }
        #endregion

        #region IComparable Members
        public int CompareTo(object obj)
        {
            if (obj is Jid)
            {
                Jid jid = obj as Jid;
                FullJidComparer comparer = new FullJidComparer();
                return comparer.Compare(obj, this);                 
            }
            throw new ArgumentException("object is not a Jid");    
        }
        #endregion
   

        #region IEquatable<Jid> Members
        public bool Equals(Jid other)
        {
            FullJidComparer comparer = new FullJidComparer();
            if (comparer.Compare(other, this) == 0)
                return true;
            else
                return false;            
        }
        #endregion


        #region << XEP-0106: JID Escaping >>
        /// <summary>
        /// <para>
        /// Escape a node according to XEP-0106
        /// </para>
        /// <para>
        /// <a href="http://www.xmpp.org/extensions/xep-0106.html">http://www.xmpp.org/extensions/xep-0106.html</a>
        /// </para>        
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static string EscapeNode(string node)
        {
            if (node == null)
                return null;

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < node.Length; i++)
            {
                /*
                <space> \20
                " 	    \22
                & 	    \26
                ' 	    \27
                / 	    \2f
                : 	    \3a
                < 	    \3c
                > 	    \3e
                @ 	    \40
                \ 	    \5c
                */
                char c = node[i];
                switch (c)
                {
                    case ' ': sb.Append(@"\20"); break;
                    case '"': sb.Append(@"\22"); break;
                    case '&': sb.Append(@"\26"); break;
                    case '\'': sb.Append(@"\27"); break;
                    case '/': sb.Append(@"\2f"); break;
                    case ':': sb.Append(@"\3a"); break;
                    case '<': sb.Append(@"\3c"); break;
                    case '>': sb.Append(@"\3e"); break;
                    case '@': sb.Append(@"\40"); break;
                    case '\\': sb.Append(@"\5c"); break;
                    default: sb.Append(c); break;
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// <para>
        /// unescape a node according to XEP-0106
        /// </para>
        /// <para>
        /// <a href="http://www.xmpp.org/extensions/xep-0106.html">http://www.xmpp.org/extensions/xep-0106.html</a>
        /// </para>        
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static string UnescapeNode(string node)
        {
            if (node == null)
                return null;

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < node.Length; i++)
            {
                char c1 = node[i];
                if (c1 == '\\' && i + 2 < node.Length)
                {
                    i += 1;
                    char c2 = node[i];
                    i += 1;
                    char c3 = node[i];
                    if (c2 == '2')
                    {
                        switch (c3)
                        {
                            case '0':
                                sb.Append(' ');
                                break;
                            case '2':
                                sb.Append('"');
                                break;
                            case '6':
                                sb.Append('&');
                                break;
                            case '7':
                                sb.Append('\'');
                                break;
                            case 'f':
                                sb.Append('/');
                                break;
                        }
                    }
                    else if (c2 == '3')
                    {
                        switch (c3)
                        {
                            case 'a':
                                sb.Append(':');
                                break;
                            case 'c':
                                sb.Append('<');
                                break;
                            case 'e':
                                sb.Append('>');
                                break;
                        }
                    }
                    else if (c2 == '4')
                    {
                        if (c3 == '0')
                            sb.Append("@");
                    }
                    else if (c2 == '5')
                    {
                        if (c3 == 'c')
                            sb.Append("\\");
                    }
                }
                else
                    sb.Append(c1);
            }
            return sb.ToString();
        }

        #endregion
    }
}