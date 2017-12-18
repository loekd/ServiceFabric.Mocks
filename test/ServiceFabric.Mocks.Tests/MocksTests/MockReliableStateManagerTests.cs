using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace ServiceFabric.Mocks.Tests.MocksTests
{
    [TestClass]
    public class MockReliableStateManagerTests
    {
        [TestMethod]
        public async Task GetOrAddAsync_Dictionary()
        {
            var collection = "GetOrAddAsync_Dictionary";
            var sut = new MockReliableStateManager();

            var actual = await sut.GetOrAddAsync<IReliableDictionary<string, string>>(collection);

            Assert.IsNotNull(actual);
        }

        [TestMethod]
        public async Task GetOrAddAsync_Dictionary2()
        {
            var collection = "GetOrAddAsync_Dictionary2";
            var sut = new MockReliableStateManager();

            var actual = await sut.GetOrAddAsync<IReliableDictionary2<string, string>>(collection);

            Assert.IsNotNull(actual);
        }

        [TestMethod]
        public async Task GetOrAddAsync_ConcurrentQueue()
        {
            var collection = "GetOrAddAsync_ConcurrentQueue";
            var sut = new MockReliableStateManager();

            var actual = await sut.GetOrAddAsync<IReliableConcurrentQueue<string>>(collection);

            Assert.IsNotNull(actual);
        }

        [TestMethod]
        public async Task GetOrAddAsync_Queue()
        {
            var collection = "GetOrAddAsync_Queue";
            var sut = new MockReliableStateManager();

            var actual = await sut.GetOrAddAsync<IReliableQueue<string>>(collection);

            Assert.IsNotNull(actual);
        }

        [TestMethod]
        public async Task TryGetAsync_Dictionary()
        {
            var collection = "TryGetAsync_Dictionary";
            var sut = new MockReliableStateManager();

            await sut.GetOrAddAsync<IReliableDictionary<string, string>>(collection);
            var actual = await sut.TryGetAsync<IReliableDictionary<string, string>>(collection);

            Assert.IsNotNull(actual);
            Assert.IsTrue(actual.HasValue);
        }

        [TestMethod]
        public async Task TryGetAsync_Dictionary2()
        {
            var collection = "TryGetAsync_Dictionary2";
            var sut = new MockReliableStateManager();

            await sut.GetOrAddAsync<IReliableDictionary2<string, string>>(collection);
            var actual = await sut.TryGetAsync<IReliableDictionary2<string, string>>(collection);

            Assert.IsNotNull(actual);
            Assert.IsTrue(actual.HasValue);
        }

        [TestMethod]
        public async Task TryGetAsync_ConcurrentQueue()
        {
            var collection = "TryGetAsync_ConcurrentQueue";
            var sut = new MockReliableStateManager();

            await sut.GetOrAddAsync<IReliableConcurrentQueue<string>>(collection);
            var actual = await sut.TryGetAsync<IReliableConcurrentQueue<string>>(collection);

            Assert.IsNotNull(actual);
            Assert.IsTrue(actual.HasValue);
        }

        [TestMethod]
        public async Task TryGetAsync_Queue()
        {
            var collection = "TryGetAsync_Queue";
            var sut = new MockReliableStateManager();

            await sut.GetOrAddAsync<IReliableQueue<string>>(collection);
            var actual = await sut.TryGetAsync<IReliableQueue<string>>(collection);

            Assert.IsNotNull(actual);
            Assert.IsTrue(actual.HasValue);
        }

        [TestMethod]
        public async Task RemoveAsync()
        {
            var collection = "RemoveAsync";
            var sut = new MockReliableStateManager();

            await sut.GetOrAddAsync<IReliableDictionary<string, string>>(collection);
            await sut.RemoveAsync(collection);
            var actual = await sut.TryGetAsync<IReliableDictionary<string, string>>(collection);

            Assert.IsNotNull(actual);
            Assert.IsFalse(actual.HasValue);
        }
    }
}
