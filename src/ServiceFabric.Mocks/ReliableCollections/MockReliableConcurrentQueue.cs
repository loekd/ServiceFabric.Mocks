namespace ServiceFabric.Mocks.ReliableCollections
{
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Collections;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Implements IReliableConcurrentQueue
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MockReliableConcurrentQueue<T> : TransactedCollection, IReliableConcurrentQueue<T>
    {
        private List<T> _queue = new List<T>();
        private long _queueEmptyTransactionId = -1;
        private Lock<long> _queueEmptyLock = new Lock<long>();

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="uri">Uri</param>
        public MockReliableConcurrentQueue(Uri uri)
            : base(uri)
        { }

        /// <summary>
        /// Release any locks the transaction may have on _queueEmptyLock.
        /// </summary>
        /// <param name="tx"></param>
        public override void ReleaseLocks(ITransaction tx)
        {
            _queueEmptyLock.Release(tx.TransactionId);
        }

        public long Count => _queue.Count;

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

            AddCommitAction(tx, () => OnCommit(tx, value));

            return Task.FromResult(true);
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

            long totalMilliseconds = (long)(timeout ?? Constants.DefaultTimeout).TotalMilliseconds;
            Stopwatch sw = new Stopwatch();
            sw.Start();

            // Try to get committed item from the queue
            for (long milliseconds = totalMilliseconds; milliseconds > 0; milliseconds = totalMilliseconds - sw.ElapsedMilliseconds)
            {
                if (AcquireResult.Denied != await _queueEmptyLock.Acquire(tx.TransactionId, LockMode.Default, milliseconds, cancellationToken))
                {
                    _queueEmptyLock.Release(tx.TransactionId);
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
                            for (byte i = 1; AcquireResult.Denied == _queueEmptyLock.TryAcquire(_queueEmptyTransactionId, LockMode.Update); i++)
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

        /// <summary>
        /// Enqueue any items that were enqueued in the transaction. Release _queueEmptyLock if any items were enqueued to unblock any
        /// threads that are waiting to dequeue an item.
        /// </summary>
        /// <param name="tx"></param>
        /// <returns></returns>
        private bool OnCommit(ITransaction tx, T value)
        {
            lock (_queue)
            {
                _queue.Add(value);
                _queueEmptyLock.Release(_queueEmptyTransactionId);
            }

            return true;
        }

        /// <summary>
        /// Re-enqueue any items that were dequeued by the transaction. Release _queueEmptyLock if any items were enqueued to unblock any
        /// threads that are waiting to dequeue an item.
        /// </summary>
        /// <param name="tx"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool OnAbort(ITransaction tx, T value)
        {
            lock (_queue)
            {
                _queue.Insert(0, value);
                _queueEmptyLock.Release(_queueEmptyTransactionId);
            }

            return true;
        }
    }
}
