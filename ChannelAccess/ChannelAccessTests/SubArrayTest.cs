using EpicsSharp.ChannelAccess.Client;
using EpicsSharp.ChannelAccess.Server;
using EpicsSharp.ChannelAccess.Server.RecordTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace EpicsSharp.ChannelAccess.Tests
{
    [TestClass]
    public class SubArrayTest
    {
        private const string IntArrayChannelName = "TEST:INTARR";
        private const string IntSubArrayChannelName = "TEST:INTSUBARR";

        private const string FloatSubArrayChannelName = "TEST:FLOATSUBARR";

        private const string ByteSubArrayChannelName = "TEST:BYTESUBARR";

        private const int TIMEOUT = 2000;

        private CAServer Server;
        private CAClient Client;

        private CAIntArrayRecord IntArrayRecord;
        private CASubArrayRecord<int> IntSubArrayRecord;

        private CASubArrayRecord<float> FloatSubArrayRecord;

        private CASubArrayRecord<byte> ByteSubArrayRecord;

        [TestInitialize]
        public void Initialize()
        {
            Server = new CAServer(IPAddress.Parse("127.0.0.1"));
            Client = new CAClient();
            Client.Configuration.SearchAddress = "127.0.0.1";
            Client.Configuration.WaitTimeout = TIMEOUT;

            var countChange = new AutoResetEvent(false);
            long count = 3;

            IntArrayRecord = Server.CreateArrayRecord<CAIntArrayRecord>(IntArrayChannelName, 20);
            IntSubArrayRecord = Server.CreateSubArrayRecord(IntSubArrayChannelName, IntArrayRecord);
            for (var i = 0; i < IntArrayRecord.Value.Length; i++)
                IntArrayRecord.Value[i] = i;

            FloatSubArrayRecord = Server.CreateSubArrayRecord<CAFloatSubArrayRecord>(FloatSubArrayChannelName, 10);
            for (byte i = 0; i < FloatSubArrayRecord.FullArray.Length; i++)
                FloatSubArrayRecord.FullArray[i] = i;

            ByteSubArrayRecord = Server.CreateSubArrayRecord<CAByteSubArrayRecord>(ByteSubArrayChannelName, 35);
            var str = "Hello world";
            var bytes = Encoding.ASCII.GetBytes(str);
            for (byte i = 0; i < bytes.Length; i++)
                ByteSubArrayRecord.FullArray[i] = bytes[i];
            ByteSubArrayRecord.Scan = Constants.ScanAlgorithm.HZ10;

            void ProcessedHandler(object obj, EventArgs args)
            {
                Interlocked.Decrement(ref count);
                countChange.Set();
            };

            IntArrayRecord.RecordProcessed += ProcessedHandler;
            FloatSubArrayRecord.RecordProcessed += ProcessedHandler;
            ByteSubArrayRecord.RecordProcessed += ProcessedHandler;

            while (Interlocked.Read(ref count) > 0)
            {
                if (!countChange.WaitOne(TIMEOUT))
                {
                    Server.Dispose();
                    throw new Exception("Timed out");
                }
            }
            Server.Start();
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

        [TestMethod]
        [Timeout(15000)]
        public void TestByteSubArrayGet()
        {
            var byteSubArrayChannel = Client.CreateChannel<byte[]>(ByteSubArrayChannelName);
            var bytes = byteSubArrayChannel.Get<byte[]>(11);
            var str = Encoding.ASCII.GetString(bytes);
            Assert.AreEqual("Hello world", str);
        }

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