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
    static class SimpleChannel
    {
        static public DataPacket Encode(EpicsType type, object value, CARecord record, int nbElements = 1)
        {
            int size = 0;
            switch (type)
            {
                case EpicsType.Double:
                    size += 4 + nbElements * 8;
                    break;
                case EpicsType.Byte:
                    size += nbElements;
                    break;
                case EpicsType.Int:
                case EpicsType.Float:
                    size += nbElements * 4;
                    break;
                case EpicsType.Short:
                    size += nbElements * 2;
                    break;
                case EpicsType.String:
                    size += 40;
                    break;
                default:
                    break;
            }

            DataPacket res = DataPacket.Create(16 + size + DataPacketBuilder.Padding(size));
            res.DataCount = (uint)nbElements;
            res.DataType = (ushort)type;
            DataPacketBuilder.Encode(res, type, 0, value, nbElements);
            return res;
        }
    }
}
