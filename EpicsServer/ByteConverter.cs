using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CaSharpServer.Constants;
using System.Globalization;

namespace CaSharpServer
{
    /// <summary>
    /// Extension class which contains the converters from byte array or to byte array.
    /// </summary>
    static class ByteConverter
    {
        private static DateTime TimestampBase = new DateTime(1990, 1, 1, 0, 0, 0);

        /// <summary>
        /// Converts a byte array to a string
        /// </summary>
        /// <param name="bytes">An array of bytes.</param>
        /// <param name="startPos">The starting position within value.</param>
        /// <param name="len">The number of bytes to decode.</param>
        /// <returns>The converted string</returns>
        public static String ToString(this byte[] bytes, int startPos, int len)
        {
            char[] chars = new char[len];

            Encoding.ASCII.GetChars(bytes, startPos, len, chars, 0);
            String ret = new String(chars);
            int indexOf = ret.IndexOf('\0');
            if (indexOf != -1)
                ret = ret.Substring(0, indexOf);
            return ret;
        }
        /// <summary>
        /// Converts a byte array (40 bytes) to a string (max. 39 chars, because of '/0')
        /// </summary>
        /// <param name="bytes">An array of bytes.</param>
        /// <param name="startPos">The starting position within value.</param>
        /// <returns>The converted string</returns>
        public static String ToString(this byte[] bytes, int startPos)
        {
            if ((bytes.Length - startPos) < 40)
                return ToString(bytes, startPos, (bytes.Length - startPos));
            else
                return ToString(bytes, startPos, 40);
        }
        /// <summary>
        /// Convert a byte array to a string
        /// </summary>
        /// <param name="data">byte array</param>
        /// <returns>the converted string</returns>
        public static String ToString(byte[] data)
        {
            return ToString(data, 0, data.Length);
        }

        /// <summary>
        /// Converts a string to a byte array
        /// </summary>
        /// <param name="str">The string to convert to a byte array</param>
        /// <returns>The converted string as byte array</returns>
        public static byte[] ToByteArray(this string str)
        {
            return Encoding.ASCII.GetBytes(str);
        }

        public static byte[] ToByteArray(this string str, int length)
        {
            byte[] result = new byte[length];
            byte[] src = Encoding.ASCII.GetBytes(str);
            Buffer.BlockCopy(src, 0, result, 0, src.Length);
            return result;
        }

        public static byte[] ToByteArray(this string str, bool forceLength)
        {
            byte[] result = new byte[40];
            byte[] src = Encoding.ASCII.GetBytes(str);
            Buffer.BlockCopy(src, 0, result, 0, Math.Min(src.Length, 40));
            return result;
        }
        /// <summary>
        /// Converts a Byte to network byte order
        /// </summary>
        /// <param name="var">The Byte to convert to a byte array</param>
        /// <returns>The converted byte as byte array</returns>
        public static byte[] ToByteArray(this byte var)
        {
            byte[] ret = { var };
            return ret;
        }
        /// <summary>
        /// Converts a Int16 to network byte order
        /// </summary>
        /// <param name="var">The Int16 to convert to a byte array</param>
        /// <returns>The converted Int16 as byte array</returns>
        public static byte[] ToByteArray(this Int16 var)
        {
            byte[] ret = System.BitConverter.GetBytes(var);
            Array.Reverse(ret);
            return ret;
        }
        /// <summary>
        /// Converts a Int32 to network byte order
        /// </summary>
        /// <param name="var">The Int32 to convert to a byte array</param>
        /// <returns>The converted Int32 as byte array</returns>
        public static byte[] ToByteArray(this Int32 var)
        {
            byte[] ret = System.BitConverter.GetBytes(var);
            Array.Reverse(ret);
            return ret;
        }
        /// <summary>
        /// Converts a UInt16 to network byte order
        /// </summary>
        /// <param name="var">The UInt16 to convert to a byte array</param>
        /// <returns>The converted UInt16 as byte array</returns>
        public static byte[] ToByteArray(this UInt16 var)
        {
            byte[] ret = System.BitConverter.GetBytes(var);
            Array.Reverse(ret);
            return ret;
        }
        /// <summary>
        /// Converts a UInt32 to network byte order
        /// </summary>
        /// <param name="var">The UInt32 to convert to a byte array</param>
        /// <returns>The converted UInt32 as byte array</returns>
        public static byte[] ToByteArray(this UInt32 var)
        {
            byte[] ret = System.BitConverter.GetBytes(var);
            Array.Reverse(ret);
            return ret;
        }
        /// <summary>
        /// Converts a Double to network byte order
        /// </summary>
        /// <param name="var">The double to convert to a byte array</param>
        /// <returns>The converted Double as byte array</returns>
        public static byte[] ToByteArray(this Double var)
        {
            byte[] ret = System.BitConverter.GetBytes(var);
            Array.Reverse(ret);
            return ret;
        }
        /// <summary>
        /// Converts a float to network byte order
        /// </summary>
        /// <param name="var">The float to convert to a byte array</param>
        /// <returns>The converted float as byte array</returns>
        public static byte[] ToByteArray(this float var)
        {
            byte[] ret = System.BitConverter.GetBytes(var);
            Array.Reverse(ret);
            return ret;
        }
        public static byte[] ToByteArray(this DateTime time)
        {
            if (time == null)
                return new byte[0];

            byte[] dateTimeBytes = new byte[8];
            //long Diff = time.Ticks - TimestampBase.ToUniversalTime().Ticks;
            long Diff = time.Ticks - TimestampBase.ToLocalTime().Ticks;

            UInt32 secs = (UInt32)Math.Round((double)(Diff / 10000000));
            UInt32 nanosecs = (UInt32)(Diff - (secs * 10000000)) * 100;

            Buffer.BlockCopy(ToByteArray(secs), 0, dateTimeBytes, 0, 4);
            Buffer.BlockCopy(ToByteArray(nanosecs), 0, dateTimeBytes, 4, 4);

            return dateTimeBytes;
        }

