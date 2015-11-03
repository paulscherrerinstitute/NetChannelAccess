function RectangleDisplay()
{
    this.data = {};

    this.init = function (parent)
    {
        if (this.element == null || this.element == undefined)
        {
            this.element = document.createElement("canvas");
            this.element.style.position = 'absolute';
            this.element.style.left = this.data.X + "px";
            this.element.style.top = this.data.Y + "px";
            this.element.style.width = this.data.Width + "px";
            this.element.style.height = this.data.Height + "px";
            this.element.width = this.data.Width;
            this.element.height = this.data.Height;

            this.ctx = this.element.getContext("2d");

            this.draw(null);

            if (this.data.ColorAlarmChannel != "")
                conn.server.registerChannel(this.data.ColorAlarmChannel + ".SEVR");

            parent.appendChild(this.element);
        }
    }

    this.disconnect = function ()
    {
    }

    this.draw = function(color)
    {
        this.ctx.rect((this.data.StrokeWidth + 1) + 0.5, (this.data.StrokeWidth + 1) + 0.5, this.data.Width - (this.data.StrokeWidth + 1) * 2, this.data.Height - (this.data.StrokeWidth + 1) * 2);

        if (this.data.Fill == "outline")
        {
            if (this.data.Style == "dash")
                this.ctx.setLineDash([5, 5]);
            else if (this.data.Style == "bigdash")
                this.ctx.setLineDash([10, 10]);
            if (color != null && color != undefined)
                this.ctx.strokeStyle = color;
            else
                this.ctx.strokeStyle = this.data.Color;
            this.ctx.lineWidth = this.data.StrokeWidth;
            this.ctx.stroke();
        }
        else
        {
            if (color != null && color != undefined)
                this.ctx.fillStyle = color;
            else
                this.ctx.fillStyle = this.data.Color;
            this.ctx.fill();
        }
    }

    this.update = function (channel, value)
    {
        if (channel == this.data.ColorAlarmChannel + ".SEVR")
        {
            var status = parseInt(value);
            this.draw(alarmColors[status]);
        }

        DisplayVisibility(this, channel, value);
    }
}