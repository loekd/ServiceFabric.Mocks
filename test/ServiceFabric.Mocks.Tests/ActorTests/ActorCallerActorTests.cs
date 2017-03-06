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
    public class ActorCallerActorTests
    {
        static MockActorProxyFactory _mockProxyFactory;

        static ActorCallerActorTests()
        {
            _mockProxyFactory = new MockActorProxyFactory(); 
        }


        [TestMethod]
        public async Task TestServiceProxyFactory()
        {
            //mock out the called service

            _mockProxyFactory.MisingActor += MockProxyFactory_MisingActorId;
            

            //prepare the actor:
            Func<ActorService, ActorId, ActorBase> actorFactory = (service, actorId) => new ActorCallerActor(service, actorId, _mockProxyFactory);
            var svc = MockActorServiceFactory.CreateActorServiceForActor<ActorCallerActor>(actorFactory);
            var actor = svc.Activate(ActorId.CreateRandom());

            //act:
            await actor.InsertAsync("test", new Payload("some other value"));

            //check if the other actor was called
            var statefulActorId = await actor.StateManager.GetStateAsync<ActorId>(ActorCallerActor.ChildActorIdKeyName);

            Func<ActorService, ActorId, ActorBase> statefulActorFactory = (service, actorId) => new MyStatefulActor(service, actorId);
            var statefulActor = _mockProxyFactory.CreateActorProxy<IMyStatefulActor>(ActorCallerActor.CalledServiceName, statefulActorId);
            
            var payload = await ((MyStatefulActor)statefulActor).StateManager.GetStateAsync<Payload>("test");

            //assert:
            Assert.AreEqual("some other value", payload.Content);
        }

        private void MockProxyFactory_MisingActorId(object sender, MisingActorEventArgs args)
        {
            var registrar = (MockActorProxyFactory)sender;

            Func<ActorService, ActorId, ActorBase> actorFactory = (service, actorId) => new MyStatefulActor(service, actorId);
            var svc = MockActorServiceFactory.CreateActorServiceForActor<MyStatefulActor>(actorFactory);
            var actor = svc.Activate(args.Id);
            registrar.RegisterActor(actor);
        }
        
    }
}
