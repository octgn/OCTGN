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

namespace agsXMPP.protocol
{
    /*
        <stream:error>
          <defined-condition xmlns='urn:ietf:params:xml:ns:xmpp-streams'/>
          <text xmlns='urn:ietf:params:xml:ns:xmpp-streams'
                xml:lang='langcode'>
            OPTIONAL descriptive text
          </text>
          [OPTIONAL application-specific condition element]
        </stream:error>
    */

    /*
        RFC 4.7.3.  Defined Conditions

        The following stream-level error conditions are defined:

        * <bad-format/> -- the entity has sent XML that cannot be processed; this error MAY be used instead of the more specific XML-related errors, such as <bad-namespace-prefix/>, <invalid-xml/>, <restricted-xml/>, <unsupported-encoding/>, and <xml-not-well-formed/>, although the more specific errors are preferred.
        * <bad-namespace-prefix/> -- the entity has sent a namespace prefix that is unsupported, or has sent no namespace prefix on an element that requires such a prefix (see XML Namespace Names and Prefixes (XML Namespace Names and Prefixes)).
        * <conflict/> -- the server is closing the active stream for this entity because a new stream has been initiated that conflicts with the existing stream.
        * <connection-timeout/> -- the entity has not generated any traffic over the stream for some period of time (configurable according to a local service policy).
        * <host-gone/> -- the value of the 'to' attribute provided by the initiating entity in the stream header corresponds to a hostname that is no longer hosted by the server.
        * <host-unknown/> -- the value of the 'to' attribute provided by the initiating entity in the stream header does not correspond to a hostname that is hosted by the server.
        * <improper-addressing/> -- a stanza sent between two servers lacks a 'to' or 'from' attribute (or the attribute has no value).
        * <internal-server-error/> -- the server has experienced a misconfiguration or an otherwise-undefined internal error that prevents it from servicing the stream.
        * <invalid-from/> -- the JID or hostname provided in a 'from' address does not match an authorized JID or validated domain negotiated between servers via SASL or dialback, or between a client and a server via authentication and resource binding.
        * <invalid-id/> -- the stream ID or dialback ID is invalid or does not match an ID previously provided.
        * <invalid-namespace/> -- the streams namespace name is something other than "http://etherx.jabber.org/streams" or the dialback namespace name is something other than "jabber:server:dialback" (see XML Namespace Names and Prefixes (XML Namespace Names and Prefixes)).
        * <invalid-xml/> -- the entity has sent invalid XML over the stream to a server that performs validation (see Validation (Validation)).
        * <not-authorized/> -- the entity has attempted to send data before the stream has been authenticated, or otherwise is not authorized to perform an action related to stream negotiation; the receiving entity MUST NOT process the offending stanza before sending the stream error.
        * <policy-violation/> -- the entity has violated some local service policy; the server MAY choose to specify the policy in the <text/> element or an application-specific condition element.
        * <remote-connection-failed/> -- the server is unable to properly connect to a remote entity that is required for authentication or authorization.
        * <resource-constraint/> -- the server lacks the system resources necessary to service the stream.
        * <restricted-xml/> -- the entity has attempted to send restricted XML features such as a comment, processing instruction, DTD, entity reference, or unescaped character (see Restrictions (Restrictions)).
        * <see-other-host/> -- the server will not provide service to the initiating entity but is redirecting traffic to another host; the server SHOULD specify the alternate hostname or IP address (which MUST be a valid domain identifier) as the XML character data of the <see-other-host/> element.
        * <system-shutdown/> -- the server is being shut down and all active streams are being closed.
        * <undefined-condition/> -- the error condition is not one of those defined by the other conditions in this list; this error condition SHOULD be used only in conjunction with an application-specific condition.
        * <unsupported-encoding/> -- the initiating entity has encoded the stream in an encoding that is not supported by the server (see Character Encoding (Character Encoding)).
        * <unsupported-stanza-type/> -- the initiating entity has sent a first-level child of the stream that is not supported by the server.
        * <unsupported-version/> -- the value of the 'version' attribute provided by the initiating entity in the stream header specifies a version of XMPP that is not supported by the server; the server MAY specify the version(s) it supports in the <text/> element.
        * <xml-not-well-formed/> -- the initiating entity has sent XML that is not well-formed as defined by [XML] (Bray, T., Paoli, J., Sperberg-McQueen, C., and E. Maler, “Extensible Markup Language (XML) 1.0 (2nd ed),” October 2000.).

    */

