using System;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceFabric.Mocks.Tests.Actors;
using ServiceFabric.Mocks.Tests.Services;

namespace ServiceFabric.Mocks.Tests.ServiceTests
{
    [TestClass]
    public class ActorCallerServiceTests
    {
        [TestMethod]
        public async Task TestActorProxyFactory()
        {
            //mock out the called actor
            var id = new ActorId(ActorCallerService.CalledActorId);
            Func<ActorService, ActorId, ActorBase> actorFactory = (service, actorId) => new MockTestStatefulActor(service, id);
            var svc = MockActorServiceFactory.CreateActorServiceForActor<MockTestStatefulActor>(actorFactory);
            var actor = svc.Activate(id);

            //prepare the service:
            var mockProxyFactory = new MockActorProxyFactory();
            mockProxyFactory.RegisterActor(actor);
            var serviceInstance = new ActorCallerService(MockStatelessServiceContextFactory.Default, mockProxyFactory);

            //act:
            await serviceInstance.CallActorAsync();

            //assert:
            Assert.IsTrue(actor.InsertAsyncCalled);
        }

        [TestMethod]
        public void TestMultipleActorsForSingleActorId()
        {
            var sharedId = new ActorId(Guid.NewGuid());

            //mock out the called actors
            var statefulActorSvc = MockActorServiceFactory.CreateActorServiceForActor<MockTestStatefulActor>(
                (service, actorId) => new MockTestStatefulActor(service, actorId));
            var reminderActorService = MockActorServiceFactory.CreateActorServiceForActor<MockReminderTimerActor>(
                (service, actorId) => new MockReminderTimerActor(service, actorId));

            var statefulActor = statefulActorSvc.Activate(sharedId);
            var reminderActor = reminderActorService.Activate(sharedId);

            //prepare the service:
            var mockProxyFactory = new MockActorProxyFactory();
            mockProxyFactory.RegisterActor(statefulActor);
            mockProxyFactory.RegisterActor(reminderActor);

            //act:
            var a1 = mockProxyFactory.CreateActorProxy<IMyStatefulActor>(sharedId);
            var a2 = mockProxyFactory.CreateActorProxy<IReminderTimerActor>(sharedId);

            //assert:
            Assert.AreSame(typeof(MockTestStatefulActor), a1.GetType());
            Assert.AreSame(typeof(MockReminderTimerActor), a2.GetType());
        }


        private class MockTestStatefulActor : Actor, IMyStatefulActor
        {
            public bool InsertAsyncCalled { get; private set; }

            public MockTestStatefulActor(ActorService actorService, ActorId actorId) : base(actorService, actorId)
            {
            }

            public Task InsertAsync(string stateName, Payload value)
            {
                InsertAsyncCalled = true;
                return Task.FromResult(true);
            }
        }

        private class MockReminderTimerActor : Actor, IReminderTimerActor
        {
            public MockReminderTimerActor(ActorService actorService, ActorId actorId) : base(actorService, actorId)
            {
            }

            public Task RegisterReminderAsync(string reminderName)
            {
                return Task.FromResult(true);
            }

            public Task RegisterTimerAsync()
            {
                return Task.FromResult(true);
            }

	        public Task<bool> IsReminderRegisteredAsync(string reminderName)
	        {
		        throw new NotImplementedException();
	        }

	        public Task UnregisterReminderAsync(string reminderName)
	        {
		        throw new NotImplementedException();
	        }
        }
    }
}
