using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace CaSharpServer
{
    public class CAServer : IDisposable
    {
        static int sid = 1;
        internal CARecordCollection records = new CARecordCollection();
        internal CARecordCollection Records { get { return records; } }
        CAUdpListener udp;
        internal CAUdpListener UdpConnection { get { return udp; } }
        CAServerFilter serverFilter;
        internal CAServerFilter Filter { get { return serverFilter; } }
        internal Dictionary<int, CAServerChannel> channelList = new Dictionary<int, CAServerChannel>();
        internal Dictionary<string, CATcpConnection> openConnection = new Dictionary<string, CATcpConnection>();
        public IPAddress ServerAddress;
        CABeacon beacon;

        int udpPort = 0;
        public int UdpPort { get { return udpPort; } }
        int tcpPort = 0;
        public int TcpPort { get { return tcpPort; } }
        bool running = true;

        Thread checkConnections;

        TcpListener tcpListener;

        public CAServer()
            : this(null, 5064, 5064, 5065)
        {
        }

        public CAServer(IPAddress serverAddress)
            : this(serverAddress, 5064, 5064, 5065)
        {
        }

        public CAServer(IPAddress serverAddress, int tcpPort)
            : this(serverAddress, tcpPort, 5064, 5065)
        {
        }

        public CAServer(IPAddress serverAddress, int tcpPort, int udpPort)
            : this(serverAddress, tcpPort, udpPort, udpPort + 1)
        {
        }

        public CAServer(IPAddress serverAddress, int tcpPort, int udpPort, int beaconPort)
        {
            //Console.WriteLine("Server on "+serverAddress+" "+tcpPort+" "+udpPort+" "+beaconPort);
            if (serverAddress == null)
                serverAddress = IPAddress.Any;
            ServerAddress = serverAddress;
            this.udpPort = udpPort;
            this.tcpPort = tcpPort;
            serverFilter = new CAServerFilter(this);
            udp = new CAUdpListener(serverAddress, udpPort, serverFilter);
            tcpListener = new TcpListener(serverAddress, tcpPort);
            tcpListener.Start();
            tcpListener.BeginAcceptSocket(RecieveConn, tcpListener);

            checkConnections = new Thread(new ThreadStart(ConnectionChecker));
            checkConnections.IsBackground = true;
            checkConnections.Start();

            beacon = new CABeacon(this, udpPort, beaconPort);
        }

        void ConnectionChecker()
        {
            while (running)
            {
                Thread.Sleep(10000);

                CATcpConnection[] list;
                lock (openConnection)
                {
                    list = openConnection.Values.Where(row => (DateTime.Now - row.EchoLastSent).TotalSeconds > 120).ToArray();
                }
                foreach (var i in list)
                {
                    i.EchoLastSent = DateTime.Now;
                    i.Send(CAServerFilter.EchoMessage);
                }
            }
        }

        public CAType CreateRecord<CAType>(string name) where CAType : CARecord
        {
            CAType result = (CAType)(typeof(CAType)).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new Type[] { }, null).Invoke(new object[] { });
            result.Name = name;
            records.Add(result);
            return result;
        }

        public CAType CreateArrayRecord<CAType>(string name, int size) where CAType : CAArrayRecord
        {
            CAType result = (CAType)(typeof(CAType)).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(int) }, null).Invoke(new object[] { size });
            result.Name = name;
            records.Add(result);
            return result;
        }

        void RecieveConn(IAsyncResult result)
        {
            TcpListener listener = null;
            Socket client = null;

            try
            {
                listener = (TcpListener)result.AsyncState;
                client = listener.EndAcceptSocket(result);
            }
            catch (Exception ex)
            {
                if (running)
                    throw ex;
            }

            if (!running)
                return;

            // Wait for the next one
            lock (openConnection)
            {
                if (!openConnection.ContainsKey(client.RemoteEndPoint.ToString()))
                {
                    openConnection.Add(client.RemoteEndPoint.ToString(), new CATcpConnection(client, this));
                }
                else
                {
                    Console.WriteLine("Trying to connect to the same one!");
                }
            }
            try
            {
                listener.BeginAcceptSocket(new AsyncCallback(RecieveConn), listener);
            }
            catch (Exception ex)
            {
                if (running)
                    throw ex;
            }
        }

        internal CAServerChannel CreateEpicsChannel(int clientId, EndPoint sender, string channelName)
        {
            lock (channelList)
            {
                sid++;
                try
                {
                    channelList.Add(sid, new CAServerChannel(this, sid, clientId, channelName, openConnection[sender.ToString()]));
                }
                catch
                {
                }
                if (channelList.ContainsKey(sid))
                    return channelList[sid];
                else
                {
                    DropEpicsConnection(sender.ToString());
                    return null;
                }
            }
        }

        internal void DropEpicsChannel(int clientId)
        {
            lock (channelList)
            {
                channelList.Remove(clientId);
            }
        }

        internal void DropEpicsConnection(string remoteKey)
        {
            lock (openConnection)
            {
                openConnection.Remove(remoteKey);
            }
        }

        public void Dispose()
        {
            if (running == false)
                return;
            running = false;

            try
            {
                if (ServerAddress == IPAddress.Any)
                {
                    TcpClient client = new TcpClient("127.0.0.1", tcpPort);
                    client.Close();
                }
                else
                {
                    TcpClient client = new TcpClient(ServerAddress.ToString(), tcpPort);
                    client.Close();
                }
            }
            catch
            {
            }

            tcpListener.Stop();
            udp.Dispose();

            beacon.Dispose();

            Records.Dispose();

            List<CAServerChannel> channelsToRemove;
            lock (channelList)
                channelsToRemove = channelList.Values.ToList();
            foreach (var i in channelsToRemove)
                i.Dispose();

            List<CATcpConnection> connsToRemove;
            lock (openConnection)
                connsToRemove = openConnection.Values.ToList();
            foreach (var i in connsToRemove)
                i.Dispose();
        }
    }
}
