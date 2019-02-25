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
using EpicsSharp.ChannelAccess.Client;
using EpicsSharp.ChannelAccess.Server;
using EpicsSharp.ChannelAccess.Server.RecordTypes;
using System;
using System.Threading;

namespace Sandbox
{
    public enum MyEnum : int
    {
        Alain,
        Krempaska,
        Daniel
    }

    internal enum GoodEnumU8 : byte { zero, one, two }

    internal class Program
    {
        private static void Main(string[] args)
        {
            var client = new CAClient(5432);
            //client.Configuration.SearchAddress = "129.129.194.45:5432";

            //client.Configuration.SearchAddress = "129.129.194.45:5055";
            client.Configuration.SearchAddress = "129.129.130.255:5064";
            //client.Configuration.SearchAddress = "129.129.130.255:5064;129.129.131.255:5064;129.129.137.255:5064;129.129.158.255:5064";
            client.Configuration.WaitTimeout = 2000;
            //var channel = client.CreateChannel<string>("SYSAD-AB:HOSTNAME");
            //var channel = client.CreateChannel<string>("ARIDI-PCT:CURRENT");
            //var r = channel.Get();
            var channel = client.CreateChannel<int[]>("DISKFREE-AB-LT:ARR-M");
            //channel.MonitorMask = EpicsSharp.ChannelAccess.Constants.MonitorMask.ALL;

            /*int sequenceNumber = 0;
            channel.StatusChanged += (sender, newStatus) =>
            {
                Console.WriteLine(
                  "{0} : status {1}",
                  sender.ChannelName,
                  newStatus
                );
            };

            channel.MonitorChanged += (sender, newValue) =>
            {
                Console.WriteLine(
                  $"{sender.ChannelName} : {newValue.Length} array elements ; #{sequenceNumber++}"
                );
            };*/

            var r = channel.Get<int[]>();


            System.Console.ReadLine();
        }

        internal static void S2()
        {
            //CAServer server = new CAServer(System.Net.IPAddress.Parse("129.129.194.45"), 5432, 5432);
            CAServer server = new CAServer(System.Net.IPAddress.Parse("127.0.0.1"), 5432, 5432);
            var record = server.CreateArrayRecord<CAIntArrayRecord>("BERTRAND:ARR", 196 * 196);
            var start = 0;
            record.Scan = EpicsSharp.ChannelAccess.Constants.ScanAlgorithm.HZ2;
            record.PrepareRecord += (sender, evt) =>
              {
                  for (var i = 0; i < record.Value.Length; i++)
                      record.Value[i] = start + i;
                  start++;
              };

            var r2 = server.CreateRecord<CAStringRecord>("BERTRAND:STR");
            r2.Value = "Hello there!";

            server.Start();

            //Thread.Sleep(1000);

            CAClient client = new CAClient();
            //client.Configuration.SearchAddress = "129.129.194.45:5432";
            client.Configuration.SearchAddress = "127.0.0.1:5432";
            var channel = client.CreateChannel<int[]>("BERTRAND:ARR");
            channel.MonitorMask = EpicsSharp.ChannelAccess.Constants.MonitorMask.ALL;

            channel.StatusChanged += (sender, newStatus) =>
            {
                Console.WriteLine(
                  "{0} : status {1}",
                  sender.ChannelName,
                  newStatus
                );
            };
            int sequenceNumber = 0;
            channel.WishedDataCount = 196 * 196;
            channel.MonitorChanged += (sender, newValue) =>
            {
                Console.WriteLine(
                  $"{sender.ChannelName} : {newValue.Length} array elements ; #{sequenceNumber++}"
                );
            };

            /*var chan2 = client.CreateChannel<string>("BERTRAND:STR");
            Console.WriteLine(chan2.Get());*/

            //var r = channel.Get();
            System.Console.ReadLine();
        }

        private static void S1()
        {


            //CAServer server = new CAServer(System.Net.IPAddress.Parse("129.129.130.44"), 5162, 5162);
            CAServer server = new CAServer(System.Net.IPAddress.Parse("129.129.130.44"), 5432, 5432);
            var record = server.CreateRecord<CAStringRecord>("BERTRAND:STR");
            record.Value = "Hello there!";
            record.Scan = EpicsSharp.ChannelAccess.Constants.ScanAlgorithm.HZ10;
            record.PrepareRecord += ((sender, e) => { record.Value = DateTime.Now.ToLongTimeString(); });

            var record2 = server.CreateRecord<CAStringRecord>("BERTRAND:STR2");
            record2.Value = "Hello there too!";
            record2.Scan = EpicsSharp.ChannelAccess.Constants.ScanAlgorithm.HZ10;
            record2.PrepareRecord += ((sender, e) => { record2.Value = DateTime.Now.ToLongTimeString(); });

            var record3 = server.CreateRecord<CAEnumRecord<MyEnum>>("BERTRAND:ENUM");
            record3.Value = MyEnum.Alain;
            record3.Scan = EpicsSharp.ChannelAccess.Constants.ScanAlgorithm.SEC1;
            record3.PrepareRecord += ((sender, e) => { record3.Value = 1 - record3.Value; });

            Thread.Sleep(1000);

            CAClient client = new CAClient();
            //client.Configuration.WaitTimeout = 1000;
            client.Configuration.SearchAddress = "129.129.130.44:5432";
            /*var c = client.CreateChannel<ExtControlEnum>("BERTRAND:ENUM");
            var r=c.Get();*/

            var record4 = server.CreateRecord<CAEnumRecord<GoodEnumU8>>("TEST");
            record4.Value = GoodEnumU8.two;

            var channel = client.CreateChannel<ExtControlEnum>("TEST");
            var result = channel.Get();


            Console.WriteLine("S to stop or start answering get");
            Console.WriteLine("Q to quit");
            Console.WriteLine("Running!");
            while (true)
            {
                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.S:
                        //server.StopGet = !server.StopGet;
                        //record2.Dispose();
                        break;
                    case ConsoleKey.Q:
                        return;
                    default:
                        break;
                }
            }


            /*CAClient client = new CAClient();
            client.Configuration.WaitTimeout = 1000;
            var c=client.CreateChannel("ARIDI-PCT:CURREsNT");
            Console.WriteLine(c.Get<string>());
            Console.ReadKey();*/
        }
    }
}
