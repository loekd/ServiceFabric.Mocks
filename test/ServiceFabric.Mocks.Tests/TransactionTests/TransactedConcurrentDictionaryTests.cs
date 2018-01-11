using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceFabric.Mocks.ReliableCollections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ServiceFabric.Mocks.ReliableCollections.TransactedConcurrentDictionary<int, string>;

namespace ServiceFabric.Mocks.Tests.TransactionTests
{
    [TestClass]
    public class TransactedConcurrentDictionaryTests
    {
        private MockReliableStateManager _stateManager = new MockReliableStateManager();

        [TestMethod]
        public async Task AddAsyncTest()
        {
            DictionaryChange change = null;
            TransactedConcurrentDictionary<int, string> d = new TransactedConcurrentDictionary<int, string>(
                new Uri("test://mocks", UriKind.Absolute),
                (c) =>
                {
                    change = c;
                    return true;
                }
            );

            using (var tx = _stateManager.CreateTransaction())
            {
                await d.AddAsync(tx, 1, "One");
                await Assert.ThrowsExceptionAsync<ArgumentException>(
                    async () =>
                    {
                        await d.AddAsync(tx, 1, "Two");
                    }
                );

                Assert.IsNull(change);

                using (var tx2 = _stateManager.CreateTransaction())
                {
                    await Assert.ThrowsExceptionAsync<TimeoutException>(
                        async () =>
                        {
                            await d.AddAsync(tx2, 1, "Three", TimeSpan.FromMilliseconds(20));
                        }
                    );
                }

                Assert.IsNull(change);

                await tx.CommitAsync();
            }

            Assert.AreEqual("One", change.Added);
            Assert.IsNull(change.Removed);

            Assert.AreEqual("One", (await GetValue(d, 1)).Value);
        }

        [TestMethod]
        public async Task AddOrUpdateAsyncTest()
        {
            DictionaryChange change = null;
            TransactedConcurrentDictionary<int, string> d = new TransactedConcurrentDictionary<int, string>(
                new Uri("test://mocks", UriKind.Absolute),
                (c) =>
                {
                    change = c;
                    return true;
                }
            );

            using (var tx = _stateManager.CreateTransaction())
            {
                await d.AddOrUpdateAsync(tx, 1, (k) => "One", (k, v) => "Two");
                Assert.IsNull(change);
                await tx.CommitAsync();
                Assert.AreEqual("One", change.Added);
            }

            change = null;
            using (var tx = _stateManager.CreateTransaction())
            {
                await d.AddOrUpdateAsync(tx, 1, (k) => "One", (k, v) => "Two");
                Assert.IsNull(change);
                await tx.CommitAsync();
                Assert.AreEqual("Two", change.Added);
            }
        }

        [TestMethod]
        public async Task ClearAsyncTest()
        {
            DictionaryChange change = null;
            TransactedConcurrentDictionary<int, string> d = new TransactedConcurrentDictionary<int, string>(
                new Uri("test://mocks", UriKind.Absolute),
                (c) =>
                {
                    change = c;
                    return true;
                }
            );

            using (var tx = _stateManager.CreateTransaction())
            {
                await d.AddAsync(tx, 1, "One");
                await tx.CommitAsync();
                Assert.AreEqual(1, await GetCount(d));
            }

            change = null;
            await d.ClearAsync();
            Assert.AreEqual(0, await GetCount(d));
            Assert.IsNull(change);
        }

        [TestMethod]
        public async Task ContainsKeyAsyncTest()
        {
            DictionaryChange change = null;
            TransactedConcurrentDictionary<int, string> d = new TransactedConcurrentDictionary<int, string>(
                new Uri("test://mocks", UriKind.Absolute),
                (c) =>
                {
                    change = c;
                    return true;
                }
            );

            using (var tx = _stateManager.CreateTransaction())
            {
                await d.AddAsync(tx, 1, "One");
                await Assert.ThrowsExceptionAsync<TimeoutException>(
                    async () =>
                    {
                        await ContainsKey(d, 1, TimeSpan.FromMilliseconds(20));
                    }
                );
                await tx.CommitAsync();
                Assert.IsTrue(await ContainsKey(d, 1));
            }
        }

