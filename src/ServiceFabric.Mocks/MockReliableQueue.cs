using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;

namespace ServiceFabric.Mocks
{
    /// <summary>
    /// Represents a reliable first-in, first-out collection of objects that are persisted and replicated.
    /// </summary>
    /// <typeparam name="T">The type of the elements contained in the reliable queue.</typeparam>
    /// <remarks>Values stored in this queue MUST NOT be mutated outside the context of an operation on the queue. It is
    ///  highly recommended to make <typeparamref name="T" /> immutable in order to avoid accidental data corruption.
    /// </remarks>
    public class MockReliableQueue<T> : IReliableQueue<T>
    {
        private readonly ConcurrentQueue<T> _state = new ConcurrentQueue<T>();

        public MockReliableQueue() 
            : this(new Uri("fabric:/MockQueue"))
        {
        }

        public MockReliableQueue(Uri name)
        {
            Name = name;
        }

        public Uri Name { get; set; }
        public Task ClearAsync()
        {
            while (!_state.IsEmpty)
            {
                T result;
                _state.TryDequeue(out result);
            }

            return Task.FromResult(true);
        }

        public Task<IAsyncEnumerable<T>> CreateEnumerableAsync(ITransaction tx)
        {
            var enumerable = new MockAsyncEnumerable<T>(_state);
            return Task.FromResult<IAsyncEnumerable<T>>(enumerable);
        }

        public Task EnqueueAsync(ITransaction tx, T item, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return EnqueueAsync(tx, item);
        }

        public Task EnqueueAsync(ITransaction tx, T item)
        {
            _state.Enqueue(item);
            return Task.FromResult(true);
        }

        public Task<long> GetCountAsync()
        {
            return Task.FromResult((long)_state.Count);
        }

        public Task<long> GetCountAsync(ITransaction tx)
        {
            return GetCountAsync();
        }

        public Task<ConditionalValue<T>> TryDequeueAsync(ITransaction tx, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return TryDequeueAsync(tx);
        }

        public Task<ConditionalValue<T>> TryDequeueAsync(ITransaction tx)
        {
            T value;
            bool result = _state.TryDequeue(out value);
            return Task.FromResult(new ConditionalValue<T>(result, value));
        }

        public Task<ConditionalValue<T>> TryPeekAsync(ITransaction tx, LockMode lockMode, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return TryPeekAsync(tx);
        }

        public Task<ConditionalValue<T>> TryPeekAsync(ITransaction tx, LockMode lockMode)
        {
            return TryPeekAsync(tx);
        }

        public Task<ConditionalValue<T>> TryPeekAsync(ITransaction tx, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return TryPeekAsync(tx);
        }

        public Task<ConditionalValue<T>> TryPeekAsync(ITransaction tx)
        {
            T value;
            bool result = _state.TryPeek(out value);
            return Task.FromResult(new ConditionalValue<T>(result, value));
        }
    }
}