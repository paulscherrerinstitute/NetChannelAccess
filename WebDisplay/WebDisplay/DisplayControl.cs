using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.UI;

namespace WebDisplay
{
    public enum DynamicCondition
    {
        NONE,
        IF_NOT_ZERO,
        IF_ZERO,
        IF_ONE,
        CALC
    }

    public abstract class DisplayControl : UserControl
    {
        static List<DisplayControl> knownControls;

        static DisplayControl()
        {
            knownControls = Assembly.GetExecutingAssembly().GetTypes()
                .Where(row => row.IsSubclassOf(typeof(DisplayControl)))
                .Select(row => (DisplayControl)row.GetConstructor(new Type[] { }).Invoke(new object[] { }))
                .ToList();
        }

        public static DisplayControl CreateControl(ADLConfig config, ADLElement element)
        {
            DisplayControl result = knownControls.Select(row => row.ADLConfig(config, element))
                .FirstOrDefault(row => row != null);
            if (result != null)
            {
                result.DynamicCondition = DynamicCondition.NONE;
                if (element["dynamic attribute"] != null)
                {
                    if (element["dynamic attribute"]["vis"] != null)
                    {
                        if (element["dynamic attribute"]["vis"].Value == "if not zero")
                            result.DynamicCondition = DynamicCondition.IF_NOT_ZERO;
                        else if (element["dynamic attribute"]["vis"].Value == "if zero")
                            result.DynamicCondition = DynamicCondition.IF_ZERO;
                        else if (element["dynamic attribute"]["vis"].Value == "calc" && element["dynamic attribute"]["calc"].Value == "A=0")
                            result.DynamicCondition = DynamicCondition.IF_ZERO;
                        else if (element["dynamic attribute"]["vis"].Value == "calc" && element["dynamic attribute"]["calc"].Value == "A=1")
                            result.DynamicCondition = DynamicCondition.IF_ONE;
                        else if (element["dynamic attribute"]["vis"].Value == "calc")
                        {
                            result.DynamicCondition = DynamicCondition.CALC;
                            result.ConditionCode = element["dynamic attribute"]["calc"];
                        }
                    }
                    if (element["dynamic attribute"]["chan"] != null)
                        result.DynamicChannelA = element["dynamic attribute"]["chan"];
                }
            }
            return result;
        }

        internal static DisplayControl CreateControl(UIElement config, UIElement element)
        {
            DisplayControl result = knownControls.Select(row => row.UIConfig(config, element))
                .FirstOrDefault(row => row != null);
            return result;
        }

        abstract protected DisplayControl UIConfig(UIElement config, UIElement element);

        public DisplayControl()
        {
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.ClientScript.IsClientScriptBlockRegistered(this.GetType(), this.GetType().Name))
                Page.ClientScript.RegisterClientScriptBlock(this.GetType(),
                    this.GetType().Name,
                    "<script src='/Components/Scripts/" + this.GetType().Name + ".js'></script>", false);
        }

        [DisplayProperty]
        public int X { get; set; }

        [DisplayProperty]
        public int Y { get; set; }

        [DisplayProperty]
        public int Width { get; set; }

        [DisplayProperty]
        public int Height { get; set; }

        protected override void Render(HtmlTextWriter writer)
        {
            writer.Write("<script>controls[controls.length]=");
            RenderProps(writer);
            writer.Write(";</script>");
        }

        private void RenderProps(HtmlTextWriter writer)
        {
            Type t = this.GetType();
            var props = t.GetProperties().Where(row => row.GetCustomAttributes(typeof(DisplayPropertyAttribute), true).Any());
            writer.Write("{type:'" + t.Name + "'");
            foreach (var i in props)
            {
                if (i.PropertyType.IsArray && i.PropertyType.GetElementType().GetInterfaces().Any(row => row == typeof(IJSONSerializable)))
                //if (i.PropertyType.IsArray)
                {
                    IJSONSerializable[] arr = (IJSONSerializable[])i.GetValue(this, null);
                    writer.Write("," + i.Name + ":[");
                    for (int j = 0; j < arr.Length; j++)
                    {
                        if (j != 0)
                            writer.Write(",");
                        arr[j].Serialize(writer);
                    }
                    writer.Write("]");
                }
                else if (i.PropertyType.IsGenericType)
                {
                    List<Point> points = (List<Point>)i.GetValue(this, null);
                    writer.Write("," + i.Name + ":[");
                    for (int j = 0; j < points.Count; j++)
                    {
                        if (j != 0)
                            writer.Write(",");
                        writer.Write("{X:" + points[j].X + ",Y:" + points[j].Y + "}");
                    }
                    writer.Write("]");
                }
                else if (i.PropertyType == typeof(bool))
                    writer.Write("," + i.Name + ":" + i.GetValue(this, null).ToString().ToLower());
                else if (i.PropertyType == typeof(int) || i.PropertyType == typeof(double) || i.PropertyType == typeof(float))
                    writer.Write("," + i.Name + ":" + i.GetValue(this, null));
                else
                    writer.Write("," + i.Name + ":'" + i.GetValue(this, null) + "'");
            }
            if (this.Controls.Count != 0)
            {
                writer.Write(",childs:[");
                for (int i = 0; i < this.Controls.Count; i++)
                {
                    if (i != 0)
                        writer.Write(",");
                    ((DisplayControl)this.Controls[i]).RenderProps(writer);
                }
                writer.Write("]");
            }
            writer.Write("}");
        }

        protected abstract DisplayControl ADLConfig(ADLConfig config, ADLElement element);

        [DisplayProperty]
        public string DynamicChannelA { get; set; }

        [DisplayProperty]
        public DynamicCondition DynamicCondition { get; set; }

        [DisplayProperty]
        public string ConditionCode { get; set; }

    }
}
