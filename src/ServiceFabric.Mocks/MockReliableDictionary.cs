using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Data.Notifications;

namespace ServiceFabric.Mocks
{
    /// <summary>
    /// Represents a reliable collection of key/value pairs that are persisted and replicated.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the reliable dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the reliable dictionary.</typeparam>
    /// <remarks>Keys or values stored in this dictionary MUST NOT be mutated outside the context of an operation on the
    /// dictionary.  It is highly recommended to make both <typeparamref name="TKey" /> and <typeparamref name="TValue" />
    /// immutable in order to avoid accidental data corruption.</remarks>
    public class MockReliableDictionary<TKey, TValue> : IReliableDictionary<TKey, TValue>
        where TKey : IComparable<TKey>, IEquatable<TKey>
    {
        private readonly ConcurrentDictionary<TKey, TValue> _state = new ConcurrentDictionary<TKey, TValue>();

        public event EventHandler<NotifyDictionaryChangedEventArgs<TKey, TValue>> DictionaryChanged;

        public Uri Name { get; set; }

        public Func<IReliableDictionary<TKey, TValue>, NotifyDictionaryRebuildEventArgs<TKey, TValue>, Task> RebuildNotificationAsyncCallback { get; set; }


        /// <inheritdoc />
        public Task AddAsync(ITransaction tx, TKey key, TValue value)
        {
            if (!_state.TryAdd(key, value))
            {
                throw new ArgumentException($"The provided key '{key}' already exists.", nameof(key));
            }
            OnDictionaryChanged(new NotifyDictionaryItemAddedEventArgs<TKey, TValue>(tx, key, value));
            return Task.FromResult(true);
        }

        public Task AddAsync(ITransaction tx, TKey key, TValue value, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return AddAsync(tx, key, value);
        }

        public Task<TValue> AddOrUpdateAsync(ITransaction tx, TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
        {
            var value = _state.AddOrUpdate(key, addValueFactory, updateValueFactory);
            OnDictionaryChanged(new NotifyDictionaryItemUpdatedEventArgs<TKey, TValue>(tx, key, value));
            return Task.FromResult(value);
        }

        public Task<TValue> AddOrUpdateAsync(ITransaction tx, TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
        {
            return AddOrUpdateAsync(tx, key, _ => addValue, updateValueFactory);
        }

        public Task<TValue> AddOrUpdateAsync(
            ITransaction tx, TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory, TimeSpan timeout,
            CancellationToken cancellationToken)
        {
            return AddOrUpdateAsync(tx, key, addValueFactory, updateValueFactory);
        }

        public Task<TValue> AddOrUpdateAsync(
            ITransaction tx, TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return AddOrUpdateAsync(tx, key, _ => addValue, updateValueFactory);
        }

        public Task ClearAsync()
        {
            _state.Clear();
            OnDictionaryChanged(new NotifyDictionaryClearEventArgs<TKey, TValue>(1L));
            return Task.FromResult(true);
        }

        public Task ClearAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            return ClearAsync();
        }

        public Task<bool> ContainsKeyAsync(ITransaction tx, TKey key)
        {
            return Task.FromResult(_state.ContainsKey(key));
        }

        public Task<bool> ContainsKeyAsync(ITransaction tx, TKey key, LockMode lockMode)
        {
            return ContainsKeyAsync(tx, key);
        }

        public Task<bool> ContainsKeyAsync(ITransaction tx, TKey key, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return ContainsKeyAsync(tx, key);
        }

        public Task<bool> ContainsKeyAsync(ITransaction tx, TKey key, LockMode lockMode, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return ContainsKeyAsync(tx, key);
        }

        public Task<ConditionalValue<TValue>> TryGetValueAsync(ITransaction tx, TKey key)
        {
            TValue value;
            bool result = _state.TryGetValue(key, out value);
            return Task.FromResult(new ConditionalValue<TValue>(result, value));
        }

        public Task<ConditionalValue<TValue>> TryGetValueAsync(ITransaction tx, TKey key, LockMode lockMode)
        {
            return TryGetValueAsync(tx, key);
        }

        public Task<ConditionalValue<TValue>> TryGetValueAsync(ITransaction tx, TKey key, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return TryGetValueAsync(tx, key);
        }

        public Task<ConditionalValue<TValue>> TryGetValueAsync(
            ITransaction tx, TKey key, LockMode lockMode, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return TryGetValueAsync(tx, key);
        }

        public Task SetAsync(ITransaction tx, TKey key, TValue value)
        {
            _state[key] = value;
            OnDictionaryChanged(new NotifyDictionaryItemUpdatedEventArgs<TKey, TValue>(tx, key, value));
            return Task.FromResult(value);
        }

        public Task SetAsync(ITransaction tx, TKey key, TValue value, TimeSpan timeout, CancellationToken cancellationToken)
        {
           return SetAsync(tx, key, value);
        }

        public Task<TValue> GetOrAddAsync(ITransaction tx, TKey key, Func<TKey, TValue> valueFactory)
        {
            bool added = !_state.ContainsKey(key);
            var value = _state.GetOrAdd(key, valueFactory);
            if (added)
            {
                OnDictionaryChanged(new NotifyDictionaryItemAddedEventArgs<TKey, TValue>(tx, key, value));
            }
            return Task.FromResult(value);
        }

        public Task<TValue> GetOrAddAsync(ITransaction tx, TKey key, TValue value)
        {
            return GetOrAddAsync(tx, key, _ => value);
        }

        public Task<TValue> GetOrAddAsync(ITransaction tx, TKey key, Func<TKey, TValue> valueFactory, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return GetOrAddAsync(tx, key, valueFactory);
        }

        public Task<TValue> GetOrAddAsync(ITransaction tx, TKey key, TValue value, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return GetOrAddAsync(tx, key, _ => value);
        }

        public Task<bool> TryAddAsync(ITransaction tx, TKey key, TValue value)
        {
            var result = _state.TryAdd(key, value);
            if (result)
            {
                OnDictionaryChanged(new NotifyDictionaryItemAddedEventArgs<TKey, TValue>(tx, key, value));
            }
            return Task.FromResult(result);
        }

        public Task<bool> TryAddAsync(ITransaction tx, TKey key, TValue value, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return TryAddAsync(tx, key, value);
        }

        public Task<ConditionalValue<TValue>> TryRemoveAsync(ITransaction tx, TKey key)
        {
            TValue value;
            var result = new ConditionalValue<TValue>(_state.TryRemove(key, out value), value);
            if (result.HasValue)
            {
                OnDictionaryChanged(new NotifyDictionaryItemRemovedEventArgs<TKey, TValue>(tx, key));
            }
            return Task.FromResult(result);
        }

        public Task<ConditionalValue<TValue>> TryRemoveAsync(ITransaction tx, TKey key, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return TryRemoveAsync(tx, key);
        }

        public Task<bool> TryUpdateAsync(ITransaction tx, TKey key, TValue newValue, TValue comparisonValue)
        {
            bool result = _state.TryUpdate(key, newValue, comparisonValue);
            if (result)
            {
                OnDictionaryChanged(new NotifyDictionaryItemUpdatedEventArgs<TKey, TValue>(tx, key, newValue));
            }
            return Task.FromResult(result);
        }

        public Task<bool> TryUpdateAsync(ITransaction tx, TKey key, TValue newValue, TValue comparisonValue, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return TryUpdateAsync(tx, key, newValue, comparisonValue);
        }

        public Task<long> GetCountAsync()
        {
            return Task.FromResult((long)_state.Count);
        }

        public Task<IAsyncEnumerable<KeyValuePair<TKey, TValue>>> CreateEnumerableAsync(ITransaction tx)
        {
            return CreateEnumerableAsync(tx, _ => true, EnumerationMode.Unordered);
        }

        public Task<IAsyncEnumerable<KeyValuePair<TKey, TValue>>> CreateEnumerableAsync(ITransaction tx, EnumerationMode enumerationMode)
        {
            return CreateEnumerableAsync(tx, _ => true, enumerationMode);
        }

        public Task<IAsyncEnumerable<KeyValuePair<TKey, TValue>>> CreateEnumerableAsync(ITransaction tx, Func<TKey, bool> filter, EnumerationMode enumerationMode)
        {
            var source = _state.Where(s => filter(s.Key));
            if (enumerationMode == EnumerationMode.Ordered)
            {
                source = source.OrderBy(s => s.Key);
            }
            var enumerable = new MockAsyncEnumerable<KeyValuePair<TKey, TValue>>(source);
            return Task.FromResult<IAsyncEnumerable<KeyValuePair<TKey, TValue>>>(enumerable);
        }

        public Task<long> GetCountAsync(ITransaction tx)
        {
            return Task.FromResult((long)_state.Count);
        }

        protected virtual void OnDictionaryChanged(NotifyDictionaryChangedEventArgs<TKey, TValue> e)
        {
            DictionaryChanged?.Invoke(this, e);
        }
    }
}