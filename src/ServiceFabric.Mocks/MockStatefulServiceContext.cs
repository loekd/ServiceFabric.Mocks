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

        /// <summary>
        /// Returns an instance of <see cref="StatefulServiceContext"/> using <see cref="MockCodePackageActivationContext.Default"/>
        /// and the specified Service URI
        /// </summary>
        /// <param name="serviceUri">The URI that should be used by the ServiceContext</param>
        /// <returns>The constructed <see cref="StatefulServiceContext"/></returns>
        public static StatefulServiceContext WithCustomUri(Uri serviceUri)
        {
            return new StatefulServiceContext(
               new NodeContext("Node0", new NodeId(0, 1), 0, "NodeType1", "MOCK.MACHINE"),
               MockCodePackageActivationContext.Default, "MockServiceType",
               serviceUri,
               null,
               Guid.NewGuid(),
               long.MaxValue
            );
        }
    }
}
