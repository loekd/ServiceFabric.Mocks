using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;

namespace ServiceFabric.Mocks
{
    /// <summary>
    /// Preview feature. Not for production use. See the <see href="https://aka.ms/reliableconcurrentqueuepreview">documentation</see> for details.
    /// Represents a reliable collection of persisted, replicated values with best-effort first-in first-out ordering.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MockReliableConcurrentQueue<T> : IReliableConcurrentQueue<T>
    {
        private readonly ConcurrentQueue<T> _state = new ConcurrentQueue<T>();

        public Uri Name { get;  set; }

        public long Count => _state.Count;

        public MockReliableConcurrentQueue() 
            : this(new Uri("fabric:/MockConcurrentQueue"))
        {
        }

        public MockReliableConcurrentQueue(Uri name)
        {
            Name = name;
        }

        public Task EnqueueAsync(ITransaction tx, T value, CancellationToken cancellationToken = new CancellationToken(),
            TimeSpan? timeout = null)
        {
            _state.Enqueue(value);
            return Task.FromResult(true);
        }

       
	    public Task<ConditionalValue<T>> TryDequeueAsync(ITransaction tx, CancellationToken cancellationToken = new CancellationToken(),
		    TimeSpan? timeout = null)
	    {
			T result;
		    bool dequeued = _state.TryDequeue(out result);
			var conditional = dequeued ? new ConditionalValue<T>(true, result) : new ConditionalValue<T>();
		    return Task.FromResult(conditional);

		}
	}
}