using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceFabric.Mocks.ReliableCollections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            TryAcquireResult result;

            ITransaction tx1 = new MockTransaction(null, 1);
            result = await l.Acquire(tx1, LockMode.Default);
            Assert.AreEqual(TryAcquireResult.Acquired, result);

            ITransaction tx2 = new MockTransaction(null, 2);
            result = await l.Acquire(tx2, LockMode.Default);
            Assert.AreEqual(TryAcquireResult.Acquired, result);
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
            TryAcquireResult result;

            ITransaction tx1 = new MockTransaction(null, 1);
            result = await l.Acquire(tx1, LockMode.Default);
            Assert.AreEqual(TryAcquireResult.Acquired, result);

            ITransaction tx2 = new MockTransaction(null, 2);
            var task = l.Acquire(tx2, LockMode.Default);
            l.Release(tx1);
            result = await task;
            Assert.AreEqual(TryAcquireResult.Acquired, result);
        }

        [TestMethod]
        public async Task Lock_DefaultGrantedAfterUpdateDowngrade()
        {
            Lock l = new Lock();
            TryAcquireResult result;

            ITransaction tx1 = new MockTransaction(null, 1);
            result = await l.Acquire(tx1, LockMode.Update);
            Assert.AreEqual(TryAcquireResult.Acquired, result);

            ITransaction tx2 = new MockTransaction(null, 2);
            var task = l.Acquire(tx2, LockMode.Default);
            l.Downgrade(tx1);
            result = await task;
            Assert.AreEqual(TryAcquireResult.Acquired, result);
        }

        [TestMethod]
        public async Task Lock_DefaultUpgradeNotDowngraded()
        {
            Lock l = new Lock();
            TryAcquireResult result;

            ITransaction tx1 = new MockTransaction(null, 1);
            result = await l.Acquire(tx1, LockMode.Default);
            Assert.AreEqual(TryAcquireResult.Acquired, result);
            Assert.AreEqual(l.LockMode, LockMode.Default);

            result = await l.Acquire(tx1, LockMode.Update);
            Assert.AreEqual(TryAcquireResult.Owned, result);
            Assert.AreEqual(l.LockMode, LockMode.Update);

            result = await l.Acquire(tx1, LockMode.Default);
            Assert.AreEqual(TryAcquireResult.Owned, result);
            Assert.AreEqual(l.LockMode, LockMode.Update);
        }
    }
}
