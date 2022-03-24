﻿using System;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceFabric.Mocks.NetCoreTests.Actors;

namespace ServiceFabric.Mocks.NetCoreTests.ActorTests
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

        [TestMethod]
        public void TestMissingActorCreatedInEventHandler()
        {
            var mockProxyFactory = new MockActorProxyFactory();
            mockProxyFactory.MissingActor += MockProxyFactory_MisingActorId;
            var actorId = ActorId.CreateRandom();
            var instance = mockProxyFactory.CreateActorProxy<IMyStatefulActor>(actorId);

            Assert.IsInstanceOfType(instance, typeof(IMyStatefulActor));
        }

        [TestMethod]
        public void TestMissingActorNotCreatedInEventHandler()
        {
            var mockProxyFactory = new MockActorProxyFactory();
            var actorId = ActorId.CreateRandom();
            var instance = mockProxyFactory.CreateActorProxy<IMyStatefulActor>(actorId);
            Assert.IsNull(instance);
        }

        [TestMethod]
        public void TestMissingActorSecondTypeCreatedInEventHandler()
        {
            var mockProxyFactory = new MockActorProxyFactory();
            mockProxyFactory.MissingActor += MockProxyFactory_MisingActor_TwoTypes;
            var actorId = ActorId.CreateRandom();
            var instance1 = mockProxyFactory.CreateActorProxy<IMyStatefulActor>(actorId);
            var instance2 = mockProxyFactory.CreateActorProxy<IReminderTimerActor>(actorId);

            Assert.IsInstanceOfType(instance1, typeof(IMyStatefulActor));
            Assert.IsInstanceOfType(instance2, typeof(IReminderTimerActor));
        }


        private static void MockProxyFactory_MisingActorId(object sender, MissingActorEventArgs args)
        {
            if (args.ActorType != typeof(IMyStatefulActor)) return;

            Func<ActorService, ActorId, ActorBase> actorFactory = (service, actorId) => new MyStatefulActor(service, actorId);
            var svc = MockActorServiceFactory.CreateActorServiceForActor<MyStatefulActor>(actorFactory);
            var actor = svc.Activate(args.Id);
            args.ActorInstance = actor;
        }

        private static void MockProxyFactory_MisingActor_TwoTypes(object sender, MissingActorEventArgs args)
        {
            if (args.ActorType == typeof(IMyStatefulActor))
            {
                Func<ActorService, ActorId, ActorBase> actorFactory = (service, actorId) =>
                    new MyStatefulActor(service, actorId);
                var svc = MockActorServiceFactory.CreateActorServiceForActor<MyStatefulActor>(actorFactory);
                var actor = svc.Activate(args.Id);
                args.ActorInstance = actor;
            }
            else if (args.ActorType == typeof(IReminderTimerActor))
            {
                Func<ActorService, ActorId, ActorBase> actorFactory = (service, actorId) =>
                    new ReminderTimerActor(service, actorId);
                var svc = MockActorServiceFactory.CreateActorServiceForActor<ReminderTimerActor>(actorFactory);
                var actor = svc.Activate(args.Id);
                args.ActorInstance = actor;
            }
        }
    }
}
