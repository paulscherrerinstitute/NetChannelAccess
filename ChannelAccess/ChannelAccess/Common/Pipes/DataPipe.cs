/*
 *  EpicsSharp - An EPICS Channel Access library for the .NET platform.
 *
 *  Copyright (C) 2013 - 2017  Paul Scherrer Institute, Switzerland
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
using EpicsSharp.ChannelAccess.Client;
using EpicsSharp.ChannelAccess.Server;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace EpicsSharp.Common.Pipes
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

        internal static DataPipe CreateServerUdp(CAServer server, IPAddress address, int udpPort)
        {
            DataPipe res = new DataPipe();
            res.Add(new UdpReceiver(address, udpPort));
            res[0].Pipe = res;
            AddToPipe(new Type[] { typeof(PacketSplitter), typeof(ServerHandleMessage) }, res);
            ((ServerHandleMessage)res.LastFilter).Server = server;
            return res;
        }

        internal static DataPipe CreateClientUdp(CAClient client, int udpReceiverPort = 0)
        {
            DataPipe res = PopulatePipe(new object[] { new UdpReceiver(null, udpReceiverPort), typeof(PacketSplitter), typeof(ClientHandleMessage) });
            ((ClientHandleMessage)res.LastFilter).Client = client;
            return res;
        }

        internal static DataPipe CreateClientTcp(CAClient client, System.Net.IPEndPoint iPEndPoint)
        {
            DataPipe res = PopulatePipe(new Type[] { typeof(ClientTcpReceiver), typeof(PacketSplitter), typeof(ClientHandleMessage) });
            ((ClientTcpReceiver)res[0]).Init(client, iPEndPoint);
            ((ClientHandleMessage)res.LastFilter).Client = client;
            return res;
        }

        internal static DataPipe CreateServerTcp(CAServer server, Socket client)
        {
            DataPipe res = PopulatePipe(new Type[] { typeof(ServerTcpReceiver), typeof(PacketSplitter), typeof(ServerHandleMessage) });
            //((TcpReceiver)res[0]).Start(iPEndPoint);
            ((ServerHandleMessage)res.LastFilter).Server = server;
            ((ServerTcpReceiver)res.FirstFilter).Init(client);

            return res;
        }

        private static DataPipe PopulatePipe(Type[] types)
        {
            DataPipe pipe = new DataPipe();
            AddToPipe(types, pipe);
            return pipe;
        }

        private static DataPipe PopulatePipe(object[] objects)
        {
            DataPipe pipe = new DataPipe();
            AddToPipe(objects, pipe);
            return pipe;
        }

        private static void AddToPipe(Type[] types, DataPipe pipe)
        {
            foreach (Type t in types)
            {
                DataFilter w = (DataFilter)t.GetConstructor(new Type[] { }).Invoke(new object[] { });
                w.Pipe = pipe;
                pipe.Add(w);
            }
        }

        private static void AddToPipe(object[] objects, DataPipe pipe)
        {
            foreach (var o in objects)
            {
                DataFilter w;
                if (o is Type)
                {
                    var t = (Type)o;
                    w = (DataFilter)t.GetConstructor(new Type[] { }).Invoke(new object[] { });
                }
                else
                    w = (DataFilter)o;
                w.Pipe = pipe;
                pipe.Add(w);
            }

        }

        public void Dispose()
        {
            foreach (var i in this.Filters)
                i.Dispose();
        }

        internal bool GeneratedEcho { get; set; }

    }
}
