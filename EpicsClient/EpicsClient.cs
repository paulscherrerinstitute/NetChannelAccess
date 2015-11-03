using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using PSI.EpicsClient2.Pipes;

namespace PSI.EpicsClient2
{
    public class EpicsClient : IDisposable
    {
        private readonly CaConfiguration configuration = new CaConfiguration();
        /// <summary>
        /// Allows configuring the channel access client.
        /// </summary>
        public CaConfiguration Configuration { get { return configuration; } }

        internal DataPipe Udp;
        internal Dictionary<uint, EpicsChannel> Channels = new Dictionary<uint, EpicsChannel>();
        internal Dictionary<IPEndPoint, DataPipe> Iocs = new Dictionary<IPEndPoint, DataPipe>();
        private bool disposed = false;
        internal Searcher Searcher;        

        readonly Thread echoThread;

        /// <summary>
        /// Creates a new epics client
        /// </summary>
        public EpicsClient()
        {
            Udp = DataPipe.CreateUdp(this);
            Searcher = new Searcher(this);
            echoThread = new Thread(Echoer);
            echoThread.IsBackground = true;
            echoThread.Start();
        }

        void Echoer()
        {
            while (!disposed)
            {
                Thread.Sleep(10000);
                List<TcpReceiver> toEcho;
                lock (Iocs)
                {
                    DateTime now = DateTime.Now;
                    toEcho=Iocs.Values.Where(row => (row.LastMessage - now).TotalSeconds > 20).Select(row => (TcpReceiver)row[0]).ToList();
                    toEcho = Iocs.Select(row => (TcpReceiver)row.Value[0]).ToList();
                }
                foreach (var i in toEcho)
                {
                    if(i.Pipe.GeneratedEcho == true)
                        i.Dispose();
                    else
                        i.Echo();
                }
            }
        }

        /// <summary>
        /// Creates a new EpicsChannel based on the channel name.
        /// </summary>
        /// <param name="channelName">Name of the channel.</param>
        /// <returns></returns>
        public EpicsChannel CreateChannel(string channelName)
        {
            if (disposed)
                throw new ObjectDisposedException(this.GetType().Name);
            EpicsChannel channel = new EpicsChannel(this, channelName);
            lock (Channels)
            {
                Channels.Add(channel.CID, channel);
            }
            return channel;
        }

        /// <summary>
        /// Creates a new Generic EpicsChannel of the given type.
        /// </summary>
        /// <typeparam name="TType">Type of the channel which needs to be deal with.</typeparam>
        /// <param name="channelName">Name of the channel</param>
        /// <returns></returns>
        public EpicsChannel<TType> CreateChannel<TType>(string channelName)
        {
            if (disposed)
                throw new ObjectDisposedException(this.GetType().Name);
            EpicsChannel<TType> channel = new EpicsChannel<TType>(this, channelName);
            lock (Channels)
            {
                Channels.Add(channel.CID, channel);
            }
            return channel;
        }

        CountdownEvent multiActionCountDown;

        /// <summary>
        /// Connects to the channel (search, and create the virtual circuit).
        /// The function blocks till all the channels specified are connected or till the timeout is reached.
        /// If a channel is already connected it will not block.
        /// </summary>
        /// <param name="channels">The list of channels to connect</param>
        public void MultiConnect(IEnumerable<EpicsChannel> channels)
        {
            if (disposed)
                throw new ObjectDisposedException(this.GetType().Name);
            var list = channels.Where(row => row.Status != ChannelStatus.CONNECTED);
            using (multiActionCountDown = new CountdownEvent(list.Count()))
            {
                foreach (EpicsChannel c in list)
                {
                    c.HasValue = false;
                    c.AfterConnect(e => multiActionCountDown.Signal());
                }

                multiActionCountDown.Wait(Configuration.WaitTimeout);
            }
        }

        /// <summary>
        /// Gets all the channels in paralell and returns the values as a list of objects.
        /// If the value is null it means the channel didn't gave back the value in time.
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="channels"></param>
        /// <returns></returns>
        public object[] MultiGet<TType>(IEnumerable<EpicsChannel> channels)
        {
            if (disposed)
                throw new ObjectDisposedException(this.GetType().Name);
            if (channels != null)
            {
                using (multiActionCountDown = new CountdownEvent(channels.Count()))
                {
                    foreach (EpicsChannel c in channels)
                    {
                        c.HasValue = false;
                        c.AfterReadNotify += MultiGetAfterReadNotify;
                        if(c.Status == ChannelStatus.CONNECTED)
                            c.SendReadNotify<TType>();
                        else
                            c.AfterConnect(e => e.SendReadNotify<TType>());
                    }

                    multiActionCountDown.Wait(Configuration.WaitTimeout);
                }

                return channels.Select(row => row.HasValue ? (object)row.DecodeData<TType>(1) : null).ToArray();
            }
            return new object[] {};
        }

        void MultiGetAfterReadNotify(EpicsChannel sender, object newValue)
        {
            sender.AfterReadNotify -= MultiGetAfterReadNotify;
            try
            {
                multiActionCountDown.Signal();
            }
            catch
            {
            }
        }

        internal EpicsChannel GetChannelByCid(uint cid)
        {
            lock (Channels)
            {
                if (!Channels.ContainsKey(cid))
                    return null;
                return Channels[cid];
            }
        }

        internal DataPipe GetIocConnection(IPEndPoint iPEndPoint)
        {
            DataPipe ioc;
            lock (Iocs)
            {
                if (!Iocs.ContainsKey(iPEndPoint))
                {
                    ioc = DataPipe.CreateTcp(this, iPEndPoint);
                    Iocs.Add(iPEndPoint, ioc);
                }
                else
                    ioc = Iocs[iPEndPoint];
            }
            return ioc;
        }

        internal void RemoveIocConnection(DataPipe ioc)
        {
            lock (Iocs)
            {
                try
                {
                    IPEndPoint ip = Iocs.Where(row => row.Value == ioc).Select(row => row.Key).First();
                    Iocs.Remove(ip);
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Close all channels, and disconnect from the IOCs.
        /// </summary>
        public void Dispose()
        {
            if (disposed)
                return;
            disposed = true;
            Udp.Dispose();
            Searcher.Dispose();

            List<EpicsChannel> c;

            lock (Channels)
            {
                c = Channels.Values.ToList();
            }
            foreach (var i in c)
                i.Dispose();

            List<DataPipe> p;
            lock (Iocs)
            {
                p = Iocs.Values.ToList();
            }
            foreach (var i in p)
                i.Dispose();
        }
    }
}
