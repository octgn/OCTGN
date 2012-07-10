<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<% if (Request.IsAuthenticated) {%>
    <li class="dropdown">
      	<a href="#" class="dropdown-toggle" data-toggle="dropdown"> <%: Page.User.Identity.Name %><b class="caret"></b></a>
      	<ul class="dropdown-menu">
        	<li><a href="#">Action</a></li>
            <li><a href="#">Another action</a></li>
            <li><a href="#">Something else here</a></li>
            <li class="divider"></li>
            <li><%: Html.ActionLink("Log Off", "LogOff", "Account") %></li>
      	</ul>
    </li>
<%}else { %> 
    <li class="dropdown">
      	<a href="#" class="dropdown-toggle" data-toggle="dropdown"> Sign In<b class="caret"></b></a>
      	<ul class="dropdown-menu">
        	<li>
				<form class="well" action="/Account/LogOn" method="Post">
					<input name="UserName" type="text" class="input" placeholder="Username"/>
					<input name="Password" type="password" class="input" placeholder="Password"/>
					<label class="checkbox">
						<input name="RememberMe" type="checkbox"> Remember me</input>
					</label>
					<button type="submit" class="btn">Sign in</button>
				</form>
			</li>
			<li>
				<a href="#" onClick="ShowRegBox();">Register</a>
			</li>
      	</ul>
    </li>
<%}%>
