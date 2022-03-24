using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;

namespace ServiceFabric.Mocks.NetCoreTests.Services
{
    public interface IMyStatefulService : IService
    {
        Task ConcurrentEnqueueAsync(Payload value);

        Task EnqueueAsync(Payload value);

        Task InsertAsync(string stateName, Payload value);

        Task InsertAndAbortAsync(string stateName, Payload value);

        Task<IEnumerable<Payload>> GetPayloadsAsync();

        Task UpdatePayloadAsync(string stateName, string content);
    }
}