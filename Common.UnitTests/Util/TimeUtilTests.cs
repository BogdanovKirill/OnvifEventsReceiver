using System;
using Common.Util;
using NUnit.Framework;

namespace Common.UnitTests.Util
{
    [TestFixture]
    class TimeUtilTests
    {
        [Test]
        public void IsTimeOver_TimeHasNotPassed_ReturnsFalse()
        {
            int lastTimeCalled = Environment.TickCount + 60*1000;

            Assert.IsFalse(TimeUtil.IsTimeOver(lastTimeCalled, 1000));
        }

        [Test]
        public void IsTimeOver_TimeHasPassed_ReturnsTrue()
        {
            int lastTimeCalled = Environment.TickCount - 60 * 1000;

            Assert.IsTrue(TimeUtil.IsTimeOver(lastTimeCalled, 1000));
        }
    }
}
