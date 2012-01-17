<%@ Import Namespace="System.Diagnostics" %>
<%@ Import Namespace="System.Reflection" %>
<%@ Import Namespace="Skylabs.LobbyServer" %>
<%@ Import Namespace="Skylabs.Lobby" %>
<%@ Page Language="C#" AutoEventWireup="true" %>
<script runat="server">
    public void Page_Load()
    {
        string v = FileVersionInfo.GetVersionInfo(Assembly.GetAssembly(typeof(Skylabs.LobbyServer.Server)).Location).FileVersion;
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
</script>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title></title>
    <link id="Link1" rel="stylesheet" type="text/css" href="css/main.css" runat="server" />
</head>
<body>

    <form id="form1" runat="server">
    <div id="header">

            <h2><label id="VersionLabel" runat="server"></label></h2>
            <br />
            <label id="RuntimeLabel" runat="server"></label>
            <br />
            <div id="right">
                <pre><label id="ProcessorTimeLabel" runat="server"></label></pre>
                <pre><label id="MemoryUsageLabel" runat="server"></label></pre>
                <pre><label id="TotalMemoryLabel" runat="server"></label></pre>
            </div>
        </div>
        <a href="Default.aspx">Back to main</a><br />
        <br />
        <table id="HostedGamesTable" runat="server">
            <tr>
            <td>Name:
            </td>
            <td>Port:
            </td>
            <td>Status:
            </td>
            <td>Version:
            </td>
            <td>Running time:
            </td>
            <td>Hosting user:
            </td>
            </tr>
        </table>
    </form>
</body>
</html>