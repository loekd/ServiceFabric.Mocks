using System;
using Microsoft.ServiceFabric.Actors;

namespace ServiceFabric.Mocks
{
	/// <summary>
	/// Occurs when <see cref="MockActorProxyFactory"/> misses an Actor.
	/// </summary>
	public class MissingActorEventArgs : EventArgs
	{
		public IActor ActorInstance { get; set; }

		public Type ActorType { get; private set; }

		public ActorId Id { get; }

		public MissingActorEventArgs(Type actorType, ActorId id)
	    {
		    ActorType = actorType;
		    Id = id;
	    }
    }
}