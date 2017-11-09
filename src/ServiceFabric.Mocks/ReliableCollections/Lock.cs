namespace ServiceFabric.Mocks.ReliableCollections
{
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Collections;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    public enum AcquireResult
    {
        Acquired,   // The lock request was granted and the RefCount was increased
        Denied,     // The lock request was denied
        Owned,      // The lock request was granted and the RefCount was not increased
    }

    public class Lock
    {
        private HashSet<long> _lockOwners = new HashSet<long>();
        public CancellationTokenSource TokenSource { get; private set; }

        public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(4);

        #region ILock
        public bool Downgrade(ITransaction tx)
        {
            bool result = true;
            if (LockMode == LockMode.Update)
            {
                lock (_lockOwners)
                {
                    if (_lockOwners.Contains(tx.TransactionId))
                    {
                        LockMode = LockMode.Default;
                        if (TokenSource != null)
                        {
                            TokenSource.Cancel();
                            TokenSource = null;
                        }
                    }
                    else
                    {
                        result = false;
                    }
                }
            }

            return result;
        }

        public LockMode LockMode { get; internal set; }

        public bool Release(ITransaction tx)
        {
            bool result;
            lock (_lockOwners)
            {
                result = _lockOwners.Remove(tx.TransactionId);
                if (_lockOwners.Count == 0)
                {
                    LockMode = LockMode.Default;
                    if (TokenSource != null)
                    {
                        TokenSource.Cancel();
                        TokenSource = null;
                    }
                }
            }

            return result;
        }
        #endregion

        /// <summary>
        /// Try to acquire the lock in the specified timeout.
        /// If the lock was acquired because it was newly Acquired, or already Owned by the transaction, then it returns the result.
        /// If the lock was not acquired in the specitied timeout then a TimeoutExcption is thrown.
        /// </summary>
        /// <param name="tx">Transaction</param>
        /// <param name="lockMode">Lock Mode</param>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>{Acquired|Owned}</returns>
        public async Task<AcquireResult> Acquire(ITransaction tx, LockMode lockMode, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            if (timeout == default(TimeSpan))
            {
                timeout = DefaultTimeout;
            }

            var result = await Acquire(tx, lockMode, (long)timeout.TotalMilliseconds, cancellationToken);
            if (result == AcquireResult.Denied)
            {
                throw new TimeoutException();
            }

            return result;
        }

        /// <summary>
        /// Try to acquire the lock in the specified timeout.
        /// If the lock was acquired because it was newly Acquired, or already Owned by the transaction, then it retuns the result.
        /// If the lock was not acquired in the specitied timeout then Denied is returned.
        /// </summary>
        /// <param name="tx">Transaction</param>
        /// <param name="lockMode">Lock Mode</param>
        /// <param name="milliseconds">Timeout Milliseconds</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>{Acquired|Denied|Owned}</returns>
        public async Task<AcquireResult> Acquire(ITransaction tx, LockMode lockMode, long milliseconds, CancellationToken cancellationToken)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            while (true)
            {
                var result = TryAcquire(tx, lockMode);
                if (result != AcquireResult.Denied)
                    return result;

                bool keepWaiting = await Wait((int)(milliseconds - sw.ElapsedMilliseconds), cancellationToken);
                if (!keepWaiting)
                {
                    return AcquireResult.Denied;
                }
            }
        }

        /// <summary>
        /// Try to acquire the lock.
        /// </summary>
        /// <param name="tx">Transaction</param>
        /// <param name="lockMode">Lock Mode</param>
        /// <returns>{Acquired|Denied|Owned}</returns>
        public AcquireResult TryAcquire(ITransaction tx, LockMode lockMode)
        {
            lock (_lockOwners)
            {
                var result = AcquireResult.Denied;

                if (_lockOwners.Contains(tx.TransactionId))
                {
                    if (lockMode == LockMode.Default || lockMode == LockMode)
                    {
                        // The tx already owns this lock and the new request is a compatible lock.
                        result = AcquireResult.Owned;
                    }
                    else if (_lockOwners.Count == 1)
                    {
                        // The request is a lock upgrade and the tx is the only lock owner, so upgrade it.
                        LockMode = lockMode;
                        result = AcquireResult.Owned;
                    }
                }
                else
                {
                    if (_lockOwners.Count == 0 || LockMode == LockMode.Default && lockMode == LockMode.Default)
                    {
                        // The requested lock is compatible or there are no current lock holders, so acquire the lock
                        LockMode = lockMode;
                        _lockOwners.Add(tx.TransactionId);
                        result = AcquireResult.Acquired;
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Wait until:
        ///   * Cancel is called on the lock's TokenSource, returns true.
        ///   * The caller's CancellationToken is cancelled, throws OperationCancelledException.
        ///   * The timout delay has elapsed, returns false.
        /// </summary>
        /// <param name="milliseconds">Timeout delay in milliseconds</param>
        /// <param name="cancellationToken">Caller's Cancellation Token</param>
        /// <returns></returns>
        private async Task<bool> Wait(int milliseconds, CancellationToken cancellationToken)
        {
            if (milliseconds > 0)
            {
                lock (_lockOwners)
                {
                    if (TokenSource == null)
                    {
                        TokenSource = new CancellationTokenSource();
                    }
                }

                var token = TokenSource.Token;

                CancellationTokenSource linkedTokenSource = null;
                try
                {
                    if (cancellationToken != default(CancellationToken))
                    {
                        linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, token);
                        token = linkedTokenSource.Token;
                    }

                    await Task.Delay(milliseconds, token);
                }
                catch (OperationCanceledException)
                {
                    // If the caller's cancellation token is cancelled then throw
                    cancellationToken.ThrowIfCancellationRequested();

                    // Otherwise the lock's cancellation token is cancelled, so the lock is available.
                    return true;
                }
                finally
                {
                    linkedTokenSource?.Dispose();
                }
            }

            return false;
        }
    }
}
