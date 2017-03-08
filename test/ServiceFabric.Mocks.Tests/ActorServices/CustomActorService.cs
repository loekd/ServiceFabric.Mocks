using System;
using System.Fabric;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace ServiceFabric.Mocks.Tests.ActorServices
{
	/// <summary>
	/// A custom <see cref="ActorService"/> that adds nothing much.
	/// </summary>
    public class CustomActorService : ActorService
    {
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="actorTypeInfo"></param>
		/// <param name="actorFactory"></param>
		/// <param name="stateManagerFactory"></param>
		/// <param name="stateProvider"></param>
		/// <param name="settings"></param>
	    public CustomActorService(StatefulServiceContext context, ActorTypeInformation actorTypeInfo, Func<ActorService, ActorId, ActorBase> actorFactory = null, Func<ActorBase, IActorStateProvider, IActorStateManager> stateManagerFactory = null, IActorStateProvider stateProvider = null, ActorServiceSettings settings = null) : base(context, actorTypeInfo, actorFactory, stateManagerFactory, stateProvider, settings)
	    {
	    }
    }

	/// <summary>
	/// A custom <see cref="ActorService"/> with a custom constructor.
	/// </summary>
	public class AnotherCustomActorService : ActorService
	{
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="dummy">Additional ctor parameter, so the default parameter set doesn't fit.</param>
		/// <param name="context"></param>
		/// <param name="actorTypeInfo"></param>
		/// <param name="actorFactory"></param>
		/// <param name="stateManagerFactory"></param>
		/// <param name="stateProvider"></param>
		/// <param name="settings"></param>
		public AnotherCustomActorService(object dummy, StatefulServiceContext context, ActorTypeInformation actorTypeInfo, Func<ActorService, ActorId, ActorBase> actorFactory = null, Func<ActorBase, IActorStateProvider, IActorStateManager> stateManagerFactory = null, IActorStateProvider stateProvider = null, ActorServiceSettings settings = null) : base(context, actorTypeInfo, actorFactory, stateManagerFactory, stateProvider, settings)
		{
		}
	}
}
