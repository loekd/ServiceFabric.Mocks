using System;
using System.Fabric;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Services.Runtime;
using ServiceFabric.Mocks.Tests.Actors;

namespace ServiceFabric.Mocks.Tests.Services
{
    public class ActorCallerService : StatelessService
    {
        public static readonly Guid CalledActorId = Guid.Parse("{1F263E8C-78D4-4D91-AAE6-C4B9CE03D6EB}");

        public IActorProxyFactory ProxyFactory { get; }

        public ActorCallerService(StatelessServiceContext serviceContext, IActorProxyFactory proxyFactory = null) 
            : base(serviceContext)
        {
            ProxyFactory = proxyFactory ?? new ActorProxyFactory();
        }

        public async Task CallActorAsync()
        {
            var proxy = ProxyFactory.CreateActorProxy<IMyStatefulActor>(new ActorId(CalledActorId));
            var value = new Payload("some other value");
            await proxy.InsertAsync("test", value);
        }
    }
}