        /// <summary>
        /// Converts a byte array to a string
        /// </summary>
        /// <param name="bytes">An array of bytes.</param>
        /// <param name="startPos">The starting position within value.</param>
        /// <param name="len">The number of bytes to decode.</param>
        /// <returns>The converted string</returns>
        public static String ToCAString(this byte[] bytes, int startPos, int len)
        {
            String ret = Encoding.ASCII.GetString(bytes, startPos, len);
            int indexOf = ret.IndexOf('\0');
            if (indexOf != -1)
                ret = ret.Substring(0, indexOf);
            return ret;
        }
        /// <summary>
        /// Converts a byte array (40 bytes) to a string (max. 39 chars, because of '/0')
        /// </summary>
        /// <param name="bytes">An array of bytes.</param>
        /// <param name="startPos">The starting position within value.</param>
        /// <returns>The converted string</returns>
        public static String ToCAString(this byte[] bytes, int startPos)
        {
            if ((bytes.Length - startPos) < 40)
                return ToCAString(bytes, startPos, (bytes.Length - startPos));
            else
                return ToCAString(bytes, startPos, 40);
        }
        /// <summary>
        /// Convert a byte array to a string
        /// </summary>
        /// <param name="data">byte array</param>
        /// <returns>the converted string</returns>
        public static String ToCAString(this byte[] data)
        {
            return ToCAString(data, 0, data.Length);
        }
        /// <summary>
        /// Converts a byte array to a UInt16
        /// </summary>
        /// <param name="bytes">An array of bytes.</param>
        /// <param name="startPos">The starting position within value.</param>
        /// <returns>The converted UInt16</returns>
        public static UInt16 ToUInt16(this byte[] bytes, Int32 startPos)
        {
            byte[] ushortBytes = new byte[2];
            Buffer.BlockCopy(bytes, startPos, ushortBytes, 0, 2);

            Array.Reverse(ushortBytes);

            return BitConverter.ToUInt16(ushortBytes, 0);
        }
        public static UInt16 ToUInt16(this byte[] bytes)
        {
            return ToUInt16(bytes, 0);
        }
        /// <summary>
        /// Converts a byte array to an Int16
        /// </summary>
        /// <param name="bytes">An array of bytes.</param>
        /// <param name="startPos">The starting position within value.</param>
        /// <returns>The converted Int16</returns>
        public static Int16 ToInt16(this byte[] bytes, Int32 startPos)
        {
            byte[] ushortBytes = new byte[2];
            Buffer.BlockCopy(bytes, startPos, ushortBytes, 0, 2);

            Array.Reverse(ushortBytes);

            return BitConverter.ToInt16(ushortBytes, 0);
        }
        public static Int16 ToInt16(this byte[] bytes)
        {
            return ToInt16(bytes, 0);
        }
        /// <summary>
        /// Converts a byte array to a UInt32
        /// </summary>
        /// <param name="bytes">An array of bytes.</param>
        /// <param name="startPos">The starting position within value.</param>
        /// <returns>The converted UInt32</returns>
        public static UInt32 ToUInt32(this byte[] bytes, Int32 startPos)
        {
            byte[] uintBytes = new byte[4];
            Buffer.BlockCopy(bytes, startPos, uintBytes, 0, 4);

            Array.Reverse(uintBytes);

            return BitConverter.ToUInt32(uintBytes, 0);
        }
        public static UInt32 ToUInt32(this byte[] bytes)
        {
            return ToUInt32(bytes, 0);
        }
        public static Int64 ToInt64(this byte[] bytes, Int32 startPos)
        {
            byte[] intBytes = new byte[8];
            Buffer.BlockCopy(bytes, startPos, intBytes, 0, 8);

            Array.Reverse(intBytes);

            return BitConverter.ToInt64(intBytes, 0);
        }
        public static Int64 ToInt64(this byte[] bytes)
        {
            return ToInt64(bytes, 0);
        }
        /// <summary>
        /// Converts a byte array to an Int32
        /// </summary>
        /// <param name="bytes">An array of bytes.</param>
        /// <param name="startPos">The starting position within value.</param>
        /// <returns>The converted Int32</returns>
        public static Int32 ToInt32(this byte[] bytes, Int32 startPos)
        {
            byte[] uintBytes = new byte[4];
            Buffer.BlockCopy(bytes, startPos, uintBytes, 0, 4);

            Array.Reverse(uintBytes);

            return BitConverter.ToInt32(uintBytes, 0);
        }
        public static Int32 ToInt32(this byte[] bytes)
        {
            return ToInt32(bytes, 0);
        }
        /// <summary>
        /// Converts a byte array to a Byte (1 Byte)
        /// </summary>
        /// <param name="bytes">An array of bytes.</param>
        /// <param name="startPos">The starting position within value.</param>
        /// <returns>The converted Byte</returns>
        public static byte ToByte(this byte[] bytes, Int32 startPos)
        {
            return bytes[startPos];
        }
        /// <summary>
        /// Converts a byte array to a Double (8 Bytes)
        /// </summary>
        /// <param name="bytes">An array of bytes.</param>
        /// <param name="startPos">The starting position within value.</param>
        /// <returns>The converted Double</returns>
        public static double ToDouble(this byte[] bytes, Int32 startPos)
        {
            byte[] doubleBytes = new byte[8];
            Buffer.BlockCopy(bytes, startPos, doubleBytes, 0, 8);

            Array.Reverse(doubleBytes);

            return BitConverter.ToDouble(doubleBytes, 0);
        }
        public static double ToDouble(this byte[] bytes)
        {
            return ToDouble(bytes, 0);
        }
        /// <summary>
        /// Converts a byte array to a Float (4 Bytes)
        /// </summary>
        /// <param name="bytes">An array of bytes.</param>
        /// <param name="startPos">The starting position within value.</param>
        /// <returns>The converted Float</returns>
        public static float ToFloat(this byte[] bytes, Int32 startPos)
        {
            byte[] floatBytes = new byte[4];
            Buffer.BlockCopy(bytes, startPos, floatBytes, 0, 4);

            Array.Reverse(floatBytes);

            return BitConverter.ToSingle(floatBytes, 0);
        }
        public static float ToFloat(this byte[] bytes)
        {
            return ToFloat(bytes, 0);
        }

