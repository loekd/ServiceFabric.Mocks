using System;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Services.Remoting;
using System.Collections.Generic;

namespace ServiceFabric.Mocks
{
	/// <summary>
	/// Specifies the interface for the factory that creates proxies for remote communication to the specified Actor.
	/// </summary>
	public class MockActorProxyFactory : MockServiceProxyFactory, IActorProxyFactory
	{

		/// <summary>
		/// Fires when an Actor is missing, set <see cref="MissingActorEventArgs.ActorInstance"/> to resolve.
		/// </summary>
		public event EventHandler<MissingActorEventArgs> MissingActor;

		private readonly ConcurrentDictionary<ActorId, HashSet<IActor>> _actorRegistry = new ConcurrentDictionary<ActorId, HashSet<IActor>>();
		
		/// <summary>
		/// Registers an instance of a Actor combined with its Id to be able to return it from 
		/// <see cref="CreateActorProxy{TActorInterface}(Microsoft.ServiceFabric.Actors.ActorId,string,string,string)"/>
		/// </summary>
		/// <param name="actor"></param>
		public void RegisterActor(IActor actor)
		{
			_actorRegistry.AddOrUpdate(
				actor.GetActorId(),
				id => new HashSet<IActor>(new[] { actor }),
				(id, set) =>
				{
					if (set.Any(a => a.GetType() == actor.GetType()))
					{
						throw new ArgumentException($"There is already an actor of type {actor.GetType()} with the actorId {id}.");
					}

					set.Add(actor);
					return set;
				});
		}

		///<inheritdoc />
		public TActorInterface CreateActorProxy<TActorInterface>(ActorId actorId, string applicationName = null,
			string serviceName = null, string listenerName = null) where TActorInterface : IActor
		{
			if (!_actorRegistry.ContainsKey(actorId))
			{
				OnMisingActor(this, actorId, typeof(TActorInterface));
			}

			var set = _actorRegistry[actorId];
			return set.OfType<TActorInterface>().SingleOrDefault();
		}

		///<inheritdoc />
		public TActorInterface CreateActorProxy<TActorInterface>(Uri serviceUri, ActorId actorId, string listenerName = null)
			where TActorInterface : IActor
		{
			if (!_actorRegistry.ContainsKey(actorId))
			{
				OnMisingActor(this, actorId, typeof(TActorInterface));
			}

			var set = _actorRegistry[actorId];
			return set.OfType<TActorInterface>().SingleOrDefault();
		}

		///<inheritdoc />
		public TServiceInterface CreateActorServiceProxy<TServiceInterface>(Uri serviceUri, ActorId actorId, string listenerName = null)
			where TServiceInterface : IService
		{
			var serviceInterface = CreateServiceProxy<TServiceInterface>(serviceUri);
			return serviceInterface;
		}

		///<inheritdoc />
		public TServiceInterface CreateActorServiceProxy<TServiceInterface>(Uri serviceUri, long partitionKey, string listenerName = null)
			where TServiceInterface : IService
		{
			var serviceInterface = CreateServiceProxy<TServiceInterface>(serviceUri);
			return serviceInterface;
		}

		protected virtual void OnMisingActor(object sender, ActorId id, Type actorType)
		{
			var args = new MissingActorEventArgs(actorType, id);
			MissingActor?.Invoke(sender, args);
			if (args.ActorInstance != null)
			{
				RegisterActor(args.ActorInstance);
			}
		}
	}
}