        [TestMethod]
        public async Task GetOrAddAsyncTest()
        {
            DictionaryChange change = null;
            TransactedConcurrentDictionary<int, string> d = new TransactedConcurrentDictionary<int, string>(
                new Uri("test://mocks", UriKind.Absolute),
                (c) =>
                {
                    change = c;
                    return true;
                }
            );

            using (var tx = _stateManager.CreateTransaction())
            {
                await d.GetOrAddAsync(tx, 1, (k) => "One");
                await d.GetOrAddAsync(tx, 1, (k) => "Two");
              
                Assert.IsNull(change);
                await tx.CommitAsync();
                Assert.AreEqual("One", change.Added);
                Assert.AreEqual("One", (await GetValue(d, 1)).Value);
            }
        }

        [TestMethod]
        public async Task SetAsyncTest()
        {
            DictionaryChange change = null;
            TransactedConcurrentDictionary<int, string> d = new TransactedConcurrentDictionary<int, string>(
                new Uri("test://mocks", UriKind.Absolute),
                (c) =>
                {
                    change = c;
                    return true;
                }
            );

            using (var tx = _stateManager.CreateTransaction())
            {
                await d.SetAsync(tx, 1, "Zero");
            }
            using (var tx = _stateManager.CreateTransaction())
            {
                ConditionalValue<string> value = await d.TryGetValueAsync(tx, 1, LockMode.Default);
                Assert.IsFalse(value.HasValue);
            }

            using (var tx = _stateManager.CreateTransaction())
            {
                await d.SetAsync(tx, 1, "One");
                await tx.CommitAsync();
                Assert.AreEqual("One", change.Added);
                Assert.AreEqual(null, change.Removed);
            }

            using (var tx = _stateManager.CreateTransaction())
            {
                await d.SetAsync(tx, 1, "Two");
            }
            using (var tx = _stateManager.CreateTransaction())
            {
                ConditionalValue<string> value = await d.TryGetValueAsync(tx, 1, LockMode.Default);
                Assert.AreEqual("One", value.Value);
            }

            using (var tx = _stateManager.CreateTransaction())
            {
                await d.AddAsync(tx, 2, "Two");
                await tx.CommitAsync();
            }

            using (var tx = _stateManager.CreateTransaction())
            {
                await d.SetAsync(tx, 2, "Three");
                await tx.CommitAsync();
                Assert.AreEqual("Two", change.Removed);
                Assert.AreEqual("Three", change.Added);
            }

            Assert.AreEqual(2, await GetCount(d));
        }

        [TestMethod]
        public async Task TryAddAsyncTest()
        {
            DictionaryChange change = null;
            TransactedConcurrentDictionary<int, string> d = new TransactedConcurrentDictionary<int, string>(
                new Uri("test://mocks", UriKind.Absolute),
                (c) =>
                {
                    change = c;
                    return true;
                }
            );

            using (var tx = _stateManager.CreateTransaction())
            {
                Assert.IsTrue(await d.TryAddAsync(tx, 1, "One"));
                Assert.IsFalse(await d.TryAddAsync(tx, 1, "Two"));

                using (var tx2 = _stateManager.CreateTransaction())
                {
                    await Assert.ThrowsExceptionAsync<TimeoutException>(
                        async () =>
                        {
                            await d.TryAddAsync(tx2, 1, "Three", timeout: TimeSpan.FromMilliseconds(20));
                        }
                    );
                }
                await tx.CommitAsync();

                Assert.AreEqual("One", change.Added);

            }

            using (var tx = _stateManager.CreateTransaction())
            {
                Assert.IsFalse(await d.TryAddAsync(tx, 1, "Three"));
                Assert.IsTrue(await d.TryAddAsync(tx, 4, "Four"));
                Assert.IsTrue(await d.TryAddAsync(tx, 5, "Five"));
                await tx.CommitAsync();
            }

            Assert.AreEqual(3, await GetCount(d));
            Assert.AreEqual("One", (await GetValue(d, 1)).Value);
            Assert.AreEqual("Four", (await GetValue(d, 4)).Value);
            Assert.AreEqual("Five", (await GetValue(d, 5)).Value);
        }

