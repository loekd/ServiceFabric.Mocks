﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceFabric.Mocks.NetCoreTests.Actors;
using ServiceFabric.Mocks.NetCoreTests.Services;

namespace ServiceFabric.Mocks.NetCoreTests.ActorTests
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

        [TestMethod]
        public void TestNonIServiceProxyFactory()
        {
            //mock out the called service
            var mockProxyFactory = new MockServiceProxyFactory();
            var mockService = new MockTestStatefulServiceWithoutIService();
            mockProxyFactory.RegisterService(ServiceCallerActor.CalledServiceName, mockService);

            //act:
            var instance = mockProxyFactory.CreateNonIServiceProxy<MockTestStatefulServiceWithoutIService>(ServiceCallerActor.CalledServiceName);

            //assert:
            Assert.AreEqual(mockService, instance);
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

            public Task<IEnumerable<Payload>> GetPayloadsAsync()
            {
                throw new NotImplementedException();
            }

            public Task UpdatePayloadAsync(string stateName, string content)
            {
                throw new NotImplementedException();
            }
        }

        private class MockTestStatefulServiceWithoutIService
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

            public Task<IEnumerable<Payload>> GetPayloadsAsync()
            {
                throw new NotImplementedException();
            }

            public Task UpdatePayloadAsync(string stateName, string content)
            {
                throw new NotImplementedException();
            }
        }
    }
}
