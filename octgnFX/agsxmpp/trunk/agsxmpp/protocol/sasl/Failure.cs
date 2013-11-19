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

// <failure xmlns='urn:ietf:params:xml:ns:xmpp-sasl'>
//		<incorrect-encoding/>
// </failure>
namespace agsXMPP.protocol.sasl
{
	/// <summary>
	/// Summary description for Failure.
	/// </summary>
	public class Failure : Element
	{
		public Failure()
		{
			this.TagName	= "failure";
			this.Namespace	= Uri.SASL;
		}

        public Failure(FailureCondition cond) : this()
        {
            Condition = cond;
        }

        public FailureCondition Condition
        {
            get
            {
                if (HasTag("aborted"))
                    return FailureCondition.aborted;
                else if (HasTag("incorrect-encoding"))
                    return FailureCondition.incorrect_encoding;
                else if (HasTag("invalid-authzid"))
                    return FailureCondition.invalid_authzid;
                else if (HasTag("invalid-mechanism"))
                    return FailureCondition.invalid_mechanism;
                else if (HasTag("mechanism-too-weak"))
                    return FailureCondition.mechanism_too_weak;
                else if (HasTag("not-authorized"))
                    return FailureCondition.not_authorized;
                else if (HasTag("temporary-auth-failure"))
                    return FailureCondition.temporary_auth_failure;
                else
                    return FailureCondition.UnknownCondition;
            }
            set
            {
                if (value == FailureCondition.aborted)
                    SetTag("aborted");
                else if (value == FailureCondition.incorrect_encoding)
                    SetTag("incorrect-encoding");
                else if (value == FailureCondition.invalid_authzid)
                    SetTag("invalid-authzid");
                else if (value == FailureCondition.invalid_mechanism)
                    SetTag("invalid-mechanism");
                else if (value == FailureCondition.mechanism_too_weak)
                    SetTag("mechanism-too-weak");
                else if (value == FailureCondition.not_authorized)
                    SetTag("not-authorized");
                else if (value == FailureCondition.temporary_auth_failure)
                    SetTag("temporary-auth-failure");                
            }
        }
	}
}
