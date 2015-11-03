using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;

namespace WebDisplay
{
    public partial class Default : System.Web.UI.Page
    {
        const string bgColorUI = "QWidget#centralWidget {background: rgba(";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request["ui"] != null)
            {
                string ui = File.ReadAllText(System.Configuration.ConfigurationManager.AppSettings["uiPath"] + Request.Params["ui"]);
                if (ui.IndexOf(bgColorUI) != -1)
                {
                    int pos = ui.IndexOf(bgColorUI) + bgColorUI.Length;
                    string color = ui.Substring(pos, ui.IndexOf(")", pos) - pos);
                    string[] p = color.Split(',');

                    form1.Controls.Add(new Label { Text = "<script>document.body.style.backgroundColor = '#" + string.Format("{0:X2}{1:X2}{2:X2}", int.Parse(p[0].Trim()), int.Parse(p[1].Trim()), int.Parse(p[2].Trim())) + "';</script>" });
                }
                XElement xelement = XElement.Parse(ui);
                var widgets = xelement.Element("widget").Element("widget").Elements("widget");
                foreach (XElement i in widgets)
                {
                    DisplayControl elem = DisplayControl.CreateControl(new UIElement(xelement), new UIElement(i));
                    if (elem != null)
                        this.Controls.Add(elem);
                }
                return;
            }

            if (Request["adl"] == null)
            {
                string[] files = Directory.GetFiles(System.Configuration.ConfigurationManager.AppSettings["medmPath"], "*.adl");
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("<table id='fileList'>");
                foreach (var i in files)
                {
                    string f = i.Substring(System.Configuration.ConfigurationManager.AppSettings["medmPath"].Length);
                    sb.AppendLine("<tr><td><a href='/Default.aspx?adl=" + f + "'>" + f + "</a></td></tr>");
                }
                sb.AppendLine("</table>");
                displayArea.InnerHtml = sb.ToString();
                return;
            }

            string adl = File.ReadAllText(System.Configuration.ConfigurationManager.AppSettings["medmPath"] + Request["adl"]);
            if (!string.IsNullOrWhiteSpace(Request["macro"]))
            {
                string[] macros = Request["macro"].Split(',');
                foreach (var m in macros)
                {
                    string[] p = m.Trim().Split('=');
                    adl = adl.Replace("$(" + p[0] + ")", p[1]);
                }
            }

            foreach (string param in Request.Params.AllKeys)
            {
                if (param.StartsWith("macro_"))
                    adl = adl.Replace("$(" + param.Substring(6) + ")", Request.Params[param]);
            }

            Regex macroExp = new Regex(@"\$\(([^\)]+)\)");
            var matches = macroExp.Matches(adl);
            if (matches.Count > 0)
            {
                displayArea.InnerHtml += "<input type='hidden' name='adl' value='" + Request["adl"] + "'>";
                displayArea.InnerHtml += "<table id='macroDefinition'>";
                string lastM = "!!";

                foreach (Match m in matches.Cast<Match>().OrderBy(row => row.Groups[1].Value))
                {
                    if (m.Groups[1].Value == lastM)
                        continue;
                    lastM = m.Groups[1].Value;
                    displayArea.InnerHtml += "<tr><td>" + m.Groups[1] + "</td><td>";
                    displayArea.InnerHtml += "<input type='text' name='macro_" + m.Groups[1] + "'></td></tr>";
                }
                displayArea.InnerHtml += "</table>";
                displayArea.InnerHtml += "<br />";
                displayArea.InnerHtml += "<input type='submit' value='Continue'>";
                return;
            }

            ADLConfig config = ADLConfig.Parse(adl);

            form1.Controls.Add(new Label { Text = "<script>document.body.style.backgroundColor = '#" + config["color map"]["colors"].Childs[int.Parse(config["display"]["bclr"].Value)].Value + "';</script>" });

            foreach (var i in config.elements)
            {
                DisplayControl elem = DisplayControl.CreateControl(config, i);
                if (elem != null)
                    this.Controls.Add(elem);
            }
        }
    }
}