using System.Collections.ObjectModel;
using System.Fabric.Description;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ServiceFabric.Mocks.Tests.ServiceTests
{
    [TestClass]
    public class MockCodePackageActivationContextTests
    {
        [TestMethod]
        public void TestKeyedCollections()
        {
            var dut = (MockCodePackageActivationContext)MockCodePackageActivationContext.Default;

            Assert.IsInstanceOfType(dut.EndpointResourceDescriptions, typeof(KeyedCollection<string, EndpointResourceDescription>));
            Assert.IsInstanceOfType(dut.ServiceGroupTypes, typeof(KeyedCollection<string, ServiceGroupTypeDescription>));
            Assert.IsInstanceOfType(dut.ServiceTypes, typeof(KeyedCollection<string, ServiceTypeDescription>));
            
        }
    }
}