﻿using System;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceFabric.Mocks.NetCoreTests.Actors;

namespace ServiceFabric.Mocks.NetCoreTests.ActorTests
{
    [TestClass]
    public class MyStatefulActorTests
    {
        private const string StatePayload = "some value";
        private const string OtherStatePayload = "some other value";

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

        [TestMethod]
        public async Task TestMultipleActors()
        {
            Func<ActorService, ActorId, ActorBase> actorFactory = (service, actorId) => new MyStatefulActor(service, actorId);
            var svc = MockActorServiceFactory.CreateActorServiceForActor<MyStatefulActor>(actorFactory);
            var sharedId = ActorId.CreateRandom();
            var actor1 = svc.Activate(sharedId);
            var actor2 = svc.Activate(new ActorId(Guid.NewGuid()));
            var actor3 = svc.Activate(new ActorId(Guid.NewGuid()));
            var actor1_2 = svc.Activate(sharedId);

            var stateManager1 = (MockActorStateManager)actor1.StateManager;
            var stateManager1_2 = (MockActorStateManager)actor1.StateManager;
            var stateManager2 = (MockActorStateManager)actor2.StateManager;
            var stateManager3 = (MockActorStateManager)actor3.StateManager;

            const string stateName = "test";
            var payload1 = new Payload(StatePayload);
            var payload2 = new Payload(OtherStatePayload);

            //create states
            await actor1.InsertAsync(stateName, payload1);
            await actor2.InsertAsync(stateName, payload2);

            Assert.AreSame(stateManager1, stateManager1_2);
            Assert.AreNotSame(stateManager1, stateManager2);

            //get states
            var actual1 = await stateManager1.GetStateAsync<Payload>(stateName);
            Assert.AreEqual(StatePayload, actual1.Content);
            var actual2 = await stateManager2.GetStateAsync<Payload>(stateName);
            Assert.AreEqual(OtherStatePayload, actual2.Content);
            var actual3 = await stateManager3.TryGetStateAsync<Payload>(stateName);
            Assert.IsFalse(actual3.HasValue);
        }

        private static MyStatefulActor CreateActor(ActorId id)
        {
            Func<ActorService, ActorId, ActorBase> actorFactory = (service, actorId) => new MyStatefulActor(service, actorId);
            var svc = MockActorServiceFactory.CreateActorServiceForActor<MyStatefulActor>(actorFactory);
            var actor = svc.Activate(id);
            return actor;
        }
    }
}
