function TextEntryDisplay()
{
    this.data = {};

    this.init = function (parent)
    {
        conn.server.registerChannel(this.data.ChannelName);
        if (this.element == null || this.element == undefined)
        {
            this.element = document.createElement("input");
            this.element.type = "text";
            this.element.style.position = 'absolute';
            this.element.style.left = this.data.X + "px";
            this.element.style.top = this.data.Y + "px";
            this.element.style.whiteSpace = "nowrap";
            this.element.style.verticalAlign = "middle";
            this.element.style.zIndex = 30000;
            this.element.style.outline = "none";
            if (this.data.Align != "left")
            {
                this.element.style.textAlign = this.data.Align;
            }
            this.element.style.width = this.data.Width + "px";
            this.element.style.height = (this.data.Height - 6) + "px";
            //this.element.style.overflow = "hidden";
            this.data.Precision = parseInt(this.data.Precision);
            this.element.style.color = this.data.Color;
            this.element.style.backgroundColor = this.data.BackgroundColor;
            //this.element.style.padding = "0px";
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
    }

    this.disconnect = function ()
    {
        this.element.innerHTML = "---";
    }

    this.update = function (channel, value)
    {
        DisplayVisibility(this, channel, value);

        if (channel == this.data.PrecisionChannel && this.data.Precision == -1)
        {
            this.data.Precision = parseInt(value);
            if (value == "" || value == "---")
                value = 0;
            else if (!isNaN(parseFloat(this.element.value)))
            {
                this.element.value = FormatNumber(parseFloat(this.element.value), this.data.Precision, this.data.Exponential);
            }
        }
        else if (channel == this.data.ChannelName)
        {
            if (!isNaN(parseFloat(value)))
                this.element.value = FormatNumber(value, this.data.Precision, this.data.Exponential);
            else
                this.element.value = value;
        }
    }
}