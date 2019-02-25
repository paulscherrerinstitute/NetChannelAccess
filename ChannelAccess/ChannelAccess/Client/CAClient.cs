/*
 *  EpicsSharp - An EPICS Channel Access library for the .NET platform.
 *
 *  Copyright (C) 2013 - 2019  Paul Scherrer Institute, Switzerland
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using EpicsSharp.Common.Pipes;
using EpicsSharp.ChannelAccess.Constants;

namespace EpicsSharp.ChannelAccess.Client
{
    public class CAClient : IDisposable
    {
        private readonly CAConfiguration configuration = new CAConfiguration();
        /// <summary>
        /// Allows configuring the channel access client.
        /// </summary>
        public CAConfiguration Configuration { get { return configuration; } }

        internal DataPipe Udp;
        internal Dictionary<uint, Channel> Channels = new Dictionary<uint, Channel>();
        internal Dictionary<IPEndPoint, DataPipe> Iocs = new Dictionary<IPEndPoint, DataPipe>();
        private bool disposed = false;
        internal Searcher Searcher;

        readonly Thread echoThread;

        /// <summary>
        /// Creates a new epics client
        /// </summary>
        public CAClient()
        {
            Udp = DataPipe.CreateClientUdp(this);
            Searcher = new Searcher(this);
            echoThread = new Thread(Echoer);
            echoThread.IsBackground = true;
            echoThread.Start();
        }

        public CAClient(int udpReceiverPort)
        {
            Udp = DataPipe.CreateClientUdp(this,udpReceiverPort);
            Searcher = new Searcher(this);
            echoThread = new Thread(Echoer);
            echoThread.IsBackground = true;
            echoThread.Start();
        }

        void Echoer()
        {
            while (!disposed)
            {
                Thread.Sleep(Configuration.EchoSleeping);

                List<Channel> channels;
                lock (Channels)
                {
                    channels = Channels.Values.ToList();
                }
                channels.ForEach(i => i.VerifyState());

                List<TcpReceiver> toEcho;
                lock (Iocs)
                {
                    DateTime now = DateTime.Now;
                    toEcho = Iocs.Values.Where(row => (row.LastMessage - now).TotalSeconds > Configuration.EchoInterval).Select(row => (TcpReceiver)row[0]).ToList();
                    toEcho = Iocs.Select(row => (TcpReceiver)row.Value[0]).ToList();
                }
                foreach (var i in toEcho)
                {
                    if (i.Pipe.GeneratedEcho == true)
                    {
                        //Console.WriteLine("Double echo? Dispose");
                        i.Dispose();
                    }
                    else
                    {
                        //Console.WriteLine("Sending echo");
                        i.Echo();
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new Channel based on the channel name.
        /// </summary>
        /// <param name="channelName">Name of the channel.</param>
        /// <returns></returns>
        public Channel CreateChannel(string channelName)
        {
            if (disposed)
                throw new ObjectDisposedException(this.GetType().Name);
            Channel channel = new Channel(this, channelName);
            lock (Channels)
            {
                Channels.Add(channel.CID, channel);
            }
            return channel;
        }

        /// <summary>
        /// Creates a new Generic Channel of the given type.
        /// </summary>
        /// <typeparam name="TType">Type of the channel which needs to be deal with.</typeparam>
        /// <param name="channelName">Name of the channel</param>
        /// <returns></returns>
        public Channel<TType> CreateChannel<TType>(string channelName)
        {
            if (disposed)
                throw new ObjectDisposedException(this.GetType().Name);
            Channel<TType> channel = new Channel<TType>(this, channelName);
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
        public void MultiConnect(IEnumerable<Channel> channels)
        {
            if (disposed)
                throw new ObjectDisposedException(this.GetType().Name);
            var list = channels.Where(row => row.Status != ChannelStatus.CONNECTED);
            using (multiActionCountDown = new CountdownEvent(list.Count()))
            {
                foreach (Channel c in list)
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
        public object[] MultiGet<TType>(IEnumerable<Channel> channels)
        {
            if (disposed)
                throw new ObjectDisposedException(this.GetType().Name);
            if (channels != null)
            {
                using (multiActionCountDown = new CountdownEvent(channels.Count()))
                {
                    foreach (Channel c in channels)
                    {
                        c.HasValue = false;
                        c.AfterReadNotify += MultiGetAfterReadNotify;
                        if (c.Status == ChannelStatus.CONNECTED)
                            c.SendReadNotify<TType>();
                        else
                            c.AfterConnect(e => e.SendReadNotify<TType>());
                    }

                    multiActionCountDown.Wait(Configuration.WaitTimeout);
                }

                return channels.Select(row => row.HasValue ? (object)row.DecodeData<TType>(1) : null).ToArray();
            }
            return new object[] { };
        }

        void MultiGetAfterReadNotify(Channel sender, object newValue)
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

        internal Channel GetChannelByCid(uint cid)
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
                //Console.WriteLine("Getting IOC for " + iPEndPoint.ToString());
                if (!Iocs.ContainsKey(iPEndPoint))
                {
                    //Console.WriteLine("Creating new TCP");
                    ioc = DataPipe.CreateClientTcp(this, iPEndPoint);
                    Iocs.Add(iPEndPoint, ioc);
                }
                else
                {
                    //Console.WriteLine("Re-using TCP");
                    ioc = Iocs[iPEndPoint];
                }
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

        public void Dispose(bool cleanManaged)
        {
            Dispose();
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

            List<Channel> c;

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
