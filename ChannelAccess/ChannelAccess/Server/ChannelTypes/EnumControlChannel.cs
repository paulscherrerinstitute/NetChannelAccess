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
    static class EnumControlChannel
    {
        static public DataPacket Encode(EpicsType type, object value, CARecord record, int nbElements = 1)
        {
            DataPacket res = DataPacket.Create(16 + 424);
            res.DataCount = 1;
            res.DataType = (ushort)type;

            res.SetInt16(16, (short)record.AlarmStatus);
            res.SetInt16(16 + 2, (short)record.CurrentAlarmSeverity);

            string[] names = Enum.GetNames(value.GetType());
            res.SetInt16(16 + 4, (short)names.Length);
            res.SetInt16(16 + 422, Convert.ToInt16(value));

            for (int i = 0; i < names.Length; i++)
                res.SetDataAsString(names[i], 6 + i * 26, 26);

            return res;
        }
    }
}
