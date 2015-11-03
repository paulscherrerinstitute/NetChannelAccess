var conn = null;

var controls = [];

var alarmColors = ["#00E000", "#FFCC00", "#E00000", "#FFFFFF"];

var enumStates = {};

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
        var topParent = document.getElementById("displayArea");

        for (var i = 0; i < controls.length; i++)
        {
            if (controls[i].object == null || controls[i].object == undefined)
            {
                controls[i].object = eval("new " + controls[i].type + "()");
                controls[i].object.data = controls[i];
            }
            controls[i].object.init(topParent);
            if (controls[i].object.data.DynamicChannelA != "")
                conn.server.registerChannel(controls[i].object.data.DynamicChannelA);
        }
    });
}

function DisplayVisibility(elem, channel, value)
{
    if (elem.data.DynamicChannelA == channel)
    {
        if (elem.data.DynamicCondition == "IF_NOT_ZERO")
        {
            if (value == "0")
                elem.element.style.visibility = "hidden";
            else
                elem.element.style.visibility = "visible";
        }
        else if (elem.data.DynamicCondition == "IF_ZERO")
        {
            if (value != "0")
                elem.element.style.visibility = "hidden";
            else
                elem.element.style.visibility = "visible";
        }
        else if (elem.data.DynamicCondition == "IF_ONE")
        {
            if (value != "1")
                elem.element.style.visibility = "hidden";
            else
                elem.element.style.visibility = "visible";
        }
        else if (elem.data.DynamicCondition == "CALC")
        {
            if (elem.data.JSConditionCode == null || elem.data.JSConditionCode == undefined)
            {
                var exp = new RegExp("([^>!<])=", "g");
                elem.data.JSConditionCode = elem.data.ConditionCode.replace(exp, "$1==");
            }

            if (eval(elem.data.JSConditionCode.replace("A", "value")) == true)
                elem.element.style.visibility = "visible";
            else
                elem.element.style.visibility = "hidden";
        }
    }
}

function FormatNumber(value, precision, exponential)
{
    value = "" + value;
    if (value.indexOf('.') != -1)
    {
        if (exponential == true)
            value = parseFloat(value).toExponential(precision);
        else if (precision == -1)
        {
        }
        else if (precision == 0)
            value = Math.round(parseFloat(value));
        else
        {
            var prec = Math.pow(10, precision);
            value = Math.round(parseFloat(value) * prec) / prec;
        }
    }

    if (precision != -1 && !exponential)
    {
        value = "" + value;
        if (value.indexOf('.') == -1)
            value = "" + value + ".";
        var p = (value.length - 1) - value.indexOf('.');
        while (p < precision)
        {
            value = value + "0";
            p++;
        }
    }
    return value;
}