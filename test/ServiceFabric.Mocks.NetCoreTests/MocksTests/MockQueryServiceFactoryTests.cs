using System;
using System.Fabric.Health;
using System.Fabric.Query;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ServiceFabric.Mocks.NetCoreTests.MocksTests
{
    [TestClass]
    public class MockQueryServiceFactoryTests
    {
        [TestMethod]
        public void CreateStatelessServiceInstanceTest()
        {
            Assert.IsNotNull(MockQueryServiceFactory.CreateStatelessServiceInstance(new Uri("fabric:/app/service"),
                "serviceType", "1.0", HealthState.Ok, ServiceStatus.Active));
        }
    }
}
