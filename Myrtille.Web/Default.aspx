<%@ Page Language="C#" Inherits="Myrtille.Web.Default" Codebehind="Default.aspx.cs" AutoEventWireup="true" Culture="auto" UICulture="auto" %>
<%@ OutputCache Location="None" %>
<%@ Import Namespace="System.Globalization" %>
<%@ Import Namespace="Myrtille.Web" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
	
    <head>
        <!-- force IE out of compatibility mode -->
        <meta http-equiv="X-UA-Compatible" content="IE=edge,chrome=1"/>
        <meta name="viewport" content="initial-scale=1.0"/>
        <title>Myrtille</title>
        
        <!-- Bootstrap Latest compiled and minified CSS -->
        <link rel="stylesheet" href="css/vendor/bootstrap/bootstrap.min.css" />
        <!-- Bootstrap Optional theme -->
        <link rel="stylesheet" href="css/vendor/bootstrap/bootstrap-theme.min.css" />

        <!-- Site css -->
        <link rel="stylesheet" type="text/css" href="css/Default.css" />
        <link rel="stylesheet" type="text/css" href="css/myrtille.css" />

        <script language="javascript" type="text/javascript" src="js/myrtille.js"></script>
        <script language="javascript" type="text/javascript" src="js/config.js"></script>
        <script language="javascript" type="text/javascript" src="js/dialog.js"></script>
        <script language="javascript" type="text/javascript" src="js/display.js"></script>
        <script language="javascript" type="text/javascript" src="js/display/canvas.js"></script>
        <script language="javascript" type="text/javascript" src="js/display/divs.js"></script>
        <script language="javascript" type="text/javascript" src="js/network.js"></script>
        <script language="javascript" type="text/javascript" src="js/network/buffer.js"></script>
        <script language="javascript" type="text/javascript" src="js/network/longpolling.js"></script>
        <script language="javascript" type="text/javascript" src="js/network/websocket.js"></script>
        <script language="javascript" type="text/javascript" src="js/network/xmlhttp.js"></script>
        <script language="javascript" type="text/javascript" src="js/user.js"></script>
        <script language="javascript" type="text/javascript" src="js/user/keyboard.js"></script>
        <script language="javascript" type="text/javascript" src="js/user/mouse.js"></script>
        <script language="javascript" type="text/javascript" src="js/user/touchscreen.js"></script>

        <script type="text/javascript" src="js/vendor/angular/angular.min.js"></script>
        <script type="text/javascript" src="js/vendor/angular/angular-animate.min.js"></script>
        <script type="text/javascript" src="js/vendor/angular-router/angular-ui-router.min.js"></script>

        <!-- Bootstrap -->
        <script language="javascript" type="text/javascript" src="js/vendor/angular-bootstrap/ui-bootstrap-tpls-2.3.2.min.js"></script>
        

        <%--<script language="javascript" type="text/javascript" src="//code.jquery.com/jquery-3.1.1.slim.min.js"></script> --%>
        <script language="javascript" type="text/javascript" src="js/app/app.js"></script>
        <script language="javascript" type="text/javascript" src="js/app/common/modal/components/modalComponent.js"></script>

        <script language="javascript" type="text/javascript" src="js/app/RemoteDesktop/services/RemoteDesktop.js"></script>

        <script language="javascript" type="text/javascript" src="js/app/main/controllers/leftController.js"></script>
        <script language="javascript" type="text/javascript" src="js/app/main/controllers/mainController.js"></script>
        <script language="javascript" type="text/javascript" src="js/app/main/controllers/rightController.js"></script>

	</head>
	<body ng-app="myrtille" class="container-fluid">
        <script type="text/javascript">
            var availableConfigs = <%=Newtonsoft.Json.JsonConvert.SerializeObject(SessionConfig.AvailableConfigs.Select(x => new { x.ID, x.UserName, x.Programs })) %>;
        </script>
        
        <div class="row">
            <div id="left-pane" ui-view="left" style="display:inline-block; background-color:#BBBBBB; float:left;"></div>
            <div id="main-pane" ui-view="main" style="display:inline-block; background-color:#808080; float:left;"></div>
            <div id="rigth-pane" ui-view="right" style="    display:inline-block; background-color:#555555; float:left;"></div>
        </div>
	</body>

</html>