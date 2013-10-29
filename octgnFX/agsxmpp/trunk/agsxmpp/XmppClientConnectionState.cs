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

namespace agsXMPP
{
    /// <summary>
    /// Represents the current state of a XMPPConnection
    /// </summary>
    public enum XmppConnectionState
    {
        /// <summary>
        /// Session is Disconnected
        /// </summary>
        Disconnected,

        /// <summary>
        /// The Socket is Connecting
        /// </summary>
        Connecting,

        /// <summary>
        /// The Socket is Connected
        /// </summary>
        Connected,
        /// <summary>
        /// The XMPP Session is authenticating
        /// </summary>
        Authenticating,
        /// <summary>
        /// The XMPP session is autrhenticated
        /// </summary>
        Authenticated,

        /// <summary>
        /// Resource Binding gets started
        /// </summary>
        Binding,

        /// <summary>
        /// Resource Binded with sucess
        /// </summary>
        Binded,

        StartSession,

        /// <summary>
        /// Initialize Stream Compression
        /// </summary>
        StartCompression,
        
        /// <summary>
        /// Stream is compressed now
        /// </summary>
        Compressed,

        SessionStarted,

        /// <summary>
        /// We are switching from a normal connection to a secure SSL connection (StartTLS)
        /// </summary>
        Securing,

        /// <summary>
        /// started the progress to register a new account
        /// </summary>
        Registering,

        /// <summary>
        /// Account was registered successful
        /// </summary>
        Registered
    }
}
