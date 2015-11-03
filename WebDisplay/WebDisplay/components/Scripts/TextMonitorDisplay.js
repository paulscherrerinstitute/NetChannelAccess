function TextMonitorDisplay()
{
    this.data = {};

    this.init = function (parent)
    {
        conn.server.registerChannel(this.data.ChannelName);
        if (this.element == null || this.element == undefined)
        {
            this.element = document.createElement("div");
            this.element.style.position = 'absolute';
            this.element.style.left = this.data.X + "px";
            this.element.style.top = this.data.Y + "px";
            this.element.style.whiteSpace = "nowrap";
            this.element.style.verticalAlign = "middle";
            if (this.data.Align != "left")
            {
                this.element.style.textAlign = this.data.Align;
            }
            this.element.style.width = this.data.Width + "px";
            this.element.style.height = this.data.Height + "px";
            this.element.style.overflow = "hidden";
            this.data.Precision = parseInt(this.data.Precision);
            this.element.style.color = this.data.Color;
            this.element.style.backgroundColor = this.data.BackgroundColor;
            parent.appendChild(this.element);
        }

        this.data.PrecisionChannel = null;
        if (this.data.Precision == -1)
        {
            if (this.data.ChannelName.indexOf('.') != -1)
                this.data.PrecisionChannel = this.data.ChannelName.substr(0, this.data.ChannelName.indexOf('.')) + ".PREC";
            else
                this.data.PrecisionChannel = this.data.ChannelName + ".PREC";
            conn.server.registerChannel(this.data.PrecisionChannel);
        }
        this.data.AlarmChannel = null;
        if (this.data.ColorMode == "ALARM")
        {
            if (this.data.ChannelName.indexOf('.') != -1)
                this.data.AlarmChannel = this.data.ChannelName.substr(0, this.data.ChannelName.indexOf('.')) + ".SEVR";
            else
                this.data.AlarmChannel = this.data.ChannelName + ".SEVR";
            conn.server.registerChannel(this.data.AlarmChannel);
        }
    }

    this.disconnect = function ()
    {
        this.element.innerHTML = "---";
    }

    this.update = function (channel, value)
    {
        DisplayVisibility(this, channel, value);

        if (channel == this.data.AlarmChannel)
        {
            this.element.style.color = alarmColors[parseInt(value)];
        }

        if (channel == this.data.PrecisionChannel && this.data.Precision == -1)
        {
            this.data.Precision = parseInt(value);
            if (value == "" || value == "---")
                value = 0;
            else if (!isNaN(parseFloat(this.element.innerHTML)))
            {
                this.element.innerHTML = FormatNumber(parseFloat(this.element.innerHTML), this.data.Precision, this.data.Exponential);
            }
        }
        else if (channel == this.data.ChannelName)
        {
            if (enumStates[channel] != null && enumStates[channel] != undefined)
            {
                this.element.innerHTML = enumStates[channel][parseInt(value)];
                return;
            }
            if (!isNaN(parseFloat(value)))
                this.element.innerHTML = FormatNumber(value, this.data.Precision, this.data.Exponential);
            else
                this.element.innerHTML = value;
        }
    }
}