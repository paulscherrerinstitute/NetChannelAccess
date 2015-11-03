using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using PSI.EpicsClient2.Pipes;
using System.IO;
using System.Net;

namespace PSI.EpicsClient2
{
    class Searcher : IDisposable
    {
        Thread searchThread;
        EpicsClient Client;
        bool needToRun = true;
        bool disposed = false;

        List<EpicsChannel> toSearch = new List<EpicsChannel>();

        internal Searcher(EpicsClient client)
        {
            Client = client;
            searchThread = new Thread(SearchChannels);
            searchThread.IsBackground = true;
            searchThread.Start();
        }

        internal void Add(EpicsChannel channel)
        {
            lock (toSearch)
            {
                channel.SearchInvervalCounter = channel.SearchInverval = 1;

                if (!toSearch.Contains(channel))
                    toSearch.Add(channel);
            }
        }

        internal bool Contains(EpicsChannel channel)
        {
            lock (toSearch)
                return toSearch.Contains(channel);
        }

        internal void Remove(EpicsChannel channel)
        {
            lock (toSearch)
                toSearch.Remove(channel);
        }

        void SearchChannels()
        {
            while (needToRun)
            {
                Thread.Sleep(50);
                lock (toSearch)
                {
                    if (toSearch.Count == 0)
                        continue;
                }

                MemoryStream mem = new MemoryStream();
                lock (toSearch)
                {
                    foreach (EpicsChannel c in toSearch)
                        c.SearchInvervalCounter--;

                    foreach (EpicsChannel c in toSearch.Where(row => row.SearchInvervalCounter <= 0 && 
                        (this.Client.Configuration.MaxSearchSeconds == 0
                        || (DateTime.Now-row.StartSearchTime).TotalSeconds < this.Client.Configuration.MaxSearchSeconds)))
                    {
                        c.SearchInverval *= 2;
                        if (c.SearchInverval > 10)
                            c.SearchInverval = 10;
                        c.SearchInvervalCounter = c.SearchInverval;

                        mem.Write(c.SearchPacket.Data, 0, c.SearchPacket.Data.Length);
                        if (mem.Length > 1400)
                        {
                            SendBuffer(mem.ToArray());
                            mem.Dispose();
                            mem = new MemoryStream();
                        }
                    }
                }
                if (mem.Position != 0)
                    SendBuffer(mem.ToArray());
            }
        }

        void SendBuffer(byte[] buff)
        {
            foreach (IPEndPoint i in Client.Configuration.SearchAddresses)
            {
                ((UdpReceiver)Client.Udp[0]).Send(i, buff);
            }
        }

        public void Dispose()
        {
            if (disposed)
                return;
            disposed = true;
            needToRun = false;
        }
    }
}
