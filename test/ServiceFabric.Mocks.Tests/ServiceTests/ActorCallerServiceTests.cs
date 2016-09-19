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
            Func<ActorService, ActorId, ActorBase> actorFactory = (service, actorId) => new MyStatefulActor(service, id);
            var svc = MockActorServiceFactory.CreateActorServiceForActor<MyStatefulActor>(actorFactory);
            var actor = new MockTestStatefulActor(svc, id);

            //prepare the service:
            var mockProxyFactory = new MockActorProxyFactory();
            mockProxyFactory.RegisterActor(actor);
            var serviceInstance = new ActorCallerService(MockStatelessServiceContextFactory.Default, mockProxyFactory);

            //act:
            await serviceInstance.CallActorAsync();

            //assert:
            Assert.IsTrue(actor.InsertAsyncCalled);
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
    }
}
