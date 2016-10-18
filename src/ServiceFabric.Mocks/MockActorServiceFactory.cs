using System;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace ServiceFabric.Mocks
{
    public static class MockActorServiceFactory
    {
        /// <summary>
        /// Creates a new <see cref="MockActorService{TActor}"/> (which is an <see cref="ActorService"/>) using <see cref="MockActorStateManager"/> and <see cref="MockStatefulServiceContextFactory.Default"/>
        /// which returns instances of <see cref="TActor"/> using the optionally provided <paramref name="actorFactory"/>, <paramref name="actorStateProvider"/> and <paramref name="settings"/>.
        /// </summary>
        /// <typeparam name="TActor"></typeparam>
        /// <param name="settings">Optional settings. By default, null is used.</param>
        /// <param name="actorFactory">Optional Actor factory. By default, null is used.</param>
        /// <param name="actorStateProvider">Optional Actor State Provider. By default, <see cref="MockActorStateProvider"/> is used.</param>
        /// <returns></returns>
        public static MockActorService<TActor> CreateActorServiceForActor<TActor>(Func<ActorService, ActorId, ActorBase> actorFactory = null, IActorStateProvider actorStateProvider = null, ActorServiceSettings settings = null)
            where TActor : Actor
        {
            var stateManager = new MockActorStateManager();
            Func<ActorBase, IActorStateProvider, IActorStateManager> stateManagerFactory = (actr, stateProvider) => stateManager;
            if (actorStateProvider == null)
            {
                actorStateProvider = new MockActorStateProvider();
                actorStateProvider.Initialize(ActorTypeInformation.Get(typeof(TActor)));
            }
            var svc = new MockActorService<TActor>(MockStatefulServiceContextFactory.Default, ActorTypeInformation.Get(typeof(TActor)), actorFactory, stateManagerFactory, actorStateProvider, settings);
            return svc;
        }
    }
}
