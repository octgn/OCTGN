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

namespace agsXMPP.protocol.iq.register
{
    public delegate void RegisterEventHandler(object sender, RegisterEventArgs args);

    public class RegisterEventArgs
    {
        public RegisterEventArgs()
        {
        }
        
        public RegisterEventArgs(Register reg)
        {
            m_Register = reg;
        }
        
        // by default we register automatically
        private bool						m_Auto			= true;
        private Register                    m_Register;

        /// <summary>
        /// Set Auto to true if the library should register automatically
        /// Set it to false if you want to fill out the registration fields manual
        /// </summary>
        public bool Auto
        {
            get { return m_Auto; }
            set { m_Auto = value; }
        }

        public Register Register
        {
            get { return m_Register; }
            set { m_Register = value; }
        }

    }
}