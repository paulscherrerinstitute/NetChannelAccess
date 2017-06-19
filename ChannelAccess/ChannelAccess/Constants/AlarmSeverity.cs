/*
 *  EpicsSharp - An EPICS Channel Access library for the .NET platform.
 *
 *  Copyright (C) 2013 - 2017  Paul Scherrer Institute, Switzerland
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
using System.Runtime.Serialization;
using System.Text;

namespace EpicsSharp.ChannelAccess.Constants
{
    /// <summary>
    /// Defines the severity of the current alarm.
    /// </summary>
    [DataContract(Name = "AlarmSeverity")]
    public enum AlarmSeverity : ushort
    {
        /// <summary>
        /// There is no alarm. The value is in the normal range,
        /// i.e. between LowWarnLimit and HighWarnLimit.
        /// </summary>
        [EnumMember]
        NO_ALARM = 0,

        /// <summary>
        /// The alarm is minor. The value is in the warning range, i.e.
        /// either between LowWarnLimit and LowAlarmLimit or between
        /// HighWarnLimit and HighAlarmLimit.
        /// </summary>
        [EnumMember]
        MINOR = 1,
        /// <summary>
        /// The alarm is major. The value is either lower than the
        /// LowAlarmLimit or higher than the HighAlarmLimit.
        /// </summary>
        [EnumMember]
        MAJOR = 2,

        /// <summary>
        /// Invalid severity.
        /// </summary>
        [EnumMember]
        INVALID = 3
    }
}
