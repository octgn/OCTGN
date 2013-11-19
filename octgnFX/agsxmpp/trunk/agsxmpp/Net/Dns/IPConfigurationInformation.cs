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


using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;

namespace agsXMPP.Net.Dns
{
    /// <summary>
    /// Summary description for IPConfigurationInformation.
    /// </summary>
    public class IPConfigurationInformation
    {
        public static List<IPAddress> DnsServers
        {
            get
            {
                var dnsServers = new List<IPAddress>();
                var interfaces = NetworkInterface.GetAllNetworkInterfaces();
                foreach (var eth in interfaces)
                {
                    if (eth.OperationalStatus == OperationalStatus.Up)
                    {
                        var ethProperties = eth.GetIPProperties();
                        var dnsHosts = ethProperties.DnsAddresses;
                        dnsServers.AddRange(dnsHosts);
                    }
                }
                return dnsServers;
            }
        }
    }
}