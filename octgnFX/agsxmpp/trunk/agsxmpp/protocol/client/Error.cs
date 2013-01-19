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

// JEP-0086: Error Condition Mappings

// <stanza-kind to='sender' type='error'>
// [RECOMMENDED to include sender XML here]
// <error type='error-type'>
// <defined-condition xmlns='urn:ietf:params:xml:ns:xmpp-stanzas'/>
// <text xmlns='urn:ietf:params:xml:ns:xmpp-stanzas'
// xml:lang='langcode'>
// OPTIONAL descriptive text
// </text>
// [OPTIONAL application-specific condition element]
// </error>
// </stanza-kind>

// Legacy Error
// <error code="501">Not Implemented</error>

// XMPP Style Error
// <error code='404' type='cancel'>
//		<item-not-found xmlns='urn:ietf:params:xml:ns:xmpp-stanzas'/>
// </error>

namespace agsXMPP.protocol.client
{
	// XMPP error condition  		XMPP error type  	Legacy error code
	// <bad-request/> 				modify 				400
	// <conflict/> 					cancel 				409
	// <feature-not-implemented/> 	cancel 				501
	// <forbidden/> 				auth 				403
	// <gone/> 						modify 				302 (permanent)
	// <internal-server-error/> 	wait 				500
	// <item-not-found/> 			cancel 				404
	// <jid-malformed/> 			modify 				400
	// <not-acceptable/> 			modify 				406
	// <not-allowed/> 				cancel 				405
	// <not-authorized/> 			auth 				401
	// <payment-required/> 			auth 				402
	// <recipient-unavailable/> 	wait 				404
	// <redirect/> 					modify 				302 (temporary)
	// <registration-required/> 	auth 				407
	// <remote-server-not-found/> 	cancel 				404
	// <remote-server-timeout/> 	wait 				504
	// <resource-constraint/> 		wait 				500
	// <service-unavailable/> 		cancel 				503
	// <subscription-required/> 	auth 				407
	// <undefined-condition/> 		[any] 				500
	// <unexpected-request/> 		wait 				400

    /// <summary>
    /// stanza error condition as defined in RFC 3920 9.3
    /// </summary>
    public enum ErrorCondition
    {
        /// <summary>
        /// The sender has sent a stanza containing XML that does not conform to the appropriate schema or that 
        /// cannot be processed (e.g., an IQ stanza that includes an unrecognized value of the 'type' attribute);
        /// the associated error type SHOULD be "modify".
        /// </summary>
        BadRequest,

        /// <summary>
        /// Access cannot be granted because an existing resource exists with the same name or address; 
        /// the associated error type SHOULD be "cancel". 
        /// </summary>
        Conflict,

        /// <summary>
        /// The feature represented in the XML stanza is not implemented by the intended recipient or 
        /// an intermediate server and therefore the stanza cannot be processed; the associated error type SHOULD 
        /// be "cancel" or "modify".
        /// </summary>
        FeatureNotImplemented,

        /// <summary>
        /// The requesting entity does not possess the required permissions to perform the action; 
        /// the associated error type SHOULD be "auth".
        /// </summary>
        Forbidden,

        /// <summary>
        /// The recipient or server can no longer be contacted at this address 
        /// (the error stanza MAY contain a new address in the XML character data of the &lt;gone/&gt; element); 
        /// the associated error type SHOULD be "cancel" or "modify".
        /// </summary>
        Gone,

        /// <summary>
        /// The server could not process the stanza because of a misconfiguration or an otherwise-undefined 
        /// internal server error; the associated error type SHOULD be "wait" or "cancel".
        /// </summary>
        InternalServerError,

        /// <summary>
        /// The addressed JID or item requested cannot be found; the associated error type SHOULD be "cancel" or "modify".
        /// </summary>
        /// <remarks>        
        /// An application MUST NOT return this error if doing so would provide information about the intended 
        /// recipient's network availability to an entity that is not authorized to know such information; 
        /// instead it SHOULD return a &lt;service-unavailable/&gt; error.
        /// </remarks>        
        ItemNotFound,

        /// <summary>
        /// The sending entity has provided or communicated an XMPP address 
        /// (e.g., a value of the 'to' attribute) or aspect thereof (e.g., an XMPP resource identifier) 
        /// that does not adhere to the syntax defined under RFC3920 Section 3 (Addresses); 
        /// the associated error type SHOULD be "modify".
        /// </summary>
        JidMalformed,

        /// <summary>
        /// The recipient or server understands the request but is refusing to process it because it does not
        /// meet criteria defined by the recipient or server (e.g., a local policy regarding stanza size 
        /// limits or acceptable words in messages); the associated error type SHOULD be "modify".
        /// </summary>
        NotAcceptable,

