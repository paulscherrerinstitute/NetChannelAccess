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
    static class DataPacketBuilder
    {
        static public DataPacket Encode(EpicsType type, object source, CARecord record, int nbElements=1)
        {
            switch (type)
            {
                case EpicsType.Int:
                case EpicsType.Short:
                case EpicsType.Float:
                case EpicsType.Double:
                case EpicsType.String:
                    return SimpleChannel.Encode(type, source, record, nbElements);
                case EpicsType.Status_Int:
                case EpicsType.Status_Short:
                case EpicsType.Status_Float:
                case EpicsType.Status_Double:
                case EpicsType.Status_String:
                    return ExtChannel.Encode(type, source, record, nbElements);
                case EpicsType.Time_Int:
                case EpicsType.Time_Short:
                case EpicsType.Time_Float:
                case EpicsType.Time_Double:
                case EpicsType.Time_String:
                    return TimeChannel.Encode(type, source, record, nbElements);
                case EpicsType.Control_Int:
                case EpicsType.Control_Short:
                case EpicsType.Control_Float:
                case EpicsType.Control_Double:
                case EpicsType.Control_String:
                    return ControlChannel.Encode(type, source, record, nbElements);
                case EpicsType.Display_Int:
                case EpicsType.Display_Short:
                case EpicsType.Display_Float:
                case EpicsType.Display_Double:
                case EpicsType.Display_String:
                    return DisplayChannel.Encode(type, source, record, nbElements);
                case EpicsType.Labeled_Enum:
                    return EnumControlChannel.Encode(type, source, record, nbElements);
                default:
                    throw new Exception("Not yet supported");
            }
        }

        public static int Padding(int size)
        {
            return (8 - (size % 8));
        }

        public static void Encode(DataPacket result, EpicsType type, int offset, object value, int nbElements = 1)        
        {
            Type sourceType=value.GetType();

            switch (type)
            {
                case EpicsType.Control_Byte:
                case EpicsType.Display_Byte:
                case EpicsType.Status_Byte:
                case EpicsType.Time_Byte:
                case EpicsType.Byte:
                    {
                        if (nbElements == 1 && !sourceType.IsArray && !sourceType.IsGenericType)
                            result.SetInt32(16 + offset, Convert.ToByte(value));
                        else
                        {
                            dynamic t = value;
                            for (var i = 0; i < nbElements; i++)
                                result.SetInt32(16 + offset + i, Convert.ToByte(t[i]));
                        }
                    }
                    break;
                case EpicsType.Control_Int:
                case EpicsType.Display_Int:
                case EpicsType.Int:
                case EpicsType.Time_Int:
                case EpicsType.Status_Int:
                    {
                        if (nbElements == 1 && !sourceType.IsArray && !sourceType.IsGenericType)
                            result.SetInt32(16 + offset, Convert.ToInt32(value));
                        else
                        {
                            dynamic t = value;
                            for (var i = 0; i < nbElements; i++)
                                result.SetInt32(16 + offset + i * 4, Convert.ToInt32(t[i]));
                        }
                    }
                    break;
                case EpicsType.Float:
                case EpicsType.Time_Float:
                case EpicsType.Status_Float:
                case EpicsType.Control_Float:
                case EpicsType.Display_Float:
                    {
                        if (nbElements == 1 && !sourceType.IsArray && !sourceType.IsGenericType)
                            result.SetFloat(16 + offset, Convert.ToSingle(value));
                        else
                        {
                            dynamic t = value;
                            for (var i = 0; i < nbElements; i++)
                                result.SetFloat(16 + offset + i * 4, Convert.ToSingle(t[i]));
                        }
                    }
                    break;
                case EpicsType.Double:
                case EpicsType.Time_Double:
                case EpicsType.Status_Double:
                case EpicsType.Control_Double:
                case EpicsType.Display_Double:
                    {
                        if (nbElements == 1 && !sourceType.IsArray && !sourceType.IsGenericType)
                            result.SetDouble(16 + offset, Convert.ToDouble(value));
                        else
                        {
                            dynamic t = value;
                            for (var i = 0; i < nbElements; i++)
                                result.SetFloat(16 + offset + i * 8, Convert.ToDouble(t[i]));
                        }
                    }
                    break;
                case EpicsType.Short:
                case EpicsType.Time_Short:
                case EpicsType.Status_Short:
                case EpicsType.Control_Short:
                case EpicsType.Display_Short:
                    {
                        if (nbElements == 1 && !sourceType.IsArray && !sourceType.IsGenericType)
                            result.SetInt16(16 + offset, Convert.ToInt16(value));
                        else
                        {
                            dynamic t = value;
                            for (var i = 0; i < nbElements; i++)
                                result.SetFloat(16 + offset + i * 2, Convert.ToInt16(t[i]));
                        }
                    }
                    break;
                case EpicsType.String:
                case EpicsType.Time_String:
                case EpicsType.Status_String:
                case EpicsType.Control_String:
                case EpicsType.Display_String:
                    result.SetDataAsString(Convert.ToString(value), offset, 40);
                    break;
                default:
                    throw new Exception("Unsuported type");
            }
        }
    }
}