        internal static byte[] LabelsToByteArray(this object src, CARecord record)
        {
            using (MemoryStream mem = new MemoryStream(424))
            {
                using (BinaryWriter writer = new BinaryWriter(mem))
                {
                    //writer.Write(ToByteArray(((IConvertible)src).ToInt32(null)));
                    writer.Write(ToByteArray(0));
                    /*writer.Write(ToByteArray((short)((int)src)));
                    writer.Write(ToByteArray((short)((int)src)));*/
                    //writer.Write(ToByteArray((int)src));
                    string[] names = Enum.GetNames(src.GetType());
                    writer.Write(ToByteArray((Int16)names.Length));
                    foreach(var i in names)
                        writer.Write(ToByteArray(i, 26));
                    writer.Seek(422, SeekOrigin.Begin);
                    writer.Write(ToByteArray((short)((int)src)));
                }
                return mem.GetBuffer();
            }
        }

        internal static byte[] ToByteArray(this object src, EpicsType epicsType, CARecord record)
        {
            return ToByteArray(src, epicsType, record, record.dataCount);
        }

        /// <summary>
        /// recalls the object to Byte function with the right datatype-struct for easy coding
        /// </summary>
        internal static byte[] ToByteArray(this object src, EpicsType epicsType, CARecord record, int dataCount)
        {
            //Type valueType = record["VAL"].GetType();
            Type valueType = src.GetType();
            object mySrc = src;

            if (valueType.IsEnum)
                return ToByteArray<Enum>(src, epicsType, record, dataCount);
            string name;
            if (valueType.IsGenericType && valueType.Name.Split(new char[] { '`' })[0] == "ArrayContainer")
            {
                name = valueType.GetGenericArguments()[0].Name;
                mySrc = ((dynamic)src).arrayValues;
            }
            else if (valueType.IsArray)
                name = valueType.GetElementType().Name;
            else
                name = valueType.Name;
            switch (name)
            {
                case "String":
                    return ToByteArray<string>(mySrc, epicsType, record, dataCount);
                case "Int32":
                    return ToByteArray<int>(mySrc, epicsType, record, dataCount);
                case "Int16":
                    return ToByteArray<short>(mySrc, epicsType, record, dataCount);
                case "Single":
                    return ToByteArray<float>(mySrc, epicsType, record, dataCount);
                case "Double":
                    return ToByteArray<double>(mySrc, epicsType, record, dataCount);
                case "Byte":
                    return ToByteArray<byte>(mySrc, epicsType, record, dataCount);
                default:
                    throw new Exception("Wrong DataType defined");
            }
        }