        /// <summary>
        /// The recipient or server does not allow any entity to perform the action (e.g., sending to entities at 
        /// a blacklisted domain); the associated error type SHOULD be "cancel".
        /// </summary>
        NotAllowed,

        /// <summary>
        /// The sender must provide proper credentials before being allowed to perform the action, or has provided 
        /// improper credentials; the associated error type SHOULD be "auth".
        /// </summary>
        NotAuthorized,

        /// <summary>
        /// The item requested has not changed since it was last requested; the associated error type SHOULD be "continue".
        /// </summary>
        NotModified,

        /// <summary>
        /// The requesting entity is not authorized to access the requested service because payment is required; 
        /// the associated error type SHOULD be "auth".
        /// </summary>        
        PaymentRequired,

        /// <summary>
        /// The intended recipient is temporarily unavailable; the associated error type SHOULD be "wait".
        /// </summary>
        /// <remarks>
        /// An application MUST NOT return this error if doing so would provide information about the 
        /// intended recipient's network availability to an entity that is not authorized to know such 
        /// information; instead it SHOULD return a &lt;service-unavailable/&gt; error.
        /// </remarks>
        RecipientUnavailable,

        /// <summary>
        /// The recipient or server is redirecting requests for this information to another entity, 
        /// typically in a temporary fashion; the associated error type SHOULD be "modify" and the error stanza
        /// SHOULD contain the alternate address (which SHOULD be a valid JID) in the XML character data 
        /// of the &lt;redirect/&gt; element.
        /// </summary>
        Redirect,

        /// <summary>
        /// The requesting entity is not authorized to access the requested service because prior 
        /// registration is required; the associated error type SHOULD be &quot;auth&quot;.
        /// </summary>
        RegistrationRequired,

        /// <summary>
        /// A remote server or service specified as part or all of the JID of the intended recipient 
        /// does not exist; the associated error type SHOULD be &quot;cancel&quot;.
        /// </summary>
        RemoteServerNotFound,

        /// <summary>
        /// A remote server or service specified as part or all of the JID of the intended recipient 
        /// (or required to fulfill a request) could not be contacted within a reasonable amount 
        /// of time; the associated error type SHOULD be &quot;wait&quot;.
        /// </summary>
        RemoteServerTimeout,

        /// <summary>
        /// The server or recipient lacks the system resources necessary to service the request; 
        /// the associated error type SHOULD be "wait".
        /// </summary>
        ResourceConstraint,

        /// <summary>
        /// The server or recipient does not currently provide the requested service; 
        /// the associated error type SHOULD be &quot;cancel&quot;.
        /// </summary>
        /// <remarks>
        /// An application SHOULD return a &lt;service-unavailable/&gt; error instead of 
        /// &lt;item-not-found/&gt; or &lt;recipient-unavailable/&gt; if sending one of the latter 
        /// errors would provide information about the intended recipient&#39;s network 
        /// availability to an entity that is not authorized to know such information.
        /// </remarks>
        ServiceUnavailable,

        /// <summary>
        /// The requesting entity is not authorized to access the requested service 
        /// because a prior subscription is required; the associated error type SHOULD be &quot;auth&quot;.
        /// </summary>
        SubscriptionRequired,

        /// <summary>
        /// The error condition is not one of those defined by the other conditions in this list; 
        /// any error type may be associated with this condition, and it SHOULD be used only in conjunction 
        /// with an application-specific condition.
        /// </summary>
        UndefinedCondition,

        /// <summary>
        /// The recipient or server understood the request but was not expecting it at this time 
        /// (e.g., the request was out of order); the associated error type SHOULD be "wait" or "modify".
        /// </summary>
        UnexpectedRequest
    }

	// The value of the <error/> element's 'type' attribute MUST be one of the following:
	// * cancel -- do not retry (the error is unrecoverable)
	// * continue -- proceed (the condition was only a warning)
	// * modify -- retry after changing the data sent
	// * auth -- retry after providing credentials
	// * wait -- retry after waiting (the error is temporary)
	public enum ErrorType
	{
		cancel,
		@continue,
		modify,
		auth,
		wait
	}


