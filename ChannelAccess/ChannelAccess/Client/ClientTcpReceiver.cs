/*
 *  EpicsSharp - An EPICS Channel Access library for the .NET platform.
 *
 *  Copyright (C) 2013 - 2015  Paul Scherrer Institute, Switzerland
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
using System.Net.Sockets;
using System.Net;
using EpicsSharp.ChannelAccess.Constants;
using EpicsSharp.ChannelAccess.Common;
using EpicsSharp.Common.Pipes;

namespace EpicsSharp.ChannelAccess.Client
{
    internal class ClientTcpReceiver : TcpReceiver, IDisposable
    {
        Socket socket;
        bool disposed = false;
        byte[] buffer = new byte[8192 * 3];

        internal Dictionary<string, uint> ChannelSID = new Dictionary<string, uint>();
        internal Dictionary<uint, Channel> PendingIo = new Dictionary<uint, Channel>();
        internal List<Channel> ConnectedChannels = new List<Channel>();

        static readonly DataPacket echoPacket;

        public CAClient Client;

        static ClientTcpReceiver()
        {
            echoPacket = DataPacket.Create(16);
            echoPacket.Command = (ushort)CommandID.CA_PROTO_ECHO;
            echoPacket.DataType = 0;
            echoPacket.DataCount = 0;
            echoPacket.Parameter1 = 0;
            echoPacket.Parameter2 = 0;
        }

        public void Init(CAClient client, IPEndPoint dest)
        {
            this.Client = client;
            this.destination = dest;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);

            socket.Connect(dest);
            base.Start(socket);

            DataPacket p = DataPacket.Create(16);
            p.Command = (ushort)CommandID.CA_PROTO_VERSION;
            p.DataType = 1;
            p.DataCount = (uint)CAConstants.CA_MINOR_PROTOCOL_REVISION;
            p.Parameter1 = 0;
            p.Parameter2 = 0;
            Send(p);

            p = DataPacket.Create(16 + this.Client.Configuration.Hostname.Length + TypeHandling.Padding(this.Client.Configuration.Hostname.Length));
            p.Command = (ushort)CommandID.CA_PROTO_HOST_NAME;
            p.DataCount = 0;
            p.DataType = 0;
            p.Parameter1 = 0;
            p.Parameter2 = 0;
            p.SetDataAsString(this.Client.Configuration.Hostname);
            Send(p);

            p = DataPacket.Create(16 + this.Client.Configuration.Username.Length + TypeHandling.Padding(this.Client.Configuration.Username.Length));
            p.Command = (ushort)CommandID.CA_PROTO_CLIENT_NAME;
            p.DataCount = 0;
            p.DataType = 0;
            p.Parameter1 = 0;
            p.Parameter2 = 0;
            p.SetDataAsString(this.Client.Configuration.Username);
            Send(p);
        }


        internal void AddChannel(Channel channel)
        {
            lock (ConnectedChannels)
            {
                if (!ConnectedChannels.Contains(channel))
                    ConnectedChannels.Add(channel);
            }
        }

        internal void RemoveChannel(Channel channel)
        {
            lock (ConnectedChannels)
            {
                ConnectedChannels.Remove(channel);
            }

            lock (Client.Channels)
            {
                if (!Client.Channels.Any(row => row.Value.ChannelName == channel.ChannelName))
                    ChannelSID.Remove(channel.ChannelName);
            }
        }

        public override void Dispose()
        {
            if (disposed)
                return;
            lock (Client.Iocs)
                Client.Iocs.Remove(destination);
            List<Channel> toDisconnect;
            lock (ConnectedChannels)
            {
                toDisconnect = ConnectedChannels.ToList();
            }
            foreach (Channel channel in toDisconnect)
                channel.Disconnect();

            base.Dispose();
        }

        internal void Echo()
        {
            Pipe.GeneratedEcho = true;
            Send(echoPacket);
        }
    }
}
