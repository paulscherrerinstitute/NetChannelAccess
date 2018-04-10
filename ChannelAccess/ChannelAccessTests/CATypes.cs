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
    public class CATypesTest
    {
        CAServer server;
        CAClient client;
        CADoubleRecord record;

        const int TIMEOUT = 3000;

        [TestInitialize]
        public void SetUp()
        {
            server = new CAServer(IPAddress.Parse("127.0.0.1"));
            client = new CAClient();
            client.Configuration.SearchAddress = "127.0.0.1";
            client.Configuration.WaitTimeout = TIMEOUT;

            record = server.CreateRecord<CADoubleRecord>("TEST:DBL");

            record.LowAlarmLimit = 25;
            record.LowAlarmSeverity = EpicsSharp.ChannelAccess.Constants.AlarmSeverity.MINOR;
            record.LowLowAlarmLimit = 20;
            record.LowLowAlarmSeverity = EpicsSharp.ChannelAccess.Constants.AlarmSeverity.MAJOR;

            record.HighAlarmLimit = 100;
            record.HighAlarmSeverity = EpicsSharp.ChannelAccess.Constants.AlarmSeverity.MINOR;
            record.HighHighAlarmLimit = 105;
            record.HighHighAlarmSeverity = EpicsSharp.ChannelAccess.Constants.AlarmSeverity.MAJOR;

            record.Value = 10;
            record.EngineeringUnits = "My";
            server.Start();
            AutoResetEvent waitOne = new AutoResetEvent(false);
            record.RecordProcessed += delegate (object obj, EventArgs args)
            {
                waitOne.Set();
            };
            waitOne.WaitOne();
        }

        [TestCleanup]
        public void TearDown()
        {
            server.Dispose();
            client.Dispose();
        }

        [TestMethod]
        [Timeout(3000)]
        public void TestExtControlDouble()
        {
            var c = client.CreateChannel<ExtControl<double>>("TEST:DBL");
            var r = c.Get();

            Assert.AreEqual(r.Severity, EpicsSharp.ChannelAccess.Constants.AlarmSeverity.MAJOR);
            Assert.AreEqual(r.Value, 10.0);
            Assert.AreEqual(r.LowAlertLimit, 20.0);
            Assert.AreEqual(r.EGU, "My");
        }

        [TestMethod]
        [Timeout(3000)]
        public void TestExtGraphicDouble()
        {
            var c = client.CreateChannel<ExtGraphic<double>>("TEST:DBL");
            var r = c.Get();

            Assert.AreEqual(r.Severity, EpicsSharp.ChannelAccess.Constants.AlarmSeverity.MAJOR);
            Assert.AreEqual(r.Value, 10.0);
            Assert.AreEqual(r.LowAlertLimit, 20.0);
            Assert.AreEqual(r.EGU, "My");
        }

        [TestMethod]
        [Timeout(3000)]
        public void TestExtTimeDouble()
        {
            var c = client.CreateChannel<ExtTimeType<double>>("TEST:DBL");
            var r = c.Get();

            Assert.AreEqual(r.Severity, EpicsSharp.ChannelAccess.Constants.AlarmSeverity.MAJOR);
            Assert.AreEqual(r.Value, 10.0);
            Assert.AreEqual((DateTime.Now - r.Time).TotalSeconds < 1, true);
        }

        [TestMethod]
        [Timeout(3000)]
        public void TestExtDouble()
        {
            var c = client.CreateChannel<ExtType<double>>("TEST:DBL");
            var r = c.Get();

            Assert.AreEqual(r.Value, 10.0);
            Assert.AreEqual(r.Severity, EpicsSharp.ChannelAccess.Constants.AlarmSeverity.MAJOR);
        }

        //////////////////////////////////////////////

        [TestMethod]
        [Timeout(3000)]
        public void TestExtControlInt()
        {
            var c = client.CreateChannel<ExtControl<int>>("TEST:DBL");
            var r = c.Get();

            Assert.AreEqual(r.Severity, EpicsSharp.ChannelAccess.Constants.AlarmSeverity.MAJOR);
            Assert.AreEqual(r.Value, 10);
            Assert.AreEqual(r.LowAlertLimit, 20.0);
            Assert.AreEqual(r.EGU, "My");
        }

        [TestMethod]
        [Timeout(3000)]
        public void TestExtGraphicInt()
        {
            var c = client.CreateChannel<ExtGraphic<int>>("TEST:DBL");
            var r = c.Get();

            Assert.AreEqual(r.Severity, EpicsSharp.ChannelAccess.Constants.AlarmSeverity.MAJOR);
            Assert.AreEqual(r.Value, 10);
            Assert.AreEqual(r.LowAlertLimit, 20.0);
            Assert.AreEqual(r.EGU, "My");
        }

        [TestMethod]
        [Timeout(3000)]
        public void TestExtTimeInt()
        {
            var c = client.CreateChannel<ExtTimeType<int>>("TEST:DBL");
            var r = c.Get();

            Assert.AreEqual(r.Severity, EpicsSharp.ChannelAccess.Constants.AlarmSeverity.MAJOR);
            Assert.AreEqual(r.Value, 10);
            Assert.AreEqual((DateTime.Now - r.Time).TotalSeconds < 1, true);
        }

        [TestMethod]
        [Timeout(3000)]
        public void TestExtInt()
        {
            var c = client.CreateChannel<ExtType<int>>("TEST:DBL");
            var r = c.Get();

            Assert.AreEqual(r.Value, 10);
            Assert.AreEqual(r.Severity, EpicsSharp.ChannelAccess.Constants.AlarmSeverity.MAJOR);
        }

        //////////////////////////////////////////////

        [TestMethod]
        [Timeout(3000)]
        public void TestExtControlFloat()
        {
            var c = client.CreateChannel<ExtControl<float>>("TEST:DBL");
            var r = c.Get();

            Assert.AreEqual(r.Severity, EpicsSharp.ChannelAccess.Constants.AlarmSeverity.MAJOR);
            Assert.AreEqual(r.Value, 10f);
            Assert.AreEqual(r.LowAlertLimit, 20.0);
            Assert.AreEqual(r.EGU, "My");
        }

        [TestMethod]
        [Timeout(3000)]
        public void TestExtGraphicFloat()
        {
            var c = client.CreateChannel<ExtGraphic<float>>("TEST:DBL");
            var r = c.Get();

            Assert.AreEqual(r.Severity, EpicsSharp.ChannelAccess.Constants.AlarmSeverity.MAJOR);
            Assert.AreEqual(r.Value, 10f);
            Assert.AreEqual(r.LowAlertLimit, 20.0);
            Assert.AreEqual(r.EGU, "My");
        }

        [TestMethod]
        [Timeout(3000)]
        public void TestExtTimeFloat()
        {
            var c = client.CreateChannel<ExtTimeType<float>>("TEST:DBL");
            var r = c.Get();

            Assert.AreEqual(r.Severity, EpicsSharp.ChannelAccess.Constants.AlarmSeverity.MAJOR);
            Assert.AreEqual(r.Value, 10f);
            Assert.AreEqual((DateTime.Now - r.Time).TotalSeconds < 1, true);
        }

        [TestMethod]
        [Timeout(3000)]
        public void TestExtFloat()
        {
            var c = client.CreateChannel<ExtType<float>>("TEST:DBL");
            var r = c.Get();

            Assert.AreEqual(r.Value, 10f);
            Assert.AreEqual(r.Severity, EpicsSharp.ChannelAccess.Constants.AlarmSeverity.MAJOR);
        }

        //////////////////////////////////////////////

        [TestMethod]
        [Timeout(3000)]
        public void TestExtControlString()
        {
            var c = client.CreateChannel<ExtControl<string>>("TEST:DBL");
            var r = c.Get();

            Assert.AreEqual(r.Severity, EpicsSharp.ChannelAccess.Constants.AlarmSeverity.MAJOR);
            Assert.AreEqual(r.Value, "10");
        }

        [TestMethod]
        [Timeout(3000)]
        public void TestExtGraphicString()
        {
            var c = client.CreateChannel<ExtGraphic<string>>("TEST:DBL");
            var r = c.Get();

            Assert.AreEqual(r.Severity, EpicsSharp.ChannelAccess.Constants.AlarmSeverity.MAJOR);
            Assert.AreEqual(r.Value, "10");
        }

        [TestMethod]
        [Timeout(3000)]
        public void TestExtTimeString()
        {
            var c = client.CreateChannel<ExtTimeType<string>>("TEST:DBL");
            var r = c.Get();

            Assert.AreEqual(r.Severity, EpicsSharp.ChannelAccess.Constants.AlarmSeverity.MAJOR);
            Assert.AreEqual(r.Value, "10");
        }

        [TestMethod]
        [Timeout(3000)]
        public void TestExtString()
        {
            var c = client.CreateChannel<ExtType<string>>("TEST:DBL");
            var r = c.Get();

            Assert.AreEqual(r.Value, "10");
            Assert.AreEqual(r.Severity, EpicsSharp.ChannelAccess.Constants.AlarmSeverity.MAJOR);
        }

        [TestMethod]
        [Timeout(5000)]
        public void TestAnswerToSearchAndMissConnect()
        {
            server.AcceptConnections = false;
            client.Configuration.EchoInterval = 0.5;
            client.Configuration.EchoSleeping = 2000;

            var c = client.CreateChannel<ExtType<string>>("TEST:DBL");
            ExtType<string> r = null;

            client.Configuration.WaitTimeout = 1000;
            AutoResetEvent waitOne = new AutoResetEvent(false);
            c.MonitorChanged += (chan, val) =>
            {
                r = val;
                waitOne.Set();
                //Console.WriteLine("Got " + val.Value);
            };

            if (waitOne.WaitOne(1000) == true)
                Assert.Fail("Recevied data");

            server.AcceptConnections = true;
            //Console.WriteLine("====== Now should accept ======");
            //Thread.Sleep(30000);
            if (!waitOne.WaitOne(8000))
                Assert.Fail("Never received data");

            Assert.AreEqual(r.Value, "10");
            Assert.AreEqual(r.Severity, EpicsSharp.ChannelAccess.Constants.AlarmSeverity.MAJOR);
        }
    }
}
