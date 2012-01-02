using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;

namespace Skylabs.LobbyServer
{
    public static class Networking
    {
        /// <summary>
        /// Searches the system for the next open port in a range.
        /// </summary>
        /// <param name="start">Start port</param>
        /// <param name="end">End port</param>
        /// <returns>Next available port, or a -1 if all hell breaks loose.</returns>
        public static int NextPortInRange(int start, int end)
        {
            for (int i = start; i <= end; i++)
            {
                foreach (HostedGame hg in Program.Server.Games)
                {
                    if (hg.Port == i)
                        continue;
                }
                if (IsPortAvailable(i))
                    return i;
            }
            return -1;
        }
        /// <summary>
        /// Checks to see if a port is available
        /// Shouldn't need to use this anymore, but not quite ready to get rid of it.
        /// The port could be taken while the function is running.
        /// </summary>
        /// <param name="port">port to check.</param>
        /// <returns>Is the port available</returns>
        public static bool IsPortAvailable(int port)
        {

            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

            foreach (TcpConnectionInformation tcpi in tcpConnInfoArray)
            {
                if (tcpi.LocalEndPoint.Port == port)
                {
                    return false;
                }
            }
            return true;

        }
    }
}
