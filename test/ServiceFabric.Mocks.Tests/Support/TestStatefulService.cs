using System.Fabric;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Runtime;

namespace ServiceFabric.Mocks.Tests.Support
{
    public class TestStatefulService : StatefulService
    {
        public const string StateManagerDictionaryKey = "dictionaryname";
        public const string StateManagerQueueKey = "queuename";

        public TestStatefulService(StatefulServiceContext serviceContext) : base(serviceContext)
        {
        }

        public TestStatefulService(StatefulServiceContext serviceContext, IReliableStateManagerReplica reliableStateManagerReplica)
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


        public async Task EnqueueAsync(Payload value)
        {
            var queue = await StateManager.GetOrAddAsync<IReliableQueue<Payload>>(StateManagerQueueKey);

            using (var tx = StateManager.CreateTransaction())
            {
                await queue.EnqueueAsync(tx, value);
                await tx.CommitAsync();
            }
        }
    }
}
