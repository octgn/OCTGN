using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;

namespace Octgn.LobbyServer
{
    public static class Networking
    {
        public static int NextPortInRange(int start, int end)
        {
            for (int i = start; i <= end; i++)
            {
                if (IsPortAvailable(i))
                    return i;
            }
            return -1;
        }
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
