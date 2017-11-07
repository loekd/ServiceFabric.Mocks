namespace ServiceFabric.Mocks.ReliableCollections
{
    using Microsoft.ServiceFabric.Data;
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;

    public abstract class ReliableCollection
    {
        private ConcurrentDictionary<long, ConcurrentStack<Func<bool>>> _abortActions = new ConcurrentDictionary<long, ConcurrentStack<Func<bool>>>();
        private ConcurrentDictionary<long, ConcurrentQueue<Func<bool>>> _commitActions = new ConcurrentDictionary<long, ConcurrentQueue<Func<bool>>>();

        protected ReliableCollection(Uri uri)
        {
            Name = uri;
        }

        protected void AddAbortAction(ITransaction tx, Func<bool> action)
        {
            var actions = _abortActions.GetOrAdd(tx.TransactionId, (k) => new ConcurrentStack<Func<bool>>());
            actions.Push(action);
        }

        protected void AddCommitAction(ITransaction tx, Func<bool> action)
        {
            var actions = _commitActions.GetOrAdd(tx.TransactionId, (k) => new ConcurrentQueue<Func<bool>>());
            actions.Enqueue(action);
        }

        protected MockTransaction BeginTransaction(ITransaction tx)
        {
            MockTransaction mtx = tx as MockTransaction;

            if (mtx != null)
            {
                mtx.TryAddReliableCollection(this);
                return mtx;
            }

            throw new ArgumentException("Must be a non-null MockTransaction", nameof(tx));
        }

        internal void EndTransaction(ITransaction tx, bool isCommit)
        {
            if (_abortActions.TryRemove(tx.TransactionId, out var abortActions) && !isCommit)
            {
                foreach (var action in abortActions)
                {
                    action();
                }
            }

            if (_commitActions.TryRemove(tx.TransactionId, out var commitActions) && isCommit)
            {
                foreach (var action in commitActions)
                {
                    action();
                }
            }
        }

        public abstract void ReleaseLocks(ITransaction tx);

        public Uri Name { get; private set; }

        public abstract Task ClearAsync();

        public abstract Task<long> GetCountAsync(ITransaction tx);
    }
}