namespace ServiceFabric.Mocks.ReliableCollections
{
    using Microsoft.ServiceFabric.Data;
    using System;
    using System.Collections.Concurrent;

    /// <summary>
    /// Implements a subset of IReliableCollection that is common to IReliableConcurrentQueue, IReliableDictionary, and IReliableQueue.
    /// </summary>
    public abstract class TransactedCollection
    {
        private ConcurrentDictionary<long, ConcurrentStack<Func<bool>>> _abortActions = new ConcurrentDictionary<long, ConcurrentStack<Func<bool>>>();
        private ConcurrentDictionary<long, ConcurrentQueue<Func<bool>>> _commitActions = new ConcurrentDictionary<long, ConcurrentQueue<Func<bool>>>();

        protected TransactedCollection(Uri uri)
        {
            Name = uri;
        }

        /// <summary>
        /// Add an abort action to be executed if the transaction is aboretd.
        /// </summary>
        /// <param name="tx">Transaction</param>
        /// <param name="action">Action</param>
        protected void AddAbortAction(ITransaction tx, Func<bool> action)
        {
            var actions = _abortActions.GetOrAdd(tx.TransactionId, (k) => new ConcurrentStack<Func<bool>>());
            actions.Push(action);
        }

        /// <summary>
        /// Add a commit action to be executed if the transaction is committed.
        /// </summary>
        /// <param name="tx">Transaction</param>
        /// <param name="action">Action</param>
        protected void AddCommitAction(ITransaction tx, Func<bool> action)
        {
            var actions = _commitActions.GetOrAdd(tx.TransactionId, (k) => new ConcurrentQueue<Func<bool>>());
            actions.Enqueue(action);
        }

        /// <summary>
        /// Any operation on a TransactedCollection that has an abort or commit action needs to be added to the MockTransaction
        /// so the actions can be executed in EndTransaction().
        /// </summary>
        /// <param name="tx">MockTransaction</param>
        /// <returns></returns>
        protected MockTransaction BeginTransaction(ITransaction tx)
        {
            MockTransaction mtx = tx as MockTransaction;

            if (mtx != null)
            {
                mtx.TryAddTransactedCollection(this);
                return mtx;
            }

            throw new ArgumentException("Must be a non-null MockTransaction", nameof(tx));
        }

        /// <summary>
        /// Execute the commit if isCommit is true, or abort actions if isCommit is false.
        /// </summary>
        /// <param name="tx">Transaction</param>
        /// <param name="isCommit">Is Commit?</param>
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

        /// <summary>
        /// Release any locks in the TransactedCollection owned by the transaction.
        /// </summary>
        /// <param name="tx">Transaction</param>
        public abstract void ReleaseLocks(ITransaction tx);

        /// <summary>
        /// Get the name.
        /// </summary>
        public Uri Name { get; private set; }
    }
}