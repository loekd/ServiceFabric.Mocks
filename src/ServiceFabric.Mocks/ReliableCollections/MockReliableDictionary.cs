namespace ServiceFabric.Mocks.ReliableCollections
{
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Collections;
    using Microsoft.ServiceFabric.Data.Notifications;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Implements IReliableDictionary.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class MockReliableDictionary<TKey, TValue> : TransactedConcurrentDictionary<TKey, TValue>, IReliableDictionary2<TKey, TValue>
        where TKey : IEquatable<TKey>, IComparable<TKey>
    {
        public long Count => Dictionary.Count;

        public MockReliableDictionary(Uri uri)
            : base(uri, null)
        {
            // Set the OnDictionaryChanged callback to fire the DictionaryChanged event.
            OnDictionaryChanged =
                (c) =>
                {
                    if (DictionaryChanged != null)
                    {
                        NotifyDictionaryChangedEventArgs<TKey, TValue> e;
                        switch (c.ChangeType)
                        {
                            case ChangeType.Added:
                                e = new NotifyDictionaryItemAddedEventArgs<TKey, TValue>(c.Transaction, c.Key, c.Added);
                                break;
                            case ChangeType.Removed:
                                e = new NotifyDictionaryItemRemovedEventArgs<TKey, TValue>(c.Transaction, c.Key);
                                break;
                            case ChangeType.Updated:
                                e = new NotifyDictionaryItemUpdatedEventArgs<TKey, TValue>(c.Transaction, c.Key, c.Added);
                                break;
                            default:
                                return false;
                        }

                        DictionaryChanged.Invoke(this, e);
                    }

                    MockDictionaryChanged?.Invoke(this, c);

                    return true;
                };
        }

        public Func<IReliableDictionary<TKey, TValue>, NotifyDictionaryRebuildEventArgs<TKey, TValue>, Task> RebuildNotificationAsyncCallback { set => throw new NotImplementedException(); }



        public event EventHandler<NotifyDictionaryChangedEventArgs<TKey, TValue>> DictionaryChanged;
        public event EventHandler<DictionaryChange> MockDictionaryChanged;

        #region AddAsync
        public Task AddAsync(ITransaction tx, TKey key, TValue value)
        {
            return base.AddAsync(tx, key, value);
        }
        #endregion

        #region AddOrUpdateAsync
        public Task<TValue> AddOrUpdateAsync(ITransaction tx, TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
        {
            return base.AddOrUpdateAsync(tx, key, addValueFactory, updateValueFactory);
        }

        public Task<TValue> AddOrUpdateAsync(ITransaction tx, TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
        {
            return base.AddOrUpdateAsync(tx, key, (k) => addValue, updateValueFactory);
        }

        public Task<TValue> AddOrUpdateAsync(ITransaction tx, TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return base.AddOrUpdateAsync(tx, key, (k) => addValue, updateValueFactory, timeout, cancellationToken);
        }
        #endregion

        #region ClearAsync
        public Task ClearAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            return base.ClearAsync();
        }
        #endregion

        #region ContainsKeyAsync
        public Task<bool> ContainsKeyAsync(ITransaction tx, TKey key)
        {
            return base.ContainsKeyAsync(tx, key, LockMode.Default);
        }

        public Task<bool> ContainsKeyAsync(ITransaction tx, TKey key, LockMode lockMode)
        {
            return base.ContainsKeyAsync(tx, key, lockMode);
        }

        public Task<bool> ContainsKeyAsync(ITransaction tx, TKey key, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return base.ContainsKeyAsync(tx, key, LockMode.Default, timeout, cancellationToken);
        }
        #endregion

        #region CreateEnumerableAsync
        public Task<IAsyncEnumerable<KeyValuePair<TKey, TValue>>> CreateEnumerableAsync(ITransaction tx)
        {
            return CreateEnumerableAsync(tx, EnumerationMode.Unordered);
        }

        public Task<IAsyncEnumerable<KeyValuePair<TKey, TValue>>> CreateEnumerableAsync(ITransaction tx, EnumerationMode enumerationMode)
        {
            return CreateEnumerableAsync(tx, null, enumerationMode);
        }

        public async Task<IAsyncEnumerable<KeyValuePair<TKey, TValue>>> CreateEnumerableAsync(ITransaction tx, Func<TKey, bool> filter, EnumerationMode enumerationMode)
        {
            List<TKey> keys = new List<TKey>();

            try
            {
                BeginTransaction(tx);

                foreach (var key in Dictionary.Keys)
                {
                    if (filter == null || filter(key))
                    {
                        await LockManager.AcquireLock(tx.TransactionId, key, LockMode.Default);
                        keys.Add(key);
                    }
                }

                if (enumerationMode == EnumerationMode.Ordered)
                {
                    keys.Sort();
                }

                IAsyncEnumerable<KeyValuePair<TKey, TValue>> result = new MockAsyncEnumerable<KeyValuePair<TKey, TValue>>(keys.Select(k => new KeyValuePair<TKey, TValue>(k, Dictionary[k])));
                return result;
            }
            catch
            {
                foreach (var key in keys)
                {
                    LockManager.ReleaseLock(tx.TransactionId, key);
                }

                throw;
            }
        }

        #endregion

        #region CreateKeyEnumerableAsync
        public Task<IAsyncEnumerable<TKey>> CreateKeyEnumerableAsync(ITransaction tx)
        {
            return CreateKeyEnumerableAsync(tx, EnumerationMode.Unordered, default(TimeSpan), CancellationToken.None);
        }

        public Task<IAsyncEnumerable<TKey>> CreateKeyEnumerableAsync(ITransaction tx, EnumerationMode enumerationMode)
        {
            return CreateKeyEnumerableAsync(tx, enumerationMode, default(TimeSpan), CancellationToken.None);
        }

        public async Task<IAsyncEnumerable<TKey>> CreateKeyEnumerableAsync(ITransaction tx, EnumerationMode enumerationMode, TimeSpan timeout, CancellationToken cancellationToken)
        {
            List<TKey> keys = new List<TKey>();

            try
            {
                BeginTransaction(tx);
                foreach (var key in Dictionary.Keys)
                {
                    await LockManager.AcquireLock(tx.TransactionId, key, LockMode.Default, timeout, cancellationToken);
                    keys.Add(key);
                }

                if (enumerationMode == EnumerationMode.Ordered)
                {
                    keys.Sort();
                }

                IAsyncEnumerable<TKey> result = new MockAsyncEnumerable<TKey>(keys);
                return result;
            }
            catch
            {
                foreach (var key in keys)
                {
                    LockManager.ReleaseLock(tx.TransactionId, key);
                }

                throw;
            }
        }
        #endregion

        #region GetOrAddAsync
        public Task<TValue> GetOrAddAsync(ITransaction tx, TKey key, Func<TKey, TValue> valueFactory)
        {
            return base.GetOrAddAsync(tx, key, valueFactory);
        }

        public Task<TValue> GetOrAddAsync(ITransaction tx, TKey key, TValue value)
        {
            return base.GetOrAddAsync(tx, key, (k) => value);
        }

        public Task<TValue> GetOrAddAsync(ITransaction tx, TKey key, TValue value, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return base.GetOrAddAsync(tx, key, (k) => value, timeout, cancellationToken);
        }
        #endregion

        #region SetAsync
        public Task SetAsync(ITransaction tx, TKey key, TValue value)
        {
            return base.SetAsync(tx, key, value, default(TimeSpan), CancellationToken.None);
        }
        #endregion

        #region TryAddValue
        public Task<bool> TryAddAsync(ITransaction tx, TKey key, TValue value)
        {
            return base.TryAddAsync(tx, key, value, default(TimeSpan), CancellationToken.None);
        }
        #endregion

        #region TryGetValueAsync
        public Task<ConditionalValue<TValue>> TryGetValueAsync(ITransaction tx, TKey key)
        {
            return base.TryGetValueAsync(tx, key, LockMode.Default, default(TimeSpan), CancellationToken.None);
        }

        public Task<ConditionalValue<TValue>> TryGetValueAsync(ITransaction tx, TKey key, LockMode lockMode)
        {
            return base.TryGetValueAsync(tx, key, lockMode, default(TimeSpan), CancellationToken.None);
        }

        public Task<ConditionalValue<TValue>> TryGetValueAsync(ITransaction tx, TKey key, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return base.TryGetValueAsync(tx, key, LockMode.Default, timeout, cancellationToken);
        }
        #endregion

        #region TryRemoveValueAsync
        public Task<ConditionalValue<TValue>> TryRemoveAsync(ITransaction tx, TKey key)
        {
            return base.TryRemoveAsync(tx, key, default(TimeSpan), CancellationToken.None);
        }
        #endregion

        #region TryUpdateAsync
        public Task<bool> TryUpdateAsync(ITransaction tx, TKey key, TValue newValue, TValue comparisonValue)
        {
            return base.TryUpdateAsync(tx, key, newValue, comparisonValue, default(TimeSpan), CancellationToken.None);
        }
        #endregion
    }
}
