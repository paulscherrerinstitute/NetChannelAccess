using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace PSI.EpicsClient2.Pipes
{
    internal class UdpReceiver : DataFilter
    {
        UdpClient udp;
        byte[] buff = new byte[8192 * 3];
        bool disposed = false;

        const int SIO_UDP_CONNRESET = -1744830452;

        public UdpReceiver()
        {
            InitUdp();
        }

        void InitUdp()
        {
            udp = new UdpClient(0);
            try
            {
                udp.Client.IOControl(SIO_UDP_CONNRESET, new byte[] { 0, 0, 0, 0 }, null);
            }
            catch
            {
            }
            udp.BeginReceive(GotUdpMessage, null);
        }

        public void Send(DataPacket packet)
        {
            udp.Send(packet.Data, packet.Data.Length, packet.Destination);
        }

        public void Send(IPEndPoint destination, byte[] buff)
        {
            if (disposed)
                return;
            try
            {
                udp.Send(buff, buff.Length, destination);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        void GotUdpMessage(IAsyncResult ar)
        {
            Pipe.LastMessage = DateTime.Now;
            IPEndPoint ipeSender = new IPEndPoint(IPAddress.Any, 0);
            byte[] buff;

            try
            {
                buff = udp.EndReceive(ar, ref ipeSender);
            }
            catch (ObjectDisposedException)
            {
                ((PacketSplitter)this.Pipe[1]).Reset();
                // Stop receiving
                return;
            }
            catch (Exception ex)
            {
                try
                {
                    udp.BeginReceive(GotUdpMessage, null);
                }
                catch
                {
                    ((PacketSplitter)this.Pipe[1]).Reset();
                    try
                    {
                        udp.Close();
                    }
                    catch
                    {
                    }
                    InitUdp();
                    //udp = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
                    udp.BeginReceive(GotUdpMessage, null);
                }
                return;
            }

            // Get the data back
            DataPacket packet = DataPacket.Create(buff);
            packet.Sender = (IPEndPoint)ipeSender;

            // Start Accepting again
            //udp.BeginReceive(GotUdpMessage, null);
            try
            {
                udp.BeginReceive(GotUdpMessage, null);
            }
            catch
            {
                ((PacketSplitter)this.Pipe[1]).Reset();
                try
                {
                    udp.Close();
                }
                catch
                {
                }
                InitUdp();
                udp.BeginReceive(GotUdpMessage, null);
            }

            SendData(packet);
        }

        public override void Dispose()
        {
            if (disposed)
                return;

            disposed = true;
            udp.Close();
        }

        public override void ProcessData(DataPacket packet)
        {
            throw new NotImplementedException();
        }
    }
}
