using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Skylabs.LobbyServer;
using System.Diagnostics;

namespace Webcontent
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Version v = Skylabs.LobbyServer.Server.Version;
            Page.Title = "Lobby Server - " + v.ToString();

            VersionLabel.InnerText = "Lobby Server - Version " + v.ToString();

            RuntimeLabel.InnerText = "Runtime - " + Skylabs.LobbyServer.Server.ServerRunTime.ToString();

            ProcessorTimeLabel.InnerText = "Processor Time     : " + Process.GetCurrentProcess().TotalProcessorTime.ToString();
            MemoryUsageLabel.InnerText = "Memory Usage       : " + ToFileSize(Process.GetCurrentProcess().WorkingSet64);
            TotalMemoryLabel.InnerText = "Total Memory       : " + "256 MB";

            OnlineUserLabel.InnerText = "Users Online: " + Skylabs.LobbyServer.Server.OnlineCount().ToString();
            HostedGamesLabel.InnerText = "Games Hosted: " + Gaming.GameCount().ToString();

            TotalGamesHostedLabel.InnerText = "Total Games Hosted: " + Gaming.TotalHostedGames().ToString();
        }

        public static string ToFileSize(int source)
        {
            return ToFileSize(Convert.ToInt64(source));
        }

        public static string ToFileSize(long source)
        {
            const int byteConversion = 1024;
            double bytes = Convert.ToDouble(source);

            if (bytes >= Math.Pow(byteConversion, 3)) //GB Range
            {
                return string.Concat(Math.Round(bytes / Math.Pow(byteConversion, 3), 2), " GB");
            }
            else if (bytes >= Math.Pow(byteConversion, 2)) //MB Range
            {
                return string.Concat(Math.Round(bytes / Math.Pow(byteConversion, 2), 2), " MB");
            }
            else if (bytes >= byteConversion) //KB Range
            {
                return string.Concat(Math.Round(bytes / byteConversion, 2), " KB");
            }
            else //Bytes
            {
                return string.Concat(bytes, " Bytes");
            }
        }
    }
}