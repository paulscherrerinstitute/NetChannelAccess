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
using System.Net.Sockets;
using System.Net;
using EpicsSharp.ChannelAccess.Common;

namespace EpicsSharp.Common.Pipes
{
    internal class UdpReceiver : DataFilter
    {
        UdpClient udp;
        byte[] buff = new byte[8192 * 3];
        bool disposed = false;
        IPAddress address = null;
        int udpPort = 0;

        const int SIO_UDP_CONNRESET = -1744830452;
        public UdpReceiver()
            : this(null, 0)
        {

        }

        public UdpReceiver(IPAddress address, int port)
        {
            udpPort = port;
            this.address = address;
            InitUdp(address, udpPort);
        }

        void InitUdp(IPAddress address = null, int port = 0)
        {
            if (address == null)
                udp = new UdpClient(port);
            else
                udp = new UdpClient(new IPEndPoint(address, port));
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
                    InitUdp(this.address, udpPort);
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
                InitUdp(this.address, udpPort);
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
