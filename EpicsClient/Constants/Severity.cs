using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PSI.EpicsClient2
{
    /// <summary>
    /// Defines the severity of the current alarm
    /// </summary>
    public enum Severity : ushort
    {
        /// <summary>
        /// there is no alarm (value is betweend LowWarnLimit and HighWarnLimit)
        /// </summary>
        NO_ALARM = 0,
        /// <summary>
        /// the alarm is minor (value is between LowWarnLimit and LowAlarmLimit or HighWarnLimit and HighAlarmLimit)
        /// </summary>
        MINOR = 1,
        /// <summary>
        /// the alarm is major. its lower then the LowAlarmLimit or higher den the HighAlarmLimit
        /// </summary>
        MAJOR = 2,
        /// <summary>
        /// Invalid Status
        /// </summary>
        INVALID = 3
    }
}
