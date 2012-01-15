<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Webcontent.Default" Src="~/Default.aspx.cs" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title></title>
    <link rel="stylesheet" type="text/css" href="css/main.css" runat="server" />
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
        <a href="Games.aspx">Games</a><br />
        <a><label id="OnlineUserLabel" runat="server"></label></a><br />
        <a><label id="HostedGamesLabel" runat="server"></label></a><br />
        <a><label id="TotalGamesHostedLabel" runat="server"></label></a><br />
    </form>
</body>
</html>