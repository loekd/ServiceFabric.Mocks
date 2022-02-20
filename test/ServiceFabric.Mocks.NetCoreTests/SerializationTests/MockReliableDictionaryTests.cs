using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using ServiceFabric.Mocks.ReliableCollections;
using System.Threading;

namespace ServiceFabric.Mocks.NetCoreTests.SerializationTests
{
    [TestClass]
    public class MockReliableDictionaryTests
    {
        [TestMethod]
        public async Task DictionaryAddAndRetrieveTest()
        {
            const string key = "key";
            const string originalContentValue = "original value";
            const string modifiedContentValue = "modified value";
            var value = new ModifyablePayload
            { 
                Content = originalContentValue
            };

            System.Collections.Concurrent.ConcurrentDictionary<Type, object> serializers = new();
            serializers.TryAdd(typeof(ModifyablePayload), new ModifyablePayloadSerializer());
            var dictionary = new MockReliableDictionary<string, ModifyablePayload>(new Uri("fabric://MockReliableDictionary"), serializers);
            var tx = new MockTransaction(null, 1);

            await dictionary.AddAsync(tx, key, value);

            //modify in-memory state
            value.Content = modifiedContentValue;

            var actual = await dictionary.TryGetValueAsync(tx, key);

            Assert.AreEqual(originalContentValue, actual.Value.Content);
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
