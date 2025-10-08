using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceFabric.Mocks.ReliableCollections;

namespace ServiceFabric.Mocks.NetCoreTests.MocksTests
{
    [TestClass]
    public class MockReliableDictionaryTests
    {
        [TestMethod]
        public async Task DictionaryAddDuplicateKeyExceptionTypeTest()
        {
            const string key = "key";
            var dictionary = new MockReliableDictionary<string, string>(new Uri("fabric://MockReliableDictionary"));
            var tx = new MockTransaction(null, 1);

            await dictionary.AddAsync(tx, key, "value");
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await dictionary.AddAsync(tx, key, "value");
            });
        }

        [TestMethod]
        public async Task DictionaryAddAndRetrieveTest()
        {
            const string key = "key";
            const string value = "value";

            var dictionary = new MockReliableDictionary<string, string>(new Uri("fabric://MockReliableDictionary"));
            var tx = new MockTransaction(null, 1);

            await dictionary.AddAsync(tx, key, value);
            var actual = await dictionary.TryGetValueAsync(tx, key);

            Assert.AreEqual(value, actual.Value);
        }

        [TestMethod]
        public async Task DictionaryCountTest()
        {
            const string key = "key";
            const string value = "value";

            var dictionary = new MockReliableDictionary<string, string>(new Uri("fabric://MockReliableDictionary"));
            var tx = new MockTransaction(null, 1);

            await dictionary.AddAsync(tx, key, value);
            var actual = dictionary.Count;

            Assert.AreEqual(1, actual);
        }

        [TestMethod]
        public async Task DictionaryCreateKeyEnumerableAsyncTest()
        {
            const string key = "key";
            const string value = "value";

            var dictionary = new MockReliableDictionary<string, string>(new Uri("fabric://MockReliableDictionary"));
            var tx = new MockTransaction(null, 1);

            await dictionary.AddAsync(tx, key, value);
            var enumerable = await dictionary.CreateKeyEnumerableAsync(tx);
            using var enumerator = enumerable.GetAsyncEnumerator();
            await enumerator.MoveNextAsync(CancellationToken.None);
            var actual = enumerator.Current;

            Assert.AreEqual(key, actual);
        }
    }
}
