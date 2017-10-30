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
using EpicsSharp.ChannelAccess.Common;
using EpicsSharp.ChannelAccess.Constants;
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
    internal class CaServerListener : IDisposable
    {
        TcpListener tcpListener = null;
        bool disposed = false;
        readonly IPEndPoint ipSource;
        private CAServer server;

        public CaServerListener(CAServer server, IPEndPoint ipSource)
        {
            this.server = server;
            this.ipSource = ipSource;

            Rebuild();
        }

        void Rebuild()
        {
            if (disposed)
                return;
            if (tcpListener != null)
            {
                try
                {
                    tcpListener.Stop();
                }
                catch
                {
                }
                System.Threading.Thread.Sleep(100);
            }
            tcpListener = new TcpListener(ipSource);
            tcpListener.Start(10);
            tcpListener.BeginAcceptSocket(ReceiveConn, tcpListener);
        }

        void ReceiveConn(IAsyncResult result)
        {
            TcpListener listener = null;
            Socket client = null;

            try
            {
                listener = (TcpListener)result.AsyncState;
                client = listener.EndAcceptSocket(result);

                client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
            }
            catch (Exception ex)
            {
                try
                {
                    listener.BeginAcceptSocket(new AsyncCallback(ReceiveConn), listener);
                }
                catch
                {
                    if (!disposed)
                        Rebuild();
                }
                return;
            }

            if (disposed)
                return;

            if (client != null)
            {
                // Create the client chain and register the client in the Tcp Manager
                IPEndPoint clientEndPoint;
                DataPipe chain = null;
                try
                {
                    clientEndPoint = (IPEndPoint)client.RemoteEndPoint;

                    chain = DataPipe.CreateServerTcp(this.server, client);

                    server.RegisterClient(chain);

                    // Send version
                    DataPacket packet = DataPacket.Create(16);
                    packet.Sender = ipSource;
                    packet.Destination = clientEndPoint;
                    packet.Command = 0;
                    packet.DataType = 1;
                    packet.DataCount = (uint)CAConstants.CA_MINOR_PROTOCOL_REVISION;
                    packet.Parameter1 = 0;
                    packet.Parameter2 = 0;
                    client.Send(packet.Data, 0, packet.Data.Length, SocketFlags.None);
                }
                catch (Exception ex)
                {
                    try
                    {
                        if (chain != null)
                            chain.Dispose();
                    }
                    catch
                    {
                    }
                }
            }

            // Wait for the next one
            try
            {
                listener.BeginAcceptSocket(new AsyncCallback(ReceiveConn), listener);
            }
            /*catch (ObjectDisposedException)
            {
                return;
            }*/
            catch (Exception ex)
            {
                if (!disposed)
                    Rebuild();
            }
        }

        public void Dispose()
        {
            if (disposed)
                return;
            disposed = true;
            tcpListener.Stop();

            try
            {
                TcpClient client = new TcpClient(ipSource);
                client.Close();
            }
            catch
            {
            }
        }
    }
}
