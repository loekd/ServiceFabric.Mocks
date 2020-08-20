using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading;
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


        [TestMethod]
        public async Task InfiniteLoop_Issue91()
        {
            var sut = new MockReliableStateManager();

            var collection = await sut.GetOrAddAsync<IReliableDictionary2<Guid, long>>("Collection");

            using (var tx = sut.CreateTransaction())
            {

                var query = await collection.CreateEnumerableAsync(tx, key => false, EnumerationMode.Unordered);

                var list = new List<Guid>();
                //This goes into infinite loop if the query returns an empty collection with a key value pair of null for both the key and the value.
                Microsoft.ServiceFabric.Data.IAsyncEnumerator<KeyValuePair<Guid, long>> asyncEnumerator = query.GetAsyncEnumerator();
                while (await asyncEnumerator.MoveNextAsync(CancellationToken.None))
                {
                    list.Add(asyncEnumerator.Current.Key);
                }
                await tx.CommitAsync();
            }
            Assert.IsTrue(true, "Seems to work.");
            //Assert.Fail("Shouldn't reach here.");
        }


        //provided as repro, but doesn't repro in mstest
        [TestMethod][Ignore]
        public async Task LoadTest()
        {
            var stateManager = new MockReliableStateManager();
            var data = await stateManager.GetOrAddAsync<IReliableDictionary<Guid, string>>("data");
            var updates = new List<Task>();
            var id = Guid.NewGuid();

            for (var i = 0; i < 100_000; i++)
            {
                updates.Add(Task.Run(async () =>
                {
                    using (var tx = stateManager.CreateTransaction())
                    {
                        var newValue = DateTime.Now.ToString();
                        await data.AddOrUpdateAsync(tx, id, newValue, (key, value) => newValue).ConfigureAwait(false);

                        await tx.CommitAsync().ConfigureAwait(false);
                    }
                }));
            }

            await Task.WhenAll(updates);
        }
    }
}
