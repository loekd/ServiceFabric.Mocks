using System.Fabric;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Data.Notifications;

namespace ServiceFabric.Mocks.Tests.Services
{
    public class MyStatefulService : StatefulService, IMyStatefulService
    {
        public const string StateManagerDictionaryKey = "dictionaryname";
        public const string StateManagerQueueKey = "queuename";
        public const string StateManagerConcurrentQueueKey = "concurrentqueuename";


        public MyStatefulService(StatefulServiceContext serviceContext) : base(serviceContext)
        {
        }

        public MyStatefulService(StatefulServiceContext serviceContext, IReliableStateManagerReplica2 reliableStateManagerReplica)
            : base(serviceContext, reliableStateManagerReplica)
        {
        }

        public async Task InsertAsync(string stateName, Payload value)
        {
            var dictionary = await StateManager.GetOrAddAsync<IReliableDictionary<string, Payload>>(StateManagerDictionaryKey);

            using (var tx = StateManager.CreateTransaction())
            {
                await dictionary.TryAddAsync(tx, stateName, value);
                await tx.CommitAsync();
            }
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
    }
}
