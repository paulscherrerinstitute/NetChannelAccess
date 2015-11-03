using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebDisplay.Components
{
    public enum ColorMode
    {
        STATIC,
        ALARM
    }

    public class TextMonitorDisplay : DisplayControl
    {
        [DisplayProperty]
        public string ChannelName { get; set; }

        [DisplayProperty]
        public int Precision { get; set; }

        [DisplayProperty]
        public string Align { get; set; }

        protected override DisplayControl ADLConfig(ADLConfig config, ADLElement element)
        {
            if (element.Name != "text update")
                return null;
            TextMonitorDisplay result = new TextMonitorDisplay
            {
                ChannelName = element["monitor"]["chan"],
                X = element["object"]["x"],
                Y = element["object"]["y"],
                Width = element["object"]["width"],
                Height = element["object"]["height"],
                Align = "left",
                Precision = -1,
                Exponential = false,
                ColorMode = ColorMode.STATIC
            };

            if (element["align"] != null)
            {
                if (element["align"].Value.Contains("right"))
                    result.Align = "right";
                else if (element["align"].Value.Contains("centered"))
                    result.Align = "center";
            }

            if (element["limits"] != null && element["limits"]["precDefault"] != null)
                result.Precision = element["limits"]["precDefault"];
            if (element["format"] != null && element["format"].Value == "exponential")
                result.Exponential = true;
            if (element["monitor"] != null && element["monitor"]["clr"] != null)
                result.Color = "#" + config["color map"]["colors"].Childs[int.Parse(element["monitor"]["clr"].Value)].Value;
            if (element["monitor"] != null && element["monitor"]["bclr"] != null)
                result.BackgroundColor = "#" + config["color map"]["colors"].Childs[int.Parse(element["monitor"]["bclr"].Value)].Value;

            if (element["clrmod"] != null && element["clrmod"].Value == "alarm")
                result.ColorMode = ColorMode.ALARM;

            return result;
        }

        [DisplayProperty]
        public string Color { get; set; }

        [DisplayProperty]
        public bool Exponential { get; set; }

        [DisplayProperty]
        public string BackgroundColor { get; set; }

        [DisplayProperty]
        public ColorMode ColorMode { get; set; }

        protected override DisplayControl UIConfig(UIElement config, UIElement element)
        {
            if (element["@class"] != "caLineEdit")
                return null;

            TextMonitorDisplay result = new TextMonitorDisplay
            {
                ChannelName = element["property?name=channel"]["string"],
                X = element["property?name=geometry"]["rect"]["x"],
                Y = element["property?name=geometry"]["rect"]["y"],
                Width = element["property?name=geometry"]["rect"]["width"],
                Height = element["property?name=geometry"]["rect"]["height"],
                Align = "left",
                Precision = -1,
                Exponential = false,
                ColorMode = ColorMode.STATIC
            };

            if ((((string)element["property?name=alignement"]) ?? "").Contains("AlignRight"))
                result.Align = "right";
            else if ((((string)element["property?name=alignement"]) ?? "").Contains("AlignCenter"))
                result.Align = "center";

            if (element["property?name=foreground"] != null)
                result.Color = element["property?name=foreground"]["color"].ToColor();
            if (element["property?name=background"] != null)
                result.BackgroundColor = element["property?name=background"]["color"].ToColor();

            if (element["property?name=precision"] != null)
                result.Precision = element["property?name=precision"]["number"];

            if (element["property?name=formatType"] != null && (string)element["property?name=formatType"]["enum"] == "exponential")
                result.Exponential = true;
            if (element["property?name=colorMode"] != null && ((string)element["property?name=colorMode"]["enum"]).Contains("Alarm"))
                result.ColorMode = Components.ColorMode.ALARM;

            if (element["property?name=formatType"] != null && ((string)element["property?name=formatType"]).Contains("exponential"))
                result.Exponential = true;

            return result;
        }
    }
}