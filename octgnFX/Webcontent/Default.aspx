<%@ Import Namespace="System.Diagnostics" %>
<%@ Import Namespace="System.Reflection" %>
<%@ Import Namespace="Skylabs.LobbyServer" %>
<%@ Page Language="C#" AutoEventWireup="true" %>
<script runat="server">
public void Page_Load()
{
    string v = FileVersionInfo.GetVersionInfo(Assembly.GetAssembly(typeof(Skylabs.LobbyServer.Server)).Location).FileVersion;
    Page.Title = "Lobby Server - " + v.ToString();

    VersionLabel.Text = "Lobby Server - Version " + v.ToString();

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
</script>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title></title>
    <link rel="stylesheet" type="text/css" href="css/main.css" />
</head>
<body runat="server">

    <form id="form1" runat="server">
    <div id="header">

            <h2><asp:Label runat="server" ID="VersionLabel" ></asp:Label></h2>
            <br />
            <label id="RuntimeLabel" runat="server"></label>
            <br />
            <div id="right">
                <pre><label id="ProcessorTimeLabel" runat="server"></label></pre>
                <pre><label id="MemoryUsageLabel" runat="server"></label></pre>
                <pre><label id="TotalMemoryLabel" runat="server"></label></pre>
            </div>
        </div>
        <a href="Games.aspx">Games</a><br />
        <a><label id="OnlineUserLabel" runat="server"></label></a><br />
        <a><label id="HostedGamesLabel" runat="server"></label></a><br />
        <a><label id="TotalGamesHostedLabel" runat="server"></label></a><br />
    </form>
</body>
</html>