using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Data;

namespace ServiceFabric.Mocks
{
    /// <summary>
    /// Represents the interface that state manager for <see cref="T:Microsoft.ServiceFabric.Actors.Runtime.Actor" /> implements.
    /// </summary>
    public class MockActorStateManager : IActorStateManager
    {
        private readonly ConcurrentDictionary<string, object> _state = new ConcurrentDictionary<string, object>();

        /// <inheritdoc />
        public Task<T> AddOrUpdateStateAsync<T>(string stateName, T addValue, Func<string, T, T> updateValueFactory, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = (T)_state.AddOrUpdate(stateName, addValue, (key, current) => updateValueFactory(key, (T) current));
            return Task.FromResult(result);
        }
        /// <inheritdoc />
        public Task AddStateAsync<T>(string stateName, T value, CancellationToken cancellationToken = default(CancellationToken))
        {
	        if (!_state.TryAdd(stateName, value))
	        {
		        throw new InvalidOperationException("ActorStateAlreadyExists");
	        }
	        return Task.FromResult(true);
        }

        /// <summary>
        /// Not implemented!
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task ClearCacheAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task SaveStateAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            //State is always applied immediately.
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<bool> ContainsStateAsync(string stateName, CancellationToken cancellationToken = default(CancellationToken))
        {
            bool result = _state.ContainsKey(stateName);
            return Task.FromResult(result);
        }

        /// <inheritdoc />
        public Task<T> GetOrAddStateAsync<T>(string stateName, T value, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = (T)_state.GetOrAdd(stateName, value);
            return Task.FromResult(result);
        }

        /// <inheritdoc />
        public Task<T> GetStateAsync<T>(string stateName, CancellationToken cancellationToken = default(CancellationToken))
        {
            object value;
            _state.TryGetValue(stateName, out value);
            return Task.FromResult((T)value);
        }

        /// <inheritdoc />
        public Task<IEnumerable<string>> GetStateNamesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(_state.Keys.AsEnumerable());
        }

        /// <inheritdoc />
        public Task RemoveStateAsync(string stateName, CancellationToken cancellationToken = default(CancellationToken))
        {
            object result;
            return Task.FromResult(_state.TryRemove(stateName, out result));
        }

        /// <inheritdoc />
        public async Task SetStateAsync<T>(string stateName, T value, CancellationToken cancellationToken = default(CancellationToken))
        {
            await RemoveStateAsync(stateName, cancellationToken);
            await AddStateAsync(stateName, value, cancellationToken);
        }

        /// <inheritdoc />
        public Task<bool> TryAddStateAsync<T>(string stateName, T value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(_state.TryAdd(stateName, value));
        }

        /// <inheritdoc />
        public Task<ConditionalValue<T>> TryGetStateAsync<T>(string stateName, CancellationToken cancellationToken = default(CancellationToken))
        {
            object value;
	        if (_state.TryGetValue(stateName, out value))
	        {
		        return Task.FromResult(new ConditionalValue<T>(true, (T) value));
	        }
	        return Task.FromResult(new ConditionalValue<T>());

        }

        /// <inheritdoc />
        public Task<bool> TryRemoveStateAsync(string stateName, CancellationToken cancellationToken = default(CancellationToken))
        {
            return (Task<bool>)RemoveStateAsync(stateName, cancellationToken);
        }
    }
}