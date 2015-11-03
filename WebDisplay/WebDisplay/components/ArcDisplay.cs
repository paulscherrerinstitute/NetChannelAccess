using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebDisplay.Components
{
    public class ArcDisplay : DisplayControl
    {
        protected override DisplayControl ADLConfig(ADLConfig config, ADLElement element)
        {
            if (element.Name != "arc")
                return null;
            ArcDisplay result = new ArcDisplay
            {
                X = element["object"]["x"],
                Y = element["object"]["y"],
                Width = element["object"]["width"],
                Height = element["object"]["height"],
                Fill = "solid"
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

            if (element["begin"] != null)
                result.Start = (Math.PI * 2.0 * (double)element["begin"] / 23040.0);
            if (element["path"] != null)
                result.End = (Math.PI * 2.0 * (double)element["path"] / 23040.0);
            return result;
        }

        [DisplayProperty]
        public string Color { get; set; }

        [DisplayProperty]
        public string Style { get; set; }

        [DisplayProperty]
        public string Fill { get; set; }

        [DisplayProperty]
        public int StrokeWidth { get; set; }

        [DisplayProperty]
        public double Start { get; set; }

        [DisplayProperty]
        public double End { get; set; }

        [DisplayProperty]
        public string ColorAlarmChannel { get; set; }

        protected override DisplayControl UIConfig(UIElement config, UIElement element)
        {
            if (element["@class"] != "caGraphics" && element["property?name=form"] != null && element["property?name=form"]["enum"] != "caGraphics::Arc")
                return null;

            ArcDisplay result = new ArcDisplay
            {
                X = element["property?name=geometry"]["rect"]["x"],
                Y = element["property?name=geometry"]["rect"]["y"],
                Width = element["property?name=geometry"]["rect"]["width"],
                Height = element["property?name=geometry"]["rect"]["height"],
            };

            if (element["property?name=lineColor"] != null)
                result.Color = element["property?name=lineColor"].ToColor();

            if (element["property?name=polystyle"] != null && ((string)element["property?name=polystyle"]).Contains("Polyline"))
                result.Fill = "outline";
            else if (element["property?name=fillstyle"] != null && ((string)element["property?name=fillstyle"]).Contains("Filled"))
                    result.Fill = "fill";

            if (element["property?name=linestyle"] != null && ((string)element["property?name=linestyle"]).Contains("BigDash"))
                result.Style = "bigdash";
            else if (element["property?name=linestyle"] != null && ((string)element["property?name=linestyle"]).Contains("Dash"))
                result.Style = "dash";

            if (element["property?name=startAngle"] != null)
                result.Start = (Math.PI * element["property?name=startAngle"]["number"] / 180.0);
            if (element["property?name=spanAngle"] != null)
                result.End = (Math.PI * element["property?name=spanAngle"]["number"] / 180.0);

            return result;
        }
    }
}