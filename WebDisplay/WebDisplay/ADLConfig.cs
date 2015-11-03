using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebDisplay
{
    public class ADLConfig
    {
        string currentConfig = "";
        int position = 0;

        public List<ADLElement> elements = new List<ADLElement>();

        public bool HasChar
        {
            get
            {
                return (position < currentConfig.Length - 1);
            }
        }

        public char Next()
        {
            return currentConfig[position++];
        }

        public char Peek()
        {
            return currentConfig[position];
        }


        public string NextString()
        {
            string result = "";
            if (Peek() == '\"')
            {
                Next();
                while (HasChar && Peek() != '\"')
                {
                    result += Next();
                }
                Next();
            }
            else
            {
                while (HasChar && ((Peek() >= 'a' && Peek() <= 'z') || (Peek() >= 'A' && Peek() <= 'Z') || (Peek() >= '0' && Peek() <= '9') || Peek() == '-' || Peek() == '[' || Peek() == ']' || Peek() == '_'))
                {
                    result += Next();
                }
            }
            return result;
        }

        public void SkipEmpty()
        {
            while (HasChar && (Peek() == ' ' || Peek() == '\n' || Peek() == '\r' || Peek() == '\t'))
            {
                Next();
            }
        }

        public static ADLConfig Parse(string config)
        {
            ADLConfig result = new ADLConfig { currentConfig = config, position = 0 };

            while (result.HasChar)
            {
                ADLElement elem = new ADLElement(result);
                result.elements.Add(elem);
            }

            return result;
        }

        public ADLElement this[string key]
        {
            get
            {
                return elements.FirstOrDefault(row => row.Name == key);
            }
        }
    }
}