namespace ServiceFabric.Mocks.ReliableCollections
{
    using Microsoft.ServiceFabric.Data.Collections;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Implements a simple manager for key locks
    /// </summary>
    /// <typeparam name="TKey">Lock Key Type</typeparam>
    /// <typeparam name="TId">Lock Owner Id Type</typeparam>
    public class LockManager<TKey, TId>
    {
        private ConcurrentDictionary<TKey, Lock<TId>> _lockTable = new ConcurrentDictionary<TKey, Lock<TId>>();
        private ConcurrentDictionary<TId, HashSet<TKey>> _ownerLockKeys = new ConcurrentDictionary<TId, HashSet<TKey>>();

        /// <summary>
        /// Try to acquire the key lock. If it is newly acquired then add it to the lock table.
        /// Lock upgrade is performed here. If the transaction has the Default key lock, and tried to acquire an Update lock
        /// then the lock will be upgraded and AcquireResult.Owned will be returned.
        /// 
        /// If the lock cannot be acquired in the specified timeout then a TimeoutException is thrown (by Lock.Acquire).
        /// </summary>
        /// <param name="id">Owner Id</param>
        /// <param name="key">Key</param>
        /// <param name="lockMode">Lock Mode</param>
        /// <param name="timeout">Timeout</param>
        /// <param name="token">Cancellation Token</param>
        /// <returns>{Acquired|Owned}</returns>
        public async Task<AcquireResult> AcquireLock(TId id, TKey key, LockMode lockMode, TimeSpan timeout = default(TimeSpan), CancellationToken token = default(CancellationToken))
        {
            var l = _lockTable.GetOrAdd(key, (k) => new Lock<TId>());
            var result = await l.Acquire(id, lockMode, timeout, token);
            if (result == AcquireResult.Acquired)
            {
                var keys = _ownerLockKeys.GetOrAdd(id, (k) => new HashSet<TKey>());
                keys.Add(key);
            }

            return result;
        }

        /// <summary>
        /// Downgrade the key lock if the update lock is owned by the transaction.
        /// </summary>
        /// <param name="id">Owner Id</param>
        /// <param name="key">Key</param>
        /// <returns></returns>
        public bool DowngradeLock(TId id, TKey key)
        {
            if (_ownerLockKeys.TryGetValue(id, out HashSet<TKey> keys))
            {
                if (_lockTable.TryGetValue(key, out Lock<TId> l))
                {
                    return l.Downgrade(id);
                };
            }

            return false;
        }

        /// <summary>
        /// Release the locks on the key lock that are owned by the transaction.
        /// </summary>
        /// <param name="id">Owner Id</param>
        /// <param name="key">Key</param>
        /// <returns></returns>
        public bool ReleaseLock(TId id, TKey key)
        {
            if (_ownerLockKeys.TryGetValue(id, out HashSet<TKey> keys))
            {
                if (_lockTable.TryGetValue(key, out Lock<TId> l))
                {
                    return l.Release(id);
                };
            }

            return false;
        }

        /// <summary>
        /// Release all key locks owned by the transaction.
        /// </summary>
        /// <param name="id">Owner Id</param>
        public void ReleaseLocks(TId id)
        {
            if (_ownerLockKeys.TryRemove(id, out HashSet<TKey> keys))
            {
                foreach (var key in keys)
                {
                    if (_lockTable.TryGetValue(key, out Lock<TId> l))
                    {
                        l.Release(id);
                    }
                }
            }
        }
    }
}
