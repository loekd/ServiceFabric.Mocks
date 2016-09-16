using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceFabric.Mocks.Tests.Support;

namespace ServiceFabric.Mocks.Tests
{
    [TestClass]
    public class StatefulServiceTests
    {
        private const string StatePayload = "some value";


        [TestMethod]
        public async Task TestServiceState()
        {
            var context = MockStatefulServiceContextFactory.Default;
            var stateManager = new MockReliableStateManager();
            var service = new TestStatefulService(context, stateManager);

            const string stateName = "test";
            var payload = new Payload(StatePayload);

            //create state
            await service.InsertAsync(stateName, payload);

            //get state
            var dictionary = await stateManager.TryGetAsync<IReliableDictionary<string, Payload>>(TestStatefulService.StateManagerKey);
            var actual = (await dictionary.Value.TryGetValueAsync(null, stateName)).Value;
            Assert.AreEqual(payload.Content, actual.Content);
        }
    }
}
