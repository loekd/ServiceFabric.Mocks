namespace ServiceFabric.Mocks.ReliableCollections
{
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Collections;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Threading;
    using System.Threading.Tasks;

    public enum ChangeType
    {
        Added,
        Removed,
        Updated,
    }

    /// <summary>
    /// Implements the core methods of IReliableDictionary, but does not require TKey to be IComparable or IEquatable.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class TransactedConcurrentDictionary<TKey, TValue> : TransactedCollection
    {
        /// <summary>
        /// Describe the change made to the collection. ChangeType:
        ///   Added: Added is set to the value that was added.
        ///   Removed: Removed is set to the value that was removed.
        ///   Updated: Added is set to the updated value, Removed is set to the origional value.
        /// </summary>
        public sealed class DictionaryChange
        {
            public DictionaryChange(ITransaction tx, ChangeType changeType, TKey key, TValue added = default(TValue), TValue removed = default(TValue))
            {
                Transaction = tx;
                ChangeType = changeType;
                Key = key;
                Added = added;
                Removed = removed;
            }

            public ITransaction Transaction { get; private set; }
            public ChangeType ChangeType { get; private set; }
            public TKey Key { get; private set; }
            public TValue Added { get; private set; }
            public TValue Removed { get; private set; }
        }

        /// <summary>
        /// This is fired similar to the DictionaryChanged event on IReliableDictionary except that it provides the
        /// removed value on a remove operation.
        /// </summary>
        protected Func<DictionaryChange, bool> OnDictionaryChanged;

        public IEnumerable<TValue> ValuesEnumerable => Dictionary.Values;
        protected ConcurrentDictionary<TKey, TValue> Dictionary { get; private set; }
        protected LockManager<TKey, long> LockManager { get; private set; }

        public TransactedConcurrentDictionary(Uri uri, Func<DictionaryChange, bool> changeCallback)
            : base(uri)
        {
            Dictionary = new ConcurrentDictionary<TKey, TValue>();
            LockManager = new LockManager<TKey, long>();
            OnDictionaryChanged = changeCallback;
        }

        /// <summary>
        /// Release any locks in the TransactedConcurrentDictionary owned by the transaction.
        /// </summary>
        /// <param name="tx"></param>
        public override void ReleaseLocks(ITransaction tx)
        {
            LockManager.ReleaseLocks(tx.TransactionId);
        }

        /// <summary>
        /// Initialize the internal ConcurrentDictionary to the deserialized stream.
        /// </summary>
        /// <remarks>
        /// This should probably also purge the locks, and commit and abort actions.
        /// </remarks>
        /// <param name="stream">Source Stream</param>
        public void Deserialize(Stream stream)
        {
            var formatter = new BinaryFormatter();
            Dictionary = (ConcurrentDictionary<TKey, TValue>)formatter.Deserialize(stream);
        }

        /// <summary>
        /// Serialize the internal ConcurrentDictionary to the stream.
        /// </summary>
        /// <param name="stream">Target Stream</param>
        public void Serialize(Stream stream)
        {
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, Dictionary);
        }

        /// <summary>
        /// Implement IReliableCollection methods
        /// </summary>
        #region IReliableCollection
        public Task ClearAsync()
        {
            Dictionary.Clear();
            return Task.FromResult(true);
        }

        public Task<long> GetCountAsync(ITransaction tx)
        {
            long count = Dictionary.Count;
            return Task.FromResult(count);
        }
        #endregion

        /// <summary>
        /// Implement IReliableDictionary methods
        /// </summary>
        #region IReliableDictionary
        public async Task AddAsync(ITransaction tx, TKey key, TValue value, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            await LockManager.AcquireLock(BeginTransaction(tx).TransactionId, key, LockMode.Update, timeout, cancellationToken);
            if (!Dictionary.TryAdd(key, value))
            {
                throw new ArgumentException("A value with the same key already exists.", nameof(value));
            }

            AddAbortAction(tx, () => { Dictionary.TryRemove(key, out TValue v); return true; });
            AddCommitAction(tx, () => { OnDictionaryChanged(new DictionaryChange(tx, ChangeType.Added, key, added: value)); return true; });
        }

        public async Task<TValue> AddOrUpdateAsync(ITransaction tx, TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            await LockManager.AcquireLock(BeginTransaction(tx).TransactionId, key, LockMode.Update, timeout, cancellationToken);

            bool isUpdate = Dictionary.TryGetValue(key, out TValue oldValue);
            var newValue = Dictionary.AddOrUpdate(key, addValueFactory, updateValueFactory);

            if (isUpdate)
            {
                AddAbortAction(tx, () => { Dictionary[key] = oldValue; return true; });
                AddCommitAction(tx, () => { OnDictionaryChanged(new DictionaryChange(tx, ChangeType.Updated, key, added: newValue, removed: oldValue)); return true; });
            }
            else
            {
                AddAbortAction(tx, () => { Dictionary.TryRemove(key, out TValue v); return true; });
                AddCommitAction(tx, () => { OnDictionaryChanged(new DictionaryChange(tx, ChangeType.Added, key, added: newValue)); return true; });
            }

            return newValue;
        }

        public async Task<bool> ContainsKeyAsync(ITransaction tx, TKey key, LockMode lockMode, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            await LockManager.AcquireLock(BeginTransaction(tx).TransactionId, key, lockMode, timeout, cancellationToken);

            return Dictionary.ContainsKey(key);
        }

        public async Task<TValue> GetOrAddAsync(ITransaction tx, TKey key, Func<TKey, TValue> valueFactory, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            var acquireResult = await LockManager.AcquireLock(BeginTransaction(tx).TransactionId, key, LockMode.Update, timeout, cancellationToken);
            TValue value;

            if (Dictionary.TryGetValue(key, out value))
            {
                if (acquireResult == AcquireResult.Acquired)
                {
                    LockManager.DowngradeLock(tx.TransactionId, key);
                }
            }
            else
            {
                value = valueFactory(key);
                Dictionary.TryAdd(key, value);

                AddAbortAction(tx, () => { Dictionary.TryRemove(key, out TValue v); return true; });
                AddCommitAction(tx, () => { OnDictionaryChanged(new DictionaryChange(tx, ChangeType.Added, key, added: value)); return true; });
            }

            return value;
        }

        public async Task SetAsync(ITransaction tx, TKey key, TValue value, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            await LockManager.AcquireLock(BeginTransaction(tx).TransactionId, key, LockMode.Update, timeout, cancellationToken);

            bool oldValueExisted = Dictionary.TryGetValue(key, out TValue oldValue);

            Dictionary[key] = value;

            AddAbortAction(tx, () =>
            {
                if (oldValueExisted)
                {
                    Dictionary[key] = oldValue;
                }
                else
                {
                    Dictionary.TryRemove(key, out TValue removedValue);
                }

                return true;
            });
            AddCommitAction(tx, () => { OnDictionaryChanged(new DictionaryChange(tx, ChangeType.Updated, key, added: value, removed: oldValue)); return true; });
        }

        public async Task<bool> TryAddAsync(ITransaction tx, TKey key, TValue value, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            var acquireResult = await LockManager.AcquireLock(BeginTransaction(tx).TransactionId, key, LockMode.Update, timeout, cancellationToken);
            var result = Dictionary.TryAdd(key, value);
            if (result)
            {
                AddAbortAction(tx, () => { Dictionary.TryRemove(key, out TValue v); return true; });
                AddCommitAction(tx, () => { OnDictionaryChanged(new DictionaryChange(tx, ChangeType.Added, key, added: value)); return true; });
            }
            else if (acquireResult == AcquireResult.Acquired)
            {
                LockManager.ReleaseLock(tx.TransactionId, key);
            }

            return result;
        }

        public async Task<ConditionalValue<TValue>> TryGetValueAsync(ITransaction tx, TKey key, LockMode lockMode, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            await LockManager.AcquireLock(BeginTransaction(tx).TransactionId, key, lockMode, timeout, cancellationToken);

            if (Dictionary.TryGetValue(key, out TValue value))
            {
                return new ConditionalValue<TValue>(true, value);
            }

            return new ConditionalValue<TValue>();
        }

        public async Task<ConditionalValue<TValue>> TryRemoveAsync(ITransaction tx, TKey key, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            var acquireResult = await LockManager.AcquireLock(BeginTransaction(tx).TransactionId, key, LockMode.Update, timeout, cancellationToken);

            TValue value;
            bool hasValue = Dictionary.TryRemove(key, out value);
            if (hasValue)
            {
                AddAbortAction(tx, () => { Dictionary.TryAdd(key, value); return true; });
                AddCommitAction(tx, () => { OnDictionaryChanged(new DictionaryChange(tx, ChangeType.Removed, key, removed: value)); return true; });
            }
            else if (acquireResult == AcquireResult.Acquired)
            {
                LockManager.ReleaseLock(tx.TransactionId, key);
            }

            return new ConditionalValue<TValue>(hasValue, value);
        }

        public async Task<bool> TryUpdateAsync(ITransaction tx, TKey key, TValue newValue, TValue comparisonValue, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            var acquireResult = await LockManager.AcquireLock(BeginTransaction(tx).TransactionId, key, LockMode.Update, timeout, cancellationToken);
            var result = Dictionary.TryUpdate(key, newValue, comparisonValue);
            if (result)
            {
                AddAbortAction(tx, () => { Dictionary[key] = comparisonValue; return true; });
                AddCommitAction(tx, () => { OnDictionaryChanged(new DictionaryChange(tx, ChangeType.Updated, key, added: newValue, removed: comparisonValue)); return true; });
            }
            else if (acquireResult == AcquireResult.Acquired)
            {
                LockManager.ReleaseLock(tx.TransactionId, key);
            }

            return result;
        }
        #endregion
    }
}
