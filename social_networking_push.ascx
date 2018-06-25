<%@ Control Language="c#" AutoEventWireup="false" EnableViewState="true" Codebehind="social_networking_push.ascx.cs" Inherits="gembrook.club.basic_modules.club_admin.website.social_networking_push" TargetSchema="http://schemas.microsoft.com/intellisense/ie5" %>
<%@ import namespace="gembrook.club.framework.classes" %>
<%@ register Tagprefix="club" Namespace="gembrook.club.framework.controls" Assembly="club"%>
<%@ Register TagPrefix="telerik" Namespace="Telerik.Web.UI" Assembly="Telerik.Web.UI" %>


<style>
.textbox {
	width:450px;
}
.share-label {
	font-size:12px;
	font-weight:bold;
	color:#363636;
}
.test-button {
	cursor:pointer;
	padding:5px 8px; 
	background-color:#cdcdcd;
	border:1px solid #666;
	border-radius:3px;
	font-size:12px;
	font-weight:bold;
}
.test-button:hover {
	background-color:#959595;
}
.share-description {
	clear:left;
	width:400px;
	height:60px;
}
.chars-left,
.chars-left-label {
	color:#F00;
	font-size:11px;
}
.description-text {
	margin:10px 0px;
}
.post-info {
	padding-left:20px;
}
.network-select {
float:left;
margin-right:8px;
}
.available-networks {
	margin-top:5px;
}
.available-networks input img {
	max-width:30px;
	cursor:pointer;
}
.network-select:hover {
	opacity:.8;
}
.clicked {
	border:3px solid #2abd03 !important;
	border-radius:30px;
	box-shadow:0px 0px 6px #72c15d;
}
.network-select {
		border:3px solid #FFFFFF;
}
.clicked:hover {
	opacity:1 !important;
}
.check-spacer {
	 padding:0px 8px;
	 float:left;
 }
.face-check {
	background-image:url(/images/social/facebook_small.png);
}
.twitter-check {
	background-image:url(/images/social/twitter_small.png);
}
.linked-check {
	background-image:url(/images/social/linkedin_small.png);
}
.social-check {
	cursor:pointer;
	float:left;
}
.network-icon {
	height:30px;
	width:30px;
	padding:1px;
	background-repeat:no-repeat;
	float:left;
}
.network-container {
	margin-left:20px;
	margin-top:10px;
}
.push-down {
	margin-top:10px;
}
.network-title {
	font-size:14px; 
	font-weight:bold;
	color:#363636;
}
input[type=checkbox] {
	text-align:left;
	cursor:pointer;
	height:20px;
	float:left;
}
.reset-button {
display:none;
height:16px;
width:16px;
float:left;
margin-left:20px;
margin-top:5px;
}
.alert { 
	display:none; 
	}
</style>

<asp:HiddenField ID="gidHiddenField" runat="server" />
<asp:HiddenField ID="fbHiddenField" runat="server" />
<asp:HiddenField ID="fbGroupField" runat="server" />

<h1><%= pageName %></h1>
<asp:HiddenField ID="hidden_text" runat="server" />
<div class="instruction-text" runat="server">
This screen lists supported Social Networks. Click a network or logo or multiple logos to post this <%= _postType %> to. Type a short description about the <%= _postType %> and then click "Share". 
Your <%= _postType %> and short description will be posted to the selected network(s). <strong>Please make sure that Popups are enabled in your browser or enable them for this site.</strong>
</div>
<div id="redir">
<div class="alert alert-danger">
	Your popups are blocked. Please enable popups or allow an exception for this site. Popups are required to use the share tool
</div>
<div class="network-container">
	<div class="push-down" runat="server">
		<span class="network-title">Which Network(s)</span>
		<div class="available-networks" id="network_container" runat="server">
			<asp:Repeater ID="social_network_repeater" OnItemCreated="social_network_link_repeater_ItemCreated" runat="server">
				<ItemTemplate>
					<asp:CheckBox id="social_check"  runat="server" socialId="<%#_socialNetworkId %>" socialUrl="<%# _socialNetworkUrl %>" Text=" " Enabled="true" AutoPostBack="true" OnCheckedChanged="check_box_Click" CssClass="social-check" ToolTip="<%# _socialNetworkName %>" />
					<span class="network-icon"></span>
					<span class="check-spacer"></span>
				</ItemTemplate>
			</asp:Repeater>
			<asp:ImageButton ID="resetButton" ImageUrl="<%# _resetImageUrl %>" onClick="clearTokens" runat="server" ToolTip="Click here to refresh if you've switched logins for your social networking accounts"  CssClass="reset-button" />
			<div class="clear"></div>
		</div>
		<div class="description-text">
			<div style="float:left;" class="share-label">Text</div>
			<div style="float:left;margin-left:10px;" id="text_holder">
				<asp:Textbox id="social_text"  max_length="300" textmode="Multiline" rows="5" cssClass="share-description" runat="server" /><br />
			</div>
			<div class="clear"></div>
		</div>
		<div class="links-to" runat="server">
			<span class="share-label">Links To:</span>
			<span class="post-info share-label"><%= _postType %>:&nbsp;<%= _postDate %> <%= _hyphen %> <%= _postTitle %></span><br />
		</div>
	</div>	
