using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceFabric.Mocks.NetCoreTests.Services;

namespace ServiceFabric.Mocks.NetCoreTests.ServiceProxyTests
{
    [TestClass]
    public class MockServiceProxyTests
    {
        [TestMethod]
        public void TestNetStandardCompatibility()
        {
            var proxy = new MockServiceProxy<MyStatelessService>();
            Assert.IsNotNull(proxy);
            Assert.IsNull(proxy.GetType().GetProperty("ServicePartitionClient"));
            Assert.IsNotNull(proxy.GetType().GetProperty("ServicePartitionClient2"));
        }
    }
}
