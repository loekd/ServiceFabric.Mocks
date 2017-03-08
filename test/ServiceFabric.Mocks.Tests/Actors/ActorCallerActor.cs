using System;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;

namespace ServiceFabric.Mocks.Tests.Actors
{ 
    public class ActorCallerActor : Actor, IMyStatefulActor
    {
        public static readonly Uri CalledServiceName = new Uri("fabric:/MockApp/MyStatefulActor");
        public const string ChildActorIdKeyName = "ChildActorIdKeyName";

        public IActorProxyFactory ActorProxyFactory { get; }

        public ActorCallerActor(ActorService actorService, ActorId actorId, IActorProxyFactory actorProxyFactory) 
        : base(actorService, actorId)
        {
            ActorProxyFactory = actorProxyFactory ?? new ActorProxyFactory();
        }

        public async Task InsertAsync(string stateName, Payload value)
        {
            var actorProxy = ActorProxyFactory.CreateActorProxy<IMyStatefulActor>(CalledServiceName, new ActorId(Guid.NewGuid()));
			await StateManager.SetStateAsync(ChildActorIdKeyName, actorProxy.GetActorId());
			await actorProxy.InsertAsync(stateName, value);
        }
    }
}
