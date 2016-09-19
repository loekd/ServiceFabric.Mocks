using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace ServiceFabric.Mocks.Tests.Actors
{
    [StatePersistence(StatePersistence.Persisted)]
    public class MyStatefulActor : Actor, IMyStatefulActor
    {

        public MyStatefulActor(ActorService actorSerice, ActorId actorId)
            : base(actorSerice, actorId)
        {
        }

        public async Task InsertAsync(string stateName, Payload value)
        {
            await StateManager.AddStateAsync(stateName, value);
        }
    }
}