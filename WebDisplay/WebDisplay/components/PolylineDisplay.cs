using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace WebDisplay.Components
{
    public class PolylineDisplay : DisplayControl
    {
        protected override DisplayControl ADLConfig(ADLConfig config, ADLElement element)
        {
            if (element.Name != "polyline" && element.Name != "polygon")
                return null;
            PolylineDisplay result = new PolylineDisplay
            {
                X = element["object"]["x"],
                Y = element["object"]["y"],
                Width = element["object"]["width"] + 1,
                Height = element["object"]["height"] + 1,
                Fill = "solid",
                Points = new List<Point>()
            };

            if (element["basic attribute"] != null)
            {
                if (element["basic attribute"]["clr"] != null)
                    result.Color = "#" + config["color map"]["colors"].Childs[element["basic attribute"]["clr"]];
                if (element["basic attribute"]["style"] != null)
                    result.Style = element["basic attribute"]["style"];
                if (element["basic attribute"]["fill"] != null)
                    result.Fill = element["basic attribute"]["fill"];
                if (element["basic attribute"]["width"] != null)
                    result.StrokeWidth = element["basic attribute"]["width"];
            }

            if (element["dynamic attribute"] != null && element["dynamic attribute"]["clr"] != null
                && element["dynamic attribute"]["clr"] == "alarm")
            {
                result.ColorAlarmChannel = element["dynamic attribute"]["chan"];
            }

            if (element.Name == "polyline")
                result.Fill = "outline";

            foreach (var i in element["points"])
                result.Points.Add((Point)i);
            return result;
        }

        [DisplayProperty]
        public string Fill { get; set; }

        [DisplayProperty]
        public List<Point> Points { get; set; }

        [DisplayProperty]
        public string Color { get; set; }

        [DisplayProperty]
        public string Style { get; set; }

        [DisplayProperty]
        public int StrokeWidth { get; set; }

        [DisplayProperty]
        public string ColorAlarmChannel { get; set; }

        protected override DisplayControl UIConfig(UIElement config, UIElement element)
        {
            if (element["@class"] != "caPolyLine")
                return null;

            PolylineDisplay result = new PolylineDisplay
            {
                X = element["property?name=geometry"]["rect"]["x"],
                Y = element["property?name=geometry"]["rect"]["y"],
                Width = element["property?name=geometry"]["rect"]["width"],
                Height = element["property?name=geometry"]["rect"]["height"],
                Fill = "outline",
                Style = "solid"
            };

            result.Color = element["property?name=foreground"].ToColor();

            if (element["property?name=polystyle"] != null && ((string)element["property?name=polystyle"]).Contains("Polyline"))
            {
                result.Fill = "outline";
            }
            else
            {
                if (element["property?name=fillstyle"] != null && ((string)element["property?name=fillstyle"]).Contains("Filled"))
                    result.Fill = "fill";
            }

            if (element["property?name=linestyle"] != null && ((string)element["property?name=linestyle"]).Contains("BigDash"))
                result.Style = "bigdash";
            else if (element["property?name=linestyle"] != null && ((string)element["property?name=linestyle"]).Contains("Dash"))
                result.Style = "dash";

            if (element["property?name=lineColor"] != null)
                result.Color = element["property?name=lineColor"].ToColor();
            else if (element["property?name=foreground"] != null)
                result.Color = element["property?name=foreground"].ToColor();

            if (element["property?name=xyPairs"] != null)
            {
                string[] points = ((string)element["property?name=xyPairs"]["string"]).Split(';');
                result.Points.AddRange(points.Select(p => new Point(int.Parse(p.Split(',')[0]), int.Parse(p.Split(',')[1]))));
            }

            return result;
        }

    }
}