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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using EpicsSharp.ChannelAccess.Constants;
using EpicsSharp.Common.Pipes;
using EpicsSharp.ChannelAccess.Common;

namespace EpicsSharp.ChannelAccess.Client
{
    class ClientHandleMessage : DataFilter
    {
        public CAClient Client { get; set; }


        static object lockObject = new object();
        public override void ProcessData(DataPacket packet)
        {
            lock (lockObject)
            {
                switch ((CommandID)packet.Command)
                {
                    case CommandID.CA_PROTO_VERSION:
                        break;
                    case CommandID.CA_PROTO_ECHO:
                        // We sent the echo... we should therefore avoid to answer it
                        if (Pipe.GeneratedEcho)
                            Pipe.GeneratedEcho = false;
                        else
                        {
                            // Send back the echo
                            ((ClientTcpReceiver)Pipe[0]).Send(packet);
                        }
                        break;
                    case CommandID.CA_PROTO_SEARCH:
                        {
                            int port = packet.DataType;
                            //Console.WriteLine("Answer from: " + packet.Sender.Address + ":" + port);
                            Channel channel = Client.GetChannelByCid(packet.Parameter2);
                            if (channel == null)
                                return;
                            IPAddress addr = packet.Sender.Address;
                            if (packet.Parameter1 != 0xFFFFFFFF)
                                addr = IPAddress.Parse("" + packet.Data[8] + "." + packet.Data[8 + 1] + "." + packet.Data[8 + 2] + "." + packet.Data[8 + 3]);
                            channel.SearchAnswerFrom = packet.Sender.Address;
                            if (Client.Configuration.DebugTiming)
                            {
                                lock (channel.ElapsedTimings)
                                {
                                    if (!channel.ElapsedTimings.ContainsKey("SearchResponse"))
                                        channel.ElapsedTimings.Add("SearchResponse", channel.Stopwatch.Elapsed);
                                }
                            }
                            if (channel != null)
                            {
                                try
                                {
                                    channel.SetIoc(Client.GetIocConnection(new IPEndPoint(addr, port)));
                                }
                                catch (Exception ex)
                                {
                                }
                            }
                        }
                        break;
                    case CommandID.CA_PROTO_ACCESS_RIGHTS:
                        {
                            Channel channel = Client.GetChannelByCid(packet.Parameter1);
                            if (channel != null)
                                channel.AccessRight = (AccessRights)((ushort)packet.Parameter2);
                            break;
                        }
                    case CommandID.CA_PROTO_CREATE_CHAN:
                        {
                            //Console.WriteLine("Received create channel");
                            Channel channel = Client.GetChannelByCid(packet.Parameter1);
                            if (channel != null)
                                channel.SetServerChannel(packet.Parameter2, (EpicsType)packet.DataType, packet.DataCount);
                            break;
                        }
                    case CommandID.CA_PROTO_READ_NOTIFY:
                        {
                            ClientTcpReceiver ioc = (ClientTcpReceiver)Pipe[0];
                            Channel channel;
                            lock (ioc.PendingIo)
                            {
                                channel = ioc.PendingIo[packet.Parameter2];
                            }
                            if (channel != null)
                                channel.SetGetRawValue(packet);
                            break;
                        }
                    case CommandID.CA_PROTO_WRITE_NOTIFY:
                        {
                            ClientTcpReceiver ioc = (ClientTcpReceiver)Pipe[0];
                            Channel channel;
                            lock (ioc.PendingIo)
                            {
                                channel = ioc.PendingIo[packet.Parameter2];
                            }
                            if (channel != null)
                                channel.SetWriteNotify();
                            break;
                        }
                    case CommandID.CA_PROTO_EVENT_ADD:
                        {
                            Channel channel = Client.GetChannelByCid(packet.Parameter2);
                            if (channel != null)
                                channel.UpdateMonitor(packet);
                            break;
                        }
                    case CommandID.CA_PROTO_SERVER_DISCONN:
                        {
                            List<Channel> connectedChannels;
                            ClientTcpReceiver receiver = ((ClientTcpReceiver)Pipe[0]);
                            lock (receiver.ConnectedChannels)
                            {
                                connectedChannels = receiver.ConnectedChannels.Where(row => row.CID == packet.Parameter1).ToList();
                            }
                            foreach (Channel channel in connectedChannels)
                            {
                                lock (receiver.ChannelSID)
                                {
                                    receiver.ChannelSID.Remove(channel.ChannelName);
                                }

                                channel.Disconnect();
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
