namespace ServiceFabric.Mocks.Tests.MocksTests
{
    using Microsoft.ServiceFabric.Data;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ServiceFabric.Mocks.ReliableCollections;
    using System;
    using System.Threading.Tasks;

    [TestClass]
    public class MockReliableConcurrentQueueTests
    {
        private MockReliableStateManager _stateManager = new MockReliableStateManager();

        [TestMethod]
        public async Task DequeueEmptyQueueTest()
        {
            var q = new MockReliableConcurrentQueue<int>(new Uri("test://queue"));
            using (var tx = _stateManager.CreateTransaction())
            {
                Assert.IsFalse((await q.TryDequeueAsync(tx, timeout: TimeSpan.FromMilliseconds(20))).HasValue);
            }
        }

        /// <summary>
        /// Service Fabric doesn't currently allow a transaction to dequeue its own uncommited enqueues. But since I foolishly coded up 
        /// support for it, I thought I'd keep it in case SF adds support for it.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task DequeueOwnEnqueueTest_Unsupported()
        {
            var q = new MockReliableConcurrentQueue<int>(new Uri("test://queue"), true);
            using (var tx = _stateManager.CreateTransaction())
            {
                await q.EnqueueAsync(tx, 1);
                Task<ConditionalValue<int>> task = q.TryDequeueAsync(tx);
                Assert.AreEqual(1, (await task).Value);
            }
        }

        [TestMethod]
        public async Task DequeueOwnEnqueueTest()
        {
            var q = new MockReliableConcurrentQueue<int>(new Uri("test://queue"));
            using (var tx = _stateManager.CreateTransaction())
            {
                await q.EnqueueAsync(tx, 1);
                Task<ConditionalValue<int>> task = q.TryDequeueAsync(tx, timeout: TimeSpan.FromMilliseconds(20));
                Assert.IsFalse((await task).HasValue);
            }
        }

        [TestMethod]
        public async Task DequeueOtherEnqueueTest()
        {
            var q = new MockReliableConcurrentQueue<int>(new Uri("test://queue"));
            using (var tx = _stateManager.CreateTransaction())
            {
                await q.EnqueueAsync(tx, 1);
                await tx.CommitAsync();
            }

            using (var tx = _stateManager.CreateTransaction())
            {
                Task<ConditionalValue<int>> task = q.TryDequeueAsync(tx);
                Assert.AreEqual(1, (await task).Value);
            }
        }

        [TestMethod]
        public async Task DequeueAbortOtherEnqueueTest()
        {
            var q = new MockReliableConcurrentQueue<int>(new Uri("test://queue"));
            using (var tx = _stateManager.CreateTransaction())
            {
                await q.EnqueueAsync(tx, 1);
                await tx.CommitAsync();
            }

            using (var tx = _stateManager.CreateTransaction())
            {
                Task<ConditionalValue<int>> task = q.TryDequeueAsync(tx);
                Assert.AreEqual(1, (await task).Value);
            }

            using (var tx = _stateManager.CreateTransaction())
            {
                Task<ConditionalValue<int>> task = q.TryDequeueAsync(tx);
                Assert.AreEqual(1, (await task).Value);
            }
        }

        [TestMethod]
        public async Task DequeueWaitOtherEnqueueTest()
        {
            var q = new MockReliableConcurrentQueue<int>(new Uri("test://queue"));
            using (var tx = _stateManager.CreateTransaction())
            {
                Task<ConditionalValue<int>> task = q.TryDequeueAsync(tx);

                using (var tx2 = _stateManager.CreateTransaction())
                {
                    await q.EnqueueAsync(tx2, 1);
                    await tx2.CommitAsync();
                }

                Assert.AreEqual(1, (await task).Value);
            }
        }
    }
}
