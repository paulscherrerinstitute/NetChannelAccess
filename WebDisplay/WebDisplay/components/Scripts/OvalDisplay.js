function OvalDisplay()
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

            this.ctx = this.element.getContext("2d");
            this.draw(null);

            if (this.data.ColorAlarmChannel != "")
                conn.server.registerChannel(this.data.ColorAlarmChannel + ".SEVR");

            parent.appendChild(this.element);
        }
    }

    this.draw = function(color)
    {
        var r = Math.max(this.data.Width / 2.0, this.data.Height / 2.0);

        this.ctx.save();
        this.ctx.translate(this.data.Width / 2.0, this.data.Height / 2.0);
        this.ctx.scale((this.data.Width / 2.0) / r, (this.data.Height / 2.0) / r);

        this.ctx.arc(0, 0, r - (this.data.StrokeWidth + 1), 0, Math.PI * 2, false);

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
            this.ctx.lineWidth = this.data.StrokeWidth + 1;
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
        this.ctx.restore();
    }

    this.disconnect = function ()
    {
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