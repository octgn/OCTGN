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

using agsXMPP.Xml.Dom;

namespace agsXMPP.protocol.iq.browse
{
	/// <summary>
	/// Historically each category was used as the name of an element, 
	/// and the type was an attribute, such as &lt;service type="aim"/&gt;. 
	/// The proper expression for all new implementations supporting this specification is to express the type information 
	/// as attributes on a generic item element: &lt;item category="service" type="aim"/&gt;. 
	/// When processing returned browse information this new syntax should always be handled first, 
	/// and the old syntax only used if it is important to be able to access older implementations.
	/// Additional unofficial categories or types may be specified by prefixing their name with 
	/// an "x-", such as "service/x-virgeim" or "x-location/gps". 
	/// Changes to the official categories and subtypes may be defined either by revising this JEP or by activating another JEP.
	/// Removal of a category or subtype must be noted in this document.
	/// </summary>
	public class Service : Element
	{
		/*
		<iq from="myjabber.net" xmlns="jabber:client" id="agsXMPP_5" type="result" to="gnauck@myjabber.net/myJabber v3.5">

			<service name="myJabber Server" jid="myjabber.net" type="jabber" xmlns="jabber:iq:browse"> 
				
				<item version="0.6.0" name="Public Conferencing" jid="conference.myjabber.net" type="public" category="conference"> 
					<ns>http://jabber.org/protocol/muc</ns> 
				</item> 

				<service name="AIM Transport" jid="aim.myjabber.net" type="aim"> 
					<ns>jabber:iq:gateway</ns> 
					<ns>jabber:iq:register</ns> 
				</service> 

				<service name="Yahoo! Transport" jid="yahoo.myjabber.net" type="yahoo"> 
					<ns>jabber:iq:gateway</ns> 
					<ns>jabber:iq:register</ns> 
				</service> 

				<service name="ICQ Transport" jid="icq.myjabber.net" type="icq"> 
					<ns>jabber:iq:gateway</ns> 
					<ns>jabber:iq:register</ns> 
				</service> 

				<service name="MSN Transport" jid="msn.myjabber.net" type="msn"> 
					<ns>jabber:iq:gateway</ns> 
					<ns>jabber:iq:register</ns> 
				</service> 

				<item name="Online Users" jid="myjabber.net/admin"/>				
				<ns>jabber:iq:admin</ns>
			</service>
		</iq> 
		*/
		public Service()
		{
			this.TagName	= "service";
			this.Namespace	= Uri.IQ_BROWSE;
		}


		public string Name
		{
			get { return GetAttribute("name"); }
			set { SetAttribute("name", value); }
		}

		public Jid Jid
		{
			get { return new Jid(GetAttribute("jid")); }
			set { SetAttribute("jid", value.ToString()); }
		}

		public string Type
		{
			get { return GetAttribute("type"); }
			set { SetAttribute("type", value); }
		}

		/// <summary>
		/// Gets all advertised namespaces of this service
		/// </summary>
		/// <returns>string array that contains the advertised namespaces</returns>
		public string[] GetNamespaces()
		{
            ElementList elements = SelectElements("ns");
			string[] nss = new string[elements.Count];
			
			int i=0;
			foreach (Element ns in elements)
			{
				nss[i] = ns.Value;
				i++;
			}

			return nss;
		}
		
		public BrowseItem[] GetItems()
		{
            ElementList nl = SelectElements(typeof(BrowseItem));
			BrowseItem[] items = new BrowseItem[nl.Count];
			int i = 0;
			foreach (Element item in nl)
			{
				items[i] = item as BrowseItem;
				i++;
			}
			return items;
		}

		/// <summary>
		/// Gets all "ChilsServices" od this service
		/// </summary>
		/// <returns></returns>
		public Service[] GetServices()
		{
            ElementList nl = SelectElements(typeof(Service));
			Service[] Services = new Service[nl.Count];
			int i = 0;
			foreach (Element service in nl)
			{
				Services[i] = service as Service;
				i++;
			}
			return Services;
		}
	}
}
