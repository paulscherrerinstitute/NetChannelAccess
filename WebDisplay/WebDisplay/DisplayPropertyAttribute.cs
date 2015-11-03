using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebDisplay
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DisplayPropertyAttribute : Attribute
    {
    }
}