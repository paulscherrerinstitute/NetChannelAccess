using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CaSharpServer.Constants
{
    public enum AlarmStatus : ushort
    {
        NO_ALARM = 0,
        READ,
        WRITE,
        HIHI,
        HIGH,
        LOLO,
        LOW,
        STATE,
        COS,
        COMM,
        TIMEOUT,
        HWLIMIT,
        CALC,
        SCAN,
        LINK,
        SOFT,
        BAD_SUB,
        UDF,
        DISABLE,
        SIMM,
        READ_ACCESS,
        WRITE_ACCESS,
    }
}
