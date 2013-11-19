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

using agsXMPP.protocol.client;

using agsXMPP.Xml.Dom;

namespace agsXMPP.protocol.extensions.featureneg
{
	/// <summary>
	/// JEP-0020: Feature Negotiation
	/// This JEP defines a A protocol that enables two Jabber entities to mutually negotiate feature options.
	/// </summary>
	public class FeatureNegIq : IQ
	{
		/*
		<iq type='get'
			from='romeo@montague.net/orchard'
			to='juliet@capulet.com/balcony'
			id='neg1'>
			<feature xmlns='http://jabber.org/protocol/feature-neg'>
				<x xmlns='jabber:x:data' type='form'>
					<field type='list-single' var='places-to-meet'>
						<option><value>Lover's Lane</value></option>
						<option><value>Secret Grotto</value></option>
						<option><value>Verona Park</value></option>
					</field>
					<field type='list-single' var='times-to-meet'>
						<option><value>22:00</value></option>
						<option><value>22:30</value></option>
						<option><value>23:00</value></option>
						<option><value>23:30</value></option>
					</field>
				</x>
			</feature>
		</iq>
		*/

		private FeatureNeg m_FeatureNeg = new FeatureNeg();

		public FeatureNegIq()
		{		
			this.AddChild(m_FeatureNeg);
			this.GenerateId();
		}

		public FeatureNegIq(IqType type) : this()
		{			
			this.Type = type;		
		}	

		public FeatureNeg FeatureNeg
		{
			get
			{
				return m_FeatureNeg;
			}
		}
	}
}
