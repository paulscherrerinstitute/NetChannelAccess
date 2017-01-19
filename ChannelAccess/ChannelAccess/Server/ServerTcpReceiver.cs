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

            return server.Records[channelName][property];
        }

        internal void RegisterEvent(CAServer server, uint sid, uint subscriptionId, int dataCount, Constants.EpicsType type, MonitorMask mask)
        {

            CARecord record = ((ServerTcpReceiver)this.Pipe.FirstFilter).FindRecord(server, sid);
            string property = ((ServerTcpReceiver)this.Pipe.FirstFilter).FindProperty(server, sid);

            DataPacket response = DataPacketBuilder.Encode(type, record[property], record, dataCount);
            response.Command = (ushort)CommandID.CA_PROTO_EVENT_ADD;
            response.Parameter1 = 1;
            response.Parameter2 = (uint)subscriptionId;
            ((TcpReceiver)this.Pipe.FirstFilter).Send(response);

            var lastStatus = record.AlarmStatus;
            EventHandler newEvent = delegate(object obj, EventArgs evt)
            {
                if (!record.IsDirty)
                    return;
                // We have a alarm mask, if the status didn't change, don't send the event
                if (mask == MonitorMask.ALARM && lastStatus == record.AlarmStatus)
                    return;
                lastStatus = record.AlarmStatus;
                DataPacket p = DataPacketBuilder.Encode(type, record[property], record, dataCount);
                p.Command = (ushort)CommandID.CA_PROTO_EVENT_ADD;
                p.Parameter1 = 1;
                p.Parameter2 = (uint)subscriptionId;
                ((TcpReceiver)this.Pipe.FirstFilter).Send(p);
            };
            record.RecordProcessed += newEvent;

            lock (subscribedEvents)
            {
                subscribedEvents.Add(subscriptionId, new EpicsEvent { Handler = newEvent, Record = record, DataCount = dataCount, EpicsType = type, SID = sid });
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
            object obj = null;
            switch ((EpicsType)packet.DataType)
            {
                case EpicsType.Byte:
                    {
                        if (packet.DataCount == 1)
                            obj = packet.GetByte((int)packet.HeaderSize);
                        else
                        {
                            byte[] t = new byte[packet.DataCount];
                            for (var i = 0; i < packet.DataCount; i++)
                                t[i] = packet.GetByte((int)packet.HeaderSize + i);
                            obj = t;
                        }
                    }
                    break;
                case EpicsType.Int:
                    {
                        if (packet.DataCount == 1)
                            obj = packet.GetInt32((int)packet.HeaderSize);
                        else
                        {
                            int[] t = new int[packet.DataCount];
                            for (var i = 0; i < packet.DataCount; i++)
                                t[i] = packet.GetInt32((int)packet.HeaderSize + i * 4);
                            obj = t;
                        }
                    }
                    break;
                case EpicsType.Short:
                    {
                        if (packet.DataCount == 1)
                            obj = packet.GetInt16((int)packet.HeaderSize);
                        else
                        {
                            short[] t = new short[packet.DataCount];
                            for (var i = 0; i < packet.DataCount; i++)
                                t[i] = packet.GetInt16((int)packet.HeaderSize + i * 2);
                            obj = t;
                        }
                    }
                    break;
                case EpicsType.Float:
                    {
                        if (packet.DataCount == 1)
                            obj = packet.GetFloat((int)packet.HeaderSize);
                        else
                        {
                            float[] t = new float[packet.DataCount];
                            for (var i = 0; i < packet.DataCount; i++)
                                t[i] = packet.GetFloat((int)packet.HeaderSize + i * 4);
                            obj = t;
                        }
                    }
                    break;
                case EpicsType.Double:
                    {
                        if (packet.DataCount == 1)
                            obj = packet.GetDouble((int)packet.HeaderSize);
                        else
                        {
                            double[] t = new double[packet.DataCount];
                            for (var i = 0; i < packet.DataCount; i++)
                                t[i] = packet.GetDouble((int)packet.HeaderSize + i * 8);
                            obj = t;
                        }
                    }
                    break;
                case EpicsType.String:
                    obj = packet.GetDataAsString(0);
                    break;
            }
            object dest = null;
            Type destType = record.GetPropertyType(property);
            string destTypeName = destType.ToString();
            if (destType.IsGenericType)
                destTypeName = destType.GenericTypeArguments[0].ToString();
            switch (destTypeName)
            {
                case "System.String":
                    dest = Convert.ToString(obj, System.Globalization.CultureInfo.InvariantCulture);
                    break;
                case "System.Int32":
                    {
                        if (packet.DataCount == 1 && !destType.IsGenericType && !destType.IsArray)
                        {
                            dest = Convert.ToInt32(obj);
                        }
                        else
                        {
                            dynamic d = obj;
                            int[] t = new int[packet.DataCount];
                            for (var i = 0; i < packet.DataCount; i++)
                                t[i] = Convert.ToInt32(d[i]);
                            dest = t;
                        }
                    }
                    break;
                case "System.Byte":
                    {
                        if (packet.DataCount == 1 && !destType.IsGenericType && !destType.IsArray)
                        {
                            dest = Convert.ToByte(obj);
                        }
                        else
                        {
                            dynamic d = obj;
                            byte[] t = new byte[packet.DataCount];
                            for (var i = 0; i < packet.DataCount; i++)
                                t[i] = Convert.ToByte(d[i]);
                            dest = t;
                        }
                    }
                    break;
                case "System.Int16":
                    {
                        if (packet.DataCount == 1 && !destType.IsGenericType && !destType.IsArray)
                        {
                            dest = Convert.ToInt16(obj);
                        }
                        else
                        {
                            dynamic d = obj;
                            short[] t = new short[packet.DataCount];
                            for (var i = 0; i < packet.DataCount; i++)
                                t[i] = Convert.ToInt16(d[i]);
                            dest = t;
                        }
                    }
                    break;
                case "System.Single":
                    {
                        if (packet.DataCount == 1 && !destType.IsGenericType && !destType.IsArray)
                        {
                            dest = Convert.ToSingle(obj);
                        }
                        else
                        {
                            dynamic d = obj;
                            float[] t = new float[packet.DataCount];
                            for (var i = 0; i < packet.DataCount; i++)
                                t[i] = Convert.ToSingle(d[i]);
                            dest = t;
                        }
                    }
                    break;
                case "System.Double":
                    {
                        if (packet.DataCount == 1 && !destType.IsGenericType && !destType.IsArray)
                        {
                            dest = Convert.ToDouble(obj);
                        }
                        else
                        {
                            dynamic d = obj;
                            double[] t = new double[packet.DataCount];
                            for (var i = 0; i < packet.DataCount; i++)
                                t[i] = Convert.ToDouble(d[i]);
                            dest = t;
                        }
                    }
                    break;
            }
            record[property] = dest;
        }

        public override void Dispose()
        {
            ((ServerHandleMessage)this.Pipe.LastFilter).Server.DisposeClient(this.Pipe);
            base.Dispose();
        }
    }
}
