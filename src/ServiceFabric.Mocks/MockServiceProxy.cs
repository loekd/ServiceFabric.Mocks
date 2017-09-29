using System;
using System.Collections.Generic;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace ServiceFabric.Mocks
{
    /// <summary>
    /// This is the base Client side interface for Remoting. The framework provides the
    /// Remoting infrastructure for all the service contracts inheriting from IService through
    /// ServiceRemotingListener and ServiceProxy.
    /// </summary>
    public class MockServiceProxy<TService> : IServiceProxy
        where TService : IService
    {
        private readonly IDictionary<Type, Func<Uri, TService>> _serviceBuilders = new Dictionary<Type, Func<Uri, TService>>();

        public Type ServiceInterfaceType => typeof(TService);

        public Microsoft.ServiceFabric.Services.Remoting.V1.Client.IServiceRemotingPartitionClient ServicePartitionClient { get { throw new NotImplementedException(); } }

        public Microsoft.ServiceFabric.Services.Remoting.V2.Client.IServiceRemotingPartitionClient ServicePartitionClient2 { get { throw new NotImplementedException(); } }

        public TService Create(Type serviceType, Uri serviceUri, ServicePartitionKey partitionKey = null, TargetReplicaSelector targetReplicaSelector = TargetReplicaSelector.Default, string listenerName = null)
        {
            var serviceBuilder = _serviceBuilders[serviceType];
            return serviceBuilder(serviceUri);
        }

        public void AddServiceBuilder(Type serviceType, Func<Uri, TService> create)
        {
            _serviceBuilders[serviceType] = create;
        }
    }
}