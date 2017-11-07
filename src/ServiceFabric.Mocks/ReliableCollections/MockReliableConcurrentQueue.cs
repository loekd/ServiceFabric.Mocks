namespace ServiceFabric.Mocks.ReliableCollections
{
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Collections;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class MockReliableConcurrentQueue<T> : ReliableCollection, IReliableConcurrentQueue<T>
    {
        private readonly List<T> _queue = new List<T>();

        public MockReliableConcurrentQueue(Uri uri)
            : base(uri)
        { }

        public override void ReleaseLocks(ITransaction tx)
        { }

        public long Count => _queue.Count;

        public override Task ClearAsync()
        {
            lock (_queue)
            {
                _queue.Clear();
                return Task.FromResult(true);
            }
        }

        public Task EnqueueAsync(ITransaction tx, T value, CancellationToken cancellationToken = default(CancellationToken), TimeSpan? timeout = null)
        {
            BeginTransaction(tx);
            AddCommitAction(tx, () => { lock (_queue) { _queue.Add(value); } return true; });

            return Task.FromResult(true);
        }

        public override Task<long> GetCountAsync(ITransaction tx)
        {
            lock (_queue)
            {
                return Task.FromResult((long)_queue.Count);
            }
        }

        public Task<ConditionalValue<T>> TryDequeueAsync(ITransaction tx, CancellationToken cancellationToken = default(CancellationToken), TimeSpan? timeout = null)
        {
            BeginTransaction(tx);

            lock (_queue)
            {
                if (_queue.Count > 0)
                {
                    T value = _queue[0];
                    _queue.RemoveAt(0);
                    AddAbortAction(tx, () => { lock (_queue) { _queue.Insert(0, value); } return true; });

                    return Task.FromResult(new ConditionalValue<T>(true, value));
                }

                return Task.FromResult(new ConditionalValue<T>());
            }
        }
    }
}
