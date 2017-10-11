using System;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceFabric.Mocks.Tests.Actors;
using ServiceFabric.Mocks.Tests.Services;

namespace ServiceFabric.Mocks.Tests.ActorTests
{
    [TestClass]
    public class ServiceCallerActorTests
    {
        [TestMethod]
        public async Task TestServiceProxyFactory()
        {
            //mock out the called service
            var mockProxyFactory = new MockServiceProxyFactory();
            var mockService = new MockTestStatefulService();
            mockProxyFactory.RegisterService(ServiceCallerActor.CalledServiceName, mockService);

            //prepare the actor:
            Func<ActorService, ActorId, ActorBase> actorFactory = (service, actorId) => new ServiceCallerActor(service, actorId, mockProxyFactory);
            var svc = MockActorServiceFactory.CreateActorServiceForActor<ServiceCallerActor>(actorFactory);
            var actor = svc.Activate(ActorId.CreateRandom());

            //act:
            await actor.InsertAsync("test", new Payload("some other value"));

            //assert:
            Assert.IsTrue(mockService.InsertAsyncCalled);
        }
        
        private class MockTestStatefulService : IMyStatefulService
        {
            public bool InsertAsyncCalled { get; private set; }

            public Task ConcurrentEnqueueAsync(Payload value)
            {
                throw new NotImplementedException();
            }

            public Task EnqueueAsync(Payload value)
            {
                throw new NotImplementedException();
            }

            public Task InsertAsync(string stateName, Payload value)
            {
                InsertAsyncCalled = true;
                return Task.FromResult(true);
            }

            public Task InsertAndAbortAsync(string stateName, Payload value)
            {
                throw new NotImplementedException();
            }
        }
    }
}
