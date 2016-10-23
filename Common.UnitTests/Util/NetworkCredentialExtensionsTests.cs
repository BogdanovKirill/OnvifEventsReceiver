using System.Net;
using Common.Util;
using NUnit.Framework;

namespace Common.UnitTests.Util
{
    [TestFixture]
    class NetworkCredentialExtensionsTests
    {
        [TestCase("1", "1")]
        [TestCase("1", "")]
        [TestCase("1", null)]
        public void IsEmpty_LoginAndOrPasswordAreSet_ReturnsFalse(string login, string password)
        {
            var credential = new NetworkCredential(login, password);
            Assert.IsFalse(credential.IsEmpty());
        }

        [Test]
        public void IsEmpty_LoginAndOrPasswordNotSet_ReturnsTrue()
        {
            var credential = new NetworkCredential();
            Assert.IsTrue(credential.IsEmpty());
        }
    }
}
