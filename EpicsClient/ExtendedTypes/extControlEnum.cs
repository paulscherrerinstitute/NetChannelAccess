using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSI.EpicsClient2
{
    public class ExtControlEnum : ExtType<int>
    {
        internal ExtControlEnum()
        {
        }

        internal override void Decode(EpicsChannel channel, uint nbElements)
        {
            Status = (Status)channel.DecodeData<ushort>(1, 0);
            Severity = (Severity)channel.DecodeData<ushort>(1, 2);
            NbStates = channel.DecodeData<ushort>(1, 4);
            States = new string[NbStates];
            Value = channel.DecodeData<ushort>(1, 422);
            for(int i=0;i < NbStates;i++)
            {
                States[i] = channel.DecodeData<string>(1, 6 + i * 26, 26);
            }
        }

        public ushort NbStates { get; set; }

        public string[] States { get; set; }
    }
}
