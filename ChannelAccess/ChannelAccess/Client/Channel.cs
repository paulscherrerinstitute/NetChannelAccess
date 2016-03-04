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
using System.Net;
using System.Threading;
using System.IO;
using EpicsSharp.Common.Pipes;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using EpicsSharp.ChannelAccess.Constants;
using EpicsSharp.ChannelAccess.Common;

namespace EpicsSharp.ChannelAccess.Client
{
    /// <summary>
    /// Monitor delegate
    /// </summary>
    /// <param name="sender">Channel on which the changed happened</param>
    /// <param name="newValue">Object value of the type the monitor was registered for</param>
    public delegate void ChannelValueDelegate(Channel sender, object newValue);

    /// <summary>
    /// Alarm status delegate
    /// </summary>
    /// <param name="sender">Channel which had its status changed</param>
    /// <param name="newStatus">new status</param>
    public delegate void ChannelStatusDelegate(Channel sender, ChannelStatus newStatus);

    public class Channel : IDisposable
    {
        public string ChannelName { get; protected set; }
        private ChannelStatus status = ChannelStatus.REQUESTED;
        public ChannelStatus Status
        {
            get
            {
                return status;
            }
            protected set
            {
                status = value;
                if (StatusChanged != null)
                    StatusChanged(this, Status);
            }
        }
        protected CAClient Client;
        internal ClientTcpReceiver ioc;
        protected static uint NextCid = 1;
        protected static uint NextIoId = 1;
        protected uint cid = (NextCid++);
        /// <summary>
        /// The client unique channel ID
        /// </summary>
        public uint CID { get { return cid; } }
        /// <summary>
        /// The server unique channel ID
        /// </summary>
        public uint SID { get; protected set; }
        protected object ConnectionLock = new object();
        protected AutoResetEvent ConnectionEvent = new AutoResetEvent(false);
        internal AutoResetEvent GetAnswerEvent = new AutoResetEvent(false);
        protected DataPacket RawData;
        protected uint MonitoredElements;
        protected Type MonitoredType = null;
        private event ChannelValueDelegate PrivMonitorChanged;
        /// <summary>
        /// Allows to get informed when the channel connection status changes.
        /// </summary>
        public event ChannelStatusDelegate StatusChanged;
        protected List<Action<Channel>> AfterAction = new List<Action<Channel>>();
        internal event ChannelValueDelegate AfterReadNotify;
        protected bool Disposed = false;
        internal DataPacket SearchPacket;
        protected Type channelDefinedType;
        /// <summary>
        /// The number of elements the channel contains.
        /// </summary>
        public UInt32 ChannelDataCount { get; internal set; }

        /// <summary>
        /// If set defines how many elements will be requested on a monitor
        /// </summary>
        public uint? WishedDataCount { get; set; }
        /// <summary>
        /// The channel monitor mask
        /// </summary>
        public MonitorMask MonitorMask { get; set; }
        /// <summary>
        /// If the get has returned the value.
        /// </summary>
        public bool HasValue { get; internal set; }

        public Dictionary<string, TimeSpan> ElapsedTimings = new Dictionary<string, TimeSpan>();

        internal Stopwatch Stopwatch = new Stopwatch();

        internal int SearchInverval = 1;
        internal int SearchInvervalCounter = 1;
        internal DateTime StartSearchTime = DateTime.Now;

        /// <summary>
        /// Access Rights of the Channel
        /// </summary>
        public AccessRights AccessRight { get; internal set; }

        /// <summary>
        /// The type of data the channel offers natively
        /// </summary>
        public Type ChannelDefinedType { get { return channelDefinedType; } }

        /// <summary>
        /// Who answered to the search (useful to see a name server)
        /// </summary>
        public IPAddress SearchAnswerFrom { get; set; }

        public string IOC
        {
            get
            {
                if (ioc == null)
                    return null;
                return ioc.Destination.ToString();
            }
        }

        internal Channel(CAClient client, string channelName)
        {
            this.ChannelName = channelName;
            this.Status = ChannelStatus.REQUESTED;
            this.Client = client;
            MonitorMask = MonitorMask.VALUE;
            HasValue = false;

            SearchPacket = DataPacket.Create(16 + ChannelName.Length + TypeHandling.Padding(ChannelName.Length));
            SearchPacket.Command = (ushort)CommandID.CA_PROTO_SEARCH;
            SearchPacket.DataType = (ushort)CAConstants.DONT_REPLY;
            SearchPacket.DataCount = (ushort)CAConstants.CA_MINOR_PROTOCOL_REVISION;
            SearchPacket.Parameter1 = cid;
            SearchPacket.Parameter2 = cid;
            SearchPacket.SetDataAsString(ChannelName);
        }

