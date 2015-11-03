using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpicsSharp.ChannelAccess.Server
{
    internal class EpicsEvent
    {
        public EventHandler Handler { get; set; }

        public RecordTypes.CARecord Record { get; set; }

        public int DataCount { get; set; }

        public Constants.EpicsType EpicsType { get; set; }

        public uint SID { get; set; }
    }
}
