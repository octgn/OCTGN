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

namespace agsXMPP.Net.Dns
{
	/// <summary>
	/// Summary description for SRVRecord.
	/// </summary>
   public class SRVRecord : RecordBase, IComparable
   {
       /// <summary>
       /// Constructs a NS record by reading bytes from a return message
       /// </summary>
       /// <param name="pointer">A logical pointer to the bytes holding the record</param>
       internal SRVRecord(Pointer pointer)
       {
           m_Priority = pointer.ReadShort();
           m_Weight = pointer.ReadShort();
           m_Port = pointer.ReadShort();
           m_Target = pointer.ReadDomain();
       }
           
        // the fields exposed outside the assembly
        private int     m_Priority;
        private int     m_Weight;
        private int     m_Port;
        private string  m_Target;

        public int Priority	
        { 
          get { return m_Priority; } 
        }

        public int Weight	
        { 
          get { return m_Weight; } 
        }

        public int Port
        { 
          get { return m_Port; } 
        }
      
        public string Target	
        { 
            get { return m_Target; } 
        }
				
        public override string ToString()
        {
			return string.Format("\n   priority   = {0}\n   weight     = {1}\n   port       = {2}\n   target     = {3}",
            m_Priority,
            m_Weight,
            m_Port,
            m_Target);
        }

        /// <summary>
        /// Implements the IComparable interface so that we can sort the SRV records by their
        /// lowest priority
        /// </summary>
        /// <param name="other">the other SRVRecord to compare against</param>
        /// <returns>1, 0, -1</returns>
        public int CompareTo(object obj)
        {
            SRVRecord srvOther = (SRVRecord)obj;

            // we want to be able to sort them by priority from lowest to highest.
            if (m_Priority < srvOther.m_Priority) return -1;
            if (m_Priority > srvOther.m_Priority) return 1;

            // if the priority is the same, sort by highest weight to lowest (higher
            // weighting means that server should get more of the requests)
            if (m_Weight > srvOther.m_Weight) return -1;
            if (m_Weight < srvOther.m_Weight) return 1;

            return 0;
        }
    }
}