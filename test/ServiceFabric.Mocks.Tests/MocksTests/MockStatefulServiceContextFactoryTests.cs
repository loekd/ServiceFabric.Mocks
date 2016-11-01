using System;
using System.Fabric;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ServiceFabric.Mocks.Tests.MocksTests
{
	[TestClass]
    public class MockStatefulServiceContextFactoryTests
    {
		[TestMethod]
	    public void TestDefaultServiceUri()
		{
			var instance = MockStatefulServiceContextFactory.Default;

			Assert.IsInstanceOfType(instance, typeof(StatefulServiceContext));
			Assert.AreEqual(new Uri(MockStatefulServiceContextFactory.ServiceName), instance.ServiceName);
			Assert.AreEqual(MockStatefulServiceContextFactory.ServiceTypeName, instance.ServiceTypeName);
		}

		[TestMethod]
		public void TestCustom()
		{
			var newUri = new Uri("fabric:/MockApp/OtherMockStatefulService");
			var serviceTypeName = "OtherMockServiceType";
			var partitionId = Guid.NewGuid();
			var replicaId = long.MaxValue;
			var context = new MockCodePackageActivationContext("fabric:/MyApp", "MyAppType", "Code", "Ver", "Context", "Log", "Temp", "Work", "Man", "ManVer");

			var instance = MockStatefulServiceContextFactory.Create(context, serviceTypeName, newUri, partitionId, replicaId);

			Assert.IsInstanceOfType(instance, typeof(StatefulServiceContext));
			Assert.AreEqual(context, instance.CodePackageActivationContext);
			Assert.AreEqual(newUri, instance.ServiceName);
			Assert.AreEqual(serviceTypeName, instance.ServiceTypeName);
			Assert.AreEqual(partitionId, instance.PartitionId);
			Assert.AreEqual(replicaId, instance.ReplicaId);
			Assert.AreEqual(replicaId, instance.ReplicaOrInstanceId);
		}
	}
}
