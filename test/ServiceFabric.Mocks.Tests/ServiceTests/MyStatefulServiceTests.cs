using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    }
}
