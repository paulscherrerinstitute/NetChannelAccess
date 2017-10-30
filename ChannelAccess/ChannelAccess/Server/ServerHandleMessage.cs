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
using EpicsSharp.ChannelAccess.Server.ChannelTypes;
using EpicsSharp.ChannelAccess.Server.RecordTypes;

namespace EpicsSharp.ChannelAccess.Server
{
    internal class ServerHandleMessage : DataFilter
    {
        public CAServer Server { get; set; }


        static object lockObject = new object();
        public override void ProcessData(DataPacket packet)
        {
            if (DateTime.Now < Server.WaitTill)
                return;
            //Console.WriteLine("Pipe " + (Pipe.FirstFilter is UdpReceiver ? "UDP" : "TCP") + ": " + ((CommandID)packet.Command));
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
                        // Send back the echo
                        else
                            ((ServerTcpReceiver)Pipe[0]).Send(packet);
                        break;
                    case CommandID.CA_PROTO_SEARCH:
                        {
                            var channelName = packet.GetDataAsString(0).Split('.').First();
                            if (!Server.Records.Contains(channelName))
                                break;
                            DataPacket response = DataPacket.Create(8 + 16);
                            response.Command = (ushort)CommandID.CA_PROTO_SEARCH;
                            response.DataType = (ushort)Server.TcpPort;
                            response.DataCount = 0;
                            response.Parameter1 = 0xffffffff;
                            response.Parameter2 = packet.Parameter1;
                            response.SetUInt16(16, (ushort)CAConstants.CA_MINOR_PROTOCOL_REVISION);
                            response.Destination = packet.Sender;
                            ((UdpReceiver)this.Pipe[0]).Send(response);
                        }
                        break;
                    case CommandID.CA_PROTO_CREATE_CHAN:
                        {
                            var fullChannelName = packet.GetDataAsString(0);
                            var channelName = fullChannelName;
                            var property = "VAL";
                            if (fullChannelName.IndexOf('.') != -1)
                            {
                                property = fullChannelName.Split('.').Last();
                                channelName = fullChannelName.Split('.').First();
                            }
                            if (!Server.Records.Contains(channelName))
                                break;

                            DataPacket access = DataPacket.Create(16);
                            access.Command = (ushort)CommandID.CA_PROTO_ACCESS_RIGHTS;
                            access.Parameter1 = packet.Parameter1;
                            access.Parameter2 = (int)AccessRights.ReadAndWrite;
                            ((ServerTcpReceiver)this.Pipe.FirstFilter).Send(access);

                            DataPacket response = DataPacket.Create(16);
                            response.Command = (ushort)CommandID.CA_PROTO_CREATE_CHAN;
                            response.DataType = (ushort)Server.Records[channelName].FindType(property);
                            response.DataCount = (uint)(property == "VAL" ? Server.Records[channelName].dataCount : 1);
                            response.Parameter1 = packet.Parameter1;
                            response.Parameter2 = ((ServerTcpReceiver)this.Pipe.FirstFilter).RegisterChannel(channelName + "." + property);
                            ((ServerTcpReceiver)this.Pipe.FirstFilter).Send(response);
                        }
                        break;
                    case CommandID.CA_PROTO_READ_NOTIFY:
                        {
                            var record = ((ServerTcpReceiver)this.Pipe.FirstFilter).FindRecord(this.Server, packet.Parameter1);
                            DataPacket response = DataPacketBuilder.Encode((EpicsType)packet.DataType, ((ServerTcpReceiver)this.Pipe.FirstFilter).RecordValue(this.Server, packet.Parameter1),
                                record, ((int)packet.DataCount == 0 ? record.dataCount : (int)packet.DataCount));
                            response.Command = (ushort)CommandID.CA_PROTO_READ_NOTIFY;
                            response.Parameter1 = 1;
                            response.Parameter2 = packet.Parameter2;
                            ((TcpReceiver)this.Pipe.FirstFilter).Send(response);
                        }
                        break;
                    case CommandID.CA_PROTO_WRITE:
                        {
                            ((ServerTcpReceiver)this.Pipe.FirstFilter).PutValue(this.Server, packet);
                        }
                        break;
                    case CommandID.CA_PROTO_WRITE_NOTIFY:
                        {
                            ((ServerTcpReceiver)this.Pipe.FirstFilter).PutValue(this.Server, packet);
                            DataPacket response = DataPacket.Create(16);
                            response.Command = (ushort)CommandID.CA_PROTO_WRITE_NOTIFY;
                            response.DataType = packet.DataType;
                            response.DataCount = packet.DataCount;
                            response.Parameter1 = 1;
                            response.Parameter2 = packet.Parameter2;
                            ((ServerTcpReceiver)this.Pipe.FirstFilter).Send(response);
                        }
                        break;
                    case CommandID.CA_PROTO_EVENT_ADD:
                        {

                            uint sid = packet.Parameter1;
                            uint subscriptionId = packet.Parameter2;
                            uint dataCount = packet.DataCount;
                            EpicsType type = (EpicsType)packet.DataType;

                            ((ServerTcpReceiver)this.Pipe.FirstFilter).RegisterEvent(this.Server, sid, subscriptionId, (int)dataCount, type, (MonitorMask)packet.GetUInt16((int)packet.HeaderSize + 12));
                        }
                        break;
                    case CommandID.CA_PROTO_EVENT_CANCEL:
                        {
                            ((ServerTcpReceiver)this.Pipe.FirstFilter).UnregisterEvent(this.Server, packet.Parameter2);
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
