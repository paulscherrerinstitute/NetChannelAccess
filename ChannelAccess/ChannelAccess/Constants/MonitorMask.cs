/*
 *  EpicsSharp - An EPICS Channel Access library for the .NET platform.
 *
 *  Copyright (C) 2013 - 2015  Paul Scherrer Institute, Switzerland
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EpicsSharp.ChannelAccess.Constants
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
