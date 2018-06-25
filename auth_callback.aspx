<!DOCTYPE html>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="auth_callback.aspx.cs" EnableSessionState="True" Inherits="gembrook.club.basic_modules.club_admin.website.auth_callback" TargetSchema="http://schemas.microsoft.com/intellisense/ie5" %>
<%@ import namespace="gembrook.club.framework.classes" %>
<%@ register Tagprefix="club" Namespace="gembrook.club.framework.controls" Assembly="club"%>
<%@ Register TagPrefix="ajax" Namespace="Telerik.Web.UI" Assembly="telerik.Web.UI" %>


<html lang="en">
<head>
		<meta http-equiv="X-UA-Compatible" content="IE=10" />
		<meta name="robots" content="noindex,nofollow" />
</head>

<body >
	<form>
	<div id="redir">
<!--This page really doesn't do anything. I just need a solid page URL for oAuth to call back to. Resource providers will determine that this page actually exists
Capture the access token here, and then save it to session. Close this page. -->
	</div>
	</form>
</body>

<script>
	window.close();

</script>

</html>
