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
    static class ExtChannel
    {
        static public DataPacket Encode(EpicsType type, object value, CARecord record, int nbElements = 1)
        {
            int size = 4;
            int startPos = 4;
            switch (type)
            {
                case EpicsType.Status_Double:
                    size += 4 + nbElements * 8;
                    startPos = 8;
                    break;
                case EpicsType.Status_Byte:
                    size += 1 + nbElements;
                    startPos = 5;
                    break;
                case EpicsType.Status_Int:
                case EpicsType.Status_Float:
                    size += nbElements * 4;
                    break;
                case EpicsType.Status_Short:
                    size += nbElements * 2;
                    break;
                case EpicsType.Status_String:
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

            DataPacketBuilder.Encode(res, type, startPos, value, nbElements);
            return res;
        }
    }
}
