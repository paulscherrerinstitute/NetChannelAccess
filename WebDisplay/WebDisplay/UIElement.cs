using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace WebDisplay
{
    public class UIElement
    {
        private XElement element;
        private string value;

        public UIElement(XElement element)
        {
            this.element = element;
        }

        private UIElement()
        {
        }

        public UIElement this[string key]
        {
            get
            {
                if (key.StartsWith("@"))
                {
                    return new UIElement { value = this.element.Attribute(key.Substring(1)).Value };
                }
                if (key.Contains("?"))
                {
                    string tag = key.Split('?')[0];
                    string[] p = key.Split('?')[1].Split('=');
                    var elems = this.element.Elements(tag);
                    if (!elems.Any())
                        return null;
                    var res = elems.FirstOrDefault(row => row.Attribute(p[0]).Value == p[1]);
                    if (res == null)
                        return null;
                    return new UIElement(res);
                }
                return new UIElement(this.element.Element(key));
            }
        }

        public static implicit operator string(UIElement elem)
        {
            if (elem == null)
                return null;
            if (elem.value != null)
                return elem.value;
            if (elem.element == null)
                return null;
            return elem.element.Value;
        }

        public static implicit operator int(UIElement elem)
        {
            if (elem.value != null)
                return int.Parse(elem.value);
            return int.Parse(elem.element.Value);
        }

        public static implicit operator double(UIElement elem)
        {
            if (elem.value != null)
                return double.Parse(elem.value);
            return double.Parse(elem.element.Value);
        }

        public string ToColor()
        {
            return "#" + string.Format("{0:X2}{1:X2}{2:X2}", (int)this["red"], (int)this["green"], (int)this["blue"]);
        }

        internal IEnumerable<UIElement> AllChilds(string name)
        {
            return element.Elements(name).Select(row => new UIElement(row));
        }
    }
}