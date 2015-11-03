var conn = null;

var controls = [];

var alarmColors = ["#00E000", "#FFCC00", "#E00000", "#FFFFFF"];

var enumStates = {};

var components = [];

$(function ()
{
    // Declare a proxy to reference the hub.
    conn = $.connection.updateHub;
    connect();

    conn.client.updateReceived = function (channel, value)
    {
        for (var i = 0; i < controls.length; i++)
        {
            controls[i].object.update(channel, value);
        }
    }

    conn.client.UpdateEnumStates = function (channel, states)
    {
        enumStates[channel] = states;
        for (var i = 0; i < controls.length; i++)
        {
            if (controls[i].object.updateEnum != null && controls[i].object.updateEnum != undefined)
                controls[i].object.updateEnum(channel, states);
        }
    }

    // Automatically reconnect
    $.connection.hub.disconnected(function ()
    {
        for (var i = 0; i < controls.length; i++)
        {
            controls[i].object.disconnect();
        }

        setTimeout("connect();", 5000); // Restart connection after 5 seconds.
    });
});

function connect()
{
    // Start the connection.
    $.connection.hub.start().done(function ()
    {
        components = eval(conn.server.getAvailableComponents());
    });
}