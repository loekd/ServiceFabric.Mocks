namespace ServiceFabric.Mocks
{
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Data;
    using System.Collections.Concurrent;
    using System;

    /// <summary>
    /// A sequence of operations performed as a single logical unit of work.
    /// </summary>
    public class MockTransaction : ITransaction
    {
        private MockReliableStateManager _stateManager;
        private ConcurrentDictionary<Uri, ReliableCollections.TransactedCollection> _transactedCollections = new ConcurrentDictionary<Uri, ReliableCollections.TransactedCollection>();

        public bool TryAddTransactedCollection(ReliableCollections.TransactedCollection collection)
        {
            return _transactedCollections.TryAdd(collection.Name, collection);
        }

        public long CommitSequenceNumber => 0L;

        public bool IsCommitted { get; private set; }

        public bool IsAborted { get; private set; }

        public bool IsCompleted => IsCommitted || IsAborted;

        public long TransactionId { get; private set; }

        public MockTransaction(MockReliableStateManager stateManager, long transactionId)
        {
            _stateManager = stateManager;
            TransactionId = transactionId;
        }

        public void Abort()
        {
            if (IsCompleted)
            {
                throw new InvalidOperationException();
            }

            foreach (var collection in _transactedCollections.Values)
            {
                collection.EndTransaction(this, false);
                collection.ReleaseLocks(this);
            }

            IsAborted = true;
            _stateManager?.OnTransactionChanged(this, false);
        }

        public Task CommitAsync()
        {
            if (IsCompleted)
            {
                throw new InvalidOperationException();
            }

            foreach (var collection in _transactedCollections.Values)
            {
                collection.EndTransaction(this, true);
                collection.ReleaseLocks(this);
            }

            IsCommitted = true;
            _stateManager?.OnTransactionChanged(this, true);

            return Task.FromResult(true);
        }

        public void Dispose()
        {
            if (!IsCompleted)
            {
                Abort();
            }
        }

        public Task<long> GetVisibilitySequenceNumberAsync()
        {
            return Task.FromResult(0L);
        }
    }
}