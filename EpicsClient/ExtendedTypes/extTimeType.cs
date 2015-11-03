using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace PSI.EpicsClient2
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

        internal override void Decode(EpicsChannel channel, uint nbElements)
        {
            Status = (Status)channel.DecodeData<ushort>(1, 0);
            Severity = (Severity)channel.DecodeData<ushort>(1, 2);
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