    public enum StreamErrorCondition
    {
        /// <summary>
        /// unknown error condition
        /// </summary>
        UnknownCondition        = -1,


        /// <summary>
        /// the entity has sent XML that cannot be processed; this error MAY be used instead of the more specific XML-related errors, such as &lt;bad-namespace-prefix/&gt;, &lt;invalid-xml/&gt;, &lt;restricted-xml/&gt;, &lt;unsupported-encoding/&gt;, and &lt;xml-not-well-formed/&gt;, although the more specific errors are preferred.
        /// </summary>
        BadFormat,
        
        /// <summary>
        /// the entity has sent a namespace prefix that is unsupported, or has sent no namespace prefix on an element that requires such a prefix (see XML Namespace Names and Prefixes (XML Namespace Names and Prefixes)).
        /// </summary>
        BadNamespacePrefix,

        /// <summary>
        /// the server is closing the active stream for this entity because a new stream has been initiated that conflicts with the existing stream.
        /// </summary>
        Conflict,
        
        /// <summary>
        /// the entity has not generated any traffic over the stream for some period of time (configurable according to a local service policy).
        /// </summary>
        ConnectionTimeout,
        
        /// <summary>
        /// the value of the 'to' attribute provided by the initiating entity in the stream header corresponds to a hostname that is no longer hosted by the server.
        /// </summary>
        HostGone,
        
        /// <summary>
        /// the value of the 'to' attribute provided by the initiating entity in the stream header does not correspond to a hostname that is hosted by the server.
        /// </summary>
        HostUnknown,
        
        /// <summary>
        /// a stanza sent between two servers lacks a 'to' or 'from' attribute (or the attribute has no value).
        /// </summary>
        ImproperAddressing,

        /// <summary>
        /// the server has experienced a misconfiguration or an otherwise-undefined internal error that prevents it from servicing the stream.
        /// </summary>
        InternalServerError,

        /// <summary>
        /// the JID or hostname provided in a 'from' address does not match an authorized JID or validated domain negotiated between servers via SASL or dialback, or between a client and a server via authentication and resource binding.
        /// </summary>
        InvalidFrom,
        
        /// <summary>
        /// the stream ID or dialback ID is invalid or does not match an ID previously provided.
        /// </summary>
        InvalidId,

        /// <summary>
        /// the streams namespace name is something other than "http://etherx.jabber.org/streams" or the dialback namespace name is something other than "jabber:server:dialback" (see XML Namespace Names and Prefixes (XML Namespace Names and Prefixes)).
        /// </summary>
        InvalidNamespace,
        
        /// <summary>
        /// the entity has sent invalid XML over the stream to a server that performs validation.
        /// </summary>
        InvalidXml,
        
        /// <summary>
        /// the entity has attempted to send data before the stream has been authenticated, or otherwise is not authorized to perform an action related to stream negotiation; the receiving entity MUST NOT process the offending stanza before sending the stream error.
        /// </summary>
        NotAuthorized,

        /// <summary>
        /// the entity has violated some local service policy; the server MAY choose to specify the policy in the &lt;text/&gt; element or an application-specific condition element.
        /// </summary>
        PolicyViolation,

        /// <summary>
        /// the server is unable to properly connect to a remote entity that is required for authentication or authorization.
        /// </summary>
        RemoteConnectionFailed,
        
        /// <summary>
        /// the server lacks the system resources necessary to service the stream.
        /// </summary>
        ResourceConstraint,

        /// <summary>
        /// the entity has attempted to send restricted XML features such as a comment, processing instruction, DTD, entity reference, or unescaped character (see Restrictions (Restrictions)).
        /// </summary>
        RestrictedXml,
        
        /// <summary>
        /// the server will not provide service to the initiating entity but is redirecting traffic to another host; the server SHOULD specify the alternate hostname or IP address (which MUST be a valid domain identifier) as the XML character data of the &lt;see-other-host/&gt; element.
        /// </summary>
        SeeOtherHost,
        
