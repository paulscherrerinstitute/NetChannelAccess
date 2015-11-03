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
    static class ControlChannel
    {
        static public DataPacket Encode(EpicsType type, object value, CARecord record, int nbElements = 1)
        {
            int size = 0;
            int startPos = 0;
            switch (type)
            {
                case EpicsType.Control_Double:
                    size += 8 + 8 + 8 * 8 + nbElements * 8;
                    startPos = 8 + 8 + 8 * 8;
                    break;
                case EpicsType.Control_Byte:
                    size += 4 + 8 + 8 + 1 + nbElements;
                    startPos = 4 + 8 + 8 + 1;
                    break;
                case EpicsType.Control_Int:
                    size += 4 + 8 + 8 * 4 + nbElements * 4;
                    startPos = 4 + 8 + 8 * 4;
                    break;
                case EpicsType.Control_Float:
                    size += 8 + 8 + 8 * 4 + nbElements * 4;
                    startPos = 8 + 8 + 8 * 4;
                    break;
                case EpicsType.Control_Short:
                    size += 4 + 8 + 8 * 2 + nbElements * 2;
                    startPos = 4 + 8 + 8 * 2;
                    break;
                case EpicsType.Control_String:
                    startPos = 4;
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

            switch (type)
            {
                case EpicsType.Control_Double:
                    res.SetInt16(16 + 4, (short)((dynamic)record).DisplayPrecision);
                    res.SetDataAsString(((dynamic)record).EngineeringUnits, 8, 8);
                    res.SetDouble(16 + 8 + 8, (double)((dynamic)record).HighDisplayLimit);
                    res.SetDouble(16 + 8 + 8 + 8, (double)((dynamic)record).LowDisplayLimit);
                    res.SetDouble(16 + 8 + 8 + 8 * 2, (double)((dynamic)record).HighHighAlarmLimit);
                    res.SetDouble(16 + 8 + 8 + 8 * 3, (double)((dynamic)record).HighAlarmLimit);
                    res.SetDouble(16 + 8 + 8 + 8 * 4, (double)((dynamic)record).LowAlarmLimit);
                    res.SetDouble(16 + 8 + 8 + 8 * 5, (double)((dynamic)record).LowLowAlarmLimit);
                    res.SetDouble(16 + 8 + 8 + 8 * 6, (double)((dynamic)record).LowOperatingRange);
                    res.SetDouble(16 + 8 + 8 + 8 * 7, (double)((dynamic)record).HighOperatingRange);
                    break;
                case EpicsType.Control_Float:
                    res.SetInt16(16 + 4, (short)((dynamic)record).DisplayPrecision);
                    res.SetDataAsString(((dynamic)record).EngineeringUnits, 8, 8);
                    res.SetFloat(16 + 8 + 8, (float)((dynamic)record).HighDisplayLimit);
                    res.SetFloat(16 + 8 + 8 + 4, (float)((dynamic)record).LowDisplayLimit);
                    res.SetFloat(16 + 8 + 8 + 4 * 2, (float)((dynamic)record).HighHighAlarmLimit);
                    res.SetFloat(16 + 8 + 8 + 4 * 3, (float)((dynamic)record).HighAlarmLimit);
                    res.SetFloat(16 + 8 + 8 + 4 * 4, (float)((dynamic)record).LowAlarmLimit);
                    res.SetFloat(16 + 8 + 8 + 4 * 5, (float)((dynamic)record).LowLowAlarmLimit);
                    res.SetFloat(16 + 8 + 8 + 4 * 6, (float)((dynamic)record).LowOperatingRange);
                    res.SetFloat(16 + 8 + 8 + 4 * 7, (float)((dynamic)record).HighOperatingRange);
                    break;
                case EpicsType.Control_Int:
                    res.SetDataAsString(((dynamic)record).EngineeringUnits, 4, 8);
                    res.SetInt32(16 + 4 + 8, (int)((dynamic)record).HighDisplayLimit);
                    res.SetInt32(16 + 4 + 8 + 4, (int)((dynamic)record).LowDisplayLimit);
                    res.SetInt32(16 + 4 + 8 + 4 * 2, (int)((dynamic)record).HighHighAlarmLimit);
                    res.SetInt32(16 + 4 + 8 + 4 * 3, (int)((dynamic)record).HighAlarmLimit);
                    res.SetInt32(16 + 4 + 8 + 4 * 4, (int)((dynamic)record).LowAlarmLimit);
                    res.SetInt32(16 + 4 + 8 + 4 * 5, (int)((dynamic)record).LowLowAlarmLimit);
                    res.SetInt32(16 + 4 + 8 + 4 * 6, (int)((dynamic)record).LowOperatingRange);
                    res.SetInt32(16 + 4 + 8 + 4 * 7, (int)((dynamic)record).HighOperatingRange);
                    break;
                case EpicsType.Control_Short:
                    res.SetDataAsString(((dynamic)record).EngineeringUnits, 4, 8);
                    res.SetInt16(16 + 4 + 8, (short)((dynamic)record).HighDisplayLimit);
                    res.SetInt16(16 + 4 + 8 + 2, (short)((dynamic)record).LowDisplayLimit);
                    res.SetInt16(16 + 4 + 8 + 2 * 2, (short)((dynamic)record).HighHighAlarmLimit);
                    res.SetInt16(16 + 4 + 8 + 2 * 3, (short)((dynamic)record).HighAlarmLimit);
                    res.SetInt16(16 + 4 + 8 + 2 * 4, (short)((dynamic)record).LowAlarmLimit);
                    res.SetInt16(16 + 4 + 8 + 2 * 5, (short)((dynamic)record).LowLowAlarmLimit);
                    res.SetInt16(16 + 4 + 8 + 2 * 6, (short)((dynamic)record).LowOperatingRange);
                    res.SetInt16(16 + 4 + 8 + 2 * 7, (short)((dynamic)record).HighOperatingRange);
                    break;
                case EpicsType.Control_Byte:
                    res.SetDataAsString(((dynamic)record).EngineeringUnits, 4, 8);
                    res.SetByte(16 + 4 + 8, (byte)((dynamic)record).HighDisplayLimit);
                    res.SetByte(16 + 4 + 8 + 1, (byte)((dynamic)record).LowDisplayLimit);
                    res.SetByte(16 + 4 + 8 + 1 * 2, (byte)((dynamic)record).HighHighAlarmLimit);
                    res.SetByte(16 + 4 + 8 + 1 * 3, (byte)((dynamic)record).HighAlarmLimit);
                    res.SetByte(16 + 4 + 8 + 1 * 4, (byte)((dynamic)record).LowAlarmLimit);
                    res.SetByte(16 + 4 + 8 + 1 * 5, (byte)((dynamic)record).LowLowAlarmLimit);
                    res.SetByte(16 + 4 + 8 + 1 * 6, (byte)((dynamic)record).LowOperatingRange);
                    res.SetByte(16 + 4 + 8 + 1 * 7, (byte)((dynamic)record).HighOperatingRange);
                    break;
                default:
                    break;
            }

            DataPacketBuilder.Encode(res, type, startPos, value, nbElements);
            return res;
        }
    }
}