        internal void SendReadNotify<TType>()
        {
            SendReadNotify<TType>(WishedDataCount ?? ChannelDataCount);
        }

        internal void SendReadNotify<TType>(uint nbElements)
        {
            if (Client.Configuration.DebugTiming)
            {
                lock (ElapsedTimings)
                {
                    if (!ElapsedTimings.ContainsKey("SendReadNotify"))
                        ElapsedTimings.Add("SendReadNotify", Stopwatch.Elapsed);
                }
            }

            DataPacket packet = DataPacket.Create(16);
            packet.Command = (ushort)CommandID.CA_PROTO_READ_NOTIFY;
            Type t = typeof(TType);
            if (typeof(TType).IsArray)
                t = (typeof(TType)).GetElementType();
            else if (t.IsGenericType)
            {
                if (t.GetGenericArguments().First() == typeof(object))
                    t = t.GetGenericTypeDefinition().MakeGenericType(new Type[] { channelDefinedType });
            }
            if (t == typeof(object))
                t = channelDefinedType;
            packet.DataType = (ushort)TypeHandling.Lookup[t];
            packet.DataCount = nbElements;
            packet.Parameter1 = SID;
            uint ioid = (NextIoId++);
            packet.Parameter2 = ioid;

            lock (ioc.PendingIo)
            {
                ioc.PendingIo.Add(ioid, this);
            }

            ioc.Send(packet);
        }

        /// <summary>
        /// Get a value in synchronous mode. Blocks until the value come back or the timeout is reached.
        /// </summary>
        /// <typeparam name="TType">Type of value requested</typeparam>
        /// <param name="nbElements">The number of elements to read (by default all)</param>
        /// <returns>The value the IOC returned</returns>
        public TType Get<TType>(uint nbElements = 0)
        {
            if (Disposed)
                throw new ObjectDisposedException(this.GetType().Name);
            HasValue = false;
            WaitConnection();

            SendReadNotify<TType>(nbElements == 0 ? this.ChannelDataCount : nbElements);

            if (GetAnswerEvent.WaitOne(Client.Configuration.WaitTimeout) == false)
                throw new Exception("Read Notify timeout.");

            if (nbElements == 0)
                return DecodeData<TType>(this.ChannelDataCount);
            return DecodeData<TType>(nbElements);
        }

        public async Task<TType> GetAsync<TType>(uint nbElements = 0)
        {
            TType result = default(TType);
            await Task.Run(() => result = Get<TType>(nbElements));
            return result;
        }

        protected DataPacket WritePacket<TType>(TType newValue)
        {
            int headerSize = 16;
            uint nbElem = 1;
            if (newValue is IEnumerable<object>)
                nbElem = (uint)((IEnumerable<object>)newValue).Count();
            Type t = typeof(TType);
            if (t.IsArray)
            {
                nbElem = (uint)((Array)((object)newValue)).Length;
                // if too many array elements, use extended header
                if (nbElem > 0xffff)
                    headerSize = 24;
                t = t.GetElementType();
            }
            else if (t.IsGenericType)
            {
                if (t.GetGenericArguments().First() == typeof(object))
                    t = t.GetGenericTypeDefinition().MakeGenericType(new Type[] { channelDefinedType });
            }
            if (t == typeof(object))
                t = channelDefinedType;

            int payloadSize = (int)(nbElem * TypeHandling.EpicsSize(t));
            if (payloadSize % 8 > 0)
            {
                payloadSize += 8 - (payloadSize % 8);
            }
            // if payload too large, use extended header
            if (payloadSize > 0x4000)
                headerSize = 24;
            DataPacket packet = DataPacket.Create(headerSize + payloadSize);
            packet.Command = (ushort)CommandID.CA_PROTO_WRITE_NOTIFY;
            packet.DataCount = nbElem;
            packet.DataType = (ushort)TypeHandling.Lookup[t];
            packet.Parameter1 = SID;
            uint ioid = (NextIoId++);
            packet.Parameter2 = ioid;

            if (nbElem > 1)
            {
                int pos = headerSize;
                int elementSize = TypeHandling.EpicsSize(t);
                foreach (var elem in (System.Collections.IEnumerable)newValue)
                {
                    switch (TypeHandling.Lookup[t])
                    {
                        case EpicsType.Int:
                            packet.SetInt32(pos, (int)elem);
                            break;
                        case EpicsType.Short:
                            packet.SetInt16(pos, (short)elem);
                            break;
                        case EpicsType.Float:
                            packet.SetFloat(pos, (float)elem);
                            break;
                        case EpicsType.Double:
                            packet.SetDouble(pos, (double)elem);
                            break;
                        case EpicsType.Byte:
                            packet.SetByte(pos, (byte)elem);
                            break;
                        default:
                            throw new Exception("Type not supported");
                    }
                    pos += elementSize;
                }
            }
            else
            {
                switch (TypeHandling.Lookup[t])
                {
                    case EpicsType.Int:
                        packet.SetInt32((int)packet.HeaderSize, (int)(object)newValue);
                        break;
                    case EpicsType.Short:
                        packet.SetInt16((int)packet.HeaderSize, (short)(object)newValue);
                        break;
                    case EpicsType.Float:
                        packet.SetFloat((int)packet.HeaderSize, (float)(object)newValue);
                        break;
                    case EpicsType.Double:
                        packet.SetDouble((int)packet.HeaderSize, (double)(object)newValue);
                        break;
                    case EpicsType.String:
                        packet.SetDataAsString((string)(object)newValue);
                        break;
                    case EpicsType.Byte:
                        packet.SetByte((int)packet.HeaderSize, (byte)(object)newValue);
                        break;
                    default:
                        throw new Exception("Type not currently supported.");
                }
            }

            return packet;
        }

