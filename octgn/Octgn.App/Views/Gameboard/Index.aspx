<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Index
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <script language=javascript>
        $.getJSON('ResourceListing', null, function (data) {
            alert(data);
        });
    </script>
    <div id="jsondiv"></div>

</asp:Content>
