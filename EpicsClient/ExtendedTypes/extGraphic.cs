using System;
using System.Collections.Generic;
using System.Text;

namespace PSI.EpicsClient2
{
    /// <summary>
    /// extended epics graphic type <br/> serves severity, status, value, precision (for double and float), unittype 
    /// and a bunch of limits. 
    /// </summary>
    /// <typeparam name="TType">generic datatype for value</typeparam>
    public class ExtGraphic<TType> : ExtType<TType>
    {
        internal ExtGraphic()
        {
        }

        /// <summary>
        /// Epics defined precision of the valuem, only set for double or float
        /// </summary>
        public short Precision { get; internal set; }

        /// <summary>
        /// EnGineer Unit of the value
        /// </summary>
        public string EGU { get; internal set; }

        /// <summary>
        /// Low limit for correct operation.
        /// </summary>
        public double LowWarnLimit { get; internal set; }
        /// <summary>
        /// Low limit for incorrect operation
        /// </summary>
        public double LowAlertLimit { get; internal set; }
        /// <summary>
        /// Lowest possible value which is visible
        /// </summary>
        public double LowDisplayLimit { get; internal set; }

        /// <summary>
        /// High limit for correct operation  
        /// </summary>
        public double HighWarnLimit { get; internal set; }
        /// <summary>
        /// High limit for incorrect operation
        /// </summary>
        public double HighAlertLimit { get; internal set; }
        /// <summary>
        /// Highest possible value which is visible
        /// </summary>
        public double HighDisplayLimit { get; internal set; }

        internal override void Decode(EpicsChannel channel, uint nbElements)
        {
            Status = (Status)channel.DecodeData<ushort>(1, 0);
            Severity = (Severity)channel.DecodeData<ushort>(1, 2);
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
                HighDisplayLimit = Convert.ToDouble(channel.DecodeData(t,1, pos));
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
                                 "HighWarnLimit:{8},HighAlertLimit:{9},HighDisplayLimit:{10}",
                                 Value, Status, Severity, EGU, Precision, LowDisplayLimit, LowAlertLimit, LowWarnLimit,
                                 HighWarnLimit, HighAlertLimit, HighDisplayLimit);
        }
    }
}
