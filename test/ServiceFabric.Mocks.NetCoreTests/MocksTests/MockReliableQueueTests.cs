using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceFabric.Mocks.ReliableCollections;

namespace ServiceFabric.Mocks.NetCoreTests.MocksTests
{
    [TestClass]
    public class MockReliableQueueTests
    {
        private readonly MockReliableStateManager _stateManager = new MockReliableStateManager();

        [TestMethod]
        public async Task DequeueAbortDoesNotChangeOrder()
        {
            var q = new MockReliableQueue<string>(new Uri("test://queue"));
            using (var tx = _stateManager.CreateTransaction())
            {
                await q.EnqueueAsync(tx, "A");
                await q.EnqueueAsync(tx, "B");
                await q.EnqueueAsync(tx, "C");
                await q.EnqueueAsync(tx, "D");
                await tx.CommitAsync();
            }

            using (var tx = _stateManager.CreateTransaction())
            {
                var a = await q.TryDequeueAsync(tx);
                var b = await q.TryDequeueAsync(tx);
                var c = await q.TryDequeueAsync(tx);
                var d = await q.TryDequeueAsync(tx);

                Assert.AreEqual("A", a.Value);
                Assert.AreEqual("B", b.Value);
                Assert.AreEqual("C", c.Value);
                Assert.AreEqual("D", d.Value);

                tx.Abort();
            }

            using (var tx = _stateManager.CreateTransaction())
            {
                //expecting ABCD
                Assert.AreEqual(4, await q.GetCountAsync(tx));

                var a = await q.TryDequeueAsync(tx);
                var b = await q.TryDequeueAsync(tx);
                var c = await q.TryDequeueAsync(tx);
                var d = await q.TryDequeueAsync(tx);

                Assert.AreEqual("A", a.Value);
                Assert.AreEqual("B", b.Value);
                Assert.AreEqual("C", c.Value);
                Assert.AreEqual("D", d.Value);
            }
        }

        [TestMethod]
        public async Task EnqueueAbortDoesNotChangeOrder()
        {
            var q = new MockReliableQueue<string>(new Uri("test://queue"));
            using (var tx = _stateManager.CreateTransaction())
            {
                await q.EnqueueAsync(tx, "A");
                await q.EnqueueAsync(tx, "B");
                await tx.CommitAsync();
            }

            using (var tx = _stateManager.CreateTransaction())
            {
                await q.EnqueueAsync(tx, "C");
                await q.EnqueueAsync(tx, "D");

                var a = await q.TryDequeueAsync(tx);
                var b = await q.TryDequeueAsync(tx);
                var c = await q.TryDequeueAsync(tx);
                var d = await q.TryDequeueAsync(tx);

                Assert.AreEqual("A", a.Value);
                Assert.AreEqual("B", b.Value);
                Assert.AreEqual("C", c.Value);
                Assert.AreEqual("D", d.Value);

                tx.Abort();
            }

            using (var tx = _stateManager.CreateTransaction())
            {
                //expecting AB
                Assert.AreEqual(2, await q.GetCountAsync(tx));

                var a = await q.TryDequeueAsync(tx);
                var b = await q.TryDequeueAsync(tx);

                Assert.AreEqual("A", a.Value);
                Assert.AreEqual("B", b.Value);
            }
        }
    }
}
