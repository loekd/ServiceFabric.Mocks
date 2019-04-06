using System;
using System.Fabric;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceFabric.Mocks.ReliableCollections;
using ServiceFabric.Mocks.ReplicaSet;
using ServiceFabric.Mocks.Tests.Services;

namespace ServiceFabric.Mocks.Tests.ServiceTests
{
    [TestClass]
    public class MyStatefulServiceTests
    {
        private const string StatePayload = "some value";


        [TestMethod]
        public async Task TestServiceState_Dictionary()
        {
            var context = MockStatefulServiceContextFactory.Default;
            var stateManager = new MockReliableStateManager();
            var service = new MyStatefulService(context, stateManager);

            const string stateName = "test";
            var payload = new Payload(StatePayload);

            //create state
            await service.InsertAsync(stateName, payload);

            //get state
            var dictionary = await stateManager.TryGetAsync<IReliableDictionary<string, Payload>>(MyStatefulService.StateManagerDictionaryKey);
            var actual = (await dictionary.Value.TryGetValueAsync(stateManager.CreateTransaction(), stateName)).Value;
            Assert.AreEqual(StatePayload, actual.Content);
        }

        [TestMethod]
        public async Task TestServiceState_Queue()
        {
            var context = MockStatefulServiceContextFactory.Default;
            var stateManager = new MockReliableStateManager();
            var service = new MyStatefulService(context, stateManager);

            var payload = new Payload(StatePayload);

            //create state
            await service.EnqueueAsync(payload);

            //get state
            var queue = await stateManager.TryGetAsync<IReliableQueue<Payload>>(MyStatefulService.StateManagerQueueKey);
            var actual = (await queue.Value.TryPeekAsync(stateManager.CreateTransaction())).Value;
            Assert.AreEqual(StatePayload, actual.Content);
        }

        [TestMethod]
        public async Task TestServiceState_ConcurrentQueue()
        {
            var context = MockStatefulServiceContextFactory.Default;
            var stateManager = new MockReliableStateManager();
            var service = new MyStatefulService(context, stateManager);

            var payload = new Payload(StatePayload);

            //create state
            await service.ConcurrentEnqueueAsync(payload);

            //get state
            var queue = await stateManager.TryGetAsync<IReliableConcurrentQueue<Payload>>(MyStatefulService.StateManagerConcurrentQueueKey);
            var actual = (await queue.Value.TryDequeueAsync(stateManager.CreateTransaction())).Value;
            Assert.AreEqual(StatePayload, actual.Content);
        }

        [TestMethod]
        public async Task TestServiceState_InMemoryState_PromoteActiveSecondary()
        {
            var replicaSet = new MockStatefulServiceReplicaSet<MyStatefulService>(CreateStatefulService, CreateStateManagerReplica);
            await replicaSet.AddReplicaAsync(ReplicaRole.Primary, 1);
            await replicaSet.AddReplicaAsync(ReplicaRole.ActiveSecondary, 2);
            await replicaSet.AddReplicaAsync(ReplicaRole.ActiveSecondary, 3);

            const string stateName = "test";
            var payload = new Payload(StatePayload);

            //insert data
            await replicaSet.Primary.ServiceInstance.InsertAsync(stateName, payload);
            //promote one of the secondaries to primary
            await replicaSet.PromoteActiveSecondaryToPrimaryAsync(2);
            //get data
            var payloads = (await replicaSet.Primary.ServiceInstance.GetPayloadsAsync()).ToList();

            //data should match what was inserted against the primary
            Assert.IsTrue(payloads.Count == 1);
            Assert.IsTrue(payloads[0].Content == payload.Content);

            //the primary should not have any in-memory state
            var payloadsFromOldPrimary = await replicaSet[1].ServiceInstance.GetPayloadsAsync();
            Assert.IsTrue(!payloadsFromOldPrimary.Any());
        }

        [TestMethod]
        public async Task TestServiceState_InMemoryState_PromoteActiveSecondary_WithLongRunAsync()
        {
            var replicaSet = new MockStatefulServiceReplicaSet<MyLongRunningStatefulService>(CreateLongRunningStatefulService, CreateStateManagerReplica);
            await replicaSet.AddReplicaAsync(ReplicaRole.Primary, 1);
            await replicaSet.AddReplicaAsync(ReplicaRole.ActiveSecondary, 2);
            await replicaSet.AddReplicaAsync(ReplicaRole.ActiveSecondary, 3);

            const string stateName = "test";
            var payload = new Payload(StatePayload);

            //insert data
            await replicaSet.Primary.ServiceInstance.InsertAsync(stateName, payload);
            //promote one of the secondaries to primary
            await replicaSet.PromoteActiveSecondaryToPrimaryAsync(2);
            //get data
            var payloads = (await replicaSet.Primary.ServiceInstance.GetPayloadsAsync()).ToList();

            //data should match what was inserted against the primary
            Assert.IsTrue(payloads.Count == 1);
            Assert.IsTrue(payloads[0].Content == payload.Content);

            //the primary should not have any in-memory state
            var payloadsFromOldPrimary = await replicaSet[1].ServiceInstance.GetPayloadsAsync();
            Assert.IsTrue(!payloadsFromOldPrimary.Any());
        }

        [TestMethod]
        public async Task TestServiceState_InMemoryState_PromoteNewReplica()
        {
            var replicaSet = new MockStatefulServiceReplicaSet<MyStatefulService>(CreateStatefulService, CreateStateManagerReplica);
            await replicaSet.AddReplicaAsync(ReplicaRole.Primary, 1);
            await replicaSet.AddReplicaAsync(ReplicaRole.ActiveSecondary, 2);
            await replicaSet.AddReplicaAsync(ReplicaRole.ActiveSecondary, 3);

            const string stateName = "test";
            var payload = new Payload(StatePayload);

            //insert data
            await replicaSet.Primary.ServiceInstance.InsertAsync(stateName, payload);
            //promote one of the secondaries to primary
            await replicaSet.PromoteNewReplicaToPrimaryAsync(4);
            //get data
            var payloads = (await replicaSet.Primary.ServiceInstance.GetPayloadsAsync()).ToList();

            //data should match what was inserted against the primary
            Assert.IsTrue(payloads.Count == 1);
            Assert.IsTrue(payloads[0].Content == payload.Content);

            //the primary should not have any in-memory state
            var payloadsFromOldPrimary = await replicaSet[1].ServiceInstance.GetPayloadsAsync();
            Assert.IsTrue(payloadsFromOldPrimary.Any() == false);
        }

        private MyStatefulService CreateStatefulService(StatefulServiceContext context, IReliableStateManagerReplica2 stateManager)
        {
            return new MyStatefulService(context, stateManager);
        }

        private MyLongRunningStatefulService CreateLongRunningStatefulService(StatefulServiceContext context, IReliableStateManagerReplica2 stateManager)
        {
            return new MyLongRunningStatefulService(context, stateManager);
        }

        private IReliableStateManagerReplica2 CreateStateManagerReplica(StatefulServiceContext ctx, TransactedConcurrentDictionary<Uri, IReliableState> states)
        {
            return new MockReliableStateManager(states);
        }
    }
}
