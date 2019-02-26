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
using EpicsSharp.ChannelAccess.Common;
using EpicsSharp.ChannelAccess.Constants;
using EpicsSharp.ChannelAccess.Server.RecordTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpicsSharp.ChannelAccess.Server.ChannelTypes
{
    static class TimeChannel
    {
        static public DataPacket Encode(EpicsType type, object value, CARecord record, int nbElements = 1)
        {
            int size = 12;
            int startPos = 0;
            switch (type)
            {
                case EpicsType.Time_Double:
                    size += 4 + nbElements * 8;
                    startPos = 4;
                    break;
                case EpicsType.Time_Byte:
                    size += 3 + nbElements;
                    startPos = 3;
                    break;
                case EpicsType.Time_Int:
                case EpicsType.Time_Float:
                    size += nbElements * 4;
                    startPos = 0;
                    break;
                case EpicsType.Time_Short:
                    size += 2 + nbElements * 2;
                    startPos = 2;
                    break;
                case EpicsType.Time_String:
                    startPos = 0;
                    size += 40;
                    break;
                default:
                    break;
            }
            size += DataPacketBuilder.Padding(size);

            DataPacket res = DataPacket.Create(16 + size);
            res.DataCount = (uint)nbElements;
            res.DataType = (ushort)type;

            res.SetInt16(16, (short)record.AlarmStatus);
            res.SetInt16(16 + 2, (short)record.CurrentAlarmSeverity);
            res.SetDateTime(16 + 4, DateTime.Now);

            DataPacketBuilder.Encode(res, type, 12 + startPos, value, nbElements);
            return res;
        }
    }
}
