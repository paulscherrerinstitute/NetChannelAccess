using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebDisplay.Components
{
    public class ChoiceButtonDisplay : DisplayControl
    {
        protected override DisplayControl ADLConfig(ADLConfig config, ADLElement element)
        {
            if (element.Name != "choice button")
                return null;
            ChoiceButtonDisplay result = new ChoiceButtonDisplay
            {
                X = element["object"]["x"],
                Y = element["object"]["y"],
                Width = element["object"]["width"],
                Height = element["object"]["height"],
            };

            if (element["control"] != null && element["control"]["clr"] != null)
                result.Color = "#" + config["color map"]["colors"].Childs[element["control"]["clr"]];
            if (element["control"] != null && element["control"]["bclr"] != null)
                result.BackgroundColor = "#" + config["color map"]["colors"].Childs[element["control"]["bclr"]];
            if (element["control"] != null && element["control"]["chan"] != null)
                result.ChannelName = element["control"]["chan"];

            return result;
        }

        [DisplayProperty]
        public string Color { get; set; }

        [DisplayProperty]
        public string BackgroundColor { get; set; }

        [DisplayProperty]
        public string ChannelName { get; set; }

        protected override DisplayControl UIConfig(UIElement config, UIElement element)
        {
            return null;
        }
    }
}