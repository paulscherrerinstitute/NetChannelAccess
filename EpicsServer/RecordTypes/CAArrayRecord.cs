using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CaSharpServer
{
    public abstract class CAArrayRecord : CARecord
    {
    }

    public abstract class CAArrayRecord<TType> : CAArrayRecord where TType : IComparable
    {
        /// <summary>
        /// Stores the actual value of the record
        /// </summary>
        ArrayContainer<TType> currentValue;


        
        /// <summary>
        /// Access the value linked to the record
        /// </summary>
        [CAField("VAL")]
        public ArrayContainer<TType> Value
        {
            get
            {
                return currentValue;
            }
        }

        string engineeringUnits = "";
        /// <summary>
        /// Defines the value of the Engineering Units.
        /// </summary>
        [CAField("EGU")]
        public string EngineeringUnits
        {
            get
            {
                return engineeringUnits;
            }
            set
            {
                if (value.Length > 8)
                    throw new Exception("Cannot have more than 8 characters for the engineering unit.");
                engineeringUnits = value;
            }
        }

        /// <summary>
        /// Defines the Display Precision.
        /// </summary>
        [CAField("PREC")]
        public short DisplayPrecision { get; set; }

        public CAArrayRecord(int size)
        {
            //currentValue = new TType[size];
            currentValue = new ArrayContainer<TType>(size);
            currentValue.ArrayModified += currentValue_ArrayModified;
            this.dataCount = size;
        }

        void currentValue_ArrayModified(object sender, EventArgs e)
        {
            IsDirty = true;
        }

        public int Length
        {
            get
            {
                return currentValue.arrayValues.Length;
            }
        }
    }
}
