﻿using System.Fabric;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Data.Notifications;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Linq;

namespace ServiceFabric.Mocks.Tests.Services
{
    public class MyStatefulService : StatefulService, IMyStatefulService
    {
        public const string StateManagerDictionaryKey = "dictionaryname";
        public const string StateManagerQueueKey = "queuename";
        public const string StateManagerConcurrentQueueKey = "concurrentqueuename";

        private readonly ConcurrentDictionary<string, Payload> _cache = new ConcurrentDictionary<string, Payload>();
        private ReplicaRole _role = ReplicaRole.Unknown;

        public MyStatefulService(StatefulServiceContext serviceContext) : base(serviceContext)
        {
        }

        public MyStatefulService(StatefulServiceContext serviceContext, IReliableStateManagerReplica2 reliableStateManagerReplica)
            : base(serviceContext, reliableStateManagerReplica)
        {
        }

        public Task<IEnumerable<Payload>> GetPayloadsAsync()
        {
            return Task.FromResult<IEnumerable<Payload>>(_cache.Select(p => p.Value).ToList());
        }

        public async Task UpdatePayloadAsync(string stateName, string content)
        {
            var dictionary = await StateManager.GetOrAddAsync<IReliableDictionary<string, Payload>>(StateManagerDictionaryKey);

            using (var tx = StateManager.CreateTransaction())
            {
                await dictionary.SetAsync(tx, stateName, new Payload(content));
                await tx.CommitAsync();
            }

            _cache.AddOrUpdate(stateName, new Payload(content), (k, v) => new Payload(content));
        }

        public async Task InsertAsync(string stateName, Payload value)
        {
            var dictionary = await StateManager.GetOrAddAsync<IReliableDictionary<string, Payload>>(StateManagerDictionaryKey);

            using (var tx = StateManager.CreateTransaction())
            {
                await dictionary.TryAddAsync(tx, stateName, value);
                await tx.CommitAsync();
            }

            //copy this so we dont have the same reference in the read model and reliable collection
            _cache.TryAdd(stateName, new Payload(value.Content));
        }

        public async Task InsertAndAbortAsync(string stateName, Payload value)
        {
            var dictionary = await StateManager.GetOrAddAsync<IReliableDictionary<string, Payload>>(StateManagerDictionaryKey);

            using (var tx = StateManager.CreateTransaction())
            {
                await dictionary.TryAddAsync(tx, stateName, value);
                tx.Abort();
            }
        }


        public async Task EnqueueAsync(Payload value)
        {
            var queue = await StateManager.GetOrAddAsync<IReliableQueue<Payload>>(StateManagerQueueKey);

            using (var tx = StateManager.CreateTransaction())
            {
                await queue.EnqueueAsync(tx, value);
                await tx.CommitAsync();
            }
        }

        public async Task ConcurrentEnqueueAsync(Payload value)
        {
            var concurrentQueue = await StateManager.GetOrAddAsync<IReliableConcurrentQueue<Payload>>(StateManagerConcurrentQueueKey);

            using (var tx = StateManager.CreateTransaction())
            {
                await concurrentQueue.EnqueueAsync(tx, value);
                await tx.CommitAsync();
            }
        }


        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            await base.RunAsync(cancellationToken);

            //this should always be true. RunAsync is only executed for primary replicas
            if (_role == ReplicaRole.Primary)
            {
                //hydrate the in-memory state
                var dictionary = await StateManager.GetOrAddAsync<IReliableDictionary<string, Payload>>(StateManagerDictionaryKey);
                using (var tx = StateManager.CreateTransaction())
                {
                    var enumerable = await dictionary.CreateEnumerableAsync(tx, EnumerationMode.Unordered);
                    using (var enumerator = enumerable.GetAsyncEnumerator())
                    {
                        while (await enumerator.MoveNextAsync(cancellationToken))
                        {
                            //copy so we dont have the same reference in the model and the reliable collection
                            var item = new KeyValuePair<string, Payload>(enumerator.Current.Key, new Payload(enumerator.Current.Value.Content));
                            _cache.AddOrUpdate(item.Key, item.Value, (k, v) => item.Value);
                        }
                    }                    
                }                
            }
            else
            {
                throw new System.Exception("Invalid state transition. RunAsync executed on non-primary replica!");
            }
        }

        protected override Task OnChangeRoleAsync(ReplicaRole newRole, CancellationToken cancellationToken)
        {
            //if we are shifting away from primary
            if (_role == ReplicaRole.Primary && newRole != ReplicaRole.Primary)
            {
                //clear out the in-memory state
                _cache.Clear();
            }

            _role = newRole;
            return base.OnChangeRoleAsync(newRole, cancellationToken);
        }
    }
}
