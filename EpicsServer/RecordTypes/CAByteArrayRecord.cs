using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CaSharpServer
{
    public class CAByteArrayRecord  : CAArrayRecord<byte>
    {
        public CAByteArrayRecord(int size)
            : base(size)
        {
        }
    }
}
