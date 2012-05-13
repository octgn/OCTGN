using System.Linq;
using System.Net.NetworkInformation;

namespace Octgn.Lobby
{
    public static class Networking
    {
        /// <summary>
        ///   Checks to see if a port is available Shouldn't need to use this anymore, but not quite ready to get rid of it. The port could be taken while the function is running.
        /// </summary>
        /// <param name="port"> port to check. </param>
        /// <returns> Is the port available </returns>
        public static bool IsPortAvailable(int port)
        {
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

            return tcpConnInfoArray.All(tcpi => tcpi.LocalEndPoint.Port != port);
        }
    }
}