        /// <summary>
        /// the server is being shut down and all active streams are being closed.
        /// </summary>
        SystemShutdown,
        
        /// <summary>
        /// the error condition is not one of those defined by the other conditions in this list; this error condition SHOULD be used only in conjunction with an application-specific condition.
        /// </summary>
        UndefinedCondition,
        
        /// <summary>
        /// the initiating entity has encoded the stream in an encoding that is not supported by the server.
        /// </summary>
        UnsupportedEncoding,
        
        /// <summary>
        /// the initiating entity has sent a first-level child of the stream that is not supported by the server.
        /// </summary>
        UnsupportedStanzaType,
        
        /// <summary>
        /// the value of the 'version' attribute provided by the initiating entity in the stream header specifies a version of XMPP that is not supported by the server; the server MAY specify the version(s) it supports in the &lt;text/&gt; element.
        /// </summary>
        UnsupportedVersion,
        
        /// <summary>
        /// the initiating entity has sent XML that is not well-formed as defined by the XML specs.
        /// </summary>
        XmlNotWellFormed      
    }

 
	// <stream:error>Invalid handshake</stream:error>
	// <stream:error>Socket override by another connection.</stream:error>

	/// <summary>
	/// Stream Errors &lt;stream:error&gt;
	/// </summary>
	public class Error : Element
	{
		public Error()
		{
			this.TagName	= "error";
			this.Namespace	= Uri.STREAM;			
		}

        public Error(StreamErrorCondition condition) : this()
        {
            this.Condition = condition;
        }        
        
        /*
		public Error(string msg) : this()
		{
			Message = msg;
		}

		/// <summary>
		/// The error Description
		/// </summary>
		public string Message
		{
			get	{ return this.Value;  }
			set	{ this.Value = value; }
		}
        */

