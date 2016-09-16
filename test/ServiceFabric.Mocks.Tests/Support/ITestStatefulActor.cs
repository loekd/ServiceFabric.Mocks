using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;

namespace ServiceFabric.Mocks.Tests.Support
{
    public interface ITestStatefulActor : IActor
    {
        Task InsertAsync(string stateName, Payload value);
    }
}