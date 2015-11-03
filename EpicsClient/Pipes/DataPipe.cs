using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace PSI.EpicsClient2.Pipes
{
    internal class DataPipe : IDisposable
    {
        private List<DataFilter> Filters = new List<DataFilter>();
        public DataFilter FirstFilter;
        public DataFilter LastFilter;
        public DateTime LastMessage = DateTime.Now;

        /// <summary>
        /// Adds the next Worker to the chain and register it to the previous Worker to the ReceiveData event.
        /// </summary>
        /// <param name="worker"></param>
        private void Add(DataFilter filter)
        {
            Filters.Add(filter);
            if (FirstFilter == null)
                FirstFilter = filter;
            else
                LastFilter.ReceiveData += new ReceiveDataDelegate(filter.ProcessData);
            LastFilter = filter;
        }

        private DataPipe()
        {
            GeneratedEcho = false;
        }

        public DataFilter this[int key]
        {
            get
            {
                return Filters[key];
            }
        }

        static Type[] UdpChainList = new Type[] { typeof(UdpReceiver), typeof(PacketSplitter), typeof(HandleMessage) };
        internal static DataPipe CreateUdp(EpicsClient client)
        {
            DataPipe res = PopulatePipe(UdpChainList, client);
            return res;
        }

        static Type[] TcpChainList = new Type[] { typeof(TcpReceiver), typeof(PacketSplitter), typeof(HandleMessage) };
        internal static DataPipe CreateTcp(EpicsClient client, System.Net.IPEndPoint iPEndPoint)
        {
            DataPipe res = PopulatePipe(TcpChainList, client);
            ((TcpReceiver)res[0]).Start(iPEndPoint);
            return res;
        }

        static DataPipe PopulatePipe(Type[] types, EpicsClient client)
        {
            DataPipe pipe = new DataPipe();
            foreach (Type t in types)
            {
                DataFilter w = (DataFilter)t.GetConstructor(new Type[] { }).Invoke(new object[] { });
                w.Client = client;
                w.Pipe = pipe;
                pipe.Add(w);
            }
            return pipe;
        }

        public void Dispose()
        {
            foreach (var i in this.Filters)
                i.Dispose();
        }

        internal bool GeneratedEcho { get; set; }
    }
}
