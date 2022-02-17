using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceFabric.Mocks.NetCoreTests.Actors;
using ServiceFabric.Mocks.NetCoreTests.ActorServices;

namespace ServiceFabric.Mocks.NetCoreTests.ActorTests
{
    [TestClass]
    public class CustomActorServiceTests
    {
        [TestMethod]
        public void TestCustomActorServiceActivate()
        {
            //an ActorService with a standard constructor can be created by the MockActorServiceFactory
            var customActorService = MockActorServiceFactory.CreateCustomActorServiceForActor<CustomActorService, InvokeOnActor>();
            var actor = customActorService.Activate<InvokeOnActor>(new ActorId(123L));

            Assert.IsInstanceOfType(customActorService, typeof(CustomActorService));
            Assert.IsInstanceOfType(actor, typeof(InvokeOnActor));
            Assert.AreEqual(123L, actor.Id.GetLongId());
        }

        [TestMethod]
        public async Task CustomStatefulActor_ShouldHaveUniqueStateManagerPerId()
        {
            Dictionary<ActorId, IActorStateManager> stateManagerMap = new Dictionary<ActorId, IActorStateManager>();

            Func<ActorBase, IActorStateProvider, IActorStateManager> stateManagerFactory = (actr, stateProvider) =>
            {
                if (!stateManagerMap.TryGetValue(actr.Id, out var actorStateManager))
                {
                    actorStateManager = new MockActorStateManager();
                    stateManagerMap[actr.Id] = actorStateManager;
                }
                return actorStateManager;
            };

            // Every actor instance has its own state manager.
            Func<ActorService, ActorId, ActorBase> actorFactory = (service, actorId) => new MyStatefulActor(service, actorId);
            var customActorService = MockActorServiceFactory.CreateCustomActorServiceForActor<CustomActorService, MyStatefulActor>(actorFactory, stateManagerFactory: stateManagerFactory);

            var id1 = ActorId.CreateRandom();
            var id2 = ActorId.CreateRandom();
            var actor1 = customActorService.Activate<MyStatefulActor>(id1);
            var stateManager1 = (MockActorStateManager)actor1.StateManager;

            var actor2 = customActorService.Activate<MyStatefulActor>(id2);
            var stateManager2 = (MockActorStateManager)actor2.StateManager;

            var actor1_2 = customActorService.Activate<MyStatefulActor>(id1);
            var stateManager1_2 = (MockActorStateManager)actor1_2.StateManager;


            const string stateName = "test";
            const string payloadText = "foo";
            var payload = new Payload(payloadText);

            //create state
            await actor1.InsertAsync(stateName, payload);

            //get state
            var actualState1 = await stateManager1.GetStateAsync<Payload>(stateName);
            Assert.AreEqual(payloadText, actualState1.Content);

            var actualState2 = await stateManager2.TryGetStateAsync<Payload>(stateName);
            Assert.IsFalse(actualState2.HasValue);

            var actualState1_2 = await stateManager1_2.GetStateAsync<Payload>(stateName);
            Assert.AreEqual(payloadText, actualState1_2.Content);

            Assert.AreNotSame(stateManager1, stateManager2);
            Assert.AreSame(stateManager1, stateManager1_2);
        }

        [TestMethod]
        public void TestAnotherCustomActorService_CreateFails()
        {
            //an ActorService with a NON standard constructor can be created by the MockActorServiceFactory
            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                // ReSharper disable once UnusedVariable
                var customActorService =
                    MockActorServiceFactory.CreateCustomActorServiceForActor<AnotherCustomActorService, InvokeOnActor>();
            });
        }

        [TestMethod]
        public void TestAnotherCustomActorService()
        {
            //an ActorService with a NON standard constructor can be created by passing Mock arguments:

            IActorStateProvider actorStateProvider = new MockActorStateProvider();
            actorStateProvider.Initialize(ActorTypeInformation.Get(typeof(InvokeOnActor)));
            var context = MockStatefulServiceContextFactory.Default;
            var dummy = new object(); //this argument causes the 'non standard' ctor.
            var customActorService = new AnotherCustomActorService(dummy, context, ActorTypeInformation.Get(typeof(InvokeOnActor)));

            var actor = customActorService.Activate<InvokeOnActor>(new ActorId(123L));

            Assert.IsInstanceOfType(actor, typeof(InvokeOnActor));
            Assert.AreEqual(123L, actor.Id.GetLongId());
        }
    }
}
