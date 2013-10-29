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

namespace agsXMPP.protocol.x.muc
{
    /// <summary>
    /// StatusCode
    /// </summary>
    public enum StatusCode
    {
        /// <summary>
        /// Unkown status code.
        /// </summary>
        Unknown                 = -1,

        /// <summary>
        /// Inform user that any occupant is allowed to see the user's full JID.
        /// </summary>
        FullJidVisible          = 100,
        
        /// <summary>
        /// Inform user that his or her affiliation changed while not in the room.
        /// </summary>
        AffiliationChanged      = 101,

        /// <summary>
        /// Inform occupants that room now shows unavailable members.
        /// </summary>
        ShowUnavailableMembers = 102,
    
        /// <summary>
        /// Inform occupants that room now does not show unavailable members .
        /// </summary>
        HideUnavailableMembers = 103,

        /// <summary>
        /// Inform occupants that a non-privacy-related room configuration change has occurred.
        /// </summary>
        ConfigurationChanged    = 104,

        /// <summary>
        /// Inform user that presence refers to one of its own room occupants .
        /// </summary>
        SelfPresence            = 110,

        /// <summary>
        /// Inform occupants that room logging is now enabled.
        /// </summary>
        LoggingEnabled          = 170,

        /// <summary>
        /// Inform occupants that room logging is now disabled. 
        /// </summary>
        LoggingDisabled         = 171,
    
        /// <summary>
        /// Inform occupants that the room is now non-anonymous.
        /// </summary>
        RoomNonAnonymous        = 172,

        /// <summary>
        /// Inform occupants that the room is now semi-anonymous.
        /// </summary>
        RoomSemiAnonymous       = 173,

        /// <summary>
        /// Inform occupants that the room is now fully-anonymous. 
        /// </summary>
        RoomAnonymous           = 174,

        /// <summary>
        /// Inform user that a new room has been created. 
        /// </summary>
        RoomCreated             = 201,

        /// <summary>
        ///  Inform user that service has assigned or modified occupant's roomnick.
        /// </summary>
        ModifiedNick            = 210,

        /// <summary>
        /// Inform user that he or she has been banned from the room. 
        /// </summary>
        Banned                  = 301,

        /// <summary>
        /// Inform all occupants of new room nickname. 
        /// </summary>
        NewNickname             = 303,

        /// <summary>
        /// Inform user that he or she has been kicked from the room. 
        /// </summary>
        Kicked                  = 307,

        /// <summary>
        /// Inform user that he or she is being removed from the room because of an affiliation change.
        /// </summary>
        // TODO, find better name
        AffiliationChange       = 321,
        
        /// <summary>
        /// Inform user that he or she is being removed from the room because the room 
        /// has been changed to members-only and the user is not a member.
        /// </summary>
        MembersOnly             = 322,

        /// <summary>
        /// Inform user that he or she is being removed from the room because of a system shutdown.
        /// </summary>
        Shutdown                = 332
    }
}