using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Data.Collections.Preview;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceFabric.Mocks.Tests.Support;

namespace ServiceFabric.Mocks.Tests
{
    [TestClass]
    public class StatefulServiceTests
    {
        private const string StatePayload = "some value";


        [TestMethod]
        public async Task TestServiceState_Dictionary()
        {
            var context = MockStatefulServiceContextFactory.Default;
            var stateManager = new MockReliableStateManager();
            var service = new TestStatefulService(context, stateManager);

            const string stateName = "test";
            var payload = new Payload(StatePayload);

            //create state
            await service.InsertAsync(stateName, payload);

            //get state
            var dictionary = await stateManager.TryGetAsync<IReliableDictionary<string, Payload>>(TestStatefulService.StateManagerDictionaryKey);
            var actual = (await dictionary.Value.TryGetValueAsync(null, stateName)).Value;
            Assert.AreEqual(StatePayload, actual.Content);
        }


        [TestMethod]
        public async Task TestServiceState_Queue()
        {
            var context = MockStatefulServiceContextFactory.Default;
            var stateManager = new MockReliableStateManager();
            var service = new TestStatefulService(context, stateManager);

            var payload = new Payload(StatePayload);

            //create state
            await service.EnqueueAsync(payload);

            //get state
            var queue = await stateManager.TryGetAsync<IReliableQueue<Payload>>(TestStatefulService.StateManagerQueueKey);
            var actual = (await queue.Value.TryPeekAsync(null)).Value;
            Assert.AreEqual(StatePayload, actual.Content);
        }

        [TestMethod]
        public async Task TestServiceState_ConcurrentQueue()
        {
            var context = MockStatefulServiceContextFactory.Default;
            var stateManager = new MockReliableStateManager();
            var service = new TestStatefulService(context, stateManager);

            var payload = new Payload(StatePayload);

            //create state
            await service.ConcurrentEnqueueAsync(payload);

            //get state
            var queue = await stateManager.TryGetAsync<IReliableConcurrentQueue<Payload>>(TestStatefulService.StateManagerConcurrentQueueKey);
            var actual = (await queue.Value.DequeueAsync(null));
            Assert.AreEqual(StatePayload, actual.Content);
        }
    }
}
