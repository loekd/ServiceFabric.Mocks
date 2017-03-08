using System;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceFabric.Mocks.Tests.Actors;

namespace ServiceFabric.Mocks.Tests.ActorTests
{
    [TestClass]
    public class ActorCallerActorTests
    {

        [TestMethod]
        public async Task TestServiceProxyFactory()
        {
            //mock out the called service

            var mockProxyFactory = new MockActorProxyFactory();
            mockProxyFactory.MissingActor += MockProxyFactory_MisingActorId;
            

            //prepare the actor:
            Func<ActorService, ActorId, ActorBase> actorFactory = (service, actorId) => new ActorCallerActor(service, actorId, mockProxyFactory);
            var svc = MockActorServiceFactory.CreateActorServiceForActor<ActorCallerActor>(actorFactory);
            var actor = svc.Activate(ActorId.CreateRandom());

            //act:
            await actor.InsertAsync("test", new Payload("some other value"));

            //check if the other actor was called
            var statefulActorId = await actor.StateManager.GetStateAsync<ActorId>(ActorCallerActor.ChildActorIdKeyName);

            var statefulActor = mockProxyFactory.CreateActorProxy<IMyStatefulActor>(ActorCallerActor.CalledServiceName, statefulActorId);
            
            var payload = await ((MyStatefulActor)statefulActor).StateManager.GetStateAsync<Payload>("test");

            //assert:
            Assert.AreEqual("some other value", payload.Content);
        }

        private static void MockProxyFactory_MisingActorId(object sender, MissingActorEventArgs args)
        {
	        if (args.ActorType != typeof(IMyStatefulActor)) return;

	        Func<ActorService, ActorId, ActorBase> actorFactory = (service, actorId) => new MyStatefulActor(service, actorId);
	        var svc = MockActorServiceFactory.CreateActorServiceForActor<MyStatefulActor>(actorFactory);
	        var actor = svc.Activate(args.Id);
	        args.ActorInstance = actor;
        }
    }
}
