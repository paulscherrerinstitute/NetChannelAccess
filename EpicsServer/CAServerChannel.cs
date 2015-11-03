using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CaSharpServer.Constants;
using System.Collections;

namespace CaSharpServer
{
    /// <summary>
    /// Contain the communication for a given RECORD.PROPERTY (by default .VAL)
    /// Stores also the monitor
    /// </summary>
    class CAServerChannel : IDisposable
    {
        internal CAServer Server;
        internal int ServerId;
        internal int ClientId;
        private string ChannelName;
        internal CATcpConnection TcpConnection;
        public string Property { get; set; }
        public CARecord Record { get; set; }
        private AccessRights Access = AccessRights.ReadAndWrite;
        Dictionary<int, CAChannelMonitor> monitors = new Dictionary<int, CAChannelMonitor>();
        bool disposed = false;

        public CAServerChannel(CAServer cAServer, int serverId, int clientId, string channelName, CATcpConnection tcpConnection)
        {
            // TODO: Complete member initialization
            this.Server = cAServer;
            this.ServerId = serverId;
            this.ClientId = clientId;
            this.ChannelName = channelName;
            this.TcpConnection = tcpConnection;
            tcpConnection.Closing += new EventHandler(tcpConnection_Closing);
            Property = "VAL";
            if (channelName.Contains("."))
            {
                string[] splitted = channelName.Split('.');
                Record = Server.records[splitted[0]];
                Property = splitted[1].ToUpper();
            }
            else
                Record = Server.records[ChannelName];
            if (!Record.CanBeRemotlySet)
                Access = AccessRights.ReadOnly;
            TcpConnection.Send(Server.Filter.ChannelCreatedMessage(ClientId, ServerId, FindType(Record[Property].GetType()), Record.dataCount, Access));
        }

        EpicsType FindType(Type type)
        {
            if (type.IsEnum)
                return EpicsType.Enum;
            string name;
            if (type.IsGenericType && type.Name.Split(new char[] { '`' })[0] == "ArrayContainer")
                name = type.GetGenericArguments()[0].Name;
            else if (type.IsArray)
                name = type.GetElementType().Name;
            else
                name = type.Name;

            switch (name)
            {
                case "Short":
                    return EpicsType.Short;
                case "Double":
                    return EpicsType.Double;
                case "Int32":
                    return EpicsType.Int;
                case "Single":
                case "Float":
                    return EpicsType.Float;
                case "String":
                    return EpicsType.String;
                case "Int16":
                    return EpicsType.Short;
                case "Byte":
                    return EpicsType.Byte;
                default:
                    Console.WriteLine("Unkown type " + type.Name);
                    return EpicsType.Invalid;
            }
        }

        void tcpConnection_Closing(object sender, EventArgs e)
        {
            Dispose();
        }

        public void Dispose()
        {
            if (disposed)
                return;
            disposed = true;
            if (!TcpConnection.Closed)
            {
                TcpConnection.Send(Server.Filter.channelDisconnectionMessage(ClientId));
                TcpConnection.Dispose();
            }
            Server.DropEpicsChannel(ClientId);
        }

        internal void ReadValue(int ioId, EpicsType type, int dataCount)
        {
            byte[] val;
            if (Record.Scan == ScanAlgorithm.PASSIVE)
                Record.CallPrepareRecord();
            object objVal = Record[Property];
            if (objVal == null)
                objVal = 0;

            try
            {
                using (Record.CreateAtomicChange(false))
                {
                    if (type == EpicsType.Labeled_Enum)
                        val = objVal.LabelsToByteArray(Record);
                    else if (objVal.GetType().IsArray)
                        val = objVal.ToByteArray(type, Record, dataCount);
                    else
                        val = objVal.ToByteArray(type, Record);
                }
                TcpConnection.Send(Server.Filter.ChannelReadMessage(ClientId, ioId, type, dataCount, val));
            }
            catch (Exception e)
            {
                /*Console.WriteLine(e.Message + "\n\r" + e.StackTrace);
                TcpConnection.Send(Server.Filter.ErrorMessage(ClientId, EpicsTransitionStatus.ECA_BADTYPE, "WRONG TYPE", new byte[16]));*/
            }
        }

        internal void AddMonitor(EpicsType type, int dataCount, int subscriptionId, MonitorMask mask)
        {
            //does he request to add a subscriptionId at the same Id as it already exist?
            lock (monitors)
            {
                if (monitors.ContainsKey(subscriptionId))
                    return;

                monitors.Add(subscriptionId, new CAChannelMonitor(Record, Property, this, type, dataCount, mask, subscriptionId));
            }
        }

        internal void RemoveMonitor(int key)
        {
            lock (monitors)
            {
                if (monitors.ContainsKey(key))
                {
                    monitors[key].Dispose();
                    monitors.Remove(key);
                }
            }
        }

        internal void PutValue(int ioId, EpicsType type, int dataCount, byte[] payload, bool notify)
        {
            try
            {
                object val;

                if (!Record.CanBeRemotlySet)
                {
                    TcpConnection.Send(Server.Filter.ChannelWroteMessage(ClientId, ioId, type, dataCount, EpicsTransitionStatus.ECA_NORMAL));
                    return;
                }

                if (dataCount == 1)
                {
                    val = payload.ByteToObject(type);
                    using (Record.CreateAtomicChange())
                    {
                        if (Record[Property] is Enum)
                        {
                            if(val is String)
                                Record[Property] = Enum.Parse(Record[Property].GetType(), (string)val);
                            else
                                Record[Property] = int.Parse(val.ToString());
                        }
                        else
                            Record[Property] = Convert.ChangeType(val, Record.GetPropertyType(Property));
                    }
                }
                else
                {
                    val = payload.ByteToObject(type, dataCount);
                    int nb = Math.Min(((dynamic)Record[Property]).Length, dataCount);
                    Type t = Record.GetPropertyType(Property);
                    if (t.Name.Split('`')[0] == "ArrayContainer")
                        t = t.GetGenericArguments().First();
                    else
                        t = t.GetElementType();
                    if (Record.CallPropertySet(new PropertyDelegateEventArgs { OldValue = Record[Property], NewValue = val, Property = Property }))
                    {
                        int i = 0;
                        using (Record.CreateAtomicChange())
                        {
                            foreach (object element in ((IEnumerable)val))
                            {
                                Record.SetArrayValue(Property, i, Convert.ChangeType(element, t));
                                i++;
                            }
                        }
                    }
                }

                if (notify)
                    TcpConnection.Send(Server.Filter.ChannelWroteMessage(ClientId, ioId, type, dataCount, EpicsTransitionStatus.ECA_NORMAL));
            }
            catch (Exception exp)
            {
                TcpConnection.Send(Server.Filter.ErrorMessage(ClientId, EpicsTransitionStatus.ECA_BADSTR, "Message was not correct", new byte[16]));
                return;
            }
        }
    }
}