	/// <summary>
	/// The legacy Error Code
	/// </summary>
	public enum ErrorCode
	{		
		/// <summary>
		/// Bad request
		/// </summary>
		BadRequest				= 400,
		/// <summary>
		/// Unauthorized
		/// </summary>
		Unauthorized			= 401,
		/// <summary>
		/// Payment required
		/// </summary>
		PaymentRequired			= 402,
		/// <summary>
		/// Forbidden
		/// </summary>
		Forbidden				= 403,
		/// <summary>
		/// Not found
		/// </summary>
		NotFound				= 404,
		/// <summary>
		/// Not allowed
		/// </summary>
		NotAllowed				= 405,
		/// <summary>
		/// Not acceptable
		/// </summary>
		NotAcceptable			= 406,
		/// <summary>
		/// Registration required 
		/// </summary>
		RegistrationRequired	= 407,
		/// <summary>
		/// Request timeout
		/// </summary>
		RequestTimeout			= 408,
		/// <summary>
		/// Conflict
		/// </summary>
		Conflict                = 409,
		/// <summary>
		/// Internal server error
		/// </summary>
		InternalServerError		= 500,
		/// <summary>
		/// Not implemented
		/// </summary>
		NotImplemented			= 501,
		/// <summary>
		/// Remote server error
		/// </summary>
		RemoteServerError		= 502,
		/// <summary>
		/// Service unavailable
		/// </summary>
		ServiceUnavailable		= 503,
		/// <summary>
		/// Remote server timeout
		/// </summary>
		RemoteServerTimeout		= 504,
		/// <summary>
		/// Disconnected
		/// </summary>
		Disconnected            = 510
	}

	
	/// <summary>
	/// Summary description for Error.
	/// </summary>
	public class Error : Element
	{

		#region << Constructors >>
		public Error()
		{
            this.Namespace  = Uri.CLIENT;
			this.TagName    = "error";
        }

        #region << Obsolete Constructors >>
        [Obsolete("Please don't use old Jabber style errors. Use XMPP ErrorCondition instead")]
		public Error(int code) : this()
		{			
			this.SetAttribute("code", code.ToString());
		}

        [Obsolete("Please don't use old Jabber style errors. Use XMPP ErrorCondition instead")]
        public Error(ErrorCode code) : this()
        {
            this.SetAttribute("code", (int)code);
        }
        #endregion

        /// <summary>
		/// Creates an error Element according the the condition
		/// The type attrib as added automatically as decribed in the XMPP specs
		/// This is the prefered way to create error Elements
		/// </summary>
		/// <param name="condition"></param>
		public Error(ErrorCondition condition) : this()
		{			
			this.Condition	= condition;
		}

        public Error(ErrorCondition condition, string text) : this(condition)
        {
            ErrorText = text;
        }

        public Error(ErrorType type)
            : this()
        {
            Type = type;
        }

        public Error(ErrorType type, ErrorCondition condition) : this(type)
        {
            this.Condition = condition;
        }
		#endregion

		/// <summary>
		/// The error Description
		/// </summary>
        [Obsolete("Use ErrorText Property instead")]
		public string Message
		{
			get
			{
				return this.Value;
			}
			set
			{
				this.Value = value;
			}
		}

        /// <summary>
        /// The optional error text
        /// </summary>
        public string ErrorText
        {
            get
            {
                return GetTag("text");
            }
            set
            {
                SetTag("text", value, Uri.STANZAS);
            }
        }

		public ErrorCode Code
		{
			get
			{
				return (ErrorCode) GetAttributeInt("code");
			}
			set
			{
				SetAttribute("code", (int) value);
			}
		}
       
		public ErrorType Type
		{
			get
			{
				return (ErrorType) GetAttributeEnum("type", typeof(ErrorType));
			}
			set
			{
				SetAttribute("type", value.ToString());
			}
		}

