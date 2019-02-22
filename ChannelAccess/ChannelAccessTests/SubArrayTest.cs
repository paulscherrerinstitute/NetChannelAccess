using EpicsSharp.ChannelAccess.Client;
using EpicsSharp.ChannelAccess.Server;
using EpicsSharp.ChannelAccess.Server.RecordTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Net;
using System.Threading;

namespace EpicsSharp.ChannelAccess.Tests
{
    [TestClass]
    public class SubArrayTest
    {
        private const string IntArrayChannelName = "TEST:INTARR";
        private const string IntSubArrayChannelName = "TEST:INTSUBARR";
        private const string FloatArrayChannelName = "TEST:FLOATARR";
        private const string FloatSubArrayChannelName = "TEST:FLOATSUBARR";
        private const int TIMEOUT = 2000;

        private CAServer Server;
        private CAClient Client;
        private CAIntArrayRecord IntArrayRecord;
        private CASubArrayRecord<int> IntSubArrayRecord;
        private CAFloatArrayRecord FloatArrayRecord;
        private CASubArrayRecord<float> FloatSubArrayRecord;

        [TestInitialize]
        public void Initialize()
        {
            Server = new CAServer(IPAddress.Parse("127.0.0.1"));
            Client = new CAClient();
            Client.Configuration.SearchAddress = "127.0.0.1";
            Client.Configuration.WaitTimeout = TIMEOUT;

            IntArrayRecord = Server.CreateArrayRecord<CAIntArrayRecord>(IntArrayChannelName, 20);
            IntSubArrayRecord = Server.CreateSubArrayRecord(IntSubArrayChannelName, IntArrayRecord);
            IntSubArrayRecord.MaxLength = IntSubArrayRecord.FullArrayLength;
            for (var i = 0; i < IntArrayRecord.Value.Length; i++)
                IntArrayRecord.Value[i] = i;

            FloatArrayRecord = Server.CreateArrayRecord<CAFloatArrayRecord>(FloatArrayChannelName, 10);
            FloatSubArrayRecord = Server.CreateSubArrayRecord(FloatSubArrayChannelName, FloatArrayRecord);
            FloatSubArrayRecord.MaxLength = FloatSubArrayRecord.FullArrayLength;
            for (byte i = 0; i < FloatArrayRecord.Value.Length; i++)
                FloatArrayRecord.Value[i] = i;

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
        public void TestIntSubArrayGet()
        {
            var intSubArrayChannel = Client.CreateChannel<int[]>(IntSubArrayChannelName);

            IntSubArrayRecord.Index = 9;
            IntSubArrayRecord.Length = 5;
            var intSubArrayResponse = intSubArrayChannel.Get<int[]>();
            CollectionAssert.AreEqual(new int[] { 9, 10, 11, 12, 13 }, intSubArrayResponse);

            IntSubArrayRecord.Index = 0;
            IntSubArrayRecord.Length = 4;
            intSubArrayResponse = intSubArrayChannel.Get<int[]>();
            CollectionAssert.AreEqual(new int[] { 0, 1, 2, 3 }, intSubArrayResponse);

            IntSubArrayRecord.Index = 1;
            IntSubArrayRecord.Length = 6;
            intSubArrayResponse = intSubArrayChannel.Get<int[]>(5);  // Take only first 5 of the subarray
            CollectionAssert.AreEqual(new int[] { 1, 2, 3, 4, 5 }, intSubArrayResponse);
        }

        [TestMethod]
        [Timeout(15000)]
        public void TestFloatSubArrayGet()
        {
            var floatSubArrayChannel = Client.CreateChannel<float[]>(FloatSubArrayChannelName);

            FloatSubArrayRecord.Index = 9;
            FloatSubArrayRecord.Length = 5;
            var floatSubArrayResponse = floatSubArrayChannel.Get<float[]>();
            CollectionAssert.AreEqual(new float[] { 9 }, floatSubArrayResponse);

            FloatSubArrayRecord.Index = 0;
            FloatSubArrayRecord.Length = 4;
            floatSubArrayResponse = floatSubArrayChannel.Get<float[]>();
            CollectionAssert.AreEqual(new float[] { 0, 1, 2, 3 }, floatSubArrayResponse);

            FloatSubArrayRecord.Index = 1;
            FloatSubArrayRecord.Length = 6;
            floatSubArrayResponse = floatSubArrayChannel.Get<float[]>(5); // Take only first 5 of the subarray
            CollectionAssert.AreEqual(new float[] { 1, 2, 3, 4, 5 }, floatSubArrayResponse);
        }

        //[TestMethod]
        //[Timeout(15000)]
        //public void TestClientConfiguredIntSubArray()
        //{
        //    var intSubArrayChannel = Client.CreateChannel<ExtControl<int[]>>(IntSubArrayChannelName);
        //    var control = intSubArrayChannel.Get();

        //    Assert.Fail("Not implemented");
        //}

        [TestMethod]
        [Timeout(5000)]
        public void TestSubArrayMonitor()
        {
            var intSubArrayChannel = Client.CreateChannel<int[]>(IntSubArrayChannelName);
            AutoResetEvent resetEvent = new AutoResetEvent(false);

            int index = 0;
            var numCalled = 0;
            void Handler(Channel<int[]> c, int[] v)
            {
                switch (numCalled)
                {
                    case 0:
                        CollectionAssert.AreEqual(new int[] { 0, 1, 2, 3 }, v);
                        index++;
                        break;
                    case 1:
                        CollectionAssert.AreEqual(new int[] { 1, 2, 3, 4 }, v);
                        resetEvent.Set();
                        break;
                    default:
                        resetEvent.Set();
                        break;
                }
                numCalled++;
            };

            IntSubArrayRecord.MaxLength = IntArrayRecord.Value.Length;
            IntSubArrayRecord.Length = 4;
            IntSubArrayRecord.Index = 0;
            void Prepare(object sender, EventArgs e)
            {
                IntSubArrayRecord.Index = index; // Will trigger "Dirty" status if changed
            }

            IntSubArrayRecord.Scan = Constants.ScanAlgorithm.HZ10;
            IntSubArrayRecord.PrepareRecord += Prepare;
            intSubArrayChannel.MonitorChanged += Handler;

            resetEvent.WaitOne();
        }
    }
}