        internal void SendWriteNotify<TType>(TType newValue)
        {
            DataPacket packet = WritePacket<TType>(newValue);

            lock (ioc.PendingIo)
            {
                ioc.PendingIo.Add(packet.Parameter2, this);
            }

            ioc.Send(packet);
        }

        internal void SendWrite<TType>(TType newValue)
        {
            DataPacket packet = WritePacket<TType>(newValue);
            packet.Command = (ushort)CommandID.CA_PROTO_WRITE;

            ioc.Send(packet);
        }

        /// <summary>
        /// Put a value to the IOC in blocking mode.
        /// </summary>
        /// <example><![CDATA[
        /// CAClient client = new CAClient();
        /// Channel channel = client.CreateChannel("SEILER_C:CPU");
        /// channel.Put<int>(1);
        /// ]]></example>
        /// <typeparam name="TType">The type of values to put</typeparam>
        /// <param name="newValue">The new value to set</param>
        public void Put<TType>(TType newValue)
        {
            if (Disposed)
                throw new ObjectDisposedException(this.GetType().Name);
            WaitConnection();
            SendWriteNotify<TType>(newValue);
            if (GetAnswerEvent.WaitOne(Client.Configuration.WaitTimeout) == false)
                throw new Exception("Write Notify timeout.");
        }

        public async Task PutAsync<TType>(TType newValue)
        {
            await Task.Run(() => Put<TType>(newValue));
        }

        /// <summary>
        /// Put a value to the IOC in blocking mode.
        /// </summary>
        /// <example><![CDATA[
        /// CAClient client = new CAClient();
        /// Channel channel = client.CreateChannel("SEILER_C:CPU");
        /// channel.PutNoWait<int>(1);
        /// ]]></example>
        /// <typeparam name="TType">The type of values to put</typeparam>
        /// <param name="newValue">The new value to set</param>
        public void PutNoWait<TType>(TType newValue)
        {
            if (Disposed)
                throw new ObjectDisposedException(this.GetType().Name);
            WaitConnection();
            SendWrite<TType>(newValue);
        }

        internal TType DecodeData<TType>(uint nbElements, int startPos = 0, int maxSize = 40)
        {
            return (TType)DecodeData(typeof(TType), nbElements, startPos, maxSize);
        }

        static DateTime timestampBase = new DateTime(1990, 1, 1, 0, 0, 0);
        internal object DecodeData(Type t, uint nbElements = 1, int startPost = 0, int maxSize = 40)
        {
            if (t.IsSubclassOf(typeof(Decodable)) && !t.IsArray)
            {
                Decodable res = (Decodable)t.GetConstructor(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, new Type[] { }, null).Invoke(new object[] { });
                res.Decode(this, nbElements);
                return res;
            }

            if (t == typeof(object))
                t = channelDefinedType;

            Type baseT = t;
            if (baseT.IsArray)
                baseT = t.GetElementType();

