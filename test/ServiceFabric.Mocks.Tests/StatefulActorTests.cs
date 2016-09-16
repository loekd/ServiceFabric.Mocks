using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceFabric.Mocks.Tests.Support;

namespace ServiceFabric.Mocks.Tests
{
    [TestClass]
    public class StatefulActorTests
    {
        private const string StatePayload = "some value";

        [TestMethod]
        public async Task TestActorState()
        {
            var actorGuid = Guid.NewGuid();
            var id = new ActorId(actorGuid);
            var stateManager = new MockActorStateManager();

            var actor = CreateActor(id, stateManager);

            const string stateName = "test";
            var payload = new Payload(StatePayload);

            //create state
            await actor.InsertAsync(stateName, payload);

            //get state
            var actual = await stateManager.GetStateAsync<Payload>(stateName);
            Assert.AreEqual(payload.Content, actual.Content);

        }

        private static TestStatefulActor CreateActor(ActorId id, MockActorStateManager stateManager)
        {
            Func<ActorService, ActorId, ActorBase> actorFactory = (service, actorId) => new TestStatefulActor(service, id);
            Func<ActorBase, IActorStateProvider, IActorStateManager> stateManagerFactory = (actr, stateProvider) => stateManager;
            var svc = new ActorService(MockStatefulServiceContextFactory.Default, ActorTypeInformation.Get(typeof(TestStatefulActor)), actorFactory, stateManagerFactory);
            var actor = new TestStatefulActor(svc, id);
            return actor;
        }
    }
}