</div>

<div class="button-bar">
	<club:stylebutton id="save_button" allowPostback="true" causesvalidation="true" linktext="Share" onClick="save_button_Click" buttonlayout="IconRight" buttonIcon="ok" runat="server" />
	<club:CancelButton id="cancel_button" runat="server" />
</div>	
		
<div id="fb-root"></div>
</div>

<script type="text/javascript">
	//-- start of snippet --
	function PopupBlocked() {
		var PUtest = window.open("http://www.clubexpress.com", "", "width=100,height=100");
		try { PUtest.close(); return false; }
		catch (e) { return true; }
	}

	if (PopupBlocked()) {
		$('.alert').css('display', 'block');
		$('.network-container').hide();
		$('.button-bar').hide();
	}


	window.fbAsyncInit = function () {
		FB.init({
			appId: '551830351604240',
			cookie: true,
			xfbml: true,
			oauth:true,
			version: 'v2.4'
		});
	};

	(function (d, s, id) {
		var js, fjs = d.getElementsByTagName(s)[0];
		if (d.getElementById(id)) { return; }
		js = d.createElement(s); js.id = id;
		js.src = "//connect.facebook.net/en_US/sdk.js";
		fjs.parentNode.insertBefore(js, fjs);
	} (document, 'script', 'facebook-jssdk'));
</script>


<script type="text/javascript">
	shareType = '<%= _shareType %>'; // will determine which type of page is being shared. Blogs, events, volunteering opportunity, etc.
	itemId = '<%= _itemId %>'; //item id of the particular page, event, etc.
	var faceClick = false;
	var twitterClick = false;
	var linkedinClick = false;
	var shareText = "";


	$(document).ready(function () {
		setupTextareas();
	});

	$('.link-usage').hide();

	$(".social-check").each(function () {
		var $socialId = $(this).attr("socialid");
		if ($socialId == 2) {
			$(this).next('span').addClass('twitter-check');
		}
		if ($socialId == 3) {
			$(this).next('span').addClass('face-check');
		}
		if ($socialId == 1) {
			$(this).next('span').addClass('linked-check');
		}
	});
	function setupIcons() {
		$(".social-check").each(function () {
			var $socialId = $(this).attr("socialid");
			if ($socialId == 2) {
				$(this).next('span').addClass('twitter-check');
			}
			if ($socialId == 3) {
				$(this).next('span').addClass('face-check');
			}
			if ($socialId == 1) {
				$(this).next('span').addClass('linked-check');
			}
		});
	}
	$('input').each(function () {
		var $socialId = $(this).parent().attr("socialid");
		var $socialUrl = $(this).parent().attr("socialurl");
		//
		if ($(this).is(':checked') && $socialId == 3) {
			$("#<%= social_text.ClientID %>").prop('max_length', 300);
			$('.chars-left').text("300");
			$('.link-usage').hide();
		} //end facebook
		if ($(this).is(':checked') && $socialId == 1) {
			$("#<%= social_text.ClientID %>").prop('max_length', 250);
			$('.chars-left').text("250");
			$('.link-usage').hide();
		}
		if ($(this).is(':checked') && $socialId == 2) {
			$("#<%= social_text.ClientID %>").prop('max_length', 120);
			$('.chars-left').text("120");
			$('.link-usage').show();
		} // end twitter
			// end linkedin
	});

	function isMaxLength(obj) {
		var totalLength = $('.share-description').val().length;
		var mlength = obj.getAttribute ? parseInt(obj.getAttribute("maxlength")) : "";
		$('.chars-left').text(mlength - totalLength);
		
	}



</script>