            if (t.IsArray)
            {
                Type dl = typeof(List<>);
                Type genList = dl.MakeGenericType(new Type[] { baseT });

                System.Collections.IList res = (System.Collections.IList)Activator.CreateInstance(genList);

                int pos = (int)RawData.HeaderSize + startPost;
                int elementSize = TypeHandling.EpicsSize(baseT);
                for (int i = 0; i < nbElements; i++)
                {
                    switch (TypeHandling.Lookup[baseT])
                    {
                        case EpicsType.Int:
                            res.Add(RawData.GetInt32(pos));
                            break;
                        case EpicsType.Short:
                            res.Add(RawData.GetInt16(pos));
                            break;
                        case EpicsType.Float:
                            res.Add(RawData.GetFloat(pos));
                            break;
                        case EpicsType.Double:
                            res.Add(RawData.GetDouble(pos));
                            break;
                        case EpicsType.Byte:
                            res.Add(RawData.GetByte(pos));
                            break;
                        default:
                            throw new Exception("Type not supported");
                    }
                    pos += elementSize;
                }
                return ((dynamic)res).ToArray();
            }

            if (baseT == typeof(DateTime))
            {
                long secs = RawData.GetUInt32((int)RawData.HeaderSize + startPost);
                long nanoSecs = RawData.GetUInt32((int)RawData.HeaderSize + startPost + 4);
                DateTime d = (new DateTime(timestampBase.Ticks + (secs * 10000000L) + (nanoSecs / 100L))).ToLocalTime();
                return d;
            }

            switch (TypeHandling.Lookup[baseT])
            {
                case EpicsType.Internal_UInt:
                    return RawData.GetUInt32((int)RawData.HeaderSize + startPost);
                case EpicsType.Internal_UShort:
                    return RawData.GetUInt16((int)RawData.HeaderSize + startPost);
                case EpicsType.Int:
                    return RawData.GetInt32((int)RawData.HeaderSize + startPost);
                case EpicsType.Short:
                    return RawData.GetInt16((int)RawData.HeaderSize + startPost);
                case EpicsType.Float:
                    return RawData.GetFloat((int)RawData.HeaderSize + startPost);
                case EpicsType.Double:
                    return RawData.GetDouble((int)RawData.HeaderSize + startPost);
                case EpicsType.String:
                    return RawData.GetDataAsString(startPost, maxSize);
                case EpicsType.Byte:
                    return RawData.GetByte((int)RawData.HeaderSize + startPost);
                default:
                    //throw new Exception("Type not supported");
                    return new object();
            }
        }

        /// <summary>
        /// Event-Monitor which calls as soon a change on the channel happened which fits into the defined
        /// Monitormask (channel.MonitorMask).<br />The properties channel.MonitorMask and channel.MonitorDataCount
        /// do touch the behavior of this event and can't be changed when a monitor is already connected.
        /// <example><![CDATA[
        /// CAClient client = new CAClient();
        /// Channel channel = client.CreateChannel("SEILER_C:CPU");
        /// channel.MonitorMask = MonitorMask.VALUE;
        /// channel.MonitorDataCount = 1;
        /// channel.MonitorChanged += new ChannelValueDelegate(channel_MonitorChanged);
        /// ]]></example>
        /// </summary>
        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
        public event ChannelValueDelegate MonitorChanged
        {
            add
            {
                //if (PrivMonitorChanged == null && MonitoredType != null)
                if (PrivMonitorChanged == null)
                {
                    AfterConnect(action =>
                                     {
                                         if (MonitoredType == null)
                                             return;
                                         DataPacket p = DataPacket.Create(16 + 16);
                                         p.Command = (ushort)CommandID.CA_PROTO_EVENT_ADD;
                                         p.DataType = (ushort)TypeHandling.Lookup[MonitoredType];
                                         p.DataCount = WishedDataCount ?? ChannelDataCount;
                                         MonitoredElements = (WishedDataCount ?? ChannelDataCount);
                                         p.Parameter1 = SID;
                                         p.Parameter2 = CID;

                                         p.SetUInt16(12 + 16, (ushort)MonitorMask);

                                         ioc.Send(p);
                                     });
                }
                else if (RawData != null)
                {
                    value(this, DecodeData(MonitoredType, MonitoredElements));
                }
                PrivMonitorChanged += value;
            }
            remove
            {
                PrivMonitorChanged -= value;

                if (PrivMonitorChanged == null)
                {
                    DataPacket p = DataPacket.Create(16);
                    p.Command = (ushort)CommandID.CA_PROTO_EVENT_CANCEL;
                    p.DataType = (ushort)TypeHandling.Lookup[MonitoredType];
                    p.DataCount = MonitoredElements;
                    p.Parameter1 = SID;
                    p.Parameter2 = CID;
                    if (ioc != null)
                        ioc.Send(p);
                }
            }
        }

