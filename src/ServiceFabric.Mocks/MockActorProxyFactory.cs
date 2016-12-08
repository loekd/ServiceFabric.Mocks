using System;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Services.Remoting;

namespace ServiceFabric.Mocks
{
    /// <summary>
    /// Specifies the interface for the factory that creates proxies for remote communication to the specified Actor.
    /// </summary>
    public class MockActorProxyFactory : MockServiceProxyFactory, IActorProxyFactory
    {
        readonly ConcurrentDictionary<ActorId, ConcurrentBag<IActor>> _actorRegistry = new ConcurrentDictionary<ActorId, ConcurrentBag<IActor>>();

        /// <summary>
        /// Registers an instance of a Actor combined with its Id to be able to return it from 
        /// <see cref="CreateActorProxy{TActorInterface}(Microsoft.ServiceFabric.Actors.ActorId,string,string,string)"/>
        /// </summary>
        /// <param name="actor"></param>
        public void RegisterActor(IActor actor)
        {
            _actorRegistry.AddOrUpdate(
                actor.GetActorId(),
                id => new ConcurrentBag<IActor>(new [] { actor }),
                (id, bag) =>
                {
                    if (bag.Any(a => a.GetType() == actor.GetType()))
                    {
                        throw new ArgumentException($"There is already an actor of type {actor.GetType()} with the actorId {id}.");
                    }

                    bag.Add(actor);
                    return bag;
                });
        }

        ///<inheritdoc />
        public TActorInterface CreateActorProxy<TActorInterface>(ActorId actorId, string applicationName = null,
            string serviceName = null, string listenerName = null) where TActorInterface : IActor
        {
            var bag = _actorRegistry[actorId];
            return bag.OfType<TActorInterface>().SingleOrDefault();
        }

        ///<inheritdoc />
        public TActorInterface CreateActorProxy<TActorInterface>(Uri serviceUri, ActorId actorId, string listenerName = null) 
            where TActorInterface : IActor
        {
            var bag = _actorRegistry[actorId];
            return bag.OfType<TActorInterface>().SingleOrDefault();
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
    }
}