        /// <summary>
        /// Function for full change from an Object to Byte.
        /// </summary>
        /// <typeparam name="dataType">Datatype of the source</typeparam>
        /// <param name="src">Src object which shall be transferred</param>
        /// <param name="epicsType">Target epics type</param>
        /// <param name="record">Record from where the value comes</param>
        /// <param name="dataCount">Count of data requested</param>
        /// <returns></returns>
        internal static byte[] ToByteArray<dataType>(object src, EpicsType epicsType, CARecord record, int dataCount)
        {
            dataType[] source;

            if (src.GetType().IsArray)
            {
                source = new dataType[dataCount];
                int i = 0;
                foreach (object element in ((System.Collections.IEnumerable)src))
                {
                    if (i >= source.Length)
                        break;
                    source[i] = (dataType)Convert.ChangeType(element, typeof(dataType));
                    i++;
                }
            }
            else
                source = new dataType[] { (dataType)Convert.ChangeType(src, typeof(dataType)) };

            using (MemoryStream mem = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(mem))
                {
                    switch (epicsType)
                    {
                        #region simple Types
                        case EpicsType.String:
                            for (int i = 0; i < dataCount; i++)
                                writer.Write(ToByteArray(source[i].ToString(), true));
                            return mem.ToArray();
                        case EpicsType.Short:
                            for (int i = 0; i < dataCount; i++)
                                writer.Write(ToByteArray(Convert.ToInt16(source[i])));
                            return mem.ToArray();
                        case EpicsType.Float:
                            for (int i = 0; i < dataCount; i++)
                                writer.Write(ToByteArray(Convert.ToSingle(source[i])));
                            return mem.ToArray();
                        case EpicsType.Byte:
                            for (int i = 0; i < dataCount; i++)
                                writer.Write(ToByteArray(Convert.ToByte(source[i])));
                            return mem.ToArray();
                        case EpicsType.Int:
                            for (int i = 0; i < dataCount; i++)
                                writer.Write(ToByteArray(Convert.ToInt32(source[i])));
                            return mem.ToArray();
                        case EpicsType.Double:
                            for (int i = 0; i < dataCount; i++)
                                writer.Write(ToByteArray(Convert.ToDouble(source[i])));
                            return mem.ToArray();
                        #endregion

                        #region StatusTypes
                        case EpicsType.Status_Double:
                            writer.Write(ToByteArray((ushort)(record["STAT"] ?? -1)));
                            writer.Write(ToByteArray((ushort)(record["SEVR"] ?? -1)));
                            writer.Write(ToByteArray((uint)0)); // Skip 4 bytes
                            for (int i = 0; i < dataCount; i++)
                                writer.Write(ToByteArray(Convert.ToDouble(source[i])));
                            return mem.ToArray();
                        case EpicsType.Status_Float:
                            writer.Write(ToByteArray((ushort)(record["STAT"] ?? 0)));
                            writer.Write(ToByteArray((ushort)(record["SEVR"] ?? 0)));
                            for (int i = 0; i < dataCount; i++)
                                writer.Write(ToByteArray(Convert.ToSingle(source[i])));
                            return mem.ToArray();
                        case EpicsType.Status_Int:
                            writer.Write(ToByteArray((ushort)(record["STAT"] ?? 0)));
                            writer.Write(ToByteArray((ushort)(record["SEVR"] ?? 0)));
                            for (int i = 0; i < dataCount; i++)
                                writer.Write(ToByteArray(Convert.ToInt32(source[i])));
                            return mem.ToArray();
                        case EpicsType.Status_Byte:
                            writer.Write(ToByteArray((ushort)(record["STAT"] ?? 0)));
                            writer.Write(ToByteArray((ushort)(record["SEVR"] ?? 0)));
                            for (int i = 0; i < dataCount; i++)
                                writer.Write(ToByteArray(Convert.ToByte(source[i])));
                            return mem.ToArray();
                        case EpicsType.Status_Short:
                            writer.Write(ToByteArray((ushort)(record["STAT"] ?? 0)));
                            writer.Write(ToByteArray((ushort)(record["SEVR"] ?? 0)));
                            for (int i = 0; i < dataCount; i++)
                                writer.Write(ToByteArray(Convert.ToInt16(source[i])));
                            return mem.ToArray();
                        case EpicsType.Status_String:
                            writer.Write(ToByteArray((ushort)(record["STAT"] ?? 0)));
                            writer.Write(ToByteArray((ushort)(record["SEVR"] ?? 0)));
                            for (int i = 0; i < dataCount; i++)
                                writer.Write(ToByteArray(source[i].ToString(), true));
                            return mem.ToArray();
                        #endregion

                        #region TimeTypes
                        case EpicsType.Time_Double:
                            writer.Write(ToByteArray((ushort)(record["STAT"] ?? 0)));
                            writer.Write(ToByteArray((ushort)(record["SEVR"] ?? 0)));
                            writer.Write(ToByteArray((DateTime)(record["TIME"] ?? 0)));
                            writer.Write(ToByteArray((UInt32)0)); // Skip 4 bytes
                            for (int i = 0; i < dataCount; i++)
                                writer.Write(ToByteArray(Convert.ToDouble(source[i])));
                            return mem.ToArray();
                        case EpicsType.Time_Float:
                            writer.Write(ToByteArray((ushort)(record["STAT"] ?? 0)));
                            writer.Write(ToByteArray((ushort)(record["SEVR"] ?? 0)));
                            writer.Write(ToByteArray((DateTime)(record["TIME"] ?? 0)));
                            for (int i = 0; i < dataCount; i++)
                                writer.Write(ToByteArray(Convert.ToSingle(source[i])));
                            return mem.ToArray();
                        case EpicsType.Time_Int:
                            writer.Write(ToByteArray((ushort)(record["STAT"] ?? 0)));
                            writer.Write(ToByteArray((ushort)(record["SEVR"] ?? 0)));
                            writer.Write(ToByteArray((DateTime)(record["TIME"] ?? 0)));
                            for (int i = 0; i < dataCount; i++)
                                writer.Write(ToByteArray(Convert.ToInt32(source[i])));
                            return mem.ToArray();
                        case EpicsType.Time_Short:
                            writer.Write(ToByteArray((ushort)(record["STAT"] ?? 0)));
                            writer.Write(ToByteArray((ushort)(record["SEVR"] ?? 0)));
                            writer.Write(ToByteArray((DateTime)(record["TIME"] ?? 0)));
                            writer.Write(new byte[2]);
                            for (int i = 0; i < dataCount; i++)
                                writer.Write(ToByteArray(Convert.ToInt16(source[i])));
                            return mem.ToArray();
                        case EpicsType.Time_String:
                            writer.Write(ToByteArray((ushort)(record["STAT"] ?? 0)));
                            writer.Write(ToByteArray((ushort)(record["SEVR"] ?? 0)));
                            writer.Write(ToByteArray((DateTime)(record["TIME"] ?? 0)));
                            for (int i = 0; i < dataCount; i++)
                                writer.Write(ToByteArray(source[i].ToString(), true));
                            return mem.ToArray();
                        case EpicsType.Time_Byte:
                            writer.Write(ToByteArray((ushort)(record["STAT"] ?? 0)));
                            writer.Write(ToByteArray((ushort)(record["SEVR"] ?? 0)));
                            writer.Write(ToByteArray((DateTime)(record["TIME"] ?? 0)));
                            for (int i = 0; i < dataCount; i++)
                                writer.Write(ToByteArray(Convert.ToByte(source[i])));
                            return mem.ToArray();
                        #endregion

                        #region ControlTypes
                        case EpicsType.Control_Double:
                            writer.Write(ToByteArray((ushort)(record["STAT"] ?? 0)));
                            writer.Write(ToByteArray((ushort)(record["SEVR"] ?? 0)));
                            writer.Write(ToByteArray((short)(record["PREC"] ?? 0)));
                            writer.Write(new byte[] { 0, 0 });
                            writer.Write(ToByteArray((string)(record["EGU"] ?? ""), 8));
                            writer.Write(ToByteArray(record.GetDouble("HIGHDISP")));
                            writer.Write(ToByteArray(record.GetDouble("LOWDISP")));
                            writer.Write(ToByteArray(record.GetDouble("HIHI")));
                            writer.Write(ToByteArray(record.GetDouble("HIGH")));
                            writer.Write(ToByteArray(record.GetDouble("LOW")));
                            writer.Write(ToByteArray(record.GetDouble("LOLO")));
                            writer.Write(ToByteArray(record.GetDouble("HOPR")));
                            writer.Write(ToByteArray(record.GetDouble("LOPR")));

                            for (int i = 0; i < dataCount; i++)
                                writer.Write(ToByteArray(Convert.ToDouble(source[i])));
                            return mem.ToArray();
                        case EpicsType.Control_Float:
                            writer.Write(ToByteArray((ushort)(record["STAT"] ?? 0)));
                            writer.Write(ToByteArray((ushort)(record["SEVR"] ?? 0)));
                            writer.Write(ToByteArray((short)(record["PREC"] ?? 0)));
                            writer.Write(new byte[] { 0, 0 });
                            writer.Write(ToByteArray((string)(record["EGU"] ?? ""), 8));
                            writer.Write(ToByteArray(record.GetFloat("HIGHDISP")));
                            writer.Write(ToByteArray(record.GetFloat("LOWDISP")));
                            writer.Write(ToByteArray(record.GetFloat("HIHI")));
                            writer.Write(ToByteArray(record.GetFloat("HIGH")));
                            writer.Write(ToByteArray(record.GetFloat("LOW")));
                            writer.Write(ToByteArray(record.GetFloat("LOLO")));
                            writer.Write(ToByteArray(record.GetFloat("HOPR")));
                            writer.Write(ToByteArray(record.GetFloat("LOPR")));
                            for (int i = 0; i < dataCount; i++)
                                writer.Write(ToByteArray(Convert.ToSingle(source[i])));
                            return mem.ToArray();
                        case EpicsType.Control_Int:
                            writer.Write(ToByteArray((ushort)(record["STAT"] ?? 0)));
                            writer.Write(ToByteArray((ushort)(record["SEVR"] ?? 0)));
                            //writer.Write(new byte[] { 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77 });
                            writer.Write(ToByteArray((string)(record["EGU"] ?? ""), 8));
                            writer.Write(ToByteArray(record.GetInt("HIGHDISP")));
                            writer.Write(ToByteArray(record.GetInt("LOWDISP")));
                            writer.Write(ToByteArray(record.GetInt("HIHI")));
                            writer.Write(ToByteArray(record.GetInt("HIGH")));
                            writer.Write(ToByteArray(record.GetInt("LOW")));
                            writer.Write(ToByteArray(record.GetInt("LOLO")));
                            writer.Write(ToByteArray(record.GetInt("HOPR")));
                            writer.Write(ToByteArray(record.GetInt("LOPR")));
                            for (int i = 0; i < dataCount; i++)
                                writer.Write(ToByteArray(Convert.ToInt32(source[i])));
                            return mem.ToArray();
                        case EpicsType.Control_Short:
                            writer.Write(ToByteArray((ushort)(record["STAT"] ?? 0)));
                            writer.Write(ToByteArray((ushort)(record["SEVR"] ?? 0)));
                            writer.Write(ToByteArray((string)(record["EGU"] ?? ""), 8));
                            writer.Write(ToByteArray(record.GetShort("HIGHDISP")));
                            writer.Write(ToByteArray(record.GetShort("LOWDISP")));
                            writer.Write(ToByteArray(record.GetShort("HIHI")));
                            writer.Write(ToByteArray(record.GetShort("HIGH")));
                            writer.Write(ToByteArray(record.GetShort("LOW")));
                            writer.Write(ToByteArray(record.GetShort("LOLO")));
                            writer.Write(ToByteArray(record.GetShort("HOPR")));
                            writer.Write(ToByteArray(record.GetShort("LOPR")));
                            for (int i = 0; i < dataCount; i++)
                                writer.Write(ToByteArray(Convert.ToInt16(source[i])));
                            return mem.ToArray();
                        case EpicsType.Control_Byte:
                            throw new Exception("NOT IMPLEMENTED");
                        case EpicsType.Control_String:
                            writer.Write(ToByteArray((ushort)record["STAT"]));
                            writer.Write(ToByteArray((ushort)record["SEVR"]));
                            for (int i = 0; i < dataCount; i++)
                                writer.Write(ToByteArray(Convert.ToString(source[i]), true));
                            return mem.ToArray();
                        #endregion

                        #region GraphicsTypes
                        case EpicsType.Display_Double:
                            writer.Write(ToByteArray((ushort)(record["STAT"] ?? 0)));
                            writer.Write(ToByteArray((ushort)(record["SEVR"] ?? 0)));
                            writer.Write(ToByteArray((short)(record["PREC"] ?? 0)));
                            writer.Write(new byte[] { 0, 0 });
                            writer.Write(ToByteArray((string)(record["EGU"] ?? ""), 8));
                            writer.Write(ToByteArray(record.GetDouble("HIGHDISP")));
                            writer.Write(ToByteArray(record.GetDouble("LOWDISP")));
                            writer.Write(ToByteArray(record.GetDouble("HIHI")));
                            writer.Write(ToByteArray(record.GetDouble("HIGH")));
                            writer.Write(ToByteArray(record.GetDouble("LOW")));
                            writer.Write(ToByteArray(record.GetDouble("LOLO")));
                            for (int i = 0; i < dataCount; i++)
                                writer.Write(ToByteArray(Convert.ToDouble(source[i])));
                            return mem.ToArray();
                        case EpicsType.Display_Float:
                            writer.Write(ToByteArray((ushort)(record["STAT"] ?? 0)));
                            writer.Write(ToByteArray((ushort)(record["SEVR"] ?? 0)));
                            writer.Write(ToByteArray((short)(record["PREC"] ?? 0)));
                            writer.Write(new byte[] { 0, 0 });
                            writer.Write(ToByteArray((string)(record["EGU"] ?? ""), 8));
                            writer.Write(ToByteArray(record.GetFloat("HIGHDISP")));
                            writer.Write(ToByteArray(record.GetFloat("LOWDISP")));
                            writer.Write(ToByteArray(record.GetFloat("HIHI")));
                            writer.Write(ToByteArray(record.GetFloat("HIGH")));
                            writer.Write(ToByteArray(record.GetFloat("LOW")));
                            writer.Write(ToByteArray(record.GetFloat("LOLO")));
                            for (int i = 0; i < dataCount; i++)
                                writer.Write(ToByteArray(Convert.ToSingle(source[i])));
                            return mem.ToArray();
                        case EpicsType.Display_Int:
                            writer.Write(ToByteArray((ushort)(record["STAT"] ?? 0)));
                            writer.Write(ToByteArray((ushort)(record["SEVR"] ?? 0)));
                            writer.Write(ToByteArray((string)(record["EGU"] ?? ""), 8));
                            writer.Write(ToByteArray(record.GetInt("HIGHDISP")));
                            writer.Write(ToByteArray(record.GetInt("LOWDISP")));
                            writer.Write(ToByteArray(record.GetInt("HIHI")));
                            writer.Write(ToByteArray(record.GetInt("HIGH")));
                            writer.Write(ToByteArray(record.GetInt("LOW")));
                            writer.Write(ToByteArray(record.GetInt("LOLO")));
                            for (int i = 0; i < dataCount; i++)
                                writer.Write(ToByteArray(Convert.ToInt32(source[i])));
                            return mem.ToArray();
                        case EpicsType.Display_Short:
                            writer.Write(ToByteArray((ushort)(record["STAT"] ?? 0)));
                            writer.Write(ToByteArray((ushort)(record["SEVR"] ?? 0)));
                            writer.Write(ToByteArray((string)(record["EGU"] ?? ""), 8));
                            writer.Write(ToByteArray(record.GetShort("HIGHDISP")));
                            writer.Write(ToByteArray(record.GetShort("LOWDISP")));
                            writer.Write(ToByteArray(record.GetShort("HIHI")));
                            writer.Write(ToByteArray(record.GetShort("HIGH")));
                            writer.Write(ToByteArray(record.GetShort("LOW")));
                            writer.Write(ToByteArray(record.GetShort("LOLO")));
                            for (int i = 0; i < dataCount; i++)
                                writer.Write(ToByteArray(Convert.ToInt16(source[i])));
                            return mem.ToArray();
                        case EpicsType.Display_Byte:
                            throw new Exception("NOT IMPLEMENTED");
                        case EpicsType.Display_String:
                            writer.Write(ToByteArray((ushort)(record["STAT"] ?? 0)));
                            writer.Write(ToByteArray((ushort)(record["SEVR"] ?? 0)));
                            for (int i = 0; i < dataCount; i++)
                                writer.Write(ToByteArray(Convert.ToString(source[i]), true));
                            return mem.ToArray();
                        #endregion
                        default:
                            return null;
                    }
                }
            }
        }

