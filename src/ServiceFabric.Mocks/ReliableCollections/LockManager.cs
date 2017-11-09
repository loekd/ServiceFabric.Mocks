namespace ServiceFabric.Mocks.ReliableCollections
{
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Collections;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Implements a simple manager for key locks
    /// </summary>
    /// <typeparam name="TKey">Key Type</typeparam>
    public class LockManager<TKey>
    {
        private ConcurrentDictionary<TKey, Lock> _lockTable = new ConcurrentDictionary<TKey, Lock>();
        private ConcurrentDictionary<long, HashSet<TKey>> _txLocks = new ConcurrentDictionary<long, HashSet<TKey>>();

        /// <summary>
        /// Try to acquire the key lock. If it is newly acquired then add it to the lock table.
        /// Lock upgrade is performed here. If the transaction has the Default key lock, and tried to acquire an Update lock
        /// then the lock will be upgraded and AcquireResult.Owned will be returned.
        /// 
        /// If the lock cannot be acquired in the specified timeout then a TimeoutException is thrown (by Lock.Acquire).
        /// </summary>
        /// <param name="tx">Transaction</param>
        /// <param name="key">Key</param>
        /// <param name="lockMode">Lock Mode</param>
        /// <param name="timeout">Timeout</param>
        /// <param name="token">Cancellation Token</param>
        /// <returns>{Acquired|Owned}</returns>
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

        /// <summary>
        /// Downgrade the key lock if the update lock is owned by the transaction.
        /// </summary>
        /// <param name="tx">Transaction</param>
        /// <param name="key">Key</param>
        /// <returns></returns>
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

        /// <summary>
        /// Release the locks on the key lock that are owned by the transaction.
        /// </summary>
        /// <param name="tx">Transaction</param>
        /// <param name="key">Key</param>
        /// <returns></returns>
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

        /// <summary>
        /// Release all key locks owned by the transaction.
        /// </summary>
        /// <param name="tx">Transaction</param>
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
