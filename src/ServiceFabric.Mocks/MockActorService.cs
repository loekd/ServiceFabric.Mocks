using System;
using System.Fabric;
using System.Reflection;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace ServiceFabric.Mocks
{
    /// <summary>
    /// An <see cref="ActorService"/> that exposes a way to create Actor instances.
    /// </summary>
    public class MockActorService<TActor> : ActorService
        where TActor : ActorBase
    {
        private readonly Func<ActorService, ActorId, ActorBase> _actorFactory;

        public MockActorService(StatefulServiceContext context, ActorTypeInformation actorTypeInfo, Func<ActorService, ActorId, ActorBase> actorFactory = null, Func<ActorBase, IActorStateProvider, IActorStateManager> stateManagerFactory = null, IActorStateProvider stateProvider = null, ActorServiceSettings settings = null) 
            : base(context, actorTypeInfo, actorFactory, stateManagerFactory, stateProvider, settings)
        {
            _actorFactory = actorFactory ?? ((svc, id) =>
                            {
                                var ctor = actorTypeInfo.ImplementationType.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null,
                                    new[] {typeof(ActorService), typeof(ActorId)}, null);
                                return (ActorBase) ctor.Invoke(new object[] {svc, id});
                            });
        }

        /// <summary>
        /// Creates a new <see cref="TActor"/> instance using the provided <paramref name="actorId"/>.
        /// </summary>
        /// <param name="actorId"></param>
        /// <returns></returns>
        public TActor Activate(ActorId actorId)
        {
            return (TActor)_actorFactory(this, actorId);
        }
    }
}
