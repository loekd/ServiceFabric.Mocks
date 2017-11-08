using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;
using System.Collections.Concurrent;
using Microsoft.ServiceFabric.Data.Collections;
using System.Threading;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ServiceFabric.Mocks.ReliableCollections
{
    public enum ChangeType
    {
        Added,
        Removed,
        Updated,
    }

    public class TransactedConcurrentDictionary<TKey, TValue> : ReliableCollection
    {
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

        protected Func<DictionaryChange, bool> OnDictionaryChanged;

        public IEnumerable<TValue> ValuesEnumerable => Dictionary.Values;
        protected ConcurrentDictionary<TKey, TValue> Dictionary { get; private set; }
        protected LockManager<TKey> LockManager { get; private set; }

        public TransactedConcurrentDictionary(Uri uri, Func<DictionaryChange, bool> changeCallback)
            : base(uri)
        {
            Dictionary = new ConcurrentDictionary<TKey, TValue>();
            LockManager = new LockManager<TKey>();
            OnDictionaryChanged = changeCallback;
        }

        public override void ReleaseLocks(ITransaction tx)
        {
            LockManager.ReleaseLocks(tx);
        }

        public void Deserialize(Stream stream)
        {
            var formatter = new BinaryFormatter();
            Dictionary = (ConcurrentDictionary<TKey, TValue>)formatter.Deserialize(stream);
        }

        public void Serialize(Stream stream)
        {
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, Dictionary);
        }

        #region IReliableCollection
        public override Task ClearAsync()
        {
            Dictionary.Clear();
            return Task.FromResult(true);
        }

        public override Task<long> GetCountAsync(ITransaction tx)
        {
            long count = Dictionary.Count;
            return Task.FromResult(count);
        }
        #endregion

        #region IReliableDictionary
        public async Task AddAsync(ITransaction tx, TKey key, TValue value, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            await LockManager.AcquireLock(BeginTransaction(tx), key, LockMode.Update, timeout, cancellationToken);
            if (!Dictionary.TryAdd(key, value))
            {
                throw new ArgumentException("A value with the same key already exists.", nameof(value));
            }

            AddAbortAction(tx, () => { Dictionary.TryRemove(key, out TValue v); return true; });
            AddCommitAction(tx, () => { OnDictionaryChanged(new DictionaryChange(tx, ChangeType.Added, key, added: value)); return true; });
        }

        public async Task<TValue> AddOrUpdateAsync(ITransaction tx, TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            await LockManager.AcquireLock(BeginTransaction(tx), key, LockMode.Update, timeout, cancellationToken);

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
            await LockManager.AcquireLock(BeginTransaction(tx), key, lockMode, timeout, cancellationToken);

            return Dictionary.ContainsKey(key);
        }

        public async Task<TValue> GetOrAddAsync(ITransaction tx, TKey key, Func<TKey, TValue> valueFactory, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            var l = await LockManager.AcquireLock(BeginTransaction(tx), key, LockMode.Update, timeout, cancellationToken);
            TValue value;

            if (Dictionary.TryGetValue(key, out value))
            {
                l.Downgrade(tx);
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
            await LockManager.AcquireLock(BeginTransaction(tx), key, LockMode.Update, timeout, cancellationToken);

            TValue oldValue = Dictionary[key];
            Dictionary[key] = value;

            AddAbortAction(tx, () => { Dictionary[key] = oldValue; return true; });
            AddCommitAction(tx, () => { OnDictionaryChanged(new DictionaryChange(tx, ChangeType.Updated, key, added: value, removed: oldValue)); return true; });
        }

        public async Task<bool> TryAddAsync(ITransaction tx, TKey key, TValue value, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            await LockManager.AcquireLock(BeginTransaction(tx), key, LockMode.Update, timeout, cancellationToken);
            var result = Dictionary.TryAdd(key, value);
            if (result)
            {
                AddAbortAction(tx, () => { Dictionary.TryRemove(key, out TValue v); return true; });
                AddCommitAction(tx, () => { OnDictionaryChanged(new DictionaryChange(tx, ChangeType.Added, key, added: value)); return true; });
            }
            else
            {
                LockManager.ReleaseLock(tx, key);
            }

            return result;
        }

        public async Task<ConditionalValue<TValue>> TryGetValueAsync(ITransaction tx, TKey key, LockMode lockMode, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            await LockManager.AcquireLock(BeginTransaction(tx), key, lockMode, timeout, cancellationToken);

            if (Dictionary.TryGetValue(key, out TValue value))
            {
                return new ConditionalValue<TValue>(true, value);
            }

            return new ConditionalValue<TValue>();
        }

        public async Task<ConditionalValue<TValue>> TryRemoveAsync(ITransaction tx, TKey key, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            await LockManager.AcquireLock(BeginTransaction(tx), key, LockMode.Update, timeout, cancellationToken);

            if (Dictionary.TryRemove(key, out TValue value))
            {
                AddAbortAction(tx, () => { Dictionary.TryAdd(key, value); return true; });
                AddCommitAction(tx, () => { OnDictionaryChanged(new DictionaryChange(tx, ChangeType.Removed, key, removed: value)); return true; });
                return new ConditionalValue<TValue>(true, value);
            }

            LockManager.ReleaseLock(tx, key);

            return new ConditionalValue<TValue>();
        }

        public async Task<bool> TryUpdateAsync(ITransaction tx, TKey key, TValue newValue, TValue comparisonValue, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            await LockManager.AcquireLock(BeginTransaction(tx), key, LockMode.Update, timeout, cancellationToken);
            var result = Dictionary.TryUpdate(key, newValue, comparisonValue);
            if (result)
            {
                AddAbortAction(tx, () => { Dictionary[key] = comparisonValue; return true; });
                AddCommitAction(tx, () => { OnDictionaryChanged(new DictionaryChange(tx, ChangeType.Updated, key, added: newValue, removed: comparisonValue)); return true; });
            }
            else
            {
                LockManager.ReleaseLock(tx, key);
            }

            return result;
        }
        #endregion
    }
}