        [TestMethod]
        public async Task TryGetValueAsyncTest()
        {
            DictionaryChange change = null;
            TransactedConcurrentDictionary<int, string> d = new TransactedConcurrentDictionary<int, string>(
                new Uri("test://mocks", UriKind.Absolute),
                (c) =>
                {
                    change = c;
                    return true;
                }
            );

            using (var tx = _stateManager.CreateTransaction())
            {
                Assert.IsFalse((await d.TryGetValueAsync(tx, 1, LockMode.Default)).HasValue);
                Assert.IsTrue(await d.TryAddAsync(tx, 1, "One"));

                using (var tx2 = _stateManager.CreateTransaction())
                {
                    await Assert.ThrowsExceptionAsync<TimeoutException>(
                        async () =>
                        {
                            await d.TryGetValueAsync(tx2, 1, LockMode.Default, timeout: TimeSpan.FromMilliseconds(20));
                        }
                    );
                }
                await tx.CommitAsync();

                Assert.AreEqual("One", change.Added);
            }
        }

        [TestMethod]
        public async Task TryRemoveAsyncTest()
        {
            DictionaryChange change = null;
            TransactedConcurrentDictionary<int, string> d = new TransactedConcurrentDictionary<int, string>(
                new Uri("test://mocks", UriKind.Absolute),
                (c) =>
                {
                    change = c;
                    return true;
                }
            );

            using (var tx = _stateManager.CreateTransaction())
            {
                Assert.IsFalse((await d.TryRemoveAsync(tx, 1)).HasValue);
                Assert.IsTrue(await d.TryAddAsync(tx, 1, "One"));

                using (var tx2 = _stateManager.CreateTransaction())
                {
                    await Assert.ThrowsExceptionAsync<TimeoutException>(
                        async () =>
                        {
                            await d.TryRemoveAsync(tx2, 1, timeout: TimeSpan.FromMilliseconds(20));
                        }
                    );
                }
                await tx.CommitAsync();

                Assert.AreEqual("One", change.Added);
            }

            using (var tx = _stateManager.CreateTransaction())
            {
                var result = await d.TryRemoveAsync(tx, 1);
                await tx.CommitAsync();

                Assert.AreEqual(ChangeType.Removed, change.ChangeType);
                Assert.AreEqual("One", change.Removed);
                Assert.AreEqual(0, await GetCount(d));
            }
        }

        [TestMethod]
        public async Task TryUpdateAsyncTest()
        {
            DictionaryChange change = null;
            TransactedConcurrentDictionary<int, string> d = new TransactedConcurrentDictionary<int, string>(
                new Uri("test://mocks", UriKind.Absolute),
                (c) =>
                {
                    change = c;
                    return true;
                }
            );

            using (var tx = _stateManager.CreateTransaction())
            {
                Assert.IsFalse((await d.TryUpdateAsync(tx, 1, "Two", "One")));
                Assert.IsTrue(await d.TryAddAsync(tx, 1, "One"));

                using (var tx2 = _stateManager.CreateTransaction())
                {
                    await Assert.ThrowsExceptionAsync<TimeoutException>(
                        async () =>
                        {
                            await d.TryRemoveAsync(tx2, 1, timeout: TimeSpan.FromMilliseconds(20));
                        }
                    );
                }
                await tx.CommitAsync();

                Assert.AreEqual("One", change.Added);
            }

            using (var tx = _stateManager.CreateTransaction())
            {
                Assert.IsFalse((await d.TryUpdateAsync(tx, 1, "Three", "Two")));

                using (var tx2 = _stateManager.CreateTransaction())
                {
                    Assert.IsTrue((await d.TryUpdateAsync(tx, 1, "Two", "One")));
                }
                await tx.CommitAsync();

                Assert.AreEqual("Two", change.Added);
            }
        }

        private async Task<bool> ContainsKey(TransactedConcurrentDictionary<int, string> d, int key, TimeSpan timeout = default(TimeSpan))
        {
            using (var tx = _stateManager.CreateTransaction())
            {
                return await d.ContainsKeyAsync(tx, 1, LockMode.Default, timeout: timeout);
            }
        }

        private async Task<long> GetCount(TransactedConcurrentDictionary<int, string> d)
        {
            using (var tx = _stateManager.CreateTransaction())
            {
                return await d.GetCountAsync(tx);
            }
        }

        private async Task<ConditionalValue<string>> GetValue(TransactedConcurrentDictionary<int, string> d, int key, TimeSpan timeout = default(TimeSpan))
        {
            using (var tx = _stateManager.CreateTransaction())
            {
                return await d.TryGetValueAsync(tx, key, LockMode.Default, timeout: timeout);
            }
        }
    }
}