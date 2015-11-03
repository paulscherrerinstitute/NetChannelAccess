using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CaSharpServer.Constants;

namespace CaSharpServer
{
    /// <summary>
    /// An enum record.
    /// </summary>
    public class CAEnumRecord<TType> : CARecord where TType : struct, IComparable, IFormattable, IConvertible
    {
        TType currentValue;

        [CAField("VAL")]
        public TType Value
        {
            get
            {
                return this.currentValue;
            }
            set
            {
                if (!currentValue.Equals(value))
                    this.IsDirty = true;
                currentValue = value;
                if (Scan == ScanAlgorithm.ON_CHANGE && this.IsDirty)
                    ProcessRecord();
            }
        }
    }
}