        internal void AfterConnect(Action<Channel> action)
        {
            lock (AfterAction)
            {
                if (AfterAction.Count > 0)
                {
                    AfterAction.Add(action);
                    return;
                }
            }

            //Console.WriteLine("Add after connect");
            if (Client.Configuration.DebugTiming)
            {
                lock (ElapsedTimings)
                {
                    ElapsedTimings.Clear();
                    Stopwatch.Start();
                }
            }

            lock (ConnectionLock)
            {
                if (Status == ChannelStatus.CONNECTED)
                {
                    action(this);
                    return;
                }
                if (ioc == null)
                    SendSearch();
            }
            lock (AfterAction)
            {
                AfterAction.Add(action);
                this.StatusChanged += AfterActionStatusChanged;
            }
        }

        void AfterActionStatusChanged(Channel sender, ChannelStatus newStatus)
        {
            List<Action<Channel>> todo = new List<Action<Channel>>();
            lock (AfterAction)
            {
                //Console.WriteLine("After Action AlarmStatus Changed");
                this.StatusChanged -= AfterActionStatusChanged;
                if (Disposed)
                    return;
                if (AfterAction == null)
                    return;

                todo = AfterAction.ToList();
                AfterAction.Clear();
            }

            foreach (var i in todo)
                i(this);
        }

        public void Connect()
        {
            if (Disposed)
                throw new ObjectDisposedException(this.GetType().Name);
            WaitConnection();
        }

        public async Task<bool> ConnectAsync()
        {
            bool result = true;
            await Task.Run(() => result = WaitConnectionResult());
            return result;
        }

        protected bool WaitConnectionResult()
        {
            lock (ConnectionLock)
            {
                if (Status == ChannelStatus.CONNECTED)
                    return true;
                if (ioc == null)
                {
                    //Console.WriteLine("Need to connect");
                    SendSearch();
                }
            }

            if (Client.Configuration.DebugTiming)
            {
                lock (ElapsedTimings)
                {
                    ElapsedTimings.Clear();
                    Stopwatch.Start();
                }
            }

            return ConnectionEvent.WaitOne(Client.Configuration.WaitTimeout);
        }

        protected void WaitConnection()
        {
            if (!WaitConnectionResult())
                throw new Exception("Connection timeout.");
        }

        protected void SendSearch()
        {
            Client.Searcher.Add(this);
        }

        internal void SetIoc(DataPipe pipe)
        {
            ClientTcpReceiver tcpReceiver = (ClientTcpReceiver)pipe[0];
            tcpReceiver.AddChannel(this);
            lock (ConnectionLock)
            {
                if (!Client.Searcher.Contains(this))
                    return;
                Client.Searcher.Remove(this);
                SID = 0;

                ioc = tcpReceiver;
                lock (ioc.ChannelSID)
                {
                    //Console.WriteLine(ioc.ChannelSID.Count);
                    // Channel already known
                    if (ioc.ChannelSID.ContainsKey(ChannelName))
                    {
                        SID = ioc.ChannelSID[ChannelName];
                        //Console.WriteLine("Here");
                        Channel chan = ioc.ConnectedChannels.FirstOrDefault(row => row.ChannelName == ChannelName && row.ChannelDataCount != 0);
                        if (chan != null)
                        {
                            this.ChannelDataCount = chan.ChannelDataCount;
                            this.channelDefinedType = chan.ChannelDefinedType;
                            Status = ChannelStatus.CONNECTED;
                            ConnectionEvent.Set();
                        }
                    }
                }
            }
            if (SID != 0)
            {
                //Console.WriteLine("SID " + SID + " STATUS CHANGED");
                if (StatusChanged != null)
                    StatusChanged(this, Status);
                return;
            }
            if (Client.Configuration.DebugTiming)
            {
                lock (ElapsedTimings)
                {
                    if (!ElapsedTimings.ContainsKey("IocConnection"))
                        ElapsedTimings.Add("IocConnection", Stopwatch.Elapsed);
                }
            }

            // We need to create the channel
            int padding;
            if (ChannelName.Length % 8 == 0)
                padding = 8;
            else
                padding = (8 - (ChannelName.Length % 8));

            DataPacket packet = DataPacket.Create(16 + ChannelName.Length + padding);
            packet.Command = (ushort)CommandID.CA_PROTO_CREATE_CHAN;
            packet.DataType = 0;
            packet.DataCount = 0;
            packet.Parameter1 = cid;
            packet.Parameter2 = (uint)CAConstants.CA_MINOR_PROTOCOL_REVISION;
            packet.SetDataAsString(ChannelName);

            if (ioc != null)
                ioc.Send(packet);
            else
            {
                Disconnect();
                return;
            }
            lock (ElapsedTimings)
            {
                if (!ElapsedTimings.ContainsKey("SendCreateChannel"))
                    ElapsedTimings.Add("SendCreateChannel", Stopwatch.Elapsed);
            }
        }

