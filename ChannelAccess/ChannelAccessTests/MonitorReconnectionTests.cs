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
using System.Diagnostics;
using System.Net;
using System.Threading;

namespace EpicsSharp.ChannelAccess.Tests
{
    [TestClass]
    public class MonitorReconnectionTests
    {
        private CAServer server;
        private CAIntRecord record;
        private AutoResetEvent eventMontiorReceived = new AutoResetEvent(false);


        public void StartServer()
        {
            server = new CAServer(IPAddress.Parse("127.0.0.1"));
            record = server.CreateRecord<CAIntRecord>("SECOND");
            record.PrepareRecord += record_PrepareRecord;
            record.Scan = Constants.ScanAlgorithm.HZ2;
            server.Start();
        }

        void record_PrepareRecord(object sender, EventArgs e)
        {
            record.Value = DateTime.Now.Second;
        }

        public void StopServer()
        {
            server.Dispose();
            server = null;
        }

        [TestCleanup]
        public void TearDown()
        {
            if (server != null)
                StopServer();
        }

        [TestMethod]
        [Timeout(3000 * 10)]
        public void TestMonitorReconnection()
        {
            const int MAX_WAIT = 500;
            // Unfortunately the problem does not always occur.
            // Let's try to repeat this test, until it breaks, for ... EVER!
            // NOTE: We need to change that later to a more useful value
            //for (long i = 1; ; i++)
            for (long i = 1; i < 10; i++)
            {
                StartServer();
                CAClient client = new CAClient();
                client.Configuration.WaitTimeout = MAX_WAIT;
                client.Configuration.SearchAddress = "127.0.0.1";
                var channel = client.CreateChannel<int>("SECOND");
                channel.MonitorChanged += channel_MonitorChanged;
                Assert.IsTrue(eventMontiorReceived.WaitOne(MAX_WAIT), "Iteration {0}: No monitor events received (check 1 of 2)", i);
                StopServer();
                Thread.Sleep(200);  // give it a bit of time to settle (server is multi-threaded)
                eventMontiorReceived.Reset();
                Assert.IsFalse(eventMontiorReceived.WaitOne(MAX_WAIT), "Iteration {0}: Events received although server is stopped", i);
                StartServer();
                Assert.IsTrue(eventMontiorReceived.WaitOne(MAX_WAIT), "Iteration {0}: No monitor events received (check 2 of 2)", i);
                StopServer();
            }
        }

        void channel_MonitorChanged(Channel<int> sender, int newValue)
        {
            eventMontiorReceived.Set();
        }
    }
}
