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
using EpicsSharp.ChannelAccess.Client;
using EpicsSharp.ChannelAccess.Server;
using EpicsSharp.ChannelAccess.Server.RecordTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PerformanceCheck
{
    class Program
    {
        static void Main(string[] args)
        {
            DoubleEvents();
            Console.ReadKey();
        }

        static void DoubleEvents()
        {
            CAClient client = new CAClient();
            client.Configuration.SearchAddress = "127.0.0.1";
            client.Configuration.WaitTimeout = 500;  // .5 seconds

            CAServer server = new CAServer(IPAddress.Parse("127.0.0.1"));
            CADoubleRecord record = server.CreateRecord<CADoubleRecord>("TEST:DBL");
            record.Scan = EpicsSharp.ChannelAccess.Constants.ScanAlgorithm.ON_CHANGE;
            record.Value = 0;

            long nbEvents = 0;

            //AutoResetEvent waitOne = new AutoResetEvent(false);
            Channel<double> channel = client.CreateChannel<double>("TEST:DBL");
            channel.MonitorChanged += delegate(Channel<double> c, double d)
            {
                nbEvents++;
                record.Value = nbEvents;
            };

            Stopwatch sw = new Stopwatch();
            sw.Start();

            while (sw.Elapsed.TotalSeconds < 10)
            {
                //Console.WriteLine(sw.Elapsed.TotalSeconds);
                Console.Write(string.Format("Time remaining {0:0.0}   \r", 10 - sw.Elapsed.TotalSeconds));
                Thread.Sleep(100);
            }
            Console.WriteLine("NB Double events: " + (nbEvents/10) + " / sec.           ");
            server.Dispose();
            client.Dispose();
        }
    }
}
