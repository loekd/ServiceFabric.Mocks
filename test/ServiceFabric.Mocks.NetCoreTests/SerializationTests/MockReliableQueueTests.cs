using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceFabric.Mocks.ReliableCollections;

namespace ServiceFabric.Mocks.NetCoreTests.SerializationTests
{
    [TestClass]
    public class MockReliableQueueTests
    {
        const string originalContentValue = "original value";
        const string modifiedContentValue = "modified value";

        [TestMethod]
        public async Task QueueSerializedValueChangesIgnoredTest()
        {
            var value = new ModifyablePayload
            {
                Content = originalContentValue
            };

            SerializerCollection serializers = new();
            serializers.AddSerializer(new ModifyablePayloadSerializer());
            var q = new MockReliableQueue<ModifyablePayload>(new Uri("test://queue"), serializers);

            var tx = new MockTransaction(null, 1);
            await q.EnqueueAsync(tx, value);
            await tx.CommitAsync();


            //modify in-memory state
            value.Content = modifiedContentValue;

            tx = new MockTransaction(null, 1);
            var actual = await q.TryDequeueAsync(tx);

            //original content remains the same
            Assert.AreEqual(originalContentValue, actual.Value.Content);
            Assert.AreNotSame(value, actual.Value);
        }

        [TestMethod]
        public async Task QueueNotSerializedValueChangesRemainTest()
        {
            var value = new ModifyablePayload
            {
                Content = originalContentValue
            };

            var q = new MockReliableQueue<ModifyablePayload>(new Uri("test://queue"));

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


    }
}
