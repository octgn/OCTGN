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

using agsXMPP.protocol;
using agsXMPP.protocol.client;
using agsXMPP.Collections;

namespace agsXMPP
{	
	public delegate void PresenceCB(object sender, Presence pres, object data);
	
	public class PresenceGrabber : PacketGrabber
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="PresenceGrabber"/> class.
        /// </summary>
        /// <param name="conn">The conn.</param>
		public PresenceGrabber(XmppClientConnection conn)
		{
			m_connection		= conn;			
			conn.OnPresence += new PresenceHandler(m_connection_OnPresence);
		}
        
		public void Add(Jid jid, PresenceCB cb, object cbArg)
		{
            lock (m_grabbing)
            {
                if (m_grabbing.ContainsKey(jid.ToString()))
                    return;
            }

			TrackerData td = new TrackerData();
			td.cb		= cb;
			td.data		= cbArg;
			td.comparer = new BareJidComparer();

            lock (m_grabbing)
            {
                m_grabbing.Add(jid.ToString(), td);
            }
		}

        /// <summary>
        /// Adds the specified jid.
        /// </summary>
        /// <param name="jid">The jid.</param>
        /// <param name="comparer">The comparer.</param>
        /// <param name="cb">The callback.</param>
        /// <param name="cbArg">The callback Arguments.</param>
		public void Add(Jid jid, IComparer comparer, PresenceCB cb, object cbArg)
		{
            lock (m_grabbing)
            {
                if (m_grabbing.ContainsKey(jid.ToString()))
                    return;
            }

			TrackerData td = new TrackerData();
			td.cb		= cb;
			td.data		= cbArg;
			td.comparer = comparer;

            lock (m_grabbing)
            {
                m_grabbing.Add(jid.ToString(), td);
            }
		}

		/// <summary>
		/// Pending request can be removed.
		/// This is useful when a ressource for the callback is destroyed and
		/// we are not interested anymore at the result.
		/// </summary>
		/// <param name="id">ID of the Iq we are not interested anymore</param>
		public void Remove(Jid jid)
		{
            lock (m_grabbing)
            {
                if (m_grabbing.ContainsKey(jid.ToString()))
                    m_grabbing.Remove(jid.ToString());
            }
		}

		private class TrackerData
		{
			public PresenceCB	cb;
			public object		data;
			// by default the Bare Jid is compared
			public IComparer	comparer;
				
		}
		
		/// <summary>
		/// A presence is received. Now check if its from a Jid we are looking for and
		/// raise the event in this case.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="pres"></param>
		private void m_connection_OnPresence(object sender, Presence pres)
		{
			if (pres == null)
				return;
			
			lock (m_grabbing)
			{
				IDictionaryEnumerator myEnum = m_grabbing.GetEnumerator();

				while(myEnum.MoveNext())
				{
					TrackerData t = myEnum.Value as TrackerData;
					if (t.comparer.Compare(new Jid((string)myEnum.Key), pres.From) == 0)
					{
						// Execute the callback
						t.cb(this, pres, t.data);
					}
				}				
			}			
		}
	}
}