        public StreamErrorCondition Condition
        {
            get
            {
                if (HasTag("bad-format"))
                    return StreamErrorCondition.BadFormat;
                else if (HasTag("bad-namespace-prefix"))
                    return StreamErrorCondition.BadNamespacePrefix;
                else if (HasTag("conflict"))
                    return StreamErrorCondition.Conflict;
                else if (HasTag("connection-timeout"))
                    return StreamErrorCondition.ConnectionTimeout;
                else if (HasTag("host-gone"))
                    return StreamErrorCondition.HostGone;
                else if (HasTag("host-unknown"))
                    return StreamErrorCondition.HostUnknown;
                else if (HasTag("improper-addressing"))
                    return StreamErrorCondition.ImproperAddressing;
                else if (HasTag("internal-server-error"))
                    return StreamErrorCondition.InternalServerError;
                else if (HasTag("invalid-from"))
                    return StreamErrorCondition.InvalidFrom;
                else if (HasTag("invalid-id"))
                    return StreamErrorCondition.InvalidId;
                else if (HasTag("invalid-namespace"))
                    return StreamErrorCondition.InvalidNamespace;
                else if (HasTag("invalid-xml"))
                    return StreamErrorCondition.InvalidXml;
                else if (HasTag("not-authorized"))
                    return StreamErrorCondition.NotAuthorized;
                else if (HasTag("policy-violation"))
                    return StreamErrorCondition.PolicyViolation;
                else if (HasTag("remote-connection-failed"))
                    return StreamErrorCondition.RemoteConnectionFailed;
                else if (HasTag("resource-constraint"))
                    return StreamErrorCondition.ResourceConstraint;
                else if (HasTag("restricted-xml"))
                    return StreamErrorCondition.RestrictedXml;
                else if (HasTag("see-other-host"))
                    return StreamErrorCondition.SeeOtherHost;
                else if (HasTag("system-shutdown"))
                    return StreamErrorCondition.SystemShutdown;
                else if (HasTag("undefined-condition"))
                    return StreamErrorCondition.UndefinedCondition;
                else if (HasTag("unsupported-encoding"))
                    return StreamErrorCondition.UnsupportedEncoding;
                else if (HasTag("unsupported-stanza-type"))
                    return StreamErrorCondition.UnsupportedStanzaType;
                else if (HasTag("unsupported-version"))
                    return StreamErrorCondition.UnsupportedVersion;
                else if (HasTag("xml-not-well-formed"))
                    return StreamErrorCondition.XmlNotWellFormed;
                else
                    return StreamErrorCondition.UnknownCondition;
            }

            set
            {
                switch (value)
                {
                    case StreamErrorCondition.BadFormat:
                        SetTag("bad-format", "", Uri.STREAMS);
                        break;
                    case StreamErrorCondition.BadNamespacePrefix:
                        SetTag("bad-namespace-prefix", "", Uri.STREAMS);
                        break;
                    case StreamErrorCondition.Conflict:
                        SetTag("conflict", "", Uri.STREAMS);
                        break;
                    case StreamErrorCondition.ConnectionTimeout:
                        SetTag("connection-timeout", "", Uri.STREAMS);
                        break;
                    case StreamErrorCondition.HostGone:
                        SetTag("host-gone", "", Uri.STREAMS);
                        break;
                    case StreamErrorCondition.HostUnknown:
                        SetTag("host-unknown", "", Uri.STREAMS);
                        break;
                    case StreamErrorCondition.ImproperAddressing:
                        SetTag("improper-addressing", "", Uri.STREAMS);
                        break;
                    case StreamErrorCondition.InternalServerError:
                        SetTag("internal-server-error", "", Uri.STREAMS);
                        break;
                    case StreamErrorCondition.InvalidFrom:
                        SetTag("invalid-from", "", Uri.STREAMS);
                        break;
                    case StreamErrorCondition.InvalidId:
                        SetTag("invalid-id", "", Uri.STREAMS);
                        break;
                    case StreamErrorCondition.InvalidNamespace:
                        SetTag("invalid-namespace", "", Uri.STREAMS);
                        break;
                    case StreamErrorCondition.InvalidXml:
                        SetTag("invalid-xml", "", Uri.STREAMS);
                        break;
                    case StreamErrorCondition.NotAuthorized:
                        SetTag("not-authorized", "", Uri.STREAMS);
                        break;
                    case StreamErrorCondition.PolicyViolation:
                        SetTag("policy-violation", "", Uri.STREAMS);
                        break;
                    case StreamErrorCondition.RemoteConnectionFailed:
                        SetTag("remote-connection-failed", "", Uri.STREAMS);
                        break;
                    case StreamErrorCondition.ResourceConstraint:
                        SetTag("resource-constraint", "", Uri.STREAMS);
                        break;
                    case StreamErrorCondition.RestrictedXml:
                        SetTag("restricted-xml", "", Uri.STREAMS);
                        break;
                    case StreamErrorCondition.SeeOtherHost:
                        SetTag("see-other-host", "", Uri.STREAMS);
                        break;
                    case StreamErrorCondition.SystemShutdown:
                        SetTag("system-shutdown", "", Uri.STREAMS);
                        break;
                    case StreamErrorCondition.UndefinedCondition:
                        SetTag("undefined-condition", "", Uri.STREAMS);
                        break;
                    case StreamErrorCondition.UnsupportedEncoding:
                        SetTag("unsupported-encoding", "", Uri.STREAMS);
                        break;
                    case StreamErrorCondition.UnsupportedStanzaType:
                        SetTag("unsupported-stanza-type", "", Uri.STREAMS);
                        break;
                    case StreamErrorCondition.UnsupportedVersion:
                        SetTag("unsupported-version", "", Uri.STREAMS);
                        break;
                    case StreamErrorCondition.XmlNotWellFormed:
                        SetTag("xml-not-well-formed", "", Uri.STREAMS);
                        break;
                    default:
                        return;

                }
            }
        }

        /// <summary>
        /// <para>
        /// The &lt;text/&gt; element is OPTIONAL. If included, it SHOULD be used only to provide descriptive or diagnostic information
        /// that supplements the meaning of a defined condition or application-specific condition. 
        /// </para>
        /// <para>
        /// It SHOULD NOT be interpreted programmatically by an application.
        /// It SHOULD NOT be used as the error message presented to a user, but MAY be shown in addition to the error message 
        /// associated with the included condition element (or elements).
        /// </para>
        /// </summary>
        public string Text
        {
            get { return GetTag("text"); }
            set
            {
                SetTag("text", value, Uri.STREAMS);
            }
        }
    
    }
}
