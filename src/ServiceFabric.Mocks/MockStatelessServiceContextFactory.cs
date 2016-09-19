using System;
using System.Fabric;

namespace ServiceFabric.Mocks
{
    /// <summary>
    /// Factory that returns an instance of <see cref="StatelessServiceContext"/> using <see cref="MockCodePackageActivationContext.Default"/>
    /// </summary>
    public class MockStatelessServiceContextFactory
    {
        /// <summary>
        /// Returns an instance of <see cref="StatelessServiceContext"/> using <see cref="MockCodePackageActivationContext.Default"/>
        /// </summary>
        public static StatelessServiceContext Default { get; } = new StatelessServiceContext(
            new NodeContext("Node0", new NodeId(0, 1), 0, "NodeType1", "MOCK.MACHINE"),
            MockCodePackageActivationContext.Default, "MockServiceType",
            new Uri("fabric:/MockApp/MockStatelessService"),
            null,
            Guid.NewGuid(),
            long.MaxValue
        );
    }
}