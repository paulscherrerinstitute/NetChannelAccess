/*
 *  EpicsSharp - An EPICS Channel Access library for the .NET platform.
 *
 *  Copyright (C) 2013 - 2019  Paul Scherrer Institute, Switzerland
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
    /// Informs about the status of the device behind this Channel
    /// </summary>
    [DataContract(Name = "AlarmStatus")]
    public enum AlarmStatus : ushort
    {
        /// <summary>
        /// Device is working properly correctly
        /// </summary>
        [EnumMember]
        NO_ALARM = 0,
        [EnumMember]
        READ = 1,
        [EnumMember]
        WRITE = 2,
        /// <summary>
        /// Device is malfunctioning, and hit the upper Alarm Limit
        /// </summary>
        [EnumMember]
        HIHI = 3,
        /// <summary>
        /// Device is missbehaving, and hit the upper Warn Limit
        /// </summary>
        [EnumMember]
        HIGH = 4,
        /// <summary>
        /// Device is malfunctioning, and hit the lower Alarm Limit
        /// </summary>
        [EnumMember]
        LOLO = 5,
        /// <summary>
        /// Device is missbehaving, and hit theu lower Warn Limit
        /// </summary>
        [EnumMember]
        LOW = 6,

        [EnumMember]
        STATE = 7,
        [EnumMember]
        COS = 8,
        [EnumMember]
        COMM = 9,
        [EnumMember]
        TIMEOUT = 10,
        [EnumMember]
        HARDWARE_LIMIT = 11,
        [EnumMember]
        CALC = 12,
        [EnumMember]
        SCAN = 13,
        [EnumMember]
        LINK = 14,
        [EnumMember]
        SOFT = 15,
        [EnumMember]
        BAD_SUB = 16,
        /// <summary>
        /// Undefined alarm status
        /// </summary>
        [EnumMember]
        UDF = 17,
        [EnumMember]
        DISABLE = 18,
        [EnumMember]
        SIMM = 19,
        [EnumMember]
        READ_ACCESS = 20,
        [EnumMember]
        WRITE_ACCESS = 21
    }
}
