function RelatedDisplay()
{
    this.data = {};

    this.init = function (parent)
    {
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

            this.element.add(new Option("-> " + this.data.Label));
            this.element.obj = this;
            this.element.onchange = function ()
            {
                this.obj.choose();
            }

            for (var i = 0; i < this.data.Options.length; i++)
            {
                this.element.add(new Option(this.data.Options[i].Label));
            }

            parent.appendChild(this.element);
        }
    }

    this.choose = function ()
    {
        var o = this.data.Options[this.element.selectedIndex - 1];
        //window.location = "/Default.aspx?adl=" + o.Panel + "&macro=" + escape(o.Arguments);
        window.open("/Default.aspx?adl=" + o.Panel + "&macro=" + escape(o.Arguments));
        this.element.selectedIndex = 0;
    }

    this.disconnect = function ()
    {
    }

    this.update = function (channel, value)
    {
    }
}