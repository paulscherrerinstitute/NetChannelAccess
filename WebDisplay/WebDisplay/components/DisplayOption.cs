using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Xml.Linq;

namespace WebDisplay.Components
{
    public class DisplayOption : IJSONSerializable
    {
        public string Panel { get; set; }
        public string Label { get; set; }

        public string Arguments { get; set; }

        public void Serialize(HtmlTextWriter writer)
        {
            writer.Write("{Panel:'" + Panel + "',Label:'" + Label + "',Arguments:'" + Arguments + "'}");
        }
    }
}
