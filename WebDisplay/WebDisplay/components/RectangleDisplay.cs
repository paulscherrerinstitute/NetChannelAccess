using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebDisplay.Components
{
    public class RectangleDisplay : DisplayControl
    {
        protected override DisplayControl ADLConfig(ADLConfig config, ADLElement element)
        {
            if (element.Name != "rectangle")
                return null;
            RectangleDisplay result = new RectangleDisplay
            {
                X = element["object"]["x"],
                Y = element["object"]["y"],
                Width = element["object"]["width"],
                Height = element["object"]["height"],
                Fill = "solid",
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

            return result;
        }

        [DisplayProperty]
        public string Fill { get; set; }

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
            if (element["@class"] != "caGraphics" && (element["property?name=form"] == null || element["property?name=form"]["enum"] != "caGraphics::Rectangle"))
                return null;

            OvalDisplay result = new OvalDisplay
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

            return result;
        }

    }
}