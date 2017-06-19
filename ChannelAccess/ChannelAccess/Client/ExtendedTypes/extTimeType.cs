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
using System.Text;
using System.Runtime.InteropServices;
using EpicsSharp.ChannelAccess.Constants;

namespace EpicsSharp.ChannelAccess.Client
{
    /// <summary>
    /// extended time epics type <br/> serves severity,status,value and time of last change.
    /// </summary>
    /// <typeparam name="TType">generic datatype for value</typeparam>
    public class ExtTimeType<TType> : ExtType<TType>
    {
        internal ExtTimeType()
        {
        }

        /// <summary>
        /// Time of the last change on channel as local datetime
        /// </summary>
        public DateTime Time { get; private set; }

        internal override void Decode(Channel channel, uint nbElements)
        {
            Status = (AlarmStatus)channel.DecodeData<ushort>(1, 0);
            Severity = (AlarmSeverity)channel.DecodeData<ushort>(1, 2);
            Time = channel.DecodeData<DateTime>(1, 4);
            int pos = 12;
            Type t = typeof(TType);
            if (t.IsArray)
                t = t.GetElementType();
            if (t == typeof(object))
                t = channel.ChannelDefinedType;
            // padding for "RISC alignment"
            if (t == typeof(byte))
                pos += 3;
            else if (t == typeof(double))
                pos += 4;
            else if (t == typeof(short))
                pos += 2;
            Value = channel.DecodeData<TType>(nbElements, pos);
        }

        /// <summary>
        /// builds a string line of all properties
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("Value:{0},Status:{1},Severity:{2},Time:{3}", Value, Status, Severity, Time.ToString());
        }
    }
}
