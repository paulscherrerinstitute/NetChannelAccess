using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebDisplay.Components
{
    public class TextDisplay : DisplayControl
    {
        [DisplayProperty]
        public string Text { get; set; }

        [DisplayProperty]
        public string Color { get; set; }

        override protected DisplayControl ADLConfig(ADLConfig config, ADLElement element)
        {
            if (element.Name != "text")
                return null;
            TextDisplay result = new TextDisplay
            {
                Text = element["textix"],
                X = element["object"]["x"],
                Y = element["object"]["y"],
                Width = element["object"]["width"],
                Height = element["object"]["height"],
                Align = "left"
            };

            if (element["align"] != null)
            {
                if (element["align"].Value.Contains("right"))
                    result.Align = "right";
                else if (element["align"].Value.Contains("centered"))
                    result.Align = "center";
            }
            if (element["basic attribute"] != null && element["basic attribute"]["clr"] != null)
                result.Color = "#" + config["color map"]["colors"].Childs[int.Parse(element["basic attribute"]["clr"].Value)].Value;
            return result;
        }

        [DisplayProperty]
        public string Align { get; set; }

        protected override DisplayControl UIConfig(UIElement config, UIElement element)
        {
            if (element["@class"] != "caLabel")
                return null;

            TextDisplay result = new TextDisplay
            {
                Text = element["property?name=text"]["string"],
                X = element["property?name=geometry"]["rect"]["x"],
                Y = element["property?name=geometry"]["rect"]["y"],
                Width = element["property?name=geometry"]["rect"]["width"],
                Height = element["property?name=geometry"]["rect"]["height"],
                Align = "left"
            };

            if ((((string)element["property?name=alignement"])??"").Contains("AlignRight"))
                result.Align = "right";
            else if ((((string)element["property?name=alignement"])??"").Contains("AlignCenter"))
                result.Align = "center";

            if (element["property?name=foreground"] != null)
                result.Color = element["property?name=foreground"]["color"].ToColor();
            return result;
        }

    }
}