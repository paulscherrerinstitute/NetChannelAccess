using System;
using System.Net;
using System.Threading;
using EpicsSharp.ChannelAccess.Client;
using EpicsSharp.ChannelAccess.Server;
using EpicsSharp.ChannelAccess.Server.RecordTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EpicsSharp.ChannelAccess.Tests
{
    [TestClass]
    public class ArrayTest
    {
        private const string IntArrayChannelName = "TEST:INTARR";
        private const int TIMEOUT = 2000;

        private CAServer Server;
        private CAClient Client;
        private CAIntArrayRecord IntArrayRecord;

        [TestInitialize]
        public void Initialize()
        {
            Server = new CAServer(IPAddress.Parse("127.0.0.1"));
            Client = new CAClient();
            Client.Configuration.SearchAddress = "127.0.0.1";
            Client.Configuration.WaitTimeout = TIMEOUT;

            IntArrayRecord = Server.CreateArrayRecord<CAIntArrayRecord>(IntArrayChannelName, 20);
            for (var i = 0; i < IntArrayRecord.Value.Length; i++)
                IntArrayRecord.Value[i] = i;
            Server.Start();

            AutoResetEvent waitOne = new AutoResetEvent(false);
            IntArrayRecord.RecordProcessed += (obj, args) =>
            {
                waitOne.Set();
            };
            waitOne.WaitOne();
        }

        [TestCleanup]
        public void Cleanup()
        {
            Server.Dispose();
            Client.Dispose();
        }

        [TestMethod]
        [Timeout(15000)]
        public void TestIntArrayGet()
        {
            var intArrayChannel = Client.CreateChannel<int[]>(IntArrayChannelName);

            var intArrayResponse = intArrayChannel.Get<int[]>();
            Assert.AreEqual(20, intArrayResponse.Length);

            intArrayResponse = intArrayChannel.Get<int[]>(3);
            Assert.AreEqual(3, intArrayResponse.Length);
            CollectionAssert.AreEqual(new int[] { 0, 1, 2 }, intArrayResponse);
        }
    }
}
