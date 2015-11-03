function MenuDisplay()
{
    this.data = {};

    this.init = function (parent)
    {
        conn.server.registerChannel(this.data.ChannelName);
        if (this.element == null || this.element == undefined)
        {
            this.element = document.createElement("select");
            this.element.style.position = 'absolute';
            this.element.style.left = this.data.X + "px";
            this.element.style.top = this.data.Y + "px";
            this.element.style.width = this.data.Width + "px";
            this.element.style.height = this.data.Height + "px";
            this.element.style.zIndex = 30000;
            this.element.style.backgroundColor = this.data.BackgroundColor;
            this.element.style.color = this.data.Color;
            this.element.style.border = "outset 2px";
            this.element.style.outline = "none";

            parent.appendChild(this.element);
        }
    }

    this.disconnect = function ()
    {
    }

    this.update = function (channel, value)
    {
        if (channel != this.data.ChannelName)
            return;

        value = parseInt(value);
        this.element.selectedIndex = value;
    }

    this.updateEnum = function (channel, states)
    {
        if (channel == this.data.ChannelName)
        {
            while (this.element.options.length > 0)
            {
                this.element.remove(0);
            }

            for (var i = 0; i < states.length; i++)
            {
                this.element.add(new Option(states[i], i));
            }
        }
    }
}