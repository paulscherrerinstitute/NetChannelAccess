using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebDisplay.Components
{
    public class RelatedDisplay : DisplayControl
    {
        protected override DisplayControl ADLConfig(ADLConfig config, ADLElement element)
        {
            if (element.Name != "related display")
                return null;
            RelatedDisplay result = new RelatedDisplay
            {
                X = element["object"]["x"],
                Y = element["object"]["y"],
                Width = element["object"]["width"],
                Height = element["object"]["height"],
            };

            if (element["clr"] != null)
                result.Color = "#" + config["color map"]["colors"].Childs[element["clr"]];
            if (element["bclr"] != null)
                result.BackgroundColor = "#" + config["color map"]["colors"].Childs[element["bclr"]];

            result.Label = element["label"];

            List<DisplayOption> options = new List<DisplayOption>();
            for (int i = 0; ; i++)
            {
                if (element["display[" + i + "]"] == null)
                    break;
                options.Add(new DisplayOption
                {
                    Label = (string)element["display[" + i + "]"]["label"],
                    Panel = (string)element["display[" + i + "]"]["name"],
                    Arguments = (string)element["display[" + i + "]"]["args"],
                });
            }
            result.Options = options.ToArray();

            return result;
        }

        [DisplayProperty]
        public string Color { get; set; }

        [DisplayProperty]
        public string BackgroundColor { get; set; }

        [DisplayProperty]
        public string Label { get; set; }

        [DisplayProperty]
        public DisplayOption[] Options { get; set; }

        protected override DisplayControl UIConfig(UIElement config, UIElement element)
        {
            return null;
        }

    }
}