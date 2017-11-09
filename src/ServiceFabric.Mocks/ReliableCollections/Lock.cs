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

    /// <summary>
    /// Implements a primitive lock to be used to controll access to other objects.
    /// A lock owner is identified by a long, In this case ITransaction.TransactionId.
    /// </summary>
    public class Lock
    {
        private HashSet<long> _lockOwners = new HashSet<long>();
        public CancellationTokenSource TokenSource { get; private set; }

        /// <summary>
        /// The default timeout if default(TimeSpan) is specified.
        /// </summary>
        public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(4);

        /// <summary>
        /// Downgrade the lock to the Default LockMode if the transaction is the only owner.
        /// If the lock was downgraded and there is a non-null TokenSource than cancel it to free other
        /// threads that may be waiting for the lock.
        /// </summary>
        /// <param name="tx">Transaction</param>
        /// <returns>true if the LockMode is now Default, false otherwise</returns>
        public bool Downgrade(ITransaction tx)
        {
            bool result = true;
            if (LockMode != LockMode.Default)
            {
                lock (_lockOwners)
                {
                    if (_lockOwners.Contains(tx.TransactionId) && _lockOwners.Count == 1)
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

        /// <summary>
        /// Get the current lock mode.
        /// </summary>
        public LockMode LockMode { get; private set; }

        /// <summary>
        /// Release the specified transaction from the lock.
        /// If the lock now has no owners and there is a non-null TokenSource than cancel it to free other
        /// threads that may be waiting for the lock.
        /// </summary>
        /// <param name="tx">Transaction</param>
        /// <returns></returns>
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
