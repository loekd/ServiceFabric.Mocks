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
        public CancellationTokenSource TokenSource;

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
                        TokenSource?.Cancel();
                        TokenSource = null;
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
                    TokenSource?.Cancel();
                    TokenSource = null;
                }
            }

            return result;
        }
        #endregion

        public async Task<AcquireResult> Acquire(ITransaction tx, LockMode lockMode, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            Stopwatch sw = new Stopwatch();

            if (timeout == default(TimeSpan))
            {
                timeout = TimeSpan.FromSeconds(4);
            }

            while (true)
            {
                var result = TryAcquire(tx, lockMode);
                if (result != AcquireResult.Denied)
                    return result;

                await Wait((int)((timeout.Ticks - sw.ElapsedTicks) / TimeSpan.TicksPerMillisecond), cancellationToken);
            }
        }

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
                        // The request lock is compatible or there are no current lock holders, so acquire the lock
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
        ///   * Cancel is called on the lock's TokenSource.
        ///   * The caller's CancellationToken is cancelled.
        ///   * The timout delay has elapsed.
        /// </summary>
        /// <param name="milliseconds">Timeout delay in milliseconds</param>
        /// <param name="cancellationToken">Caller's Cancellation Token</param>
        /// <returns></returns>
        public async Task Wait(int milliseconds, CancellationToken cancellationToken)
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
                    cancellationToken.ThrowIfCancellationRequested();

                    return;
                }
                finally
                {
                    linkedTokenSource?.Dispose();
                }
            }

            throw new TimeoutException();
        }
    }
}
