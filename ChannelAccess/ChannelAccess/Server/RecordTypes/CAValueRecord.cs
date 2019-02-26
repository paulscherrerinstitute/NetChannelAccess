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
using System.Text;
using EpicsSharp.ChannelAccess.Constants;
using System.Reflection;

namespace EpicsSharp.ChannelAccess.Server.RecordTypes
{
    /// <summary>
    /// A double record which handles the limits of the value and set the alarm accordingly.
    /// </summary>
    public abstract class CAValueRecord<TType> : CARecord<TType> where TType : IComparable<TType>
    {
        /// <summary>
        /// Defines the value on which the High High alarm will be triggered.
        /// </summary>
        [CAField("HIHI")]
        public TType HighHighAlarmLimit { get; set; }

        /// <summary>
        /// Defines the value on which the High alarm will be triggered.
        /// </summary>
        [CAField("HIGH")]
        public TType HighAlarmLimit { get; set; }

        /// <summary>
        /// Defines the value on which the Low Low alarm will be triggered.
        /// </summary>
        [CAField("LOLO")]
        public TType LowLowAlarmLimit { get; set; }

        /// <summary>
        /// Defines the value on which the Low alarm will be triggered.
        /// </summary>
        [CAField("LOW")]
        public TType LowAlarmLimit { get; set; }

        /// <summary>
        /// Defines the value of the severity for a low low alarm.
        /// </summary>
        [CAField("LLSV")]
        public AlarmSeverity LowLowAlarmSeverity { get; set; }

        /// <summary>
        /// Defines the value of the severity for an high low alarm.
        /// </summary>
        [CAField("HSV")]
        public AlarmSeverity HighAlarmSeverity { get; set; }

        /// <summary>
        /// Defines the value of the severity for a low alarm.
        /// </summary>
        [CAField("LSV")]
        public AlarmSeverity LowAlarmSeverity { get; set; }

        /// <summary>
        /// Defines the value of the severity for an High High alarm.
        /// </summary>
        [CAField("HHSV")]
        public AlarmSeverity HighHighAlarmSeverity { get; set; }

        string engineeringUnits = "";
        /// <summary>
        /// Defines the value of the Engineering Units.
        /// </summary>
        [CAField("EGU")]
        public string EngineeringUnits
        {
            get
            {
                return engineeringUnits;
            }
            set
            {
                if (value.Length > 8)
                    throw new Exception("Cannot have more than 8 characters for the engineering unit.");
                engineeringUnits = value;
            }
        }

        /// <summary>
        /// Defines the Display Precision.
        /// </summary>
        [CAField("PREC")]
        public short DisplayPrecision { get; set; }

        /// <summary>
        /// Defines the high display limit.
        /// </summary>
        [CAField("HIGHDISP")]
        public TType HighDisplayLimit { get; set; }

        /// <summary>
        /// Defines the low display limit.
        /// </summary>
        [CAField("LOWDISP")]
        public TType LowDisplayLimit { get; set; }

        /// <summary>
        /// Defines the High Operating Range.
        /// </summary>
        [CAField("HOPR")]
        public TType HighOperatingRange { get; set; }

        /// <summary>
        /// Defines the Low Operating Range.
        /// </summary>
        [CAField("LOPR")]
        public TType LowOperatingRange { get; set; }


        /// <summary>
        /// Initialize the record with default alarm limits which are the max and min double values.
        /// </summary>
        internal CAValueRecord()
        {
            Dictionary<string, TType> constants = typeof(TType).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).Where(row => row.IsLiteral && !row.IsInitOnly && row.FieldType == typeof(TType)).ToDictionary(key => key.Name, val => (TType)val.GetValue(null));

            LowLowAlarmLimit = constants["MinValue"];
            LowAlarmLimit = constants["MinValue"];
            HighAlarmLimit = constants["MaxValue"];
            HighHighAlarmLimit = constants["MaxValue"];

            LowDisplayLimit = constants["MinValue"];
            HighDisplayLimit = constants["MaxValue"];

            LowLowAlarmSeverity = AlarmSeverity.MAJOR;
            HighHighAlarmSeverity = AlarmSeverity.MAJOR;
            LowAlarmSeverity = AlarmSeverity.MINOR;
            HighAlarmSeverity = AlarmSeverity.MINOR;

            EngineeringUnits = "";
            DisplayPrecision = 0;
        }

        internal override void ProcessRecord()
        {
            if (Value.CompareTo(LowLowAlarmLimit) <= 0)
                TriggerAlarm(LowLowAlarmSeverity, AlarmStatus.LOLO);
            else if (Value.CompareTo(LowAlarmLimit) <= 0)
                TriggerAlarm(LowAlarmSeverity, AlarmStatus.LOW);
            else if (Value.CompareTo(HighHighAlarmLimit) >= 0)
                TriggerAlarm(HighHighAlarmSeverity, AlarmStatus.HIHI);
            else if (Value.CompareTo(HighAlarmLimit) >= 0)
                TriggerAlarm(HighAlarmSeverity, AlarmStatus.HIGH);
            else
                TriggerAlarm(AlarmSeverity.NO_ALARM, AlarmStatus.NO_ALARM);

            base.ProcessRecord();
        }

        internal override int ElementsInRecord => 1;

    }
}
