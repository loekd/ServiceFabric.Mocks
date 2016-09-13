using System;
using System.Collections.Concurrent;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace ServiceFabric.Mocks
{
    /// <summary>
    /// Specifies the interface for the factory that creates proxies for remote communication to the specified service.
    /// </summary>
    public class MockServiceProxyFactory : IServiceProxyFactory
    {
        private readonly ConcurrentDictionary<Uri, IService> _serviceRegistry = new ConcurrentDictionary<Uri, IService>();

        /// <inheritdoc />
        public TServiceInterface CreateServiceProxy<TServiceInterface>(Uri serviceUri, ServicePartitionKey partitionKey = null, TargetReplicaSelector targetReplicaSelector = TargetReplicaSelector.Default, string listenerName = null)
            where TServiceInterface : IService
        {
            var serviceProxy = new MockServiceProxy<TServiceInterface>();
            serviceProxy.AddServiceBuilder(typeof(TServiceInterface), uri => (TServiceInterface)_serviceRegistry[uri]);
            return serviceProxy.Create(typeof(TServiceInterface), serviceUri, partitionKey, targetReplicaSelector, listenerName);

        }

        /// <summary>
        /// Registers an instance of a service combined with its name to be able to return it from <see cref="CreateServiceProxy{TServiceInterface}"/>
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="service"></param>
        public void RegisterService(Uri serviceName, IService service)
        {
            _serviceRegistry.AddOrUpdate(serviceName, service, (name, svc) => service);
        }
    }
}