using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Text;
using static ServiceFabric.Mocks.MockFabricClient;

namespace ServiceFabric.Mocks.NetCoreTests.FabricClientTests
{
    [TestClass]
    public class MockFabricClientTests
    {
        [TestMethod]
        public void Test()
        {
            var client = new ServiceManagementClient();
            client.AddResolvedServicePartition(new Uri("test://mocks", UriKind.Absolute), MockQueryPartitionFactory.CreateIntPartitonInfo(), new[] { MockQueryPartitionFactory.CreateResolvedServiceEndpoint("test://mocks") });
        }
    }
}
