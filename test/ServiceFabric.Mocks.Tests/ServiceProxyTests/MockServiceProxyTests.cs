using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceFabric.Mocks.Tests.Services;

namespace ServiceFabric.Mocks.Tests.ServiceProxyTests
{
    [TestClass]
    public class MockServiceProxyTests
    {
        [TestMethod]
        public void TestNetFxFullCompatibility()
        {
            var proxy = new MockServiceProxy<MyStatelessService>();
            Assert.IsNotNull(proxy);
            Assert.IsNotNull(proxy.GetType().GetProperty("ServicePartitionClient"));
            Assert.IsNotNull(proxy.GetType().GetProperty("ServicePartitionClient2"));
        }
    }
}
