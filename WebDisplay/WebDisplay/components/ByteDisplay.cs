using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebDisplay.Components
{
    public class ByteDisplay : DisplayControl
    {
        [DisplayProperty]
        public string ChannelName { get; set; }

        [DisplayProperty]
        public string OkColor { get; set; }

        [DisplayProperty]
        public string ErrorColor { get; set; }

        [DisplayProperty]
        public string OkValue { get; set; }

        protected override DisplayControl ADLConfig(ADLConfig config, ADLElement element)
        {
            if (element.Name != "byte")
                return null;
            return new ByteDisplay
            {
                X = int.Parse(element["object"]["x"].Value),
                Y = int.Parse(element["object"]["y"].Value),
                Width = int.Parse(element["object"]["width"].Value),
                Height = int.Parse(element["object"]["height"].Value),
                ChannelName = element["monitor"]["chan"].Value,
                OkValue = element["sbit"].Value,
                OkColor = "#" + config["color map"]["colors"].Childs[int.Parse(element["monitor"]["clr"].Value)].Value,
                ErrorColor = "#" + config["color map"]["colors"].Childs[int.Parse(element["monitor"]["bclr"].Value)].Value,
            };
        }

        protected override DisplayControl UIConfig(UIElement config, UIElement element)
        {
            if (element["@class"] != "caLed")
                return null;

            ByteDisplay result = new ByteDisplay
            {
                X = element["property?name=geometry"]["rect"]["x"],
                Y = element["property?name=geometry"]["rect"]["y"],
                Width = element["property?name=geometry"]["rect"]["width"],
                Height = element["property?name=geometry"]["rect"]["height"],
                OkValue = "0"
            };

            if (element["property?name=trueColor"] != null)
                result.OkColor = element["property?name=trueColor"].ToColor();
            if (element["property?name=falseColor"] != null)
                result.ErrorColor = element["property?name=falseColor"].ToColor();
            if (element["property?name=bitNr"] != null)
                result.OkValue = element["property?name=bitNr"]["number"];
            if (element["property?name=channel"] != null)
                result.ChannelName = element["property?name=channel"]["string"];

            return result;
        }
    }
}