        internal void SetServerChannel(uint sid, EpicsType epicsType, uint dataCount)
        {
            //Console.WriteLine("Setting channel SID " + this.ChannelName + " " + sid);
            lock (ConnectionLock)
            {
                if (ioc == null)
                {
                    Disconnect();
                    return;
                }
                lock (ioc.ChannelSID)
                {
                    if (!ioc.ChannelSID.ContainsKey(this.ChannelName))
                        ioc.ChannelSID.Add(this.ChannelName, sid);
                }

                SID = sid;
                channelDefinedType = TypeHandling.ReverseLookup[epicsType];
                if (MonitoredType == null)
                    MonitoredType = channelDefinedType;
                MonitoredElements = ChannelDataCount = dataCount;

                Status = ChannelStatus.CONNECTED;

                ConnectionEvent.Set();
            }
            if (Client.Configuration.DebugTiming)
            {
                lock (ElapsedTimings)
                {
                    if (!ElapsedTimings.ContainsKey("CreateChannel"))
                        ElapsedTimings.Add("CreateChannel", Stopwatch.Elapsed);
                }
            }
        }

        internal void SetGetRawValue(DataPacket packet)
        {
            if (Client.Configuration.DebugTiming)
            {
                lock (ElapsedTimings)
                {
                    if (!ElapsedTimings.ContainsKey("ReadNotify"))
                        ElapsedTimings.Add("ReadNotify", Stopwatch.Elapsed);
                }
            }
            RawData = packet;
            HasValue = true;
            GetAnswerEvent.Set();
            if (AfterReadNotify != null)
                AfterReadNotify(this, packet);
        }

        internal void SetWriteNotify()
        {
            GetAnswerEvent.Set();
        }

        internal virtual void UpdateMonitor(DataPacket packet)
        {
            if (Client.Configuration.DebugTiming)
            {
                lock (ElapsedTimings)
                {
                    if (!ElapsedTimings.ContainsKey("MonitorUpdate"))
                        ElapsedTimings.Add("MonitorUpdate", Stopwatch.Elapsed);
                }
            }
            RawData = packet;
            if (PrivMonitorChanged != null)
            {
                PrivMonitorChanged(this, DecodeData(MonitoredType, MonitoredElements == 0 ? packet.DataCount : MonitoredElements));
            }
        }

        internal virtual void Disconnect()
        {
            if (Disposed)
                return;
            if (ioc != null)
                ioc.RemoveChannel(this);
            lock (ConnectionLock)
            {
                if (Status != ChannelStatus.CONNECTED)
                    return;
                Status = ChannelStatus.DISCONNECTED;
                StartSearchTime = DateTime.Now;
                ioc = null;
                SID = 0;

                if (PrivMonitorChanged != null)
                {
                    AfterConnect(action =>
                                     {
                                         if (MonitoredType == null)
                                             return;
                                         DataPacket p = DataPacket.Create(16 + 16);
                                         p.Command = (ushort)CommandID.CA_PROTO_EVENT_ADD;
                                         p.DataType = (ushort)TypeHandling.Lookup[MonitoredType];
                                         p.DataCount = MonitoredElements;
                                         p.Parameter1 = SID;
                                         p.Parameter2 = CID;

                                         p.SetUInt16(12 + 16, (ushort)MonitorMask);

                                         if (ioc != null)
                                             ioc.Send(p);
                                         else
                                             Disconnect();
                                     });
                }
            }
        }

        public void Dispose()
        {
            if (Disposed)
                return;
            Disposed = true;
            lock (Client.Channels)
                Client.Channels.Remove(this.CID);
            if (ioc != null)
                ioc.RemoveChannel(this);
            Client.Searcher.Remove(this);
            Status = ChannelStatus.DISPOSED;
        }
    }
}
