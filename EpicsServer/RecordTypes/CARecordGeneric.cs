using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CaSharpServer.Constants;

namespace CaSharpServer
{
    /// <summary>
    /// Generic CARecord allows to store a type in the VAL property
    /// </summary>
    /// <typeparam name="TType"></typeparam>
    public abstract class CARecord<TType> : CARecord
    {
        /// <summary>
        /// Stores the actual value of the record
        /// </summary>
        TType currentValue;

        /// <summary>
        /// Access the value linked to the record
        /// </summary>
        [CAField("VAL")]
        public TType Value
        {
            get
            {
                return currentValue;
            }
            set
            {
                if ((currentValue == null && value != null) || !currentValue.Equals(value))
                    this.IsDirty = true;
                currentValue = value;
                if (Scan == ScanAlgorithm.ON_CHANGE && this.IsDirty)
                    ProcessRecord();
            }
        }
    }
}
