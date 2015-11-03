using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PSI.EpicsClient2.Pipes
{
    public delegate void ReceiveDataDelegate(DataPacket packet);

    abstract class DataFilter : IDisposable
    {
        public event ReceiveDataDelegate ReceiveData;
        public DataPipe Pipe;

        public abstract void ProcessData(DataPacket packet);

        /// <summary>
        /// Sends the DataPacket further in the chain
        /// </summary>
        /// <param name="packet"></param>
        public void SendData(DataPacket packet)
        {
            if (ReceiveData != null)
                ReceiveData(packet);
        }

        public EpicsClient Client { get; set; }

        public virtual void Dispose()
        {
        }
    }
}
