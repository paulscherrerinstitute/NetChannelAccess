using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebDisplay.Components
{
    public class CompositeDisplay : DisplayControl
    {
        protected override DisplayControl ADLConfig(ADLConfig config, ADLElement element)
        {
            if (element.Name != "composite")
                return null;

            CompositeDisplay result = new CompositeDisplay
            {
                X = element["object"]["x"],
                Y = element["object"]["y"],
                Width = element["object"]["width"],
                Height = element["object"]["height"],
            };

            foreach (var i in element["children"])
            {
                DisplayControl ctrl = DisplayControl.CreateControl(config, i);
                if (ctrl != null)
                    result.Controls.Add(ctrl);
            }

            return result;
        }

        protected override DisplayControl UIConfig(UIElement config, UIElement element)
        {
            if (element["@class"] != "caFrame")
                return null;

            CompositeDisplay result = new CompositeDisplay
            {
                X = element["property?name=geometry"]["rect"]["x"],
                Y = element["property?name=geometry"]["rect"]["y"],
                Width = element["property?name=geometry"]["rect"]["width"],
                Height = element["property?name=geometry"]["rect"]["height"],
            };

            foreach (UIElement i in element.AllChilds("widget"))
            {
                var ctrl = DisplayControl.CreateControl(config, i);
                if (ctrl != null)
                    result.Controls.Add(ctrl);
            }

            return result;
        }
    }
}