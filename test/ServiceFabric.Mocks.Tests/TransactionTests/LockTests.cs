using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceFabric.Mocks.ReliableCollections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceFabric.Mocks.Tests.TransactionTests
{
    /// <summary>
    /// Test that 2 default locks can be granted at the same time.
    /// </summary>
    [TestClass]
    public class LockTests
    {
        [TestMethod]
        public async Task Lock_DefaultLock()
        {
            Lock l = new Lock();
            AcquireResult result;

            ITransaction tx1 = new MockTransaction(null, 1);
            result = await l.Acquire(tx1, LockMode.Default);
            Assert.AreEqual(AcquireResult.Acquired, result);

            ITransaction tx2 = new MockTransaction(null, 2);
            result = await l.Acquire(tx2, LockMode.Default);
            Assert.AreEqual(AcquireResult.Acquired, result);
        }

        /// <summary>
        /// Test that Default look cannot be granted while an Update lock is held
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Lock_UpdateBlockDefaultLock()
        {
            Lock l = new Lock();

            ITransaction tx1 = new MockTransaction(null, 1);
            await l.Acquire(tx1, LockMode.Update);

            await Assert.ThrowsExceptionAsync<TimeoutException>(
                async () =>
                {
                    ITransaction tx2 = new MockTransaction(null, 2);
                    await l.Acquire(tx2, LockMode.Default, timeout: TimeSpan.FromMilliseconds(10));
                }
            );
        }

        [TestMethod]
        public async Task Lock_UpdateGrantedAfterDefaultRelease()
        {
            Lock l = new Lock();
            AcquireResult result;

            ITransaction tx1 = new MockTransaction(null, 1);
            result = await l.Acquire(tx1, LockMode.Default);
            Assert.AreEqual(AcquireResult.Acquired, result);

            ITransaction tx2 = new MockTransaction(null, 2);
            var task = l.Acquire(tx2, LockMode.Default);
            l.Release(tx1);
            result = await task;
            Assert.AreEqual(AcquireResult.Acquired, result);
        }

        [TestMethod]
        public async Task Lock_DefaultGrantedAfterUpdateDowngrade()
        {
            Lock l = new Lock();
            AcquireResult result;

            ITransaction tx1 = new MockTransaction(null, 1);
            result = await l.Acquire(tx1, LockMode.Update);
            Assert.AreEqual(AcquireResult.Acquired, result);

            ITransaction tx2 = new MockTransaction(null, 2);
            var task = l.Acquire(tx2, LockMode.Default);
            l.Downgrade(tx1);
            result = await task;
            Assert.AreEqual(AcquireResult.Acquired, result);
        }

        [TestMethod]
        public async Task Lock_DefaultUpgradeNotDowngraded()
        {
            Lock l = new Lock();
            AcquireResult result;

            ITransaction tx1 = new MockTransaction(null, 1);
            result = await l.Acquire(tx1, LockMode.Default);
            Assert.AreEqual(AcquireResult.Acquired, result);
            Assert.AreEqual(l.LockMode, LockMode.Default);

            result = await l.Acquire(tx1, LockMode.Update);
            Assert.AreEqual(AcquireResult.Owned, result);
            Assert.AreEqual(l.LockMode, LockMode.Update);

            result = await l.Acquire(tx1, LockMode.Default);
            Assert.AreEqual(AcquireResult.Owned, result);
            Assert.AreEqual(l.LockMode, LockMode.Update);
        }

        [TestMethod]
        public async Task Lock_UpdateBlockedAndCancelled()
        {
            Lock l = new Lock();

            ITransaction tx1 = new MockTransaction(null, 1);
            await l.Acquire(tx1, LockMode.Update);

            await Assert.ThrowsExceptionAsync<OperationCanceledException>(
                async () =>
                {
                    CancellationTokenSource tokenSource = new CancellationTokenSource();
                    ITransaction tx2 = new MockTransaction(null, 2);
                    var task =  l.Acquire(tx2, LockMode.Default, cancellationToken: tokenSource.Token);
                    tokenSource.Cancel();
                    await task;
                }
            );
        }

        [TestMethod]
        public async Task LockManager_BasicTest()
        {
            LockManager<int> lockManager = new LockManager<int>();
            ITransaction tx1 = new MockTransaction(null, 1);
            await lockManager.AcquireLock(tx1, 1, LockMode.Update);
            await lockManager.AcquireLock(tx1, 2, LockMode.Update);
            await lockManager.AcquireLock(tx1, 3, LockMode.Update);

            ITransaction tx2 = new MockTransaction(null, 2);
            ITransaction tx3 = new MockTransaction(null, 3);
            ITransaction tx4 = new MockTransaction(null, 4);
            Task[] tasks = new Task[]
                {
                    lockManager.AcquireLock(tx2, 1, LockMode.Update),
                    lockManager.AcquireLock(tx3, 2, LockMode.Update),
                    lockManager.AcquireLock(tx4, 3, LockMode.Update),
                };

            lockManager.ReleaseLocks(tx1);
            Task.WaitAll(tasks);

            lockManager.ReleaseLocks(tx2);
            lockManager.ReleaseLocks(tx3);
            ITransaction tx5 = new MockTransaction(null, 1);
            await lockManager.AcquireLock(tx5, 1, LockMode.Default);
            await lockManager.AcquireLock(tx5, 2, LockMode.Default);
            await Assert.ThrowsExceptionAsync<TimeoutException>(
                async () =>
                {
                    await lockManager.AcquireLock(tx5, 3, LockMode.Default, timeout: TimeSpan.FromMilliseconds(10));
                }
            );
        }
    }
}
