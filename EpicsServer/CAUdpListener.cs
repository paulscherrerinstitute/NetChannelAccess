using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace CaSharpServer
{
    /// <summary>
    /// Answers to the search requests
    /// </summary>
    internal class CAUdpListener : IDisposable
    {
        Socket UDPSocket;
        byte[] buff = new byte[300000];
        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        CAServerFilter filter;
        bool running = true;

        /// <summary>
        /// Bind to the
        /// </summary>
        /// <param name="port"></param>
        public CAUdpListener(IPAddress serverAddress, int port, CAServerFilter filter)
        {
            this.filter = filter;

            UDPSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            UDPSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
            UDPSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            UDPSocket.Bind(new IPEndPoint(serverAddress, port));
            EndPoint tempRemoteEP = (EndPoint)sender;
            UDPSocket.BeginReceiveFrom(buff, 0, buff.Length, SocketFlags.None, ref tempRemoteEP, GotUdpMessage, tempRemoteEP);
        }

        void GotUdpMessage(IAsyncResult ar)
        {
            IPEndPoint ipeSender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint epSender = (EndPoint)ipeSender;
            int size = 0;

            try

            {
                size = UDPSocket.EndReceiveFrom(ar, ref epSender);
            }
            catch(Exception ex)
            {
                if (running)
                    throw ex;
                return;
            }

            string senderAddress = sender.Address.ToString();
            int senderPort = sender.Port;

            // Get the data back
            /*byte[] data = new byte[buff.Length];
            buff.CopyTo(data, 0);*/
            Pipe pipe = new Pipe();
            //pipe.Write(data, 0, size);
            pipe.Write(buff, 0, size);
            filter.ProcessReceivedData(pipe, epSender, size, false);

            // Start Accepting again
            UDPSocket.BeginReceiveFrom(buff, 0, buff.Length, SocketFlags.None, ref epSender, GotUdpMessage, epSender);
        }

        internal void Send(byte[] data, IPEndPoint receiver)
        {
            lock (UDPSocket)
            {
                UDPSocket.SendTo(data, receiver);
            }
        }

        public void Dispose()
        {
            if (running == false)
                return;
            running = false;
            UDPSocket.Shutdown(SocketShutdown.Both);
            UDPSocket.Close();
        }
    }
}
