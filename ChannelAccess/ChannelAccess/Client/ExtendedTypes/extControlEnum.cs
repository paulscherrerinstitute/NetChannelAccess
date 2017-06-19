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
using EpicsSharp.ChannelAccess.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpicsSharp.ChannelAccess.Client
{
    public class ExtControlEnum : ExtType<int>
    {
        internal ExtControlEnum()
        {
        }

        internal override void Decode(Channel channel, uint nbElements)
        {
            Status = (AlarmStatus)channel.DecodeData<ushort>(1, 0);
            Severity = (AlarmSeverity)channel.DecodeData<ushort>(1, 2);
            NbStates = channel.DecodeData<ushort>(1, 4);
            States = new string[NbStates];
            Value = channel.DecodeData<ushort>(1, 422);
            for(int i=0;i < NbStates;i++)
            {
                States[i] = channel.DecodeData<string>(1, 6 + i * 26, 26);
            }
        }

        public ushort NbStates { get; set; }

        public string[] States { get; set; }
    }
}
