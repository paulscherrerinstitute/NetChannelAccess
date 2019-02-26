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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EpicsSharp.ChannelAccess.Tests
{
    [TestClass]
    public class TestOperations
    {
        CAServer server;
        CAClient client;
        CADoubleRecord[] records;

        const int TIMEOUT = 3000;

        [TestInitialize]
        public void SetUp()
        {
            client = new CAClient();
            client.Configuration.SearchAddress = "127.0.0.1";
            client.Configuration.WaitTimeout = TIMEOUT;

            ServerInit();
        }

        void ServerInit()
        {
            server = new CAServer(IPAddress.Parse("127.0.0.1"));
            records = new CADoubleRecord[10];

            var countChange = new AutoResetEvent(false);
            long count = 10;

            for (var i = 0; i < 10; i++)
            {
                records[i] = server.CreateRecord<CADoubleRecord>("TEST:DBL:" + i);

                records[i].Scan = Constants.ScanAlgorithm.HZ10;
                records[i].LowAlarmLimit = 25;
                records[i].LowAlarmSeverity = Constants.AlarmSeverity.MINOR;
                records[i].LowLowAlarmLimit = 20;
                records[i].LowLowAlarmSeverity = Constants.AlarmSeverity.MAJOR;
                records[i].HighAlarmLimit = 100;
                records[i].HighAlarmSeverity = Constants.AlarmSeverity.MINOR;
                records[i].HighHighAlarmLimit = 105;
                records[i].HighHighAlarmSeverity = Constants.AlarmSeverity.MAJOR;
                records[i].EngineeringUnits = "My";

                records[i].RecordProcessed += (obj, args) =>
                {
                    Interlocked.Decrement(ref count);
                    countChange.Set();
                };
                records[i].Value = 10;

            }
            server.Start();

            while(Interlocked.Read(ref count) > 0)
            {
                if (!countChange.WaitOne(TIMEOUT))
                    throw new Exception("Timed out");
            }
        }

        [TestCleanup]
        public void TearDown()
        {
            server.Dispose();
            client.Dispose();
        }

        [TestMethod]
        [Timeout(3000)]
        public void TestGet()
        {
            var c = client.CreateChannel("TEST:DBL:5");
            var d = c.Get<double>();
            Assert.AreEqual(10, d);
        }

        [TestMethod]
        [Timeout(3000)]
        public void TestCheckInfo()
        {
            var c = client.CreateChannel("TEST:DBL:5");
            c.Connect();
            Assert.AreEqual(typeof(double), c.ChannelDefinedType);
        }

        [TestMethod]
        [Timeout(3000)]
        public void TestGetAsync()
        {
            var c = client.CreateChannel("TEST:DBL:5");
            var d = c.GetAsync<double>();
            d.Wait();
            Assert.AreEqual(10, d.Result);
        }

        [TestMethod]
        [Timeout(3000)]
        public void TestPut()
        {
            var c = client.CreateChannel("TEST:DBL:5");
            c.Put<double>(50);
            Assert.AreEqual(50, records[5].Value);
            AutoResetEvent waitOne = new AutoResetEvent(false);
            records[5].RecordProcessed += delegate (object obj, EventArgs args)
            {
                waitOne.Set();
            };
            waitOne.WaitOne();
            Assert.AreEqual(Constants.AlarmStatus.NO_ALARM, records[5].AlarmStatus);
        }

        [TestMethod]
        [Timeout(3000)]
        public void MultiGet()
        {
            List<Channel> channels = new List<Channel>();
            for (var i = 0; i < 10; i++)
                channels.Add(client.CreateChannel("TEST:DBL:" + i));
            var res = client.MultiGet<double>(channels);
            for (var i = 0; i < 10; i++)
                Assert.AreEqual(10.0, res[i]);
        }

        [TestMethod]
        [Timeout(3000)]
        public void MultiConnect()
        {
            List<Channel> channels = new List<Channel>();
            for (var i = 0; i < 10; i++)
                channels.Add(client.CreateChannel("TEST:DBL:" + i));
            client.MultiConnect(channels);
            for (var i = 0; i < 10; i++)
                Assert.AreEqual(Constants.ChannelStatus.CONNECTED, channels[i].Status);
        }

        [TestMethod]
        [Timeout(3000)]
        public void Monitor()
        {
            var channel = client.CreateChannel("TEST:DBL:5");
            AutoResetEvent waitOne = new AutoResetEvent(false);
            double findValue = 0;
            channel.MonitorChanged += delegate (Channel sender, object newValue)
            {
                findValue = (double)newValue;
                waitOne.Set();
            };

            records[5].Value = 10;
            waitOne.WaitOne();
            Assert.AreEqual(10.0, findValue);
        }

        [TestMethod]
        [Timeout(20000)]
        public void MonitorSequence()
        {
            var channel = client.CreateChannel("TEST:DBL:5");
            AutoResetEvent waitOne = new AutoResetEvent(false);
            double findValue = 0;
            channel.MonitorChanged += delegate (Channel sender, object newValue)
            {
                findValue = (double)newValue;
                Console.WriteLine("Got " + newValue);
                waitOne.Set();
            };
            waitOne.WaitOne();
            records[5].Value = 2;
            waitOne.WaitOne();
            records[5].Value = 3;
            waitOne.WaitOne();
            records[5].Value = 1;
            waitOne.WaitOne();
            Assert.AreEqual(1.0, findValue);
        }

        [TestMethod]
        [Timeout(12000)]
        public void DisconnectReconnect()
        {
            var channel = client.CreateChannel("TEST:DBL:5");
            AutoResetEvent waitOne = new AutoResetEvent(false);
            double findValue = 0;
            channel.MonitorChanged += delegate (Channel sender, object newValue)
            {
                findValue = (double)newValue;
                waitOne.Set();
            };
            if (!waitOne.WaitOne(TIMEOUT))
                throw new Exception("Timeout 1");
            //records[5].Value = 2;
            findValue = 0;
            server.Dispose();
            Thread.Sleep(100);
            ServerInit();
            if (!waitOne.WaitOne(TIMEOUT))
                throw new Exception("Timeout 2");
            Assert.AreEqual(10.0, findValue);
        }

    }
}
