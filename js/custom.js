$(document).ready(function() {

	$("a#showImage").fancybox({
		'transitionIn'	: 'none',
		'transitionOut'	: 'none'	
	});

	$(function(){
		// Accordion
		$("#accordion").accordion({ header: "h3", autoHeight: false });
		// Tabs
		$('#tabs').tabs();
	
		// Dialog
		$('#dialog').dialog({
			autoOpen: false,
			width: 600,
			buttons: {
				"Ok": function() {
					$(this).dialog("close");
				},
				"Cancel": function() {
					$(this).dialog("close");
				}
			}
		});
	
		// Dialog Link
		$('#dialog_link').click(function(){
			$('#dialog').dialog('open');
			return false;
		});
	
		// Datepicker
		$('#datepicker').datepicker({
			inline: true
		});
	
		// Slider
		$('#slider').slider({
			range: true,
			values: [17, 67]
		});
	
		// Progressbar
		$("#progressbar").progressbar({
			value: 20
		});
	
		//hover states on the static widgets
		$('#dialog_link, ul#icons li').hover(
			function() { $(this).addClass('ui-state-hover'); },
			function() { $(this).removeClass('ui-state-hover'); }
		);
	
	});

	$(document).ready(addhandlers);
	function addhandlers()
	{
		$(".flist").prop("lhidden",true);
		$(".flist").click(showhideflist);
	}
	function showhideflist(obj)
	{
		var $l = $(obj.currentTarget).children("ul");
		if($(obj.currentTarget).prop("lhidden"))
			$l.slideDown(300,function(){$(obj.currentTarget).prop("lhidden",false);});
		else 
			$l.slideUp(300,function(){$(obj.currentTarget).prop("lhidden",true);});
	}

	var _gaq = _gaq || [];
	_gaq.push(['_setAccount', 'UA-31097468-1']);
	_gaq.push(['_trackPageview']);
	
	(function() {
		var ga = document.createElement('script'); ga.type = 'text/javascript'; ga.async = true;
		ga.src = ('https:' == document.location.protocol ? 'https://ssl' : 'http://www') + '.google-analytics.com/ga.js';
		var s = document.getElementsByTagName('script')[0]; s.parentNode.insertBefore(ga, s);
	})();
}); //Closes the ready function.