﻿/*
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
using System;
using System.Net;
using System.Net.Sockets;

namespace EpicsSharp.Common.Pipes
{
    internal class UdpReceiver : DataFilter
    {
        private UdpClient udp;
        private byte[] buff = new byte[8192 * 3];
        private bool disposed = false;
        private IPAddress address = null;
        private int udpPort = 0;
        private const int SIO_UDP_CONNRESET = -1744830452;
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

        private void InitUdp(IPAddress address = null, int port = 0)
        {
            udp = new UdpClient();
            udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            if (address == null)
                udp.Client.Bind(new IPEndPoint(IPAddress.Any, port));
            else
                udp.Client.Bind(new IPEndPoint(address, port));

            /*if (address == null)
                udp = new UdpClient(port);
            else            
                udp = new UdpClient(new IPEndPoint(address, port));*/
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
                //Console.WriteLine(ex.ToString());
            }
        }

        private void GotUdpMessage(IAsyncResult ar)
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
                    //InitUdp(this.address, udpPort);
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
