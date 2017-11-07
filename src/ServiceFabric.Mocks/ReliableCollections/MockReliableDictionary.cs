namespace ServiceFabric.Mocks.ReliableCollections
{
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Collections;
    using Microsoft.ServiceFabric.Data.Notifications;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class MockReliableDictionary<TKey, TValue> : ReliableCollection, IReliableDictionary<TKey, TValue>
        where TKey : IEquatable<TKey>, IComparable<TKey>
    {
        LockManager<TKey> _lockManager = new LockManager<TKey>();
        ConcurrentDictionary<TKey, TValue> _dictionary = new ConcurrentDictionary<TKey, TValue>();

        public MockReliableDictionary(Uri uri)
            : base(uri)
        { }

        public override void ReleaseLocks(ITransaction tx)
        {
            _lockManager.ReleaseLocks(tx);
        }

        public Func<IReliableDictionary<TKey, TValue>, NotifyDictionaryRebuildEventArgs<TKey, TValue>, Task> RebuildNotificationAsyncCallback { set => throw new NotImplementedException(); }

        public event EventHandler<NotifyDictionaryChangedEventArgs<TKey, TValue>> DictionaryChanged;

        public Task AddAsync(ITransaction tx, TKey key, TValue value)
        {
            return AddAsync(tx, key, value, default(TimeSpan), CancellationToken.None);
        }

        public async Task AddAsync(ITransaction tx, TKey key, TValue value, TimeSpan timeout, CancellationToken cancellationToken)
        {
            await _lockManager.AcquireLock(BeginTransaction(tx), key, LockMode.Update, timeout, cancellationToken);
            if (!_dictionary.TryAdd(key, value))
            {
                throw new ArgumentException("A value with the same key already exists.", nameof(value));
            }

            AddAbortAction(tx, () => { _dictionary.TryRemove(key, out TValue v); return true; });
            AddCommitAction(tx, () => { DictionaryChanged?.Invoke(this, new NotifyDictionaryItemAddedEventArgs<TKey, TValue>(tx, key, value)); return true; });
        }

        public Task<TValue> AddOrUpdateAsync(ITransaction tx, TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
        {
            return AddOrUpdateAsync(tx, key, addValueFactory, updateValueFactory, default(TimeSpan), CancellationToken.None);
        }

        public Task<TValue> AddOrUpdateAsync(ITransaction tx, TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
        {
            return AddOrUpdateAsync(tx, key, (k) => addValue, updateValueFactory);
        }

        public async Task<TValue> AddOrUpdateAsync(ITransaction tx, TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory, TimeSpan timeout, CancellationToken cancellationToken)
        {
            await _lockManager.AcquireLock(BeginTransaction(tx), key, LockMode.Update, timeout, cancellationToken);

            bool isUpdate = _dictionary.TryGetValue(key, out TValue oldValue);
            var newValue = _dictionary.AddOrUpdate(key, addValueFactory, updateValueFactory);

            if (isUpdate)
            {
                AddAbortAction(tx, () => { _dictionary[key] = oldValue; return true; });
                AddCommitAction(tx, () => { DictionaryChanged?.Invoke(this, new NotifyDictionaryItemUpdatedEventArgs<TKey, TValue>(tx, key, newValue)); return true; });
            }
            else
            {
                AddAbortAction(tx, () => { _dictionary.TryRemove(key, out TValue v); return true; });
                AddCommitAction(tx, () => { DictionaryChanged?.Invoke(this, new NotifyDictionaryItemAddedEventArgs<TKey, TValue>(tx, key, newValue)); return true; });
            }

            return newValue;
        }

        public Task<TValue> AddOrUpdateAsync(ITransaction tx, TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return AddOrUpdateAsync(tx, key, (k) => addValue, updateValueFactory, timeout, cancellationToken);
        }

        public Task ClearAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            return ClearAsync();
        }

        public override Task ClearAsync()
        {
            _dictionary.Clear();

            return Task.FromResult(true);
        }

        public Task<bool> ContainsKeyAsync(ITransaction tx, TKey key)
        {
            return ContainsKeyAsync(tx, key, LockMode.Default, default(TimeSpan), CancellationToken.None);
        }

        public Task<bool> ContainsKeyAsync(ITransaction tx, TKey key, LockMode lockMode)
        {
            return ContainsKeyAsync(tx, key, lockMode, default(TimeSpan), CancellationToken.None);
        }

        public Task<bool> ContainsKeyAsync(ITransaction tx, TKey key, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return ContainsKeyAsync(tx, key, LockMode.Default, timeout, cancellationToken);
        }

        public async Task<bool> ContainsKeyAsync(ITransaction tx, TKey key, LockMode lockMode, TimeSpan timeout, CancellationToken cancellationToken)
        {
            await _lockManager.AcquireLock(BeginTransaction(tx), key, lockMode, timeout, cancellationToken);

            return _dictionary.ContainsKey(key);
        }

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

                foreach (var key in _dictionary.Keys)
                {
                    if (filter == null || filter(key))
                    {
                        await _lockManager.AcquireLock(tx, key, LockMode.Default, default(TimeSpan), CancellationToken.None);
                        keys.Add(key);
                    }
                }

                if (enumerationMode == EnumerationMode.Ordered)
                {
                    keys.Sort();
                }

                IAsyncEnumerable<KeyValuePair<TKey, TValue>> result = new MockAsyncEnumerable<KeyValuePair<TKey, TValue>>(keys.Select(k => new KeyValuePair<TKey, TValue>(k, _dictionary[k])));
                return result;
            }
            catch
            {
                foreach (var key in keys)
                {
                    _lockManager.ReleaseLock(tx, key);
                }

                throw;
            }
        }

        public override Task<long> GetCountAsync(ITransaction tx)
        {
            return Task.FromResult((long)_dictionary.Count());
        }

        public Task<TValue> GetOrAddAsync(ITransaction tx, TKey key, Func<TKey, TValue> valueFactory)
        {
            return GetOrAddAsync(tx, key, valueFactory, default(TimeSpan), CancellationToken.None);
        }

        public Task<TValue> GetOrAddAsync(ITransaction tx, TKey key, TValue value)
        {
            return GetOrAddAsync(tx, key, (k) => value, default(TimeSpan), CancellationToken.None);
        }

        public async Task<TValue> GetOrAddAsync(ITransaction tx, TKey key, Func<TKey, TValue> valueFactory, TimeSpan timeout, CancellationToken cancellationToken)
        {
            var l = await _lockManager.AcquireLock(BeginTransaction(tx), key, LockMode.Update, timeout, cancellationToken);
            TValue value;

            if (_dictionary.TryGetValue(key, out value))
            {
                l.Downgrade(tx);
            }
            else
            {
                value = valueFactory(key);
                _dictionary[key] = value;
            }

            return value;
        }

        public Task<TValue> GetOrAddAsync(ITransaction tx, TKey key, TValue value, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return GetOrAddAsync(tx, key, (k) => value, timeout, cancellationToken);
        }

        public Task SetAsync(ITransaction tx, TKey key, TValue value)
        {
            return SetAsync(tx, key, value, default(TimeSpan), CancellationToken.None);
        }

        public async Task SetAsync(ITransaction tx, TKey key, TValue value, TimeSpan timeout, CancellationToken cancellationToken)
        {
            await _lockManager.AcquireLock(BeginTransaction(tx), key, LockMode.Update, timeout, cancellationToken);

            _dictionary[key] = value;
        }

        public Task<bool> TryAddAsync(ITransaction tx, TKey key, TValue value)
        {
            return TryAddAsync(tx, key, value, default(TimeSpan), CancellationToken.None);
        }

        public async Task<bool> TryAddAsync(ITransaction tx, TKey key, TValue value, TimeSpan timeout, CancellationToken cancellationToken)
        {
            await _lockManager.AcquireLock(BeginTransaction(tx), key, LockMode.Update, timeout, cancellationToken);
            var result = _dictionary.TryAdd(key, value);
            if (!result)
            {
                _lockManager.ReleaseLock(tx, key);
            }

            return result;
        }

        public Task<ConditionalValue<TValue>> TryGetValueAsync(ITransaction tx, TKey key)
        {
            return TryGetValueAsync(tx, key, LockMode.Default, default(TimeSpan), CancellationToken.None);
        }

        public Task<ConditionalValue<TValue>> TryGetValueAsync(ITransaction tx, TKey key, LockMode lockMode)
        {
            return TryGetValueAsync(tx, key, lockMode, default(TimeSpan), CancellationToken.None);
        }

        public Task<ConditionalValue<TValue>> TryGetValueAsync(ITransaction tx, TKey key, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return TryGetValueAsync(tx, key, LockMode.Default, timeout, cancellationToken);
        }

        public async Task<ConditionalValue<TValue>> TryGetValueAsync(ITransaction tx, TKey key, LockMode lockMode, TimeSpan timeout, CancellationToken cancellationToken)
        {
            await _lockManager.AcquireLock(BeginTransaction(tx), key, lockMode, timeout, cancellationToken);

            if (_dictionary.TryGetValue(key, out TValue value))
            {
                return new ConditionalValue<TValue>(true, value);
            }

            _lockManager.ReleaseLock(tx, key);

            return new ConditionalValue<TValue>();
        }

        public Task<ConditionalValue<TValue>> TryRemoveAsync(ITransaction tx, TKey key)
        {
            return TryRemoveAsync(tx, key, default(TimeSpan), CancellationToken.None);
        }

        public async Task<ConditionalValue<TValue>> TryRemoveAsync(ITransaction tx, TKey key, TimeSpan timeout, CancellationToken cancellationToken)
        {
            await _lockManager.AcquireLock(BeginTransaction(tx), key, LockMode.Update, timeout, cancellationToken);

            if (_dictionary.TryRemove(key, out TValue value))
            {
                return new ConditionalValue<TValue>(true, value);
            }

            _lockManager.ReleaseLock(tx, key);

            return new ConditionalValue<TValue>();
        }

        public Task<bool> TryUpdateAsync(ITransaction tx, TKey key, TValue newValue, TValue comparisonValue)
        {
            return TryUpdateAsync(tx, key, newValue, comparisonValue, default(TimeSpan), CancellationToken.None);
        }

        public async Task<bool> TryUpdateAsync(ITransaction tx, TKey key, TValue newValue, TValue comparisonValue, TimeSpan timeout, CancellationToken cancellationToken)
        {
            await _lockManager.AcquireLock(BeginTransaction(tx), key, LockMode.Update, timeout, cancellationToken);
            var result = _dictionary.TryUpdate(key, newValue, comparisonValue);
            if (!result)
            {
                _lockManager.ReleaseLock(tx, key);
            }

            return result;
        }
    }
}
