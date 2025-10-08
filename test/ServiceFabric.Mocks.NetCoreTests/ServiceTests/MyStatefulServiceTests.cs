using System;
using System.Fabric;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceFabric.Mocks.NetCoreTests.Services;
using ServiceFabric.Mocks.ReliableCollections;
using ServiceFabric.Mocks.ReplicaSet;

namespace ServiceFabric.Mocks.NetCoreTests.ServiceTests
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
        public async Task ReplicateSet_InitDataPassed()
        {
            var initData = Encoding.UTF8.GetBytes("blah");

            var replicaSet = new MockStatefulServiceReplicaSet<MyStatefulService>(CreateStatefulService, CreateStateManagerReplica);
            await replicaSet.AddReplicaAsync(ReplicaRole.Primary, 1, initializationData: initData);
            await replicaSet.AddReplicaAsync(ReplicaRole.ActiveSecondary, 2, initializationData: initData);
            await replicaSet.AddReplicaAsync(ReplicaRole.ActiveSecondary, 3, initializationData: initData);

            CollectionAssert.AreEqual(Encoding.UTF8.GetBytes("blah"), replicaSet.Primary.ServiceInstance.Context.InitializationData);
            foreach (var i in replicaSet.SecondaryReplicas)
                CollectionAssert.AreEqual(Encoding.UTF8.GetBytes("blah"),
                i.ServiceInstance.Context.InitializationData);
        }

        [TestMethod]
        [Ignore]
        public async Task TestServiceStateChangesDuringPromote()
        {
            var replicaSet = new MockStatefulServiceReplicaSet<MyStatefulService>(CreateStatefulService, CreateStateManagerReplica);
            await replicaSet.AddReplicaAsync(ReplicaRole.Primary, 1);
            var originalPrimary = replicaSet.Primary;
            await replicaSet.AddReplicaAsync(ReplicaRole.ActiveSecondary, 2);
            var originalSecondary = replicaSet.SecondaryReplicas.Single();

            //promote the secondary to primary
            await replicaSet.PromoteActiveSecondaryToPrimaryAsync(2);

            bool hasRun = originalPrimary.ServiceInstance.RunAsyncHasRun.WaitOne(500);
            Assert.IsTrue(hasRun, "RunAsync did not run on original primary");

            hasRun = originalSecondary.ServiceInstance.RunAsyncHasRun.WaitOne(500);
            Assert.IsTrue(hasRun, "RunAsync did not run on new primary");

            bool hasChanged = originalPrimary.ServiceInstance.ChangeRoleAsyncHasRun.WaitOne(500);
            Assert.IsTrue(hasChanged, "ChangeRole did not run on original primary");

            hasRun = originalSecondary.ServiceInstance.ChangeRoleAsyncHasRun.WaitOne(500);
            Assert.IsTrue(hasRun, "ChangeRole did not run on new primary");

            bool hasClearedCache = originalPrimary.ServiceInstance.CacheCleared.Wait(500);
            Assert.IsTrue(hasClearedCache, "Cache was not cleared on original primary");
        }

        [TestMethod]
        [Ignore]
        public async Task TestServiceState_InMemoryState_PromoteActiveSecondary()
        {
            var replicaSet = new MockStatefulServiceReplicaSet<MyStatefulService>(CreateStatefulService, CreateStateManagerReplica);
            await replicaSet.AddReplicaAsync(ReplicaRole.Primary, 1);
            var originalPrimary = replicaSet.Primary;
            await replicaSet.AddReplicaAsync(ReplicaRole.ActiveSecondary, 2);
            var originalSecondary = replicaSet.SecondaryReplicas.Single();

            const string stateName = "test";
            var payload = new Payload(StatePayload);

            //insert data
            await replicaSet.Primary.ServiceInstance.InsertAsync(stateName, payload);

            //promote one of the secondaries to primary
            await replicaSet.PromoteActiveSecondaryToPrimaryAsync(2);

            //wait
            bool hasRun = originalPrimary.ServiceInstance.RunAsyncHasRun.WaitOne(500);
            Assert.IsTrue(hasRun, "RunAsync did not run on original primary");

            hasRun = originalSecondary.ServiceInstance.RunAsyncHasRun.WaitOne(500);
            Assert.IsTrue(hasRun, "RunAsync did not run on new primary");

            //get data
            var payloads = (await replicaSet.Primary.ServiceInstance.GetPayloadsAsync()).ToList();

            //data should match what was inserted against the primary
            Assert.AreEqual(payloads.Count, 1, "Unexpected payload count");
            Assert.AreEqual(payloads[0].Content, payload.Content, "Unexpected payload content");

            //the original primary should not have any in-memory state
            var payloadsFromOldPrimary = await originalPrimary.ServiceInstance.GetPayloadsAsync();
            Assert.IsFalse(payloadsFromOldPrimary.Any(), "Original primary payload should have been erased");
        }

        [TestMethod]
        [Ignore]
        public async Task TestServiceState_InMemoryState_PromoteActiveSecondary_WithLongRunAsync()
        {
            var replicaSet = new MockStatefulServiceReplicaSet<MyLongRunningStatefulService>(CreateLongRunningStatefulService, CreateStateManagerReplica);
            await replicaSet.AddReplicaAsync(ReplicaRole.Primary, 1);
            var originalPrimary = replicaSet.Primary;
            await replicaSet.AddReplicaAsync(ReplicaRole.ActiveSecondary, 2);
            var originalSecondary = replicaSet.SecondaryReplicas.Single();
            await replicaSet.AddReplicaAsync(ReplicaRole.ActiveSecondary, 3);

            const string stateName = "test";
            var payload = new Payload(StatePayload);

            //insert data
            await replicaSet.Primary.ServiceInstance.InsertAsync(stateName, payload);
            //promote one of the secondaries to primary
            await replicaSet.PromoteActiveSecondaryToPrimaryAsync(2);

            //wait
            bool hasRun = originalPrimary.ServiceInstance.RunAsyncHasRun.WaitOne(500);
            Assert.IsTrue(hasRun, "RunAsync did not run on original primary");

            hasRun = originalSecondary.ServiceInstance.RunAsyncHasRun.WaitOne(500);
            Assert.IsTrue(hasRun, "RunAsync did not run on new primary");

            //get data
            var payloads = (await replicaSet.Primary.ServiceInstance.GetPayloadsAsync()).ToList();

            //data should match what was inserted against the primary
            Assert.AreEqual(payloads.Count, 1, "Unexpected payload count");
            Assert.AreEqual(payloads[0].Content, payload.Content, "Unexpected payload content");

            //the original primary should not have any in-memory state
            var payloadsFromOldPrimary = await originalPrimary.ServiceInstance.GetPayloadsAsync();
            Assert.IsFalse(payloadsFromOldPrimary.Any(), "Original primary payload should have been erased");
        }

        [TestMethod]
        [Ignore]
        public async Task TestServiceState_InMemoryState_PromoteNewReplica()
        {
            var replicaSet = new MockStatefulServiceReplicaSet<MyStatefulService>(CreateStatefulService, CreateStateManagerReplica);
            await replicaSet.AddReplicaAsync(ReplicaRole.Primary, 1);
            var originalPrimary = replicaSet.Primary;
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
            Assert.AreEqual(payloads.Count, 1, "Unexpected payload count");
            Assert.AreEqual(payloads[0].Content, payload.Content, "Unexpected payload content");

            //the original primary should not have any in-memory state
            var payloadsFromOldPrimary = await originalPrimary.ServiceInstance.GetPayloadsAsync();
            Assert.IsFalse(payloadsFromOldPrimary.Any(), "Original primary payload should have been erased");
        }

        private static MyStatefulService CreateStatefulService(StatefulServiceContext context, IReliableStateManagerReplica2 stateManager)
        {
            return new MyStatefulService(context, stateManager);
        }

        private static MyLongRunningStatefulService CreateLongRunningStatefulService(StatefulServiceContext context, IReliableStateManagerReplica2 stateManager)
        {
            return new MyLongRunningStatefulService(context, stateManager);
        }

        private static IReliableStateManagerReplica2 CreateStateManagerReplica(StatefulServiceContext ctx, TransactedConcurrentDictionary<Uri, IReliableState> states)
        {
            return new MockReliableStateManager(states);
        }
    }
}
