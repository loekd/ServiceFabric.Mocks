namespace ServiceFabric.Mocks.ReliableCollections
{
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Collections;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    public class MockReliableConcurrentQueue<T> : ReliableCollection, IReliableConcurrentQueue<T>
    {
        private List<T> _queue = new List<T>();
        private Dictionary<long, Queue<T>> _pendingEnqueueItems = new Dictionary<long, Queue<T>>();
        ITransaction _queueEmptyTransaction = new MockTransaction(null, -1);
        Lock _queueEmptyLock = new Lock();

        public MockReliableConcurrentQueue(Uri uri)
            : base(uri)
        { }

        public override void ReleaseLocks(ITransaction tx)
        {
            _queueEmptyLock.Release(tx);
        }

        public long Count => _queue.Count;

        public override Task ClearAsync()
        {
            lock (_queue)
            {
                _queue.Clear();
                return Task.FromResult(true);
            }
        }

        /// <summary>
        /// Enqueue value into the queue.
        /// </summary>
        /// <param name="tx">Transaction</param>
        /// <param name="value">Value</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <param name="timeout">Timeout</param>
        public Task EnqueueAsync(ITransaction tx, T value, CancellationToken cancellationToken = default(CancellationToken), TimeSpan? timeout = null)
        {
            BeginTransaction(tx);

            Queue<T> queue;
            lock (_pendingEnqueueItems)
            {
                if (!_pendingEnqueueItems.ContainsKey(tx.TransactionId))
                {
                    queue = new Queue<T>();
                    _pendingEnqueueItems.Add(tx.TransactionId, queue);
                    AddCommitAction(tx, () => OnCommit(tx));
                }
                else
                {
                    queue = _pendingEnqueueItems[tx.TransactionId];
                }

                Monitor.Enter(queue);
            }

            queue.Enqueue(value);
            Monitor.Exit(queue);

            return Task.FromResult(true);
        }

        /// <summary>
        /// Get the count of items in the queue.
        /// </summary>
        /// <remarks>
        /// This count may not be accurate. Transactions see their own enqueued item count, but do see uncommitted transactions' dequeue count.
        /// </remarks>
        /// <param name="tx">Transaction</param>
        /// <returns>Current count of items in the queue</returns>
        public override Task<long> GetCountAsync(ITransaction tx)
        {
            lock (_pendingEnqueueItems)
            {
                long count = Count;
                if (_pendingEnqueueItems.TryGetValue(tx.TransactionId, out Queue<T> queue))
                {
                    count += queue.Count;
                }

                return Task.FromResult(count);
            }
        }

        /// <summary>
        /// Try to dequeue the next available item in the queue. Try to wait specified timtout if the queue is empty.
        /// </summary>
        /// <param name="tx">Transaction</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <param name="timeout">Timeout</param>
        /// <returns></returns>
        public async Task<ConditionalValue<T>> TryDequeueAsync(ITransaction tx, CancellationToken cancellationToken = default(CancellationToken), TimeSpan? timeout = null)
        {
            BeginTransaction(tx);

            long totalMilliseconds = (long)(timeout ?? Lock.DefaultTimeout).TotalMilliseconds;
            Stopwatch sw = new Stopwatch();
            sw.Start();

            // Try to get an uncommitted item on the transaction
            Queue<T> queue;
            lock (_pendingEnqueueItems)
            {
                if (_pendingEnqueueItems.TryGetValue(tx.TransactionId, out queue))
                {
                    Monitor.Enter(queue);
                }
            }

            if (queue != null)
            {
                ConditionalValue<T> value = queue.Count > 0 ? new ConditionalValue<T>(true, queue.Dequeue()) : default(ConditionalValue<T>);
                Monitor.Exit(queue);
                if (value.HasValue)
                {
                    return value;
                }
            }

            // Try to get committed item from the queue
            for (long milliseconds = totalMilliseconds; milliseconds > 0; milliseconds = totalMilliseconds - sw.ElapsedMilliseconds)
            {
                if (AcquireResult.Denied != await _queueEmptyLock.Acquire(tx, LockMode.Default, milliseconds, cancellationToken))
                {
                    _queueEmptyLock.Release(tx);
                    lock (_queue)
                    {
                        if (_queue.Count > 0)
                        {
                            T value = _queue[0];
                            _queue.RemoveAt(0);
                            AddAbortAction(tx, () => OnAbort(tx, value));

                            return new ConditionalValue<T>(true, value);
                        }
                        else
                        {
                            // We acquired the Default Lock on _queueEmptyLock, but then found the queue was empty. We could just return
                            // an empty ConditionalValue, but we're supposed to wait the specified timout before doing that. Let's try again...
                            //
                            // At this point there may be many threads that think they have the (big L) Lock, but only we have the (little L) lock.
                            // All the threads (includeing us) released our Lock before trying to acquire the lock.
                            //
                            // So, let's acquire the Update lock on Lock. Ideally, this would just succeed, but there may be other threads acquiring
                            // Default locks who snuck in, but have not released it yet. So, we'll spin until we actually get it. 
                            for (uint i = 1; AcquireResult.Denied == _queueEmptyLock.TryAcquire(_queueEmptyTransaction, LockMode.Update); i++)
                            {
                                // Should I yield while spinning? If I don't yield then I risk a thread deadlock if every physical thread is also in
                                // a spin lock/tight loop waiting for something to be tirggered on an async thread that can't get scheduled since all
                                // the physical threads are busy and not cooperating. If I do yield then I'm increasing competition for the Lock I
                                // want to own.
                                //
                                // Let's yield every now and then, just in case...
                                if (0 == (i % 128))
                                {
                                    Task.Yield();
                                }
                            }
                        }
                    }
                }
            }

            return default(ConditionalValue<T>);
        }

        private bool OnCommit(ITransaction tx)
        {
            Queue<T> queue = null;
            lock(_pendingEnqueueItems)
            {
                if (_pendingEnqueueItems.TryGetValue(tx.TransactionId, out queue))
                {
                    _pendingEnqueueItems.Remove(tx.TransactionId);
                }
            }

            lock(_queue)
            {
                while (queue.Count > 0)
                {
                    _queue.Add(queue.Dequeue());
                }
            }

            return true;
        }

        private bool OnAbort(ITransaction tx, T value)
        {
            lock (_pendingEnqueueItems)
            {
                _pendingEnqueueItems.Remove(tx.TransactionId);
            }

            lock (_queue)
            {
                _queue.Insert(0, value);
                _queueEmptyLock.Release(_queueEmptyTransaction);
            }

            return true;
        }
    }
}
