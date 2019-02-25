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
using EpicsSharp.ChannelAccess.Constants;
using EpicsSharp.ChannelAccess.Server;
using EpicsSharp.ChannelAccess.Server.RecordTypes;
using System;

namespace SubArrayDemo
{
    public class Program
    {
        public const string ArrayChannel = "MPC2000:ARR";
        public const string SubArrayChannel = "MPC2000:SUBARR";

        public static void Main(string[] args)
        {
            using (var server = new CAServer())
            {
                var intArrChannel = server.CreateArrayRecord<CAIntArrayRecord>(ArrayChannel, 20);
                var subArrChannel = server.CreateSubArrayRecord(SubArrayChannel, intArrChannel);
                subArrChannel.Scan = ScanAlgorithm.SEC1;

                // Fill array
                for (var i = 0; i < 20; i++)
                    intArrChannel.Value[i] = i;

                subArrChannel.Index = 0;
                subArrChannel.Length = 10;

                // Shift subarray position and length every second
                var counter = 0;
                subArrChannel.PrepareRecord += (s, e) =>
                {
                    counter++;
                    counter = counter > 5 ? 0 : counter;
                    subArrChannel.Index = counter;
                    subArrChannel.Length = 10 + counter;
                };

                server.Start();
                Console.WriteLine("Server started");

                using (var client = new CAClient())
                {
                    var arrayChannel = client.CreateChannel<int[]>(ArrayChannel);
                    var subArrayChannel = client.CreateChannel<int[]>(SubArrayChannel);

                    void Handler(Channel<int[]> s, int[] v)
                    {
                        Console.WriteLine($"{s.ChannelName}: {string.Join(", ", v)}");
                    };
                    arrayChannel.MonitorChanged += Handler;
                    subArrayChannel.MonitorChanged += Handler;
                    Console.ReadKey();
                    subArrayChannel.MonitorChanged -= Handler;
                    arrayChannel.MonitorChanged -= Handler;
                }
            }
        }
    }
}
