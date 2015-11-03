using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CaSharpServer.Constants
{
    /// <summary>
    /// Monitor Mask allows to define what a Monitor shall monitor
    /// </summary>
    public enum MonitorMask : ushort
    {
        /// <summary>
        /// Value type
        /// </summary>
        VALUE = 0x01,
        /// <summary>
        /// Log type
        /// </summary>
        LOG = 0x02,
        /// <summary>
        /// Value and log together
        /// </summary>
        VALUE_LOG = MonitorMask.VALUE | MonitorMask.LOG,
        /// <summary>
        /// Alarm status type
        /// </summary>
        ALARM = 0x04,
        /// <summary>
        /// Value and alarm together
        /// </summary>
        VALUE_ALARM = MonitorMask.VALUE | MonitorMask.ALARM,
        /// <summary>
        /// Log and alarm together
        /// </summary>
        LOG_ALARM = MonitorMask.LOG | MonitorMask.ALARM,
        /// <summary>
        /// All three (value, log and alarm) together
        /// </summary>
        ALL = MonitorMask.VALUE | MonitorMask.LOG | MonitorMask.ALARM
    }
}
