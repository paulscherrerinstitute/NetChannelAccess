using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PSI.EpicsClient2
{
    /// <summary>
    /// Informs about the status of the device behind this Channel
    /// </summary>
    public enum Status : ushort
    {
        /// <summary>
        /// Device is working properly correctly
        /// </summary>
        NO_ALARM = 0,
        READ = 1,
        WRITE = 2,
        /// <summary>
        /// Device is malfunctioning, and hit the upper Alarm Limit
        /// </summary>
        HIHI = 3,
        /// <summary>
        /// Device is missbehaving, and hit the upper Warn Limit
        /// </summary>
        HIGH = 4,
        /// <summary>
        /// Device is malfunctioning, and hit the lower Alarm Limit
        /// </summary>
        LOLO = 5,
        /// <summary>
        /// Device is missbehaving, and hit theu lower Warn Limit
        /// </summary>
        LOW = 6,

        STATE = 7,
        COS = 8,
        COMM = 9,
        TIMEOUT = 10,
        HARDWARE_LIMIT = 11,
        CALC = 12,
        SCAN = 13,
        LINK = 14,
        SOFT = 15,
        BAD_SUB = 16,
        /// <summary>
        /// Undefined Status
        /// </summary>
        UDF = 17,

        DISABLE = 18,
        SIMM = 19,
        READ_ACCESS = 20,
        WRITE_ACCESS = 21
    }
}
