<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<% if (!Request.IsAuthenticated) {%>
	<script type="text/javascript">
		$(function(){
			$("#register-button").click(register);
			$("#rPass1").change(pass_change);
			$("#rPass2").change(pass_change);
		});
		function register()
		{
			var data = $("#register-form").serialize();
			$.post("/Account/Register",data,register_done,"json");
		}
		function register_done(data)
		{
			data = jQuery.parseJSON(data);
			$(".help-inline").text("");
			$(".control-group").attr("class","control-group");
			if(data.Result == "success")
			{
				window.location.reload(true);
			}
			else if(data.Result == "error")
			{
				var f = $("#r" + data.For);
				f.parent().parent().toggleClass("error",true);
				f.siblings(".help-inline").text(data.Message);
			}
		}
		function pass_change()
		{
			var obj = $("#rPass1");
			var pobj = obj.parent().parent();
			var obj2 = $("#rPass2");
			var pobj2 = obj2.parent().parent();
			if(obj.val().length < 5)
			{
				pobj.toggleClass("error",true);
				obj.siblings(".help-inline").text("Password must be at least 5 characters long.");
			}
			else
			{
				pobj.toggleClass("error",false);
				obj.siblings(".help-inline").text("");
			}
			if(obj2.val() != obj.val() && obj2.val().length > 0)
			{
				pobj2.toggleClass("error",true);
				obj2.siblings(".help-inline").text("Passwords must match.");
			}
			else
			{
				pobj2.toggleClass("error",false);
				obj2.siblings(".help-inline").text("");
			}
			
		}
	</script>
	<div class="modal hide" id="RegBox">
	  <div class="modal-header">
	    <button type="button" class="close" data-dismiss="modal">×</button>
	    <h3>Register</h3>
	  </div>
	  <div class="modal-body">
	  	<form id="register-form">
			<div class="control-group">
				<label class="control-label" for="rUserName">Username</label>
				<div class="controls">
					<input id="rUserName"type="text" class="input" placeholder="Username" name="UserName"/>
					<span class="help-inline"></span>
				</div>
			</div>
			<div class="control-group">
				<label class="control-label" for="rEmail">Email</label>
				<div class="controls">
					<input id="rEmail" type="text" class="input" placeholder="Email" name="Email"/>
					<span class="help-inline"></span>
				</div>
			</div>
			<div class="control-group">				
				<label class="control-label" for="rPass1">Password</label>
				<div class="controls">
					<input id="rPass1" type="password" class="input" placeholder="Password" name="Password"/>
					<span class="help-inline"></span>
				</div>
			</div>
			<div class="control-group">				
				<label class="control-label" for="rPass2">Confirm Password</label>
				<div class="controls">
					<input id="rPass2" type="password" class="input" placeholder="Confirm Password" name="ConfirmPassword"/>
					<span class="help-inline"></span>
				</div>
			</div>
		</form>
	  </div>
	  <div class="modal-footer">
	    <a href="#" class="btn" data-dismiss="modal">Close</a>
	    <a id="register-button" href="#" class="btn btn-primary">Register</a>
	  </div>
	</div>
<%}%> 