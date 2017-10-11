using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;

namespace ServiceFabric.Mocks.Tests.Services
{
    public interface IMyStatefulService : IService
    {
        Task ConcurrentEnqueueAsync(Payload value);

        Task EnqueueAsync(Payload value);

        Task InsertAsync(string stateName, Payload value);

        Task InsertAndAbortAsync(string stateName, Payload value);
    }
}