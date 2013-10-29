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
using System.IO;


using agsXMPP.Xml.Dom;
using agsXMPP.protocol.iq.vcard;

// JEP-0054
// http://www.jabber.org/jeps/jep-0054.html

// Example 2. Receiving One's Own vCard
//
//	<iq 
//		to='stpeter@jabber.org/Gabber'
//		type='result'
//		id='v1'>
//	<vCard xmlns='vcard-temp'>
//		<FN>Peter Saint-Andre</FN>
//		<N>
//			<FAMILY>Saint-Andre<FAMILY>
//			<GIVEN>Peter</GIVEN>
//			<MIDDLE/>
//		</N>
//		<NICKNAME>stpeter</NICKNAME>
//		<URL>http://www.jabber.org/people/stpeter.php</URL>
//		<BDAY>1966-08-06</BDAY>
//		<ORG>
//		<ORGNAME>Jabber Software Foundation</ORGNAME>
//		<ORGUNIT/>
//		</ORG>
//		<TITLE>Executive Director</TITLE>
//		<ROLE>Patron Saint</ROLE>
//		<TEL><VOICE/><WORK/><NUMBER>303-308-3282</NUMBER></TEL>
//		<TEL><FAX/><WORK/><NUMBER/></TEL>
//		<TEL><MSG/><WORK/><NUMBER/></TEL>
//		<ADR>
//			<WORK/>
//			<EXTADD>Suite 600</EXTADD>
//			<STREET>1899 Wynkoop Street</STREET>
//			<LOCALITY>Denver</LOCALITY>
//			<REGION>CO</REGION>
//			<PCODE>80202</PCODE>
//			<CTRY>USA</CTRY>
//		</ADR>
//		<TEL><VOICE/><HOME/><NUMBER>303-555-1212</NUMBER></TEL>
//		<TEL><FAX/><HOME/><NUMBER/></TEL>
//		<TEL><MSG/><HOME/><NUMBER/></TEL>
//		<ADR>
//			<HOME/>
//			<EXTADD/>
//			<STREET/>
//			<LOCALITY>Denver</LOCALITY>
//			<REGION>CO</REGION>
//			<PCODE>80209</PCODE>
//			<CTRY>USA</CTRY>
//		</ADR>
//		<EMAIL><INTERNET/><PREF/><USERID>stpeter@jabber.org</USERID></EMAIL>
//		<JABBERID>stpeter@jabber.org</JABBERID>
//		<DESC>
//			More information about me is located on my 
//			personal website: http://www.saint-andre.com/
//		</DESC>		
//		</vCard>
//</iq>
//    
namespace agsXMPP.protocol.iq.vcard
{
	//	<!-- Telephone number property. -->
	//	<!ELEMENT TEL (
	//	HOME?, 
	//	WORK?, 
	//	VOICE?, 
	//	FAX?, 
	//	PAGER?, 
	//	MSG?, 
	//	CELL?, 
	//	VIDEO?, 
	//	BBS?, 
	//	MODEM?, 
	//	ISDN?, 
	//	PCS?, 
	//	PREF?, 
	//	NUMBER
	//	)>



	/// <summary>
	/// Summary description for Vcard.
	/// </summary>
	public class Vcard : Element
	{
		#region << Constructors >>
		public Vcard()
		{
			this.TagName	= "vCard";
			this.Namespace	= Uri.VCARD;
		}
		#endregion

		/// <summary>
		///
		/// </summary>
		public string Url
		{
			get { return GetTag("URL"); }
			set { SetTag("URL", value); }
		}

		/// <summary>
		/// 
		/// </summary>
		public DateTime Birthday
		{
			get 
			{ 
				try
				{
                    string sDate = GetTag("BDAY");
                    if (sDate != null)
					    return DateTime.Parse(sDate);
                    else
                        return DateTime.MinValue;
				}
				catch
				{
					return DateTime.MinValue;
				}
			}
			set { SetTag("BDAY", value.ToString("yyyy-MM-dd")); }            
		}
		
		/// <summary>
		/// 
		/// </summary>
		public string Title
		{
			get { return GetTag("TITLE"); }
			set { SetTag("TITLE", value); }
		}

		/// <summary>
		/// 
		/// </summary>
		public string Role
		{
			get { return GetTag("ROLE"); }
			set { SetTag("ROLE", value); }
		}

