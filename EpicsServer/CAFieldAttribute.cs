using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CaSharpServer
{
    /// <summary>
    /// Defines the binding between a C# property and an EPICS record field
    /// </summary>
    public class CAFieldAttribute : Attribute
    {
        public CAFieldAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}
