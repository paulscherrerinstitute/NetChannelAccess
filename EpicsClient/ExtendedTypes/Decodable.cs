using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PSI.EpicsClient2
{
    public abstract class Decodable
    {
        internal abstract void Decode(EpicsChannel channel, uint nbElements);
    }
}
