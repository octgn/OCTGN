
$(function()
{
	$(".chat-friend").click(newChat)
});

function newChat()
{			
    var chatboxExpandedHTML = '<ul id="chatwindow-expanded" class="dropdown-menu">' +
							    '<li><span style="background-color:red"></span></li>' +
							    '<li><b>Bob:</b></li>' +
							    '<li>I like mashed taters.</li>' +
							    '<li><b>You</b></li>' +
							    '<li>I like mashed taters too.</li>' +
							    '<li>And chicken too.</li>' +
							    '<li><b>Bob:</b></li>' +
							    '<li>We can\'t be friends any more.</li>' +
							    '<li><input type="text" placeholder="Type something…"></li>' +
							    '<li class="divider"></li>' +
							  '</ul>';
	var chatboxMiniHTML = $('<li class="dropup">' +
	  '<span href="#"class="message-box dropdown-toggle" data-toggle="dropdown"><b>Test123</b></span>'
	   + chatboxExpandedHTML +
	'</li>');

						   
	$('#chatbar-center').append(chatboxMiniHTML);
}


function openChat()
{
	alert('');
}