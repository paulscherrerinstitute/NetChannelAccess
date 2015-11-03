function CompositeDisplay()
{
    this.data = {};

    this.init = function (parent)
    {
        if (this.element == null || this.element == undefined)
        {
            this.element = document.createElement("div");
            this.element.style.position = 'absolute';
            this.element.style.left = "0px";
            this.element.style.top = "0px";

            parent.appendChild(this.element);

            for (var i = 0; i < this.data.childs.length; i++)
            {
                if (this.data.childs[i].object == null || this.data.childs[i].object == undefined)
                {
                    this.data.childs[i].object = eval("new " + this.data.childs[i].type + "()");
                    this.data.childs[i].object.data = this.data.childs[i];
                }
                this.data.childs[i].object.init(this.element);
                if (this.data.childs[i].object.data.DynamicChannelA != "")
                    conn.server.registerChannel(this.data.childs[i].object.data.DynamicChannelA);
            }
        }
    }

    this.disconnect = function ()
    {
        for (var i = 0; i < this.data.childs.length; i++)
        {
            this.data.childs[i].object.disconnect();
        }
    }

    this.update = function (channel, value)
    {
        DisplayVisibility(this, channel, value);
        for (var i = 0; i < this.data.childs.length; i++)
        {
            this.data.childs[i].object.update(channel, value);
        }
    }

    this.updateEnum = function (channel, states)
    {
        for (var i = 0; i < this.data.childs.length; i++)
        {
            if (this.data.childs[i].object != null && this.data.childs[i].object.updateEnum != null && this.data.childs[i].object.updateEnum != undefined)
                this.data.childs[i].object.updateEnum(channel, states);
        }
    }
}