		public string Fullname
		{
			get { return GetTag("FN"); }
			set { SetTag("FN", value); }
		}

		public string Nickname
		{
			get { return GetTag("NICKNAME"); }
			set { SetTag("NICKNAME", value); }
		}

		public Jid JabberId
		{
			get
			{
				return new Jid(GetTag("JABBERID"));
			}
			set
			{
				SetTag("JABBERID", value.ToString());
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public string Description
		{
			get { return GetTag("DESC"); }
			set { SetTag("DESC", value); }
		}

		/// <summary>
		/// 
		/// </summary>
		public Name Name
		{
			get { return SelectSingleElement(typeof(Name)) as Name; }
			set 
            {
                Element n = SelectSingleElement(typeof(Name));
                if (n != null)
                    n.Remove();

                AddChild(value);                
            }
		}

#if !CF
        /// <summary>
        /// a Photograph
        /// </summary>
		public Photo Photo
		{
			get { return SelectSingleElement(typeof(Photo)) as Photo; }
			set 
            {
                Element p = SelectSingleElement(typeof(Photo));
                if (p != null)
                    p.Remove();

                AddChild(value);              
            }
		}
#endif

		/// <summary>
		/// 
		/// </summary>
		public Organization Organization
		{
			get { return SelectSingleElement(typeof(Organization)) as Organization; }
			set 
            {
                Element org = SelectSingleElement(typeof(Organization));
                if (org != null)
                    org.Remove();

                AddChild(value);           
            }
		}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Address[] GetAddresses()
		{
            ElementList el = SelectElements(typeof(Address));
            int i = 0;
            Address[] result = new Address[el.Count];
            foreach (Address add in el)
            {
                result[i] = add;
                i++;
            }
            return result;			
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loc"></param>
        /// <returns></returns>
		public Address GetAddress(AddressLocation loc)
		{
			foreach (Address adr in GetAddresses())
			{
				if (adr.Location == loc)
					return adr;
			}
			return null;
		}


        public void AddAddress(Address addr)
        {
            Address a = GetAddress(addr.Location);
            if (a != null)
                a.Remove();

            this.AddChild(addr);
        }

		public Address GetPreferedAddress()
		{
			foreach (Address adr in GetAddresses())
			{
				if (adr.IsPrefered)
					return adr;
			}
			return null;
		}
              
   
        public Telephone[] GetTelephoneNumbers()
        {
            ElementList el = SelectElements(typeof(Telephone));
            int i = 0;
            Telephone[] result = new Telephone[el.Count];
            foreach (Telephone tel in el)
            {
                result[i] = tel;
                i++;
            }
            return result;
        }

		public Telephone GetTelephoneNumber(TelephoneType type, TelephoneLocation loc)
		{
			foreach (Telephone phone in GetTelephoneNumbers())
			{
				if (phone.Type == type && phone.Location == loc)
					return phone;
			}
			return null;
		}

		public void AddTelephoneNumber(Telephone tel)
		{
            Telephone t = GetTelephoneNumber(tel.Type, tel.Location);
            if (t != null)
                t.Remove();

            this.AddChild(tel);	
		}

		/// <summary>
		/// Adds a new Email Adress object
		/// </summary>
		/// <param name="mail"></param>
		public void AddEmailAddress(Email mail)
		{
            Email e = GetEmailAddress(mail.Type);
            if (e != null)
                e.Remove();

			this.AddChild(mail);
		}
               
        /// <summary>
        /// Get all Email addresses
        /// </summary>
        /// <returns></returns>
        public Email[] GetEmailAddresses()
        {
            ElementList el = SelectElements(typeof(Email));
            int i = 0;
            Email[] result = new Email[el.Count];
            foreach (Email mail in el)
            {
                result[i] = mail;
                i++;
            }
            return result;
        }

		public Email GetEmailAddress(EmailType type)
		{
			foreach (Email email in GetEmailAddresses())
			{
				if (email.Type == type)
					return email;
			}
			return null;
		}

		public Email GetPreferedEmailAddress()
		{
			foreach (Email email in GetEmailAddresses())
			{
				if (email.IsPrefered)
					return email;
			}
			return null;
		}
	}
	
}
