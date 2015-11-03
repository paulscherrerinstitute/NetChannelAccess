using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using CaSharpServer.Constants;

namespace CaSharpServer
{
    /// <summary>
    /// Message filter which decodes requests as well as create some pre-formated answers.
    /// </summary>
    internal class CAServerFilter
    {
        CAServer Server;

        internal CAServerFilter(CAServer server)
        {
            this.Server = server;
        }

        public void ProcessReceivedData(Pipe dataPipe, EndPoint remoteEndPoint, int maxPacketSize = 0, bool wait = true)
        {
            string remoteAddress = remoteEndPoint.ToString();
            uint readedBytes = 0;
            UInt16 cmdId, dataType;
            UInt32 payloadSize, dataCount, param1, param2;
            byte[] payload = null;
            byte[] header = null;

            while (maxPacketSize == 0 || maxPacketSize >= (readedBytes + 16))
            {
                if (!wait && dataPipe.AvailableBytes == 0)
                    return;
                header = dataPipe.Read(16);

                //Pipe destroyed
                if (header.Length == 0)
                    return;

                cmdId = header.ToUInt16(0);
                payloadSize = header.ToUInt16(2);
                dataType = header.ToUInt16(4);
                param1 = header.ToUInt32(8);
                param2 = header.ToUInt32(12);

                if (payloadSize == 0xFFFF)
                {
                    payloadSize = dataPipe.Read(4).ToUInt32();
                    dataCount = dataPipe.Read(4).ToUInt32();
                    readedBytes += (payloadSize + 24);
                }
                else
                {
                    dataCount = header.ToUInt16(6);
                    readedBytes += (payloadSize + 16);
                }

                payload = dataPipe.Read(Convert.ToInt32(payloadSize));

                HandleMessage(cmdId, dataType, ref payloadSize, ref dataCount,
                              ref param1, ref param2, ref header, ref payload, ref remoteEndPoint);
            }
        }

        internal byte[] channelDisconnectionMessage(int clientId)
        {
            byte[] msg = new byte[16] { 0, 27, 0, 0, 0, 0, 0, 0,
                                        0,  0, 0, 0, 0, 0, 0, 0 };
            Buffer.BlockCopy(clientId.ToByteArray(), 0, msg, 8, 4);
            return msg;
        }

        public void HandleMessage(ushort CommandId, ushort DataType, ref uint PayloadSize, ref uint DataCount, ref uint Parameter1, ref uint Parameter2, ref byte[] header, ref byte[] payload, ref System.Net.EndPoint iep)
        {
            switch ((CommandID)CommandId)
            {
                #region Creation of Connection and Channels
                case CommandID.CA_PROTO_VERSION:
                    break;
                case CommandID.CA_PROTO_SEARCH:
                    {
                        string channelName = payload.ToCAString();
                        if (channelName.Contains("."))
                            channelName = channelName.Split('.')[0];

                        if (Server.Records.Contains(channelName))
                        {
                            Server.UdpConnection.Send(ChannelFoundMessage(Parameter1), (IPEndPoint)iep);
                        }
                    }
                    break;
                case CommandID.CA_PROTO_CLIENT_NAME:
                    lock (Server.openConnection)
                    {
                        if (Server.openConnection.ContainsKey(iep.ToString()))
                            Server.openConnection[iep.ToString()].Username = payload.ToCAString();
                    }
                    break;
                case CommandID.CA_PROTO_HOST_NAME:
                    lock (Server.openConnection)
                    {
                        if (Server.openConnection.ContainsKey(iep.ToString()))
                            Server.openConnection[iep.ToString()].Hostname = payload.ToCAString();
                    }
                    break;
                case CommandID.CA_PROTO_CREATE_CHAN:
                    lock (Server.channelList)
                    {
                        Server.CreateEpicsChannel((int)Parameter1, iep, payload.ToCAString());
                    }
                    break;
                case CommandID.CA_PROTO_CLEAR_CHANNEL:
                    lock (Server.channelList)
                    {
                        if (Server.channelList.ContainsKey((int)Parameter1))
                            Server.channelList[(int)Parameter1].Dispose();
                    }
                    break;
                #endregion

                #region Monitor
                case CommandID.CA_PROTO_EVENT_ADD:
                    int mask = payload.ToUInt16(12);
                    lock (Server.channelList)
                    {
                        if (Server.channelList.ContainsKey((int)Parameter1))
                            Server.channelList[(int)Parameter1].AddMonitor((EpicsType)DataType, (int)DataCount, (int)Parameter2, (MonitorMask)mask);
                    }
                    break;
                case CommandID.CA_PROTO_EVENT_CANCEL:
                    lock (Server.channelList)
                    {
                        if (Server.channelList.ContainsKey((int)Parameter1))
                            Server.channelList[(int)Parameter1].RemoveMonitor((int)Parameter2);
                    }
                    break;
                case CommandID.CA_PROTO_EVENTS_OFF:

                    break;
                case CommandID.CA_PROTO_EVENTS_ON:

                    break;
                #endregion

                #region Read&Write
                case CommandID.CA_PROTO_READ:
                case CommandID.CA_PROTO_READ_NOTIFY:
                    lock (Server.channelList)                    
                    {
                        if (Server.channelList.ContainsKey((int)Parameter1))
                            Server.channelList[(int)Parameter1].ReadValue((int)Parameter2, (EpicsType)DataType, (int)DataCount);
                    }
                    break;
                case CommandID.CA_PROTO_WRITE:
                    lock (Server.channelList)
                    {
                        if (Server.channelList.ContainsKey((int)Parameter1))
                            Server.channelList[(int)Parameter1].PutValue((int)Parameter2, (EpicsType)DataType, (int)DataCount, payload, false);
                    }
                    break;
                case CommandID.CA_PROTO_WRITE_NOTIFY:
                    lock (Server.channelList)
                    {
                        if (Server.channelList.ContainsKey((int)Parameter1))
                            Server.channelList[(int)Parameter1].PutValue((int)Parameter2, (EpicsType)DataType, (int)DataCount, payload, true);
                    }
                    break;
                #endregion
                case CommandID.CA_PROTO_ECHO:
                    lock (Server.openConnection)
                    {
                        if (Server.openConnection.ContainsKey(iep.ToString()))
                        {
                            var con = Server.openConnection[iep.ToString()];
                            if ((DateTime.Now - con.EchoLastSent).TotalSeconds > 5)
                            {
                                con.Send(EchoMessage);
                                con.EchoLastSent = DateTime.Now;
                            }
                        }
                    }
                    break;
            }
        }

