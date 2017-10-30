using System.Net;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EpicsSharp.ChannelAccess.Server;
using EpicsSharp.ChannelAccess.Client;
using EpicsSharp.ChannelAccess.Server.RecordTypes;


namespace EpicsSharp.ChannelAccess.Tests
{
    [TestClass]
    public class DynamicArrayTest
    {
        const string CHANNEL = "TEST:ISA";
        const int TIMEOUT = 2 * 1000;

        CAServer server;
        CAClient client;
        CAIntSubArrayRecord record;

        [TestInitialize]
        public void Initialize()
        {
            server = new CAServer(IPAddress.Parse("127.0.0.1"));
            client = new CAClient();
            client.Configuration.SearchAddress = "127.0.0.1";
            client.Configuration.WaitTimeout = TIMEOUT;

            record = server.CreateArrayRecord<CAIntSubArrayRecord>(CHANNEL, 20);
            for (var i = 0;i<record.Value.Data.Length;i++)
                record.Value.Data[i] = i;

            server.Start();

            AutoResetEvent waitOne = new AutoResetEvent(false);
            record.RecordProcessed += (obj, args) =>
            {
                waitOne.Set();
            };
            waitOne.WaitOne();
        }

        [TestCleanup]
        public void Cleanup()
        {
            server.Dispose();
            client.Dispose();
        }

        [TestMethod]
        public void TestSubArrayGet()
        {
            record.Value.SetSubArray(9, 5);
            var channel = client.CreateChannel<int[]>(CHANNEL);
            var response = channel.Get<int[]>(0);
            Assert.AreEqual(5, response.Length);
            Assert.IsTrue(new int[]{9, 10, 11, 12, 13}.SequenceEqual(response));

            record.Value.SetSubArray(0, 4);

            response = channel.Get<int[]>(0);
            Assert.AreEqual(4, response.Length);
            Assert.IsTrue(new int[]{0, 1, 2, 3}.SequenceEqual(response));

            record.Value.SetSubArray(0, 6);

            response = channel.Get<int[]>(0);
            Assert.AreEqual(6, response.Length);
            Assert.IsTrue(new int[] { 0, 1, 2, 3, 4, 5 }.SequenceEqual(response));
        }

        [TestMethod]
        public void TestSubArrayMonitor()
        {
            record.Value.SetSubArray(0, 4);
            var channel = client.CreateChannel<int[]>(CHANNEL);

            AutoResetEvent are = new AutoResetEvent(false);
            int[] lastValue = null;
            channel.MonitorChanged += (s, v) => 
            {
                are.Set();
                lastValue = v;
            };
            Assert.IsTrue(are.WaitOne(1000));
            Assert.IsTrue(new int[] { 0, 1, 2, 3}.SequenceEqual(lastValue));
            are.Reset();
            record.Value.SetSubArray(1, 4);
            Assert.IsTrue(are.WaitOne(1000));
            Assert.IsTrue(new int[] { 1, 2, 3, 4}.SequenceEqual(lastValue));

        }

        [TestMethod]
        public void TestSubArrayPut()
        {
            record.Value.SetSubArray(0, 4);
            var channel = client.CreateChannel<int[]>(CHANNEL);

            var putArr = new int[] { 3, 2, 1, 0 };
            channel.Put<int[]>(putArr);
            Assert.AreEqual(4, record.Value.Length);
            for (var i = 0;i<putArr.Length;i++)
            {
                Assert.AreEqual(putArr[i], record.Value[i]);
            }
        }

    }
}
