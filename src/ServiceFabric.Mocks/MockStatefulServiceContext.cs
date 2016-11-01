using System;
using System.Fabric;

namespace ServiceFabric.Mocks
{
	/// <summary>
	/// Factory that returns an instance of <see cref="StatefulServiceContext"/> using <see cref="MockCodePackageActivationContext.Default"/> or customized instance using <see cref="Create"/>.
	/// </summary>
	public class MockStatefulServiceContextFactory
    {
	    public const string ServiceTypeName = "MockServiceType";
	    public const string ServiceName = "fabric:/MockApp/MockStatefulService";

		/// <summary>
		/// Returns an instance of <see cref="StatefulServiceContext"/> using <see cref="MockCodePackageActivationContext.Default"/>, <see cref="ServiceTypeName"/>, <see cref="ServiceName"/> and random values for Partition and Replica id's.
		/// </summary>
		public static StatefulServiceContext Default { get; } =
			Create(MockCodePackageActivationContext.Default, 
				ServiceTypeName, 
				new Uri(ServiceName), 
				Guid.NewGuid(), 
				long.MaxValue);

		/// <summary>
		/// Returns an instance of <see cref="StatefulServiceContext"/> using the specified arguments.
		/// </summary>
		/// <param name="codePackageActivationContext">Activation context</param>
		/// <param name="serviceTypeName">Name of the service type</param>
		/// <param name="serviceName">The URI that should be used by the ServiceContext</param>
		/// <param name="partitionId">PartitionId</param>
		/// <param name="replicaId">ReplicaId</param>
		/// <returns>The constructed <see cref="StatefulServiceContext"/></returns>
		public static StatefulServiceContext Create(ICodePackageActivationContext codePackageActivationContext, string serviceTypeName, Uri serviceName, Guid partitionId, long replicaId)
        {
            return new StatefulServiceContext(
               new NodeContext("Node0", new NodeId(0, 1), 0, "NodeType1", "MOCK.MACHINE"),
               codePackageActivationContext, 
			   serviceTypeName,
               serviceName,
               null,
               partitionId,
               replicaId
            );
        }
    }
}
