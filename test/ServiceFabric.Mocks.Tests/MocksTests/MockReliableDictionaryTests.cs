using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using ServiceFabric.Mocks.ReliableCollections;
using System.Threading;

namespace ServiceFabric.Mocks.Tests.MocksTests
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
            await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
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

            Assert.AreEqual(actual.Value, value);
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
            var enumerator = enumerable.GetAsyncEnumerator();
            await enumerator.MoveNextAsync(CancellationToken.None);
            var actual = enumerator.Current;

            Assert.AreEqual(key, actual);
        }
    }
}
