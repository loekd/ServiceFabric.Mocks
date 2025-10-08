using System.Collections.ObjectModel;
using System.Fabric.Description;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ServiceFabric.Mocks.NetCoreTests.ServiceTests
{
    [TestClass]
    public class MockCodePackageActivationContextTests
    {
        [TestMethod]
        public void TestKeyedCollections()
        {
            var dut = (MockCodePackageActivationContext)MockCodePackageActivationContext.Default;

            Assert.IsInstanceOfType<KeyedCollection<string, EndpointResourceDescription>>(dut.EndpointResourceDescriptions);
            Assert.IsInstanceOfType<KeyedCollection<string, ServiceGroupTypeDescription>>(dut.ServiceGroupTypes);
            Assert.IsInstanceOfType<KeyedCollection<string, ServiceTypeDescription>>(dut.ServiceTypes);
        }
    }
}