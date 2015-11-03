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

        [TestInitialize]
        public void SetUp()
        {
            server = new CAServer(IPAddress.Parse("127.0.0.1"));
            client = new CAClient();
            client.Configuration.SearchAddress = "127.0.0.1";
            client.Configuration.WaitTimeout = 500;  // .5 seconds

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
            AutoResetEvent waitOne = new AutoResetEvent(false);
            record.RecordProcessed += delegate(object obj, EventArgs args)
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
        public void TestExtTimeDouble()
        {
            var c = client.CreateChannel<ExtTimeType<double>>("TEST:DBL");
            var r = c.Get();

            Assert.AreEqual(r.Severity, EpicsSharp.ChannelAccess.Constants.AlarmSeverity.MAJOR);
            Assert.AreEqual(r.Value, 10.0);
            Assert.AreEqual((DateTime.Now - r.Time).TotalSeconds < 1, true);
        }

        [TestMethod]
        public void TestExtDouble()
        {
            var c = client.CreateChannel<ExtType<double>>("TEST:DBL");
            var r = c.Get();

            Assert.AreEqual(r.Value, 10.0);
            Assert.AreEqual(r.Severity, EpicsSharp.ChannelAccess.Constants.AlarmSeverity.MAJOR);
        }

        //////////////////////////////////////////////

        [TestMethod]
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
        public void TestExtTimeInt()
        {
            var c = client.CreateChannel<ExtTimeType<int>>("TEST:DBL");
            var r = c.Get();

            Assert.AreEqual(r.Severity, EpicsSharp.ChannelAccess.Constants.AlarmSeverity.MAJOR);
            Assert.AreEqual(r.Value, 10);
            Assert.AreEqual((DateTime.Now - r.Time).TotalSeconds < 1, true);
        }

        [TestMethod]
        public void TestExtInt()
        {
            var c = client.CreateChannel<ExtType<int>>("TEST:DBL");
            var r = c.Get();

            Assert.AreEqual(r.Value, 10);
            Assert.AreEqual(r.Severity, EpicsSharp.ChannelAccess.Constants.AlarmSeverity.MAJOR);
        }

        //////////////////////////////////////////////

        [TestMethod]
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
        public void TestExtTimeFloat()
        {
            var c = client.CreateChannel<ExtTimeType<float>>("TEST:DBL");
            var r = c.Get();

            Assert.AreEqual(r.Severity, EpicsSharp.ChannelAccess.Constants.AlarmSeverity.MAJOR);
            Assert.AreEqual(r.Value, 10f);
            Assert.AreEqual((DateTime.Now - r.Time).TotalSeconds < 1, true);
        }

        [TestMethod]
        public void TestExtFloat()
        {
            var c = client.CreateChannel<ExtType<float>>("TEST:DBL");
            var r = c.Get();

            Assert.AreEqual(r.Value, 10f);
            Assert.AreEqual(r.Severity, EpicsSharp.ChannelAccess.Constants.AlarmSeverity.MAJOR);
        }

        //////////////////////////////////////////////

        [TestMethod]
        public void TestExtControlString()
        {
            var c = client.CreateChannel<ExtControl<string>>("TEST:DBL");
            var r = c.Get();

            Assert.AreEqual(r.Severity, EpicsSharp.ChannelAccess.Constants.AlarmSeverity.MAJOR);
            Assert.AreEqual(r.Value, "10");
        }

        [TestMethod]
        public void TestExtGraphicString()
        {
            var c = client.CreateChannel<ExtGraphic<string>>("TEST:DBL");
            var r = c.Get();

            Assert.AreEqual(r.Severity, EpicsSharp.ChannelAccess.Constants.AlarmSeverity.MAJOR);
            Assert.AreEqual(r.Value, "10");
        }

        [TestMethod]
        public void TestExtTimeString()
        {
            var c = client.CreateChannel<ExtTimeType<string>>("TEST:DBL");
            var r = c.Get();

            Assert.AreEqual(r.Severity, EpicsSharp.ChannelAccess.Constants.AlarmSeverity.MAJOR);
            Assert.AreEqual(r.Value, "10");
        }

        [TestMethod]
        public void TestExtString()
        {
            var c = client.CreateChannel<ExtType<string>>("TEST:DBL");
            var r = c.Get();

            Assert.AreEqual(r.Value, "10");
            Assert.AreEqual(r.Severity, EpicsSharp.ChannelAccess.Constants.AlarmSeverity.MAJOR);
        }
    }
}
