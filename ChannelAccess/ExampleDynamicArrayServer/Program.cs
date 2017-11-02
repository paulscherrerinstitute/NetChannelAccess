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
using System.Threading;

namespace ExampleDynamicArray
{

    /// <summary>
    /// Example Server 
    /// </summary>
    public class Program
    {

        public static void Main(string[] args)
        {
            using (var server = new CAServer())
            {
                using (var client = new CAClient())
                {
                    var dynArrChannel = server.CreateArrayRecord<CAIntSubArrayRecord>("MPC2000:DYNARR", 20);
                    for (var i = 0; i < 20; i++)
                    {
                        dynArrChannel.Value.Data[i] = i;
                    }
                    server.Start();

                    var autoEvt = new AutoResetEvent(false);

                    var clientChannel = client.CreateChannel<int[]>("MPC2000:DYNARR");
                    clientChannel.WishedDataCount = 0;
                    clientChannel.MonitorChanged += (s, v) =>
                    {
                        Console.WriteLine($"ClientArray(length={v.Length}): {string.Join(", ", v)}");
                        autoEvt.Set();
                    };

                    autoEvt.WaitOne();

                    dynArrChannel.Value.SetSubArray(0, 3);

                    autoEvt.WaitOne();

                    dynArrChannel.Value.SetSubArray(1, 3);

                    autoEvt.WaitOne();

                    dynArrChannel.Value.SetSubArray(2, 4);

                    /*// Fill array
                    for (var i = 0; i < 20; i++)
                    {
                        dynArrChannel.Value.Data[i] = i;
                    }
                    dynArrChannel.Value.SetSubArray(0, 10);

                    // Shift subarray position and length every 2 seconds
                    var counter = 0;
                    dynArrChannel.PrepareRecord += (s, e) =>
                    {
                        counter++;
                        counter = counter > 5 ? 0 : counter;
                        dynArrChannel.Value.SetSubArray(counter, 10 + counter);
                    };*/


                    Console.ReadKey();
                }
            }

            /*using (var server = new CAServer())
            {
                var dynArrChannel = server.CreateArrayRecord<CAIntSubArrayRecord>("MPC2000:DYNARR", 20);
                dynArrChannel.Scan = EpicsSharp.ChannelAccess.Constants.ScanAlgorithm.SEC1;

                // Fill array
                for (var i = 0; i < 20; i++)
                {
                    dynArrChannel.Value.Data[i] = i;
                }
                dynArrChannel.Value.SetSubArray(0, 10);

                // Shift subarray position and length every 2 seconds
                var counter = 0;
                dynArrChannel.PrepareRecord += (s, e) =>
                {
                    counter++;
                    counter = counter > 5 ? 0 : counter;
                    dynArrChannel.Value.SetSubArray(counter, 10 + counter);
                };

                server.Start();

                using (var client = new CAClient())
                {
                    var clientChannel = client.CreateChannel<int[]>("MPC2000:DYNARR");
                    clientChannel.WishedDataCount = 0;
                    clientChannel.MonitorChanged += (s, v) =>
                    {
                        Console.WriteLine($"ClientArray(length={v.Length}): {string.Join(", ", v)}");
                    };

                    Console.ReadKey();
                }
            }*/
        }
    }
}