        internal static object ByteToObject(this byte[] payload, EpicsType epicsType)
        {
            switch (epicsType)
            {
                case EpicsType.String:
                    return ByteConverter.ToString(payload);
                case EpicsType.Short:
                    return ByteConverter.ToInt16(payload);
                case EpicsType.UShort:
                    return ByteConverter.ToUInt16(payload);
                case EpicsType.Float:
                    return ByteConverter.ToFloat(payload);
                case EpicsType.Byte:
                    return ByteConverter.ToByte(payload, 0);
                case EpicsType.Int:
                    return ByteConverter.ToInt32(payload);
                case EpicsType.Double:
                    return ByteConverter.ToDouble(payload);

                default:
                    throw new Exception("Type " + epicsType.ToString() + " not yet supported");
            }
        }

        internal static object ByteToObject(this byte[] payload, EpicsType epicsType, int datacount)
        {
            switch (epicsType)
            {
                /// -> basic types
                case EpicsType.String:
                    {
                        string[] arr = new string[datacount];
                        for (int i = 0; i < datacount; i++)
                        {
                            string tmp = ByteConverter.ToString(payload, i * 40);
                            arr[i] = tmp;
                            /*offset += tmp.Length + 1;*/
                        }

                        return arr;
                    }
                case EpicsType.Short:
                    {
                        short[] arr = new short[datacount];
                        for (int i = 0; i < datacount; i++)
                            arr[i] = ByteConverter.ToInt16(payload, i * 2);

                        return arr;
                    }
                case EpicsType.Float:
                    {
                        float[] arr = new float[datacount];
                        for (int i = 0; i < datacount; i++)
                            arr[i] = ByteConverter.ToFloat(payload, i * 4);

                        return arr;
                    }
                case EpicsType.Byte:
                    {
                        byte[] arr = new byte[datacount];
                        for (int i = 0; i < datacount; i++)
                            arr[i] = ByteConverter.ToByte(payload, i);

                        return arr;
                    }
                case EpicsType.Int:
                    {
                        int[] arr = new int[datacount];
                        for (int i = 0; i < datacount; i++)
                            arr[i] = ByteConverter.ToInt32(payload, i * 4);

                        return arr;
                    }
                case EpicsType.Double:
                    {
                        double[] arr = new double[datacount];
                        for (int i = 0; i < datacount; i++)
                            arr[i] = ByteConverter.ToDouble(payload, i * 8);

                        return arr;
                    }
                default:
                    throw new Exception("Type " + epicsType.ToString() + " not yet supported");
            }
        }
    }
}
