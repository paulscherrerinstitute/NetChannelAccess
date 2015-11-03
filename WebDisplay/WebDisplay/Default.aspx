<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WebDisplay.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Display</title>
    <link href="/default.css" rel="stylesheet" />
</head>
<body>
    <script src="/Scripts/jquery-1.6.4.min.js"></script>
    <script src="/Scripts/jquery.signalR-2.1.1.min.js"></script>
    <script src="/signalr/hubs"></script>
    <script src="/Scripts/Display.js"></script>

    <form id="form1" runat="server" action="Default.aspx">
        <div id="displayArea" runat="server" enableviewstate="false">
        </div>
    </form>
</body>
</html>
