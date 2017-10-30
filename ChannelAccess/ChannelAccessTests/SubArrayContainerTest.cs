using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EpicsSharp.ChannelAccess.Server.RecordTypes;
using System.Linq;

namespace EpicsSharp.ChannelAccess.Tests
{
    [TestClass]
    public class SubArrayContainerTest
    {
        const int CONTAINER_SIZE = 20;

        SubArrayContainer<int> container;

        [TestInitialize]
        public void Initialize()
        {
            container = new SubArrayContainer<int>(CONTAINER_SIZE);
            for (var i = 0; i < CONTAINER_SIZE; i++)
                container.Data[i] = i;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException), "Too small INDX inappropriately allowed.")]
        public void TestTooSmallINDX()
        {
            container.SetSubArray(-10, 2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException), "Too big INDX inappropriately allowed.")]
        public void TestTooBigINDX()
        {
            container.SetSubArray(CONTAINER_SIZE + 1, 2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException), "Too small NELM inappropriately allowed.")]
        public void TestTooSmallNELM()
        {
            container.SetSubArray(0, -10);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException), "Too big NELM inappropriately allowed.")]
        public void TestTooBigNELM()
        {
            container.SetSubArray(0, CONTAINER_SIZE + 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException), "Too big INDX + NELM inappropriately allowed.")]
        public void TestTooBigINDX_NELM()
        {
            container.SetSubArray(5, CONTAINER_SIZE);
        }

        [TestMethod]
        public void TestFullParentArray()
        {
            container.SetSubArray(0, CONTAINER_SIZE);
            Assert.AreEqual(CONTAINER_SIZE, container.Length);
        }

        [TestMethod]
        public void TestPartialParentArray()
        {
            int nelm = CONTAINER_SIZE - 3;
            container.SetSubArray(3, nelm);
            Assert.AreEqual(nelm, container.Length);
            for (var i = 0;i<nelm; i++)
            {
                Assert.AreEqual(container.Data[i+3], container[i]);
            }
        }

        [TestMethod]
        public void TestModificationEvent()
        {
            bool modified = false;
            container.Modified += (o, e) => {
                modified = true;
            };

            container.SetSubArray(3, 8);
            Assert.IsTrue(modified, "If the values change, the event should be called");
            modified = false;

            container.SetSubArray(3, 8);
            Assert.IsFalse(modified, "No event should be fired if the values don't change");

            container.Data[0] = 1;
            Assert.IsFalse(modified, "Modifications outside the subArray shouldn't trigger the event");

            container.Data[3] = 1;
            Assert.IsTrue(modified, "Modifications inside the subArray should trigger the event");
        }
    }
}
