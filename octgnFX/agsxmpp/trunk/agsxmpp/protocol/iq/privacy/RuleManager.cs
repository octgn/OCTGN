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

using agsXMPP.protocol.iq.roster;

namespace agsXMPP.protocol.iq.privacy
{
    /// <summary>
    /// Helper class for creating rules for communication blocking
    /// </summary>
    public class RuleManager
    {
        /// <summary>
        /// Block stanzas by Jid
        /// </summary>
        /// <param name="jidToBlock"></param>
        /// <param name="order"></param>
        /// <param name="stanza">stanzas you want to block</param>
        /// <returns></returns>
        public Item BlockByJid(Jid jidToBlock, int order, Stanza stanza)
        {
            return new Item(Action.deny, order, Type.jid, jidToBlock.ToString(), stanza);
        }
                

        /// <summary>
        /// Block stanzas for a given roster group
        /// </summary>
        /// <param name="group"></param>
        /// <param name="order"></param>
        /// <param name="stanza">stanzas you want to block</param>
        /// <returns></returns>
        public Item BlockByGroup(string group, int order, Stanza stanza)
        {
            return new Item(Action.deny, order, Type.group, group, stanza);
        }
                
        /// <summary>
        /// Block stanzas by subscription type
        /// </summary>
        /// <param name="subType"></param>
        /// <param name="order"></param>
        /// <param name="stanza">stanzas you want to block</param>
        /// <returns></returns>
        public Item BlockBySubscription(SubscriptionType subType, int order, Stanza stanza)
        {
            return new Item(Action.deny, order, Type.subscription, subType.ToString(), stanza);
        }

        /// <summary>
        /// Block globally (all users) the given stanzas
        /// </summary>
        /// <param name="order"></param>
        /// <param name="stanza">stanzas you want to block</param>
        /// <returns></returns>
        public Item BlockGlobal(int order, Stanza stanza)
        {
            return new Item(Action.deny, order, stanza);
        }

        /// <summary>
        /// Allow stanzas by Jid
        /// </summary>
        /// <param name="jidToBlock"></param>
        /// <param name="order"></param>
        /// <param name="stanza">stanzas you want to block</param>
        /// <returns></returns>
        public Item AllowByJid(Jid jidToBlock, int order, Stanza stanza)
        {
            return new Item(Action.allow, order, Type.jid, jidToBlock.ToString(), stanza);
        }

        /// <summary>
        /// Allow stanzas for a given roster group
        /// </summary>
        /// <param name="group"></param>
        /// <param name="order"></param>
        /// <param name="stanza">stanzas you want to block</param>
        /// <returns></returns>
        public Item AllowByGroup(string group, int order, Stanza stanza)
        {
            return new Item(Action.allow, order, Type.group, group, stanza);
        }

        /// <summary>
        /// Allow stanzas by subscription type
        /// </summary>
        /// <param name="subType"></param>
        /// <param name="order"></param>
        /// <param name="stanza">stanzas you want to block</param>
        /// <returns></returns>
        public Item AllowBySubscription(SubscriptionType subType, int order, Stanza stanza)
        {
            return new Item(Action.allow, order, Type.subscription, subType.ToString(), stanza);
        }
        
    }
}
