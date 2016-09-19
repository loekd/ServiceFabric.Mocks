using System;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using ServiceFabric.Mocks.Tests.Services;

namespace ServiceFabric.Mocks.Tests.Actors
{ 
    public class ServiceCallerActor : Actor, IMyStatefulActor
    {
        public static readonly Uri CalledServiceName = new Uri("fabric:/MockApp/MockStatefulService");

        public IServiceProxyFactory ServiceProxyFactory { get; }

        public ServiceCallerActor(ActorService actorService, ActorId actorId, IServiceProxyFactory serviceProxyFactory) 
        : base(actorService, actorId)
        {
            ServiceProxyFactory = serviceProxyFactory ?? new ServiceProxyFactory();
        }

        public Task InsertAsync(string stateName, Payload value)
        {
            var serviceProxy = ServiceProxyFactory.CreateServiceProxy<IMyStatefulService>(CalledServiceName, new ServicePartitionKey(0L));
            return serviceProxy.InsertAsync(stateName, value);
        }
    }
}
