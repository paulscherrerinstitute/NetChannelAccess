/*
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
using EpicsSharp.ChannelAccess.Server;
using EpicsSharp.ChannelAccess.Server.RecordTypes;
using System;
using System.Net;

namespace EpicsSharp.ChannelAccess.Examples
{
    /// <summary>
    /// An example Channel Access server.
    /// 
    /// This class demonstrates the server side usage of EpicsSharp,
    /// i.e. the EpicsSharp.ChannelAccess.Server.CAServer class.
    /// </summary>
    internal class ExampleServer
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("EpicsSharp Channel Access Example Server");
            Console.WriteLine("----------------------------------------");
            Console.WriteLine();

            CAServer server = new CAServer(IPAddress.Parse("129.129.194.45"), 5055, 5055);
            Console.WriteLine("Listening on:");
            Console.WriteLine("  TCP port {0}", server.TcpPort);
            Console.WriteLine("  UDP port {0}", server.UdpPort);
            Console.WriteLine();

            CAStringRecord record = server.CreateRecord<CAStringRecord>("MY-STRING");
            record.Value = "Hello, EPICS world!";
            Console.WriteLine("Registered PV 'MY-STRING'");
            Console.WriteLine();

            Console.WriteLine("Press any key to quit");
            Console.ReadKey();
        }
    }
}
