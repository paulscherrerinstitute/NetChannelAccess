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
using System.Text;
using System.Threading;
using EpicsSharp.Common.Pipes;
using System.IO;
using System.Net;

namespace EpicsSharp.ChannelAccess.Client
{
    class Searcher : IDisposable
    {
        Thread searchThread;
        CAClient Client;
        bool needToRun = true;
        bool disposed = false;

        List<Channel> toSearch = new List<Channel>();

        internal Searcher(CAClient client)
        {
            Client = client;
            searchThread = new Thread(SearchChannels);
            searchThread.IsBackground = true;
            searchThread.Start();
        }

        internal void Add(Channel channel)
        {
            lock (toSearch)
            {

                if (!toSearch.Contains(channel))
                {
                    //Console.WriteLine("Starting the search for " + channel.ChannelName);
                    channel.SearchInvervalCounter = channel.SearchInverval = 1;
                    toSearch.Add(channel);
                }
            }
        }

        internal bool Contains(Channel channel)
        {
            lock (toSearch)
                return toSearch.Contains(channel);
        }

        internal void Remove(Channel channel)
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
                    foreach (Channel c in toSearch)
                        c.SearchInvervalCounter--;

                    foreach (Channel c in toSearch.Where(row => row.SearchInvervalCounter <= 0
                        && row.Status != Constants.ChannelStatus.CONNECTED &&
                        (this.Client.Configuration.MaxSearchSeconds == 0
                        || (DateTime.Now - row.StartSearchTime).TotalSeconds < this.Client.Configuration.MaxSearchSeconds)))
                    {
                        c.SearchInverval *= 2;
                        if (c.SearchInverval > 10)
                            c.SearchInverval = 10;
                        c.SearchInvervalCounter = c.SearchInverval;

                        lock (c.ElapsedTimings)
                        {
                            if (!c.ElapsedTimings.ContainsKey("FirstSearch"))
                                c.ElapsedTimings.Add("FirstSearch", c.Stopwatch.Elapsed);
                        }

                        //Console.WriteLine("Sent search for " + c.ChannelName);
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
                mem.Dispose();
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
