using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Skylabs.LobbyServer;
using Skylabs.Lobby;
using System.Diagnostics;
using System.Reflection;

namespace Webcontent
{
    public partial class Games : System.Web.UI.Page
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

            
            string insert = string.Empty;
            List<Skylabs.Lobby.HostedGame> games = Gaming.GetLobbyList();


            //construct game table
            foreach (Skylabs.Lobby.HostedGame game in games)
            {
                TimeSpan ts = new TimeSpan(DateTime.Now.Ticks - game.TimeStarted.Ticks);
                insert = insert + "<tr>";
                insert = insert + "<td>" + game.Name + "</td>";
                insert = insert + "<td>" + game.Port + "</td>";
                insert = insert + "<td>" + game.GameStatus + "</td>";
                insert = insert + "<td>" + game.GameVersion + "</td>";
                insert = insert + "<td>" + ts.ToString() + "</td>";
                Client c = Skylabs.LobbyServer.Server.GetOnlineClientByUid(game.UserHosting.Uid);
                Skylabs.Lobby.User user;
                if (c == null)
                {
                    user = game.UserHosting;
                    user.Status = Skylabs.Lobby.UserStatus.Offline;
                }
                else
                    user = c.Me;

                insert = insert + "<td>Name: " + user.DisplayName + "<br />";
                insert = insert + "Status: " + Enum.GetName(typeof(Skylabs.Lobby.UserStatus), user.Status) + "<br />";
                insert = insert + "Email: " + user.Email + "<br />";
                insert = insert + "Uid: " + user.Uid + "</td>";

                insert = insert + "</tr>";
                HostedGamesTable.InnerHtml = HostedGamesTable.InnerHtml + insert;
            }
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