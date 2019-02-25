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
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EpicsSharp.ChannelAccess.Client;
using System.Net;
using EpicsSharp.ChannelAccess.Server;
using EpicsSharp.ChannelAccess.Server.RecordTypes;

namespace EpicsSharp.ChannelAccess.Tests
{

    [TestClass]
    public class CAEnumRecordTypeTests
    {
        enum TestEnum : long {
            foo,
            bar,
            baz
        }

        // Good enum types -- all of these should work
        enum GoodEnumS8 : sbyte { zero, one, two }
        enum GoodEnumU8 : byte { zero, one, two }
        enum GoodEnumS16 : short { zero, one, two }
        enum GoodEnumU16 : ushort { zero, one, two }
        enum GoodEnumS32 : int { zero, one, two }
        enum GoodEnumU32 : uint { zero, one, two }
        enum GoodEnumS64 : long { zero, one, two }
        enum GoodEnumU64 : ulong { zero, one, two }

        // Bad enum types -- all of these should break, for various reasons
        enum BadEnumTooManyValues
        {
            val0,
            val1,
            val2,
            val3,
            val4,
            val5,
            val6,
            val7,
            val8,
            val9,
            val10,
            val11,
            val12,
            val13,
            val14,
            val15,   // 16 values is OK
            val16    // 17 is not
        }

        enum BadEnumTooLongValueName
        {
            abcdefghijklmnopqrstuvwxyz,   // length 26 is OK
            abcdefghijklmnopqrstuvwxyz0   // 27 is too long
        }

        enum BadEnumUnsupportedValue
        {
            maxval = 0xffff,   // <-- this value still fits in 16 bits
            valtoolarge        // <-- this value needs a 17th bit
        }

        CAServer server;
        CAClient client;

        const int TIMEOUT = 3000; 

        [TestInitialize]
        public void SetUp()
        {
            server = new CAServer(IPAddress.Parse("127.0.0.1"));
            client = new CAClient();
            client.Configuration.SearchAddress = "127.0.0.1";
            client.Configuration.WaitTimeout = TIMEOUT;
            server.Start();
        }

        [TestCleanup]
        public void TearDown()
        {
            server.Dispose();
            client.Dispose();
        }

        [TestMethod]
        [Timeout(3000)]
        public void TestGoodEnumTypes()
        {
            var recordS8 = server.CreateRecord<CAEnumRecord<GoodEnumS8>>("TESTENUMS8");
            var recordU8 = server.CreateRecord<CAEnumRecord<GoodEnumU8>>("TESTENUMU8");
            var recordS16 = server.CreateRecord<CAEnumRecord<GoodEnumS16>>("TESTENUMS16");
            var recordU16 = server.CreateRecord<CAEnumRecord<GoodEnumU16>>("TESTENUMU16");
            var recordS32 = server.CreateRecord<CAEnumRecord<GoodEnumS32>>("TESTENUMS32");
            var recordU32 = server.CreateRecord<CAEnumRecord<GoodEnumU32>>("TESTENUMU32");
            var recordS64 = server.CreateRecord<CAEnumRecord<GoodEnumS64>>("TESTENUMS64");
            var recordU64 = server.CreateRecord<CAEnumRecord<GoodEnumU64>>("TESTENUMU64");
        }

        [TestMethod]
        [Timeout(3000)]
        [ExpectedException(typeof(ArgumentException))]
        public void TestTooManyValuesThrowsException()
        {
            // Don't use server.CreateRecord here, because the
            // ArgumentException will be changed by the dynamic
            // constructor invocation in CreateRecord
            //var record = new CAEnumRecord<BadEnumTooManyValues>();
            var record = server.CreateRecord<CAEnumRecord<BadEnumTooManyValues>>("ZZZ");
        }

        [TestMethod]
        [Timeout(3000)]
        [ExpectedException(typeof(ArgumentException))]
        public void TestTooLongNameThrowsException()
        {
            // Don't use server.CreateRecord here, because the
            // ArgumentException will be changed by the dynamic
            // constructor invocation in CreateRecord
            //var record = new CAEnumRecord<BadEnumTooLongValueName>();
            var record = server.CreateRecord<CAEnumRecord<BadEnumTooLongValueName>>("ZZZ");
        }

        [TestMethod]
        [Timeout(3000)]
        [ExpectedException(typeof(ArgumentException))]
        public void TestUnsupportedValue()
        {
            // Don't use server.CreateRecord here, because the
            // ArgumentException will be changed by the dynamic
            // constructor invocation in CreateRecord
            //var record = new CAEnumRecord<BadEnumUnsupportedValue>();
            var record = server.CreateRecord<CAEnumRecord<BadEnumUnsupportedValue>>("ZZZ");
        }

        [TestMethod]
        [Timeout(3000)]
        public void TestReceivingEnumAsInt()
        {
            var record = server.CreateRecord<CAEnumRecord<GoodEnumU8>>("TEST");
            record.Value = GoodEnumU8.two;

            var channel = client.CreateChannel<int>("TEST");
            var result = channel.Get();
            Assert.AreEqual(result, (byte)record.Value);
        }

        [TestMethod]
        [Timeout(3000)]
        public void TestReceivingEnumAsString()
        {
            var record = server.CreateRecord<CAEnumRecord<GoodEnumU8>>("TEST");
            record.Value = GoodEnumU8.two;

            var channel = client.CreateChannel<string>("TEST");
            var result = channel.Get();
            Assert.AreEqual(result, "two");
        }

        [TestMethod]
        [Timeout(3000)]
        public void TestReceivingEnumList()
        {
            var record = server.CreateRecord<CAEnumRecord<GoodEnumU8>>("TEST");
            record.Value = GoodEnumU8.two;

            var channel = client.CreateChannel<ExtControlEnum>("TEST");
            var result = channel.Get();
            Assert.AreEqual(result.NbStates, 3);
            Assert.AreEqual(result.States[0], "zero");
            Assert.AreEqual(result.States[1], "one");
            Assert.AreEqual(result.States[2], "two");
            Assert.AreEqual(result.Value, 2);
        }
    }
}
