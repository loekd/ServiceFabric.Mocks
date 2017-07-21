using System;
using System.Fabric;
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
        /// <param name="actorFactory">Optional Actor factory. By default, null is used.</param>
        /// <param name="actorStateProvider">Optional Actor State Provider. By default, <see cref="MockActorStateProvider"/> is used.</param>
        /// <param name="context">Optional Actor ServiceContext. By default, <see cref="MockStatefulServiceContextFactory.Default"/> is used.</param>
        /// <param name="settings">Optional settings. By default, null is used.</param>
        /// <returns></returns>
        public static MockActorService<TActor> CreateActorServiceForActor<TActor>(Func<ActorService, ActorId, ActorBase> actorFactory = null, IActorStateProvider actorStateProvider = null, StatefulServiceContext context = null, ActorServiceSettings settings = null)
            where TActor : Actor
        {
            Func<ActorBase, IActorStateProvider, IActorStateManager> stateManagerFactory = (actr, stateProvider) => new MockActorStateManager();
            if (actorStateProvider == null)
            {
                actorStateProvider = new MockActorStateProvider();
                actorStateProvider.Initialize(ActorTypeInformation.Get(typeof(TActor)));
            }

            context = context ?? MockStatefulServiceContextFactory.Default;
            var svc = new MockActorService<TActor>(context, ActorTypeInformation.Get(typeof(TActor)), actorFactory, stateManagerFactory, actorStateProvider, settings);
            return svc;
        }

		/// <summary>
		/// Creates a new <see cref="TActorService"/> (which is an <see cref="ActorService"/>) using <see cref="MockActorStateManager"/> and <see cref="MockStatefulServiceContextFactory.Default"/>
		/// which returns instances of <see cref="TActor"/> using the optionally provided <paramref name="actorFactory"/>, <paramref name="actorStateProvider"/> and <paramref name="settings"/>.
		/// </summary>
		/// <typeparam name="TActor"></typeparam>
		/// <typeparam name="TActorService"></typeparam>
		/// <param name="actorFactory">Optional Actor factory. By default, null is used.</param>
		/// <param name="actorStateProvider">Optional Actor State Provider. By default, <see cref="MockActorStateProvider"/> is used.</param>
		/// <param name="context">Optional Actor ServiceContext. By default, <see cref="MockStatefulServiceContextFactory.Default"/> is used.</param>
		/// <param name="settings">Optional settings. By default, null is used.</param>
		/// <returns></returns>
		public static TActorService CreateCustomActorServiceForActor<TActorService, TActor>(Func<ActorService, ActorId, ActorBase> actorFactory = null, IActorStateProvider actorStateProvider = null, StatefulServiceContext context = null, ActorServiceSettings settings = null)
			where TActor : Actor
			where TActorService : ActorService
		{
			var stateManager = new MockActorStateManager();
			Func<ActorBase, IActorStateProvider, IActorStateManager> stateManagerFactory = (actr, stateProvider) => stateManager;
			if (actorStateProvider == null)
			{
				actorStateProvider = new MockActorStateProvider();
				actorStateProvider.Initialize(ActorTypeInformation.Get(typeof(TActor)));
			}

			context = context ?? MockStatefulServiceContextFactory.Default;

			//StatefulServiceContext context, ActorTypeInformation actorTypeInfo, Func<ActorService, ActorId, ActorBase> actorFactory = null, Func<ActorBase, IActorStateProvider, IActorStateManager> stateManagerFactory = null, IActorStateProvider stateProvider = null, ActorServiceSettings settings = null

			var ctor = typeof(TActorService).GetConstructor(Constants.InstancePublicNonPublic, null,
				new []
				{
					typeof(StatefulServiceContext),
					typeof(ActorTypeInformation),
					typeof(Func<ActorService, ActorId, ActorBase>),
					typeof(Func<ActorBase, IActorStateProvider, IActorStateManager>),
					typeof(IActorStateProvider),
					typeof(ActorServiceSettings),
				}, null);
			if (ctor == null) throw new InvalidOperationException("This helper only works for an ActorService with a default constructor. Please create your own instance of {TActorService}.");
			var svc = ctor.Invoke(new object[] { context, ActorTypeInformation.Get(typeof(TActor)), actorFactory, stateManagerFactory, actorStateProvider, settings});
			return (TActorService) svc;
		}
	}
}
