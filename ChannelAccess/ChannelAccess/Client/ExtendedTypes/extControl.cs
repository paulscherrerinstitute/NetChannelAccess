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
using EpicsSharp.ChannelAccess.Constants;
using System;
using System.Collections.Generic;
using System.Text;

namespace EpicsSharp.ChannelAccess.Client
{
    /// <summary>
    /// extended epics control type <br/> serves severity, status, value, precision (for double and float), unittype 
    /// and a bunch of limits. 
    /// </summary>
    /// <typeparam name="TType">generic datatype for value</typeparam>
    public class ExtControl<TType> : ExtGraphic<TType>
    {
        internal ExtControl()
        {
        }

        /// <summary>
        /// Lowest Value which can be set
        /// </summary>
        public double LowControlLimit { get; internal set; }
        /// <summary>
        /// Highest Value which can be set
        /// </summary>
        public double HighControlLimit { get; internal set; }

        internal override void Decode(Channel channel, uint nbElements)
        {
            Status = (AlarmStatus)channel.DecodeData<ushort>(1, 0);
            Severity = (AlarmSeverity)channel.DecodeData<ushort>(1, 2);
            int pos = 4;
            Type t = typeof(TType);
            if (t.IsArray)
                t = t.GetElementType();
            if (t == typeof(object))
                t = channel.ChannelDefinedType;
            if (t == typeof(double) || t == typeof(float))
            {
                Precision = channel.DecodeData<short>(1, pos);
                pos += 4; // 2 for precision field + 2 padding for "RISC alignment"
            }
            if (t != typeof(string))
            {
                EGU = channel.DecodeData<string>(1, pos, 8);
                pos += 8;
                int tSize = TypeHandling.EpicsSize(t);

                //HighDisplayLimit = channel.DecodeData<TType>(1, pos);
                HighDisplayLimit = Convert.ToDouble(channel.DecodeData(t, 1, pos));
                pos += tSize;
                //LowDisplayLimit = channel.DecodeData<TType>(1, pos);
                LowDisplayLimit = Convert.ToDouble(channel.DecodeData(t, 1, pos));
                pos += tSize;
                //HighAlertLimit = channel.DecodeData<TType>(1, pos);
                HighAlertLimit = Convert.ToDouble(channel.DecodeData(t, 1, pos));
                pos += tSize;
                //HighWarnLimit = channel.DecodeData<TType>(1, pos);
                HighWarnLimit = Convert.ToDouble(channel.DecodeData(t, 1, pos));
                pos += tSize;
                //LowWarnLimit = channel.DecodeData<TType>(1, pos);
                LowWarnLimit = Convert.ToDouble(channel.DecodeData(t, 1, pos));
                pos += tSize;
                //LowAlertLimit = channel.DecodeData<TType>(1, pos);
                LowAlertLimit = Convert.ToDouble(channel.DecodeData(t, 1, pos));
                pos += tSize;
                LowControlLimit = Convert.ToDouble(channel.DecodeData(t, 1, pos));
                pos += tSize;
                //HighControlLimit = channel.DecodeData<TType>(1, pos);
                HighControlLimit = Convert.ToDouble(channel.DecodeData(t, 1, pos));
                pos += tSize;
            }
            else
            {
                EGU = "";
            }
            if (t == typeof(byte))
                pos++; // 1 padding for "RISC alignment"

            Value = channel.DecodeData<TType>(nbElements, pos);
        }

        /// <summary>
        /// builds a string line of all properties
        /// </summary>
        /// <returns>comma seperated string of keys and values</returns>
        public override string ToString()
        {
            return String.Format("Value:{0},Status:{1},Severity:{2},EGU:{3},Precision:{4}," +
                                 "LowDisplayLimit:{5},LowAlertLimit:{6},LowWarnLimit:{7}," +
                                 "HighWarnLimit:{8},HighAlertLimit:{9},HighDisplayLimit:{10}," +
                                 "LowControlLimit:{11},HighControlLimit:{12}",
                                 Value, Status, Severity, EGU, Precision, LowDisplayLimit, LowAlertLimit, LowWarnLimit,
                                 HighWarnLimit, HighAlertLimit, HighDisplayLimit, LowControlLimit, HighControlLimit);
        }
    }
}
