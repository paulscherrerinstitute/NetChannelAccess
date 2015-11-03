function TextDisplay()
{
    this.data = {};

    this.init = function (parent)
    {
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
                this.element.style.width = this.data.Width + "px";
                this.element.style.height = this.data.Height + "px";
                this.element.style.overflow = "hidden";
            }
            this.element.innerHTML = this.data.Text;
            this.element.style.color = this.data.Color;
            parent.appendChild(this.element);
        }
    }

    this.disconnect = function ()
    {
    }

    this.update = function (channel, value)
    {
        DisplayVisibility(this, channel, value);
    }
}