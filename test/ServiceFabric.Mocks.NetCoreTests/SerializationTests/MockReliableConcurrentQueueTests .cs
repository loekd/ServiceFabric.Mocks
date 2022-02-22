using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceFabric.Mocks.ReliableCollections;

namespace ServiceFabric.Mocks.NetCoreTests.SerializationTests
{
    [TestClass]
    public class MockReliableConcurrentQueueTests
    {
        const string originalContentValue = "original value";
        const string otherContentValue = "other value";
        const string modifiedContentValue = "modified value";

        [TestMethod]
        public async Task QueueSerializedValueChangesIgnoredTest()
        {
            var value = new ModifyablePayload
            {
                Content = originalContentValue,
                OtherContent = otherContentValue
            };

            SerializerCollection serializers = new();
            serializers.AddSerializer(new ModifyablePayloadSerializer());
            var q = new MockReliableConcurrentQueue<ModifyablePayload>(new Uri("test://queue"), serializers);

            var tx = new MockTransaction(null, 1);
            await q.EnqueueAsync(tx, value);
            await tx.CommitAsync();

            //modify in-memory state
            value.Content = modifiedContentValue;

            tx = new MockTransaction(null, 1);
            var actual = await q.TryDequeueAsync(tx);

            //original content remains the same
            Assert.AreEqual(originalContentValue, actual.Value.Content);
            Assert.AreEqual(otherContentValue, actual.Value.OtherContent);
            Assert.AreNotSame(value, actual.Value);
        }

        [TestMethod]
        public async Task QueueSerializedValueChangesIgnoredTest2()
        {
            var value = new ModifyablePayload
            {
                Content = null,
                OtherContent = otherContentValue
            };

            SerializerCollection serializers = new();
            serializers.AddSerializer(new ModifyablePayloadSerializer());
            var q = new MockReliableConcurrentQueue<ModifyablePayload>(new Uri("test://queue"), serializers);

            var tx = new MockTransaction(null, 1);
            await q.EnqueueAsync(tx, value);
            await tx.CommitAsync();

            //modify in-memory state
            value.Content = modifiedContentValue;

            tx = new MockTransaction(null, 1);
            var actual = await q.TryDequeueAsync(tx);

            //original content remains the same
            Assert.AreEqual(null, actual.Value.Content);
            Assert.AreEqual(otherContentValue, actual.Value.OtherContent);
            Assert.AreNotSame(value, actual.Value);
        }

        [TestMethod]
        public async Task QueueNotSerializedValueChangesRemainTest()
        {
            var value = new ModifyablePayload
            {
                Content = originalContentValue
            };

            var q = new MockReliableConcurrentQueue<ModifyablePayload>(new Uri("test://queue"));

            var tx = new MockTransaction(null, 1);
            await q.EnqueueAsync(tx, value);
            await tx.CommitAsync();

            //modify in-memory state
            value.Content = modifiedContentValue;

            tx = new MockTransaction(null, 1);
            var actual = await q.TryDequeueAsync(tx);

            //modified content remains the same
            Assert.AreEqual(modifiedContentValue, actual.Value.Content);
            Assert.AreSame(value, actual.Value);
        }

        [TestMethod]
        public async Task QueueSerializedStructTest()
        {
            int value = 1234;

            SerializerCollection serializers = new();
            serializers.AddSerializer(new ModifyablePayloadSerializer());
            var q = new MockReliableConcurrentQueue<int>(new Uri("test://queue"), serializers);

            var tx = new MockTransaction(null, 1);
            await q.EnqueueAsync(tx, value);
            await tx.CommitAsync();

            tx = new MockTransaction(null, 1);
            var actual = await q.TryDequeueAsync(tx);

            //original content remains the same
            Assert.AreEqual(1234, actual.Value);
        }
    }
}
