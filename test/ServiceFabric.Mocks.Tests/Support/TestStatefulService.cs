using System.Fabric;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Runtime;

namespace ServiceFabric.Mocks.Tests.Support
{
    public class TestStatefulService : StatefulService
    {
        public const string StateManagerKey = "name";

        public TestStatefulService(StatefulServiceContext serviceContext) : base(serviceContext)
        {
        }

        public TestStatefulService(StatefulServiceContext serviceContext, IReliableStateManagerReplica reliableStateManagerReplica)
            : base(serviceContext, reliableStateManagerReplica)
        {
        }

        public async Task InsertAsync(string stateName, Payload value)
        {
            var dictionary = await StateManager.GetOrAddAsync<IReliableDictionary<string, Payload>>(StateManagerKey);

            using (var tx = StateManager.CreateTransaction())
            {
                await dictionary.TryAddAsync(tx, stateName, value);
                await tx.CommitAsync();
            }
        }
    }
}
