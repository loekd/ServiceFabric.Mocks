namespace ServiceFabric.Mocks.ReliableCollections
{
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Collections;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class LockManager<TKey>
    {
        private ConcurrentDictionary<TKey, Lock> _lockTable = new ConcurrentDictionary<TKey, Lock>();
        private ConcurrentDictionary<long, HashSet<TKey>> _txLocks = new ConcurrentDictionary<long, HashSet<TKey>>();

        public async Task<AcquireResult> AcquireLock(ITransaction tx, TKey key, LockMode lockMode, TimeSpan timeout = default(TimeSpan), CancellationToken token = default(CancellationToken))
        {
            var l = _lockTable.GetOrAdd(key, (k) => new Lock());
            var result = await l.Acquire(tx, lockMode, timeout, token);
            if (result == AcquireResult.Acquired)
            {
                var keys = _txLocks.GetOrAdd(tx.TransactionId, (k) => new HashSet<TKey>());
                keys.Add(key);
            }

            return result;
        }

        public bool DowngradeLock(ITransaction tx, TKey key)
        {
            if (_txLocks.TryGetValue(tx.TransactionId, out HashSet<TKey> keys))
            {
                if (_lockTable.TryGetValue(key, out Lock l))
                {
                    return l.Downgrade(tx);
                };
            }

            return false;
        }

        public bool ReleaseLock(ITransaction tx, TKey key)
        {
            if (_txLocks.TryGetValue(tx.TransactionId, out HashSet<TKey> keys))
            {
                if (_lockTable.TryGetValue(key, out Lock l))
                {
                    return l.Release(tx);
                };
            }

            return false;
        }

        public void ReleaseLocks(ITransaction tx)
        {
            if (_txLocks.TryRemove(tx.TransactionId, out HashSet<TKey> keys))
            {
                foreach (var key in keys)
                {
                    if (_lockTable.TryGetValue(key, out Lock l))
                    {
                        l.Release(tx);
                    }
                }
            }
        }
    }
}
