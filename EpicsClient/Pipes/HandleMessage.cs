using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace PSI.EpicsClient2.Pipes
{
    class HandleMessage : DataFilter
    {
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
                            ((TcpReceiver)Pipe[0]).Send(packet);
                        }
                        break;
                    case CommandID.CA_PROTO_SEARCH:
                        {
                            int port = packet.DataType;
                            //Console.WriteLine("Answer from: " + packet.Sender.Address + ":" + port);
                            EpicsChannel channel = Client.GetChannelByCid(packet.Parameter2);
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
                                catch
                                {
                                }
                            }
                        }
                        break;
                    case CommandID.CA_PROTO_ACCESS_RIGHTS:
                        {
                            EpicsChannel channel = Client.GetChannelByCid(packet.Parameter1);
                            if (channel != null)
                                channel.AccessRight = (AccessRights)((ushort)packet.Parameter2);
                            break;
                        }
                    case CommandID.CA_PROTO_CREATE_CHAN:
                        {
                            EpicsChannel channel = Client.GetChannelByCid(packet.Parameter1);
                            if (channel != null)
                                channel.SetServerChannel(packet.Parameter2, (EpicsType)packet.DataType, packet.DataCount);
                            break;
                        }
                    case CommandID.CA_PROTO_READ_NOTIFY:
                        {
                            TcpReceiver ioc = (TcpReceiver)Pipe[0];
                            EpicsChannel channel;
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
                            TcpReceiver ioc = (TcpReceiver)Pipe[0];
                            EpicsChannel channel;
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
                            EpicsChannel channel = Client.GetChannelByCid(packet.Parameter2);
                            if (channel != null)
                                channel.UpdateMonitor(packet);
                            break;
                        }
                    case CommandID.CA_PROTO_SERVER_DISCONN:
                        {
                            List<EpicsChannel> connectedChannels;
                            TcpReceiver receiver = ((TcpReceiver)Pipe[0]);
                            lock (receiver.ConnectedChannels)
                            {
                                connectedChannels = receiver.ConnectedChannels.Where(row => row.CID == packet.Parameter1).ToList();
                            }
                            foreach (EpicsChannel channel in connectedChannels)
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
