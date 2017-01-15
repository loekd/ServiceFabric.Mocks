using System;
using System.Fabric;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace ServiceFabric.Mocks
{
	/// <summary>
	/// A default implementation of <see cref="ActorService"/> that exposes a way to create Actor instances.
	/// </summary>
	public class MockActorService<TActor> : ActorService
		where TActor : ActorBase
	{
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="actorTypeInfo">Info about the Actor. Use <see cref="ActorTypeInformation.Get"/> with typeof(TActor) to create this.</param>
		/// <param name="actorFactory">Optional Actor factory delegate.</param>
		/// <param name="stateManagerFactory"></param>
		/// <param name="stateProvider"></param>
		/// <param name="settings"></param>
		public MockActorService(StatefulServiceContext context, ActorTypeInformation actorTypeInfo, Func<ActorService, ActorId, ActorBase> actorFactory = null, Func<ActorBase, IActorStateProvider, IActorStateManager> stateManagerFactory = null, IActorStateProvider stateProvider = null, ActorServiceSettings settings = null)
			: base(context, actorTypeInfo, GetActorFactoryOrDefault(actorTypeInfo, actorFactory), stateManagerFactory, stateProvider, settings)
		{
		}

		/// <summary>
		/// Provides a wrapper around <see cref="ActorServiceExtensions.Activate{TActor}"/> with <typeparam name="TActor"/> as type parameter,
		/// </summary>
		/// <param name="actorId"></param>
		/// <returns></returns>
		public TActor Activate(ActorId actorId)
		{
			return this.Activate<TActor>(actorId);
		}

		/// <summary>
		/// Ensures that there's a value for <paramref name="actorFactory"/>.
		/// </summary>
		/// <param name="actorTypeInfo"></param>
		/// <param name="actorFactory"></param>
		/// <returns></returns>
		private static Func<ActorService, ActorId, ActorBase> GetActorFactoryOrDefault(ActorTypeInformation actorTypeInfo, Func<ActorService, ActorId, ActorBase> actorFactory)
		{
			return actorFactory ?? ((svc, id) =>
			{
				var ctor = actorTypeInfo.ImplementationType.GetConstructor(Constants.InstancePublicNonPublic, null,
					new[] { typeof(ActorService), typeof(ActorId) }, null);
				if (ctor == null) throw new InvalidOperationException("The default MockActorService ActorFactory expects an Actor to have a constructor that takes ActorService and ActorId arguments. Please use a custom 'actorFactory' parameter.");
				return (ActorBase)ctor.Invoke(new object[] { svc, id });
			});
		}
	}
}