		public ErrorCondition Condition
		{
			get
			{
				if (HasTag("bad-request"))					// <bad-request/> 
					return ErrorCondition.BadRequest;
				else if (HasTag("conflict"))				// <conflict/> 
					return ErrorCondition.Conflict;
				else if  (HasTag("feature-not-implemented"))// <feature-not-implemented/>
					return ErrorCondition.FeatureNotImplemented;
				else if (HasTag("forbidden"))				// <forbidden/> 
					return ErrorCondition.Forbidden;
				else if (HasTag("gone"))					// <gone/>
					return ErrorCondition.Gone;
				else if (HasTag("internal-server-error"))	// <internal-server-error/>
					return ErrorCondition.InternalServerError;
				else if (HasTag("item-not-found"))			// <item-not-found/> 
					return ErrorCondition.ItemNotFound;
				else if (HasTag("jid-malformed"))			// <jid-malformed/>
					return ErrorCondition.JidMalformed;
				else if (HasTag("not-acceptable"))			// <not-acceptable/> 
					return ErrorCondition.NotAcceptable;
                else if (HasTag("not-allowed"))             // <not-allowed/>
                    return ErrorCondition.NotAllowed;
				else if (HasTag("not-authorized"))			// <not-authorized/>
					return ErrorCondition.NotAuthorized;
				else if (HasTag("not-modified"))            // <not-modified/>
                    return ErrorCondition.NotModified;                
                else if (HasTag("payment-required"))		// <payment-required/>
					return ErrorCondition.PaymentRequired;
				else if (HasTag("recipient-unavailable"))	// <recipient-unavailable/>
					return ErrorCondition.RecipientUnavailable;
				else if (HasTag("redirect"))				// <redirect/>
					return ErrorCondition.Redirect;
				else if (HasTag("registration-required"))	// <registration-required/>
					return ErrorCondition.RegistrationRequired;
				else if (HasTag("remote-server-not-found"))	// <remote-server-not-found/> 
					return ErrorCondition.RemoteServerNotFound;
				else if (HasTag("remote-server-timeout"))	// <remote-server-timeout/> 
					return ErrorCondition.RemoteServerTimeout;	
				else if (HasTag("resource-constraint"))		// <resource-constraint/>
					return ErrorCondition.ResourceConstraint;	
				else if (HasTag("service-unavailable"))		// <service-unavailable/> 
					return ErrorCondition.ServiceUnavailable;
				else if (HasTag("subscription-required"))	// <subscription-required/> 
					return ErrorCondition.SubscriptionRequired;
				else if (HasTag("undefined-condition"))		// <undefined-condition/> 
					return ErrorCondition.UndefinedCondition;
				else if (HasTag("unexpected-request"))		// <unexpected-request/> 
					return ErrorCondition.UnexpectedRequest;
				else
 					return ErrorCondition.UndefinedCondition;
					
			}
			set
			{
				switch (value)
				{
					case ErrorCondition.BadRequest:
						SetTag("bad-request",				"", Uri.STANZAS);
						Type = ErrorType.modify;
						break;
					case ErrorCondition.Conflict:
						SetTag("conflict",					"", Uri.STANZAS);
						Type = ErrorType.cancel;
						break;
					case ErrorCondition.FeatureNotImplemented:
						SetTag("feature-not-implemented",	"", Uri.STANZAS);
						Type = ErrorType.cancel;
						break;
					case ErrorCondition.Forbidden:
						SetTag("forbidden",					"", Uri.STANZAS);
						Type = ErrorType.auth;
						break;
					case ErrorCondition.Gone:
						SetTag("gone",						"", Uri.STANZAS);
						Type = ErrorType.modify;
						break;
					case ErrorCondition.InternalServerError:
						SetTag("internal-server-error",		"", Uri.STANZAS);
						Type = ErrorType.wait;
						break;
					case ErrorCondition.ItemNotFound:
						SetTag("item-not-found",			"", Uri.STANZAS);
						Type = ErrorType.cancel;
						break;
					case ErrorCondition.JidMalformed:
						SetTag("jid-malformed",				"", Uri.STANZAS);
						Type = ErrorType.modify;
						break;
					case ErrorCondition.NotAcceptable:
						SetTag("not-acceptable",			"", Uri.STANZAS);
						Type = ErrorType.modify;
						break;
					case ErrorCondition.NotAllowed:
						SetTag("not-allowed",				"", Uri.STANZAS);
						Type = ErrorType.cancel;
						break;
					case ErrorCondition.NotAuthorized:
						SetTag("not-authorized",			"", Uri.STANZAS);
						Type = ErrorType.auth;
						break;
                    case ErrorCondition.NotModified:
                        SetTag("not-modified",			    "", Uri.STANZAS);
                        Type = ErrorType.modify;
                        break;
					case ErrorCondition.PaymentRequired:
						SetTag("payment-required",			"", Uri.STANZAS);
						Type = ErrorType.auth;
						break;
					case ErrorCondition.RecipientUnavailable:
						SetTag("recipient-unavailable",		"", Uri.STANZAS);
						Type = ErrorType.wait;
						break;
					case ErrorCondition.Redirect:
						SetTag("redirect",					"", Uri.STANZAS);
						Type = ErrorType.modify;
						break;
					case ErrorCondition.RegistrationRequired:
						SetTag("registration-required",		"", Uri.STANZAS);
						Type = ErrorType.auth;
						break;
					case ErrorCondition.RemoteServerNotFound:
						SetTag("remote-server-not-found",	"", Uri.STANZAS);
						Type = ErrorType.cancel;
						break;
					case ErrorCondition.RemoteServerTimeout:
						SetTag("remote-server-timeout",		"", Uri.STANZAS);
						Type = ErrorType.wait;
						break;
					case ErrorCondition.ResourceConstraint:	
						SetTag("resource-constraint",		"", Uri.STANZAS);
						Type = ErrorType.wait;
						break;
					case ErrorCondition.ServiceUnavailable:
						SetTag("service-unavailable",		"", Uri.STANZAS);
						Type = ErrorType.cancel;
						break;
					case ErrorCondition.SubscriptionRequired:
						SetTag("subscription-required",		"", Uri.STANZAS);
						Type = ErrorType.auth;
						break;
					case ErrorCondition.UndefinedCondition:
						SetTag("undefined-condition",		"", Uri.STANZAS);
						// could be any
						break;
					case ErrorCondition.UnexpectedRequest:
						SetTag("unexpected-request",		"", Uri.STANZAS);
						Type = ErrorType.wait;
						break;

				}
			}
		}		
	}
}