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
            Lock<int> l = new Lock<int>();
            AcquireResult result;

            result = await l.Acquire(1, LockMode.Default);
            Assert.AreEqual(AcquireResult.Acquired, result);

            result = await l.Acquire(2, LockMode.Default);
            Assert.AreEqual(AcquireResult.Acquired, result);
        }

        /// <summary>
        /// Test that Default look cannot be granted while an Update lock is held
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Lock_UpdateBlockDefaultLock()
        {
            Lock<int> l = new Lock<int>();

            await l.Acquire(1, LockMode.Update);

            await Assert.ThrowsExceptionAsync<TimeoutException>(
                async () =>
                {
                    await l.Acquire(2, LockMode.Default, timeout: TimeSpan.FromMilliseconds(10));
                }
            );
        }

        [TestMethod]
        public async Task Lock_UpdateGrantedAfterDefaultRelease()
        {
            Lock<int> l = new Lock<int>();
            AcquireResult result;

            result = await l.Acquire(1, LockMode.Default);
            Assert.AreEqual(AcquireResult.Acquired, result);

            var task = l.Acquire(2, LockMode.Default);
            l.Release(1);
            result = await task;
            Assert.AreEqual(AcquireResult.Acquired, result);
        }

        [TestMethod]
        public async Task Lock_DefaultGrantedAfterUpdateDowngrade()
        {
            Lock<int> l = new Lock<int>();
            AcquireResult result;

            result = await l.Acquire(1, LockMode.Update);
            Assert.AreEqual(AcquireResult.Acquired, result);

            var task = l.Acquire(2, LockMode.Default);
            l.Downgrade(1);
            result = await task;
            Assert.AreEqual(AcquireResult.Acquired, result);
        }

        [TestMethod]
        public async Task Lock_DefaultUpgradeNotDowngraded()
        {
            Lock<int> l = new Lock<int>();
            AcquireResult result;

            result = await l.Acquire(1, LockMode.Default);
            Assert.AreEqual(AcquireResult.Acquired, result);
            Assert.AreEqual(l.LockMode, LockMode.Default);

            result = await l.Acquire(1, LockMode.Update);
            Assert.AreEqual(AcquireResult.Owned, result);
            Assert.AreEqual(l.LockMode, LockMode.Update);

            result = await l.Acquire(1, LockMode.Default);
            Assert.AreEqual(AcquireResult.Owned, result);
            Assert.AreEqual(l.LockMode, LockMode.Update);
        }

        [TestMethod]
        public async Task Lock_UpdateBlockedAndCancelled()
        {
            Lock<int> l = new Lock<int>();

            await l.Acquire(1, LockMode.Update);

            await Assert.ThrowsExceptionAsync<OperationCanceledException>(
                async () =>
                {
                    CancellationTokenSource tokenSource = new CancellationTokenSource();
                    var task =  l.Acquire(2, LockMode.Default, cancellationToken: tokenSource.Token);
                    tokenSource.Cancel();
                    await task;
                }
            );
        }

        [TestMethod]
        public async Task LockManager_BasicTest()
        {
            LockManager<int, int> lockManager = new LockManager<int, int>();
            await lockManager.AcquireLock(1, 1, LockMode.Update);
            await lockManager.AcquireLock(1, 2, LockMode.Update);
            await lockManager.AcquireLock(1, 3, LockMode.Update);

            Task[] tasks = new Task[]
                {
                    lockManager.AcquireLock(2, 1, LockMode.Update),
                    lockManager.AcquireLock(3, 2, LockMode.Update),
                    lockManager.AcquireLock(4, 3, LockMode.Update),
                };

            lockManager.ReleaseLocks(1);
            Task.WaitAll(tasks);

            lockManager.ReleaseLocks(2);
            lockManager.ReleaseLocks(3);

            await lockManager.AcquireLock(5, 1, LockMode.Default);
            await lockManager.AcquireLock(5, 2, LockMode.Default);
            await Assert.ThrowsExceptionAsync<TimeoutException>(
                async () =>
                {
                    await lockManager.AcquireLock(5, 3, LockMode.Default, timeout: TimeSpan.FromMilliseconds(10));
                }
            );
        }
    }
}
