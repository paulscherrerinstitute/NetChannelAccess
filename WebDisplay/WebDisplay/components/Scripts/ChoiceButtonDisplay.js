function ChoiceButtonDisplay()
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
            /*this.element.style.width = this.data.Width + "px";
            this.element.style.height = this.data.Height + "px";*/

            this.element.innerHTML = "<table width='100%' height='100%'><tr><td>1...</td></tr><tr><td>2...</td></tr></table>";

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
        for (var i = 0; i < this.element.childNodes.length; i++)
        {
            this.element.childNodes[i].style.border = (i == value ? "inset 2px" : "outset 2px");
        }
    }

    this.updateEnum = function (channel, states)
    {
        if (channel == this.data.ChannelName)
        {
            while (this.element.firstChild != null)
            {
                this.element.removeChild(this.element.firstChild);
            }

            for (var i = 0; i < states.length; i++)
            {
                var item = document.createElement("div");
                item.style.width = (this.data.Width - 2) + "px";
                item.style.height = ((this.data.Height - 2) / states.length) + "px";
                item.style.boxSizing = "border-box";
                item.style.backgroundColor = this.data.BackgroundColor;
                item.style.color = this.data.Color;
                item.style.border = "outset 2px";
                item.style.overflow = "hidden";
                item.style.whiteSpace = "nowrap";
                if (((this.data.Height - 2) / states.length) < 15)
                    item.style.fontSize = Math.floor((this.data.Height - 2) / states.length - 4) + "px";
                item.innerHTML = states[i];
                this.element.appendChild(item);
            }
        }
    }
}