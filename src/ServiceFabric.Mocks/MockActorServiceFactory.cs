using System;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace ServiceFabric.Mocks
{
    public static class MockActorServiceFactory
    {
        /// <summary>
        /// Creates a new <see cref="ActorService"/> using <see cref="MockActorStateManager"/> and <see cref="MockStatefulServiceContextFactory.Default"/>
        /// which returns instances of <see cref="TActor"/> using the provided <paramref name="actorFactory"/>.
        /// </summary>
        /// <typeparam name="TActor"></typeparam>
        /// <param name="actorFactory"></param>
        /// <returns></returns>
        public static ActorService CreateActorServiceForActor<TActor>(Func<ActorService, ActorId, ActorBase> actorFactory)
            where TActor : Actor
        {
            var stateManager = new MockActorStateManager();
            Func<ActorBase, IActorStateProvider, IActorStateManager> stateManagerFactory = (actr, stateProvider) => stateManager;
            var svc = new ActorService(MockStatefulServiceContextFactory.Default, ActorTypeInformation.Get(typeof(TActor)), actorFactory, stateManagerFactory);
            return svc;
        }
    }
}
