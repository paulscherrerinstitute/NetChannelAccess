using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace WebDisplay
{
    public class ADLElement : IEnumerable<ADLElement>
    {
        private ADLConfig config;

        public ADLElement Parent;

        public ADLElement()
        {
        }

        public ADLElement(ADLConfig config)
        {
            this.config = config;
            IsEmpty = false;
            Parse();
        }
        public ADLElement(ADLConfig config, ADLElement parent)
        {
            this.config = config;
            this.Parent = parent;
            IsEmpty = false;
            Parse();
        }


        int ParentCount
        {
            get
            {
                int result = 0;
                var p = Parent;
                while (p != null)
                {
                    result++;
                    p = p.Parent;
                }
                return result;
            }
        }

        internal void Parse()
        {
            if(ParentCount > 30)
            {
                return;
            }
            config.SkipEmpty();
            if (config.Peek() == '(')
            {
                config.Next();
                Name = config.NextString();
                config.Next();
                Value = config.NextString();
                config.Next();
                return;
            }

            Name = config.NextString();
            config.SkipEmpty();
            if (config.Peek() == '=')
            {
                config.Next();
                config.SkipEmpty();
                Value = config.NextString();
            }
            else if (config.Peek() == ',')
            {
                config.Next();
                Childs.Add(new ADLElement { Name = this.Name, Parent = this });
                Name = null;

                while (config.HasChar)
                {
                    config.SkipEmpty();
                    Childs.Add(new ADLElement { Value = config.NextString(), Parent = this });
                    config.SkipEmpty();
                    if (config.Peek() == ',')
                    {
                        config.Next();
                        config.SkipEmpty();
                    }
                    if (config.Peek() == '}')
                        break;
                }
                //config.Next();
            }
            else if (config.Peek() == '{')
            {
                config.Next();
                while (config.HasChar)
                {
                    config.SkipEmpty();
                    Childs.Add(new ADLElement(config, this));
                    config.SkipEmpty();
                    if (config.Peek() == '}')
                        break;
                }
                config.Next();
                if (Childs.Count() == 1 && Childs[0].Name == null)
                {
                    var c = Childs[0];
                    Childs.Clear();
                    Childs.AddRange(c.Childs);
                }
            }
        }

        public ADLElement this[string key]
        {
            get
            {
                return Childs.FirstOrDefault(row => row.Name == key);
            }
        }


        public bool IsEmpty { get; set; }

        public string Name;
        public string Value;
        public List<ADLElement> Childs = new List<ADLElement>();

        public static implicit operator string(ADLElement elem)        
        {
            if (elem == null)
                return null;
            return elem.Value;
        }

        public static implicit operator int(ADLElement elem)
        {
            return int.Parse(elem.Value);
        }

        public static implicit operator double(ADLElement elem)
        {
            return double.Parse(elem.Value);
        }

        public static explicit operator Point(ADLElement elem)
        {
            return new Point(int.Parse(elem.Name), int.Parse(elem.Value));
        }

        public IEnumerator<ADLElement> GetEnumerator()
        {
            return Childs.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Childs.GetEnumerator();
        }
    }
}