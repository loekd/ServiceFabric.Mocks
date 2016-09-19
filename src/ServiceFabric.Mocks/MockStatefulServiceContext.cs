using System;
using System.Fabric;

namespace ServiceFabric.Mocks
{
    /// <summary>
    /// Factory that returns an instance of <see cref="StatefulServiceContext"/> using <see cref="MockCodePackageActivationContext.Default"/>
    /// </summary>
    public class MockStatefulServiceContextFactory
    {
        /// <summary>
        /// Returns an instance of <see cref="StatefulServiceContext"/> using <see cref="MockCodePackageActivationContext.Default"/>
        /// </summary>
        public static StatefulServiceContext Default { get; } = new StatefulServiceContext(
           new NodeContext("Node0", new NodeId(0, 1), 0, "NodeType1", "MOCK.MACHINE"),
           MockCodePackageActivationContext.Default, "MockServiceType",
           new Uri("fabric:/MockApp/MockStatefulService"),
           null,
           Guid.NewGuid(),
           long.MaxValue
       );
    }
}
