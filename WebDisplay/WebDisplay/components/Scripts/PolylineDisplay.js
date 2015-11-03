function PolylineDisplay()
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

            var r = Math.max(this.data.Width / 2.0, this.data.Height / 2.0);

            this.ctx = this.element.getContext("2d");

            this.draw(null);

            if (this.data.ColorAlarmChannel != "")
                conn.server.registerChannel(this.data.ColorAlarmChannel + ".SEVR");

            parent.appendChild(this.element);
        }
    }

    this.draw = function (color)
    {
        this.ctx.clearRect(0, 0, this.data.Width, this.data.Height);
        var p = this.data.Points;
        for (var i = 0; i < p.length; i++)
        {
            if (i == 0)
                this.ctx.moveTo(p[i].X + 0.5 - this.data.X, p[i].Y + 0.5 - this.data.Y);
            else
                this.ctx.lineTo(p[i].X + 0.5 - this.data.X, p[i].Y + 0.5 - this.data.Y);
        }
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