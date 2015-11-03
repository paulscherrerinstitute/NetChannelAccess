using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CaSharpServer
{
    public class CADoubleArrayRecord : CAArrayRecord<double>
    {
        public CADoubleArrayRecord(int size)
            : base(size)
        {
        }
    }
}
