using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CaSharpServer.Constants
{
    public enum AlarmSeverity : ushort
    {
        NO_ALARM = 0,
        MINOR = 1,
        MAJOR = 2,
        INVALID = 3
    }
}
