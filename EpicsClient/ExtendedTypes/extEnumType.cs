using System;
using System.Collections.Generic;
using System.Text;

namespace PSI.EpicsClient2
{
    internal class ExtEnumType : ExtType<short>
    {
        internal ExtEnumType()
        {
        }

        public string[] EnumArray { get; internal set; }
    }
}
