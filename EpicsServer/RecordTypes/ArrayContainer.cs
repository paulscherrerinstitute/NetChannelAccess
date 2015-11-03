using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CaSharpServer
{
    public class ArrayContainer<TType>: IEnumerable<TType> where TType : IComparable
    {
        internal TType[] arrayValues;
        internal event EventHandler ArrayModified;

        public ArrayContainer(int size)
        {
            arrayValues = new TType[size];
        }

        public int Length
        {
            get
            {
                return arrayValues.Length;
            }
        }


        public TType this[int key]
        {
            get
            {
                return arrayValues[key];
            }
            set
            {
                if (arrayValues[key].CompareTo(value) != 0 && ArrayModified != null)
                    ArrayModified(this, null);
                arrayValues[key] = value;
            }
        }

        public IEnumerator<TType> GetEnumerator()
        {
            return (IEnumerator<TType>)arrayValues.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return arrayValues.GetEnumerator();
        }
    }
}