        private byte[] ChannelFoundMessage(uint clientId)
        {
            MemoryStream mem = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(mem);

            mem.Capacity = 40;

            writer.Write(VersionMessage);
            writer.Write(((UInt16)CommandID.CA_PROTO_SEARCH).ToByteArray());
            writer.Write(((UInt16)8).ToByteArray());
            writer.Write(((UInt16)Server.TcpPort).ToByteArray());
            writer.Write(new byte[2]);
            writer.Write(((UInt32)0xFFFFFFFF).ToByteArray());
            writer.Write(((UInt32)clientId).ToByteArray());
            writer.Write(((UInt16)CAConstants.CA_MINOR_PROTOCOL_REVISION).ToByteArray());

            byte[] buffer = mem.GetBuffer();
            writer.Close();
            mem.Dispose();

            return buffer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId">IMPORTANT IT's not sure yet that this has to be the cliendId could also be the ioId</param>
        /// <param name=IOID>IMPORTANT IT's not sure yet that this has to be the ioId could also be the cliendId</param>
        /// <param name="dataType"></param>
        /// <param name="dataCount"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public byte[] ChannelReadMessage(int clientId, int ioId, EpicsType dataType, int dataCount, byte[] data)
        {
            MemoryStream mem = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(mem);

            int padding = 8;
            /*if (dataCount > 60000)
                padding = 0;*/

            if (data.Length % 8 != 0)
                padding = (padding - (data.Length % 8));

            mem.Capacity = 16 + data.Length + padding;

            writer.Write(((UInt16)CommandID.CA_PROTO_READ_NOTIFY).ToByteArray());
            if (dataCount > 30000)
                writer.Write(new byte[] { 0xFF, 0xFF });
            else
                writer.Write(((UInt16)(data.Length + padding)).ToByteArray());
            writer.Write(((UInt16)dataType).ToByteArray());
            if (dataCount > 30000)
                writer.Write(new byte[] { 0x00, 0x00 });
            else
                writer.Write(((UInt16)dataCount).ToByteArray());
            // Seems again an issue with the specifications where only a value of 1 works.
            //writer.Write(((UInt32)clientId).ToByteArray());
            writer.Write(((UInt32)1).ToByteArray());
            writer.Write(((UInt32)ioId).ToByteArray());
            if (dataCount > 30000)
            {
                writer.Write(((UInt32)(data.Length + padding)).ToByteArray());
                byte[] b = ((UInt32)(dataCount)).ToByteArray();
                writer.Write(b);
            }

            writer.Write(data);
            if(padding > 0)
                writer.Write(new byte[padding]);

            byte[] buffer = mem.ToArray();
            writer.Close();
            mem.Dispose();

            return buffer;
        }

        public byte[] ErrorMessage(int clientId, EpicsTransitionStatus status, string errorMessage, byte[] header)
        {
            MemoryStream mem = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(mem);

            mem.Capacity = 16 + 16 + errorMessage.Length + 1;

            writer.Write(((UInt16)CommandID.CA_PROTO_ERROR).ToByteArray());
            writer.Write((errorMessage.Length + 16).ToByteArray());
            writer.Write(new byte[4]);
            writer.Write(((UInt32)clientId).ToByteArray());
            writer.Write(((UInt32)status).ToByteArray());

            //emptyheader
            writer.Write(header);
            writer.Write(errorMessage.ToByteArray());

            byte[] buffer = mem.ToArray();
            writer.Close();
            mem.Dispose();

            return buffer;
        }

        public byte[] ChannelCreatedMessage(int clientId, int serverId, EpicsType dataType, int dataCount, AccessRights access)
        {
            MemoryStream mem = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(mem);

            mem.Capacity = 32;

            writer.Write(((UInt16)CommandID.CA_PROTO_ACCESS_RIGHTS).ToByteArray());
            writer.Write(new byte[6]);
            writer.Write(((UInt32)clientId).ToByteArray());
            writer.Write(((UInt32)access).ToByteArray());

            writer.Write(((UInt16)CommandID.CA_PROTO_CREATE_CHAN).ToByteArray());
            if (dataCount > 30000)
                writer.Write(new byte[] { 0xFF, 0xFF });
            else
                writer.Write(new byte[2]);
            writer.Write(((UInt16)dataType).ToByteArray());
            if (dataCount > 30000)
                writer.Write(new byte[] { 0x00, 0x00 });
            else
                writer.Write(((UInt16)dataCount).ToByteArray());
            writer.Write(((UInt32)clientId).ToByteArray());
            writer.Write(((UInt32)serverId).ToByteArray());
            if (dataCount > 30000)
            {
                // Size
                writer.Write(((UInt32)0).ToByteArray());
                // Data count
                writer.Write(((UInt32)dataCount).ToByteArray());
            }

            byte[] buffer = mem.ToArray();
            writer.Close();
            mem.Dispose();

            return buffer;
        }

        internal byte[] MonitorChangeMessage(int subscriptionId, int clientId, EpicsType dataType, int dataCount, byte[] data)
        {
            MemoryStream mem = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(mem);

            int padding = 0;

            if (data.Length % 8 == 0)
                padding = 8;
            else
                padding = (8 - (data.Length % 8));

            mem.Capacity = 16 + data.Length + padding;

            writer.Write(((ushort)CommandID.CA_PROTO_EVENT_ADD).ToByteArray());
            if (dataCount > 30000)
                writer.Write(new byte[] { 0xFF, 0xFF });
            else
                writer.Write(((UInt16)(data.Length + padding)).ToByteArray());
            writer.Write(((UInt16)dataType).ToByteArray());
            if (dataCount > 30000)
                writer.Write(new byte[] { 0x00, 0x00 });
            else
                writer.Write(((UInt16)dataCount).ToByteArray());
            //writer.Write(NetworkByteConverter.ToByteArray((UInt32)clientId)); // Christoph implementation
            // Cosylab would send 0 which doesn't work
            writer.Write(((UInt32)1).ToByteArray()); // Value found to work... no idea why
            writer.Write(((UInt32)subscriptionId).ToByteArray());
            if (dataCount > 30000)
            {
                writer.Write(((UInt32)(data.Length + padding)).ToByteArray());
                writer.Write(((UInt32)dataCount).ToByteArray());
            }

            writer.Write(data);
            writer.Write(new byte[padding]);

            byte[] buffer = mem.GetBuffer();
            writer.Close();
            mem.Dispose();

            return buffer;
        }

        internal byte[] MonitorCloseMessage(EpicsType dataType, int serverId, int subscriptionId)
        {
            MemoryStream mem = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(mem);

            mem.Capacity = 16;

            writer.Write(((ushort)CommandID.CA_PROTO_EVENT_CANCEL).ToByteArray());
            writer.Write(new byte[2]);
            writer.Write(((UInt16)dataType).ToByteArray());
            writer.Write(new byte[2]);
            writer.Write(((UInt32)serverId).ToByteArray());
            writer.Write(((UInt32)subscriptionId).ToByteArray());

            byte[] buffer = mem.GetBuffer();
            writer.Close();
            mem.Dispose();

            return buffer;
        }

        byte[] versionMessage = null;
        byte[] VersionMessage
        {
            get
            {
                if (versionMessage == null)
                {
                    versionMessage = new byte[16] { 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0 };
                    Buffer.BlockCopy(((ushort)CAConstants.CA_MINOR_PROTOCOL_REVISION).ToByteArray(), 0,
                                     versionMessage, 6, 2);
                }
                return versionMessage;
            }
        }

        internal static byte[] EchoMessage = new byte[16] { 0, 23, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        internal byte[] ChannelWroteMessage(int clientId, int ioId, EpicsType dataType, int dataCount, EpicsTransitionStatus status)
        {
            MemoryStream mem = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(mem);

            mem.Capacity = 16;

            writer.Write(((ushort)CommandID.CA_PROTO_WRITE_NOTIFY).ToByteArray());
            writer.Write(new byte[2]);
            writer.Write(((UInt16)dataType).ToByteArray());
            writer.Write(((UInt16)dataCount).ToByteArray());
            writer.Write(((UInt32)status).ToByteArray());
            writer.Write(((UInt32)ioId).ToByteArray());

            byte[] buffer = mem.GetBuffer();
            writer.Close();
            mem.Dispose();

            return buffer;
        }
    }
}
