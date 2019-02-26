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
using EpicsSharp.ChannelAccess.Common;
using EpicsSharp.ChannelAccess.Constants;
using EpicsSharp.ChannelAccess.Server.ChannelTypes;
using EpicsSharp.ChannelAccess.Server.RecordTypes;
using EpicsSharp.Common.Pipes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EpicsSharp.ChannelAccess.Server
{
    internal class ServerTcpReceiver : TcpReceiver, IDisposable
    {
        uint nextSid = 1;
        object locker = new object();
        Dictionary<string, uint> channelIds = new Dictionary<string, uint>();
        Dictionary<uint, EpicsEvent> subscribedEvents = new Dictionary<uint, EpicsEvent>();

        public void Init(Socket socket)
        {
            RemoteEndPoint = (IPEndPoint)socket.RemoteEndPoint;
            base.Start(socket);
        }

        public uint RegisterChannel(string channelName)
        {
            lock (locker)
            {
                if (channelIds.ContainsKey(channelName))
                    return channelIds[channelName];
                uint sid = nextSid++;
                channelIds.Add(channelName, sid);
                return sid;
            }
        }

        public string FindProperty(CAServer server, uint sid)
        {
            string channelName = null;
            lock (locker)
            {
                channelName = channelIds.Where(row => row.Value == sid).Select(row => row.Key).First();
            }
            string property = "VAL";
            if (channelName.IndexOf('.') != -1)
                property = channelName.Split('.').Last();
            return property;
        }

        public CARecord FindRecord(CAServer server, uint sid)
        {
            string channelName = null;
            lock (locker)
            {
                channelName = channelIds.Where(row => row.Value == sid).Select(row => row.Key).First();
            }
            string property = "VAL";
            if (channelName.IndexOf('.') != -1)
            {
                property = channelName.Split('.').Last();
                channelName = channelName.Split('.').First();
            }

            return server.Records[channelName];
        }

        internal object RecordValue(CAServer server, uint sid)
        {
            string channelName = null;
            lock (locker)
            {
                channelName = channelIds.Where(row => row.Value == sid).Select(row => row.Key).First();
            }
            string property = "VAL";
            if (channelName.IndexOf('.') != -1)
            {
                property = channelName.Split('.').Last();
                channelName = channelName.Split('.').First();
            }

            //Console.WriteLine("Property "+ property);
            return server.Records[channelName][property];
        }

        internal void RegisterEvent(CAServer server, uint sid, uint subscriptionId, int nbElements, EpicsType type, MonitorMask mask)
        {

            CARecord record = ((ServerTcpReceiver)this.Pipe.FirstFilter).FindRecord(server, sid);
            string property = ((ServerTcpReceiver)this.Pipe.FirstFilter).FindProperty(server, sid);

            DataPacket response = DataPacketBuilder.Encode(type, record[property], record, (nbElements == 0) ? record.ElementsInRecord : Math.Min(nbElements, record.ElementsInRecord));
            response.Command = (ushort)CommandID.CA_PROTO_EVENT_ADD;
            response.Parameter1 = 1;
            response.Parameter2 = (uint)subscriptionId;
            ((TcpReceiver)this.Pipe.FirstFilter).Send(response);

            var lastStatus = record.AlarmStatus;
            EventHandler newEvent = delegate (object obj, EventArgs evt)
            {
                if (!record.IsDirty)
                    return;
                // We have a alarm mask, if the status didn't change, don't send the event
                if (mask == MonitorMask.ALARM && lastStatus == record.AlarmStatus)
                    return;
                lastStatus = record.AlarmStatus;
                DataPacket p = DataPacketBuilder.Encode(type, record[property], record, (nbElements == 0) ? record.ElementsInRecord : Math.Min(nbElements, record.ElementsInRecord));
                p.Command = (ushort)CommandID.CA_PROTO_EVENT_ADD;
                p.Parameter1 = 1;
                p.Parameter2 = (uint)subscriptionId;
                ((TcpReceiver)this.Pipe.FirstFilter).Send(p);
            };
            record.RecordProcessed += newEvent;

            lock (subscribedEvents)
            {
                subscribedEvents.Add(subscriptionId, new EpicsEvent { Handler = newEvent, Record = record, DataCount = nbElements, EpicsType = type, SID = sid });
            }
        }

        internal void UnregisterEvent(CAServer server, uint subscriptionId)
        {
            EpicsEvent oldEvent = null;
            lock (subscribedEvents)
            {
                oldEvent = subscribedEvents[subscriptionId];
            }
            oldEvent.Record.RecordProcessed -= oldEvent.Handler;

            DataPacket response = DataPacket.Create(16);
            response.Command = (ushort)CommandID.CA_PROTO_EVENT_CANCEL;
            response.DataType = (ushort)oldEvent.EpicsType;
            response.DataCount = (uint)oldEvent.DataCount;
            response.Parameter1 = oldEvent.SID;
            response.Parameter2 = (uint)subscriptionId;
            ((TcpReceiver)this.Pipe.FirstFilter).Send(response);
        }

        internal void PutValue(CAServer server, DataPacket packet)
        {
            var record = ((ServerTcpReceiver)this.Pipe.FirstFilter).FindRecord(server, packet.Parameter1);
            var property = ((ServerTcpReceiver)this.Pipe.FirstFilter).FindProperty(server, packet.Parameter1);
            record[property] = MapPacketValueToRecordType(packet, record, property);
        }

        private object MapPacketValueToRecordType(DataPacket packet, CARecord record, string property)
        {
            object obj = ExtractValueFromPacket(packet);
            Type destType = record.GetPropertyType(property);
            string destTypeName = destType.ToString();
            if (destType.IsGenericType)
                destTypeName = destType.GenericTypeArguments[0].ToString();
            switch (destTypeName)
            {
                case "System.String":
                    return Convert.ToString(obj, System.Globalization.CultureInfo.InvariantCulture);

                case "System.Int32":
                    if (packet.DataCount == 1 && !destType.IsGenericType && !destType.IsArray)
                    {
                        return Convert.ToInt32(obj);
                    }
                    else
                    {
                        dynamic d = obj;
                        int[] t = new int[packet.DataCount];
                        for (var i = 0; i < packet.DataCount; i++)
                            t[i] = Convert.ToInt32(d[i]);
                        return t;
                    }

                case "System.Byte":
                    if (packet.DataCount == 1 && !destType.IsGenericType && !destType.IsArray)
                    {
                        return Convert.ToByte(obj);
                    }
                    else
                    {
                        dynamic d = obj;
                        byte[] t = new byte[packet.DataCount];
                        for (var i = 0; i < packet.DataCount; i++)
                            t[i] = Convert.ToByte(d[i]);
                        return t;
                    }

                case "System.Int16":
                    if (packet.DataCount == 1 && !destType.IsGenericType && !destType.IsArray)
                    {
                        return Convert.ToInt16(obj);
                    }
                    else
                    {
                        dynamic d = obj;
                        short[] t = new short[packet.DataCount];
                        for (var i = 0; i < packet.DataCount; i++)
                            t[i] = Convert.ToInt16(d[i]);
                        return t;
                    }

                case "System.Single":
                    if (packet.DataCount == 1 && !destType.IsGenericType && !destType.IsArray)
                    {
                        return Convert.ToSingle(obj);
                    }
                    else
                    {
                        dynamic d = obj;
                        float[] t = new float[packet.DataCount];
                        for (var i = 0; i < packet.DataCount; i++)
                            t[i] = Convert.ToSingle(d[i]);
                        return t;
                    }

                case "System.Double":
                    if (packet.DataCount == 1 && !destType.IsGenericType && !destType.IsArray)
                    {
                        return Convert.ToDouble(obj);
                    }
                    else
                    {
                        dynamic d = obj;
                        double[] t = new double[packet.DataCount];
                        for (var i = 0; i < packet.DataCount; i++)
                            t[i] = Convert.ToDouble(d[i]);
                        return t;
                    }
            }
            throw new Exception($"Cannot map packet type {packet.DataType} to {destTypeName}");
        }

        private object ExtractValueFromPacket(DataPacket packet)
        {
            switch ((EpicsType)packet.DataType)
            {
                case EpicsType.Byte:
                    if (packet.DataCount == 1)
                        return packet.GetByte((int)packet.HeaderSize);
                    else
                    {
                        byte[] t = new byte[packet.DataCount];
                        for (var i = 0; i < packet.DataCount; i++)
                            t[i] = packet.GetByte((int)packet.HeaderSize + i);
                        return t;
                    }

                case EpicsType.Int:
                    if (packet.DataCount == 1)
                        return packet.GetInt32((int)packet.HeaderSize);
                    else
                    {
                        int[] t = new int[packet.DataCount];
                        for (var i = 0; i < packet.DataCount; i++)
                            t[i] = packet.GetInt32((int)packet.HeaderSize + i * 4);
                        return t;
                    }

                case EpicsType.Short:
                    if (packet.DataCount == 1)
                        return packet.GetInt16((int)packet.HeaderSize);
                    else
                    {
                        short[] t = new short[packet.DataCount];
                        for (var i = 0; i < packet.DataCount; i++)
                            t[i] = packet.GetInt16((int)packet.HeaderSize + i * 2);
                        return t;
                    }

                case EpicsType.Float:
                    if (packet.DataCount == 1)
                        return packet.GetFloat((int)packet.HeaderSize);
                    else
                    {
                        float[] t = new float[packet.DataCount];
                        for (var i = 0; i < packet.DataCount; i++)
                            t[i] = packet.GetFloat((int)packet.HeaderSize + i * 4);
                        return t;
                    }

                case EpicsType.Double:
                    if (packet.DataCount == 1)
                        return packet.GetDouble((int)packet.HeaderSize);
                    else
                    {
                        double[] t = new double[packet.DataCount];
                        for (var i = 0; i < packet.DataCount; i++)
                            t[i] = packet.GetDouble((int)packet.HeaderSize + i * 8);
                        return t;
                    }

                case EpicsType.String:
                    return packet.GetDataAsString(0);
            }
            throw new Exception($"Data type {packet.DataType} not supported yet");
        }

        public override void Dispose()
        {
            ((ServerHandleMessage)this.Pipe.LastFilter).Server.DisposeClient(this.Pipe);
            base.Dispose();
        }
    }
}
