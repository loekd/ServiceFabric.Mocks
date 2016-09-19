using System;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceFabric.Mocks.Tests.Actors;

namespace ServiceFabric.Mocks.Tests.ActorTests
{
    [TestClass]
    public class MyStatefulActorTests
    {
        private const string StatePayload = "some value";

        [TestMethod]
        public async Task TestActorState()
        {
            var actor = CreateActor(new ActorId(Guid.NewGuid()));
            var stateManager = (MockActorStateManager)actor.StateManager;

            const string stateName = "test";
            var payload = new Payload(StatePayload);

            //create state
            await actor.InsertAsync(stateName, payload);

            //get state
            var actual = await stateManager.GetStateAsync<Payload>(stateName);
            Assert.AreEqual(StatePayload, actual.Content);
        }

        private static MyStatefulActor CreateActor(ActorId id)
        {
            Func<ActorService, ActorId, ActorBase> actorFactory = (service, actorId) => new MyStatefulActor(service, id);
            var svc = MockActorServiceFactory.CreateActorServiceForActor<MyStatefulActor>(actorFactory);
            var actor = new MyStatefulActor(svc, id);
            return actor;
        }
    }
}
