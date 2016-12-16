using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Health;
using System.Fabric.Query;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ServiceFabric.Mocks.Tests.MocksTests
{
	[TestClass]
    public class MockQueryPartitionFactoryTests
    {

	    [TestMethod]
	    public void CreateIntPartitonInfoTest()
	    {
		    var id = Guid.NewGuid();
		    var partition = MockQueryPartitionFactory.CreateIntPartitonInfo(long.MinValue, 0L, id);

			Assert.AreEqual(0L, partition.HighKey);
			Assert.AreEqual(long.MinValue, partition.LowKey);
			Assert.AreEqual(id, partition.Id);
			Assert.AreEqual(ServicePartitionKind.Int64Range, partition.Kind);
		}

		[TestMethod]
		public void CreateSingletonPartitonInfoTest()
		{
			var id = Guid.NewGuid();
			var partition = MockQueryPartitionFactory.CreateSingletonPartitonInfo(id);

			Assert.AreEqual(id, partition.Id);
			Assert.AreEqual(ServicePartitionKind.Singleton, partition.Kind);
		}

		[TestMethod]
		public void CreateNamedPartitonInfoTest()
		{
			var id = Guid.NewGuid();
			const string name = "name";
			var partition = MockQueryPartitionFactory.CreateNamedPartitonInfo(name, id);

			Assert.AreEqual(id, partition.Id);
			Assert.AreEqual(name, partition.Name);
			Assert.AreEqual(ServicePartitionKind.Named, partition.Kind);
		}

		[TestMethod]
	    public void CreateStatefulPartitionTest()
	    {
		    var singletonPartitionInformation = MockQueryPartitionFactory.CreateSingletonPartitonInfo();
		    var partition = MockQueryPartitionFactory.CreateStatefulPartition(singletonPartitionInformation, 0L, 1L, HealthState.Error, ServicePartitionStatus.Deleting, TimeSpan.MinValue, new Epoch());

			Assert.AreEqual(singletonPartitionInformation, partition.PartitionInformation);
			Assert.AreEqual(HealthState.Error, partition.HealthState);
			Assert.AreEqual(ServicePartitionStatus.Deleting, partition.PartitionStatus);
			Assert.AreEqual(ServiceKind.Stateful, partition.ServiceKind);

		}

		[TestMethod]
		public void CreateStatelessPartitionTest()
		{
			var int64PartitionInformation = MockQueryPartitionFactory.CreateIntPartitonInfo();
			var partition = MockQueryPartitionFactory.CreateStatelessPartition(int64PartitionInformation, 0L, HealthState.Error, ServicePartitionStatus.Deleting);

			Assert.AreEqual(int64PartitionInformation, partition.PartitionInformation);
			Assert.AreEqual(HealthState.Error, partition.HealthState);
			Assert.AreEqual(ServicePartitionStatus.Deleting, partition.PartitionStatus);
			Assert.AreEqual(ServiceKind.Stateless, partition.ServiceKind);

		}


	    [TestMethod]
	    public void CreateResolvedServicePartitionTest()
	    {
		    List<ResolvedServiceEndpoint> list = new List<ResolvedServiceEndpoint>();
		    string address = "http://localhost/service";
		    list.Add(MockQueryPartitionFactory.CreateResolvedServiceEndpoint(address));
		    var serviceName = new Uri("fabric:/service");
		    var partition = MockQueryPartitionFactory.CreateResolvedServicePartition(serviceName, list);

			Assert.AreEqual(serviceName, partition.ServiceName);
			Assert.AreEqual(list, partition.Endpoints);
			Assert.AreEqual(address, partition.Endpoints.First().Address);
	    }
    }
}
