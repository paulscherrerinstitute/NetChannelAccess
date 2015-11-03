using EpicsSharp.ChannelAccess.Server.RecordTypes;
using EpicsSharp.Common.Pipes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EpicsSharp.ChannelAccess.Server
{
    public class CAServer : IDisposable
    {
        DataPipe udpPipe;
        internal CARecordCollection records = new CARecordCollection();
        internal CARecordCollection Records { get { return records; } }
        CaServerListener listener;
        bool disposed = false;

        internal List<DataPipe> tcpConnections = new List<DataPipe>();

        public int TcpPort { get; private set; }
        public int UdpPort { get; private set; }
        public int BeaconPort { get; private set; }

        public CAServer(IPAddress ipAddress = null, int tcpPort = 5064, int udpPort = 5064, int beaconPort = 0)
        {
            if (ipAddress == null)
                ipAddress = IPAddress.Any;

            if (beaconPort == 0)
                beaconPort = udpPort + 1;
            this.TcpPort = tcpPort;
            this.UdpPort = udpPort;
            this.BeaconPort = beaconPort;
            listener = new CaServerListener(this, new IPEndPoint(ipAddress, tcpPort));
            udpPipe = DataPipe.CreateServerUdp(this, ipAddress, udpPort);
        }

        public CAType CreateRecord<CAType>(string name) where CAType : CARecord
        {
            CAType result = null;
            try
            {
                result = (CAType)(typeof(CAType)).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new Type[] { }, null).Invoke(new object[] { });
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
            result.Name = name;
            records.Add(result);
            return result;
        }

        public CAType CreateArrayRecord<CAType>(string name, int size) where CAType : CAArrayRecord
        {
            CAType result = null;
            try
            {
                result = (CAType)(typeof(CAType)).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(int) }, null).Invoke(new object[] { size });
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
            result.Name = name;
            records.Add(result);
            return result;
        }

        internal void RegisterClient(DataPipe chain)
        {
            lock (tcpConnections)
            {
                tcpConnections.Add(chain);
            }
        }

        internal void DisposeClient(DataPipe chain)
        {
            lock (tcpConnections)
            {
                tcpConnections.Remove(chain);
            }
        }

        public void Dispose()
        {
            if (disposed)
                return;
            disposed = true;

            List<DataPipe> toDrop;
            lock (tcpConnections)
            {
                toDrop = tcpConnections.ToList();
            }
            toDrop.ForEach(row => row.Dispose());

            listener.Dispose();
            udpPipe.Dispose();
            records.Dispose();
        }
    }
}
