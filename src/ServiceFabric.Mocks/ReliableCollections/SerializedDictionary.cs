using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ServiceFabric.Mocks.ReliableCollections
{
    /// <summary>
    /// Concurrent dictionary with serializer support.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class SerializedDictionary<TKey, TValue> : IConcurrentDictionary<TKey, TValue>
    {
        private readonly ConcurrentDictionary<object, object> _inner = new ConcurrentDictionary<object, object>();
        private readonly Microsoft.ServiceFabric.Data.IStateSerializer<TKey> _keySerializer;
        private readonly Microsoft.ServiceFabric.Data.IStateSerializer<TValue> _valueSerializer;

        /// <summary>
        /// Creates a new instance using the provided collection of <see cref="Microsoft.ServiceFabric.Data.IStateSerializer{T}"/>.
        /// </summary>
        /// <param name="serializers"></param>
        public SerializedDictionary(SerializerCollection serializers = null)
        {
            if (serializers == null) return;
            serializers.TryGetSerializer(out _keySerializer);
            serializers.TryGetSerializer(out _valueSerializer);
        }

        /// <inheritdoc />
        public TValue this[TKey key]
        {
            get => _valueSerializer.Deserialize(_inner[_keySerializer.Serialize(key)]);
            set => _inner[_keySerializer.Serialize(key)] = _valueSerializer.Serialize(value);
        }
        /// <inheritdoc />
        public bool IsReadOnly => false;
        /// <inheritdoc />
        public int Count => _inner.Count;
        /// <inheritdoc />
        ICollection<TKey> IDictionary<TKey, TValue>.Keys => _inner.Keys.Select(k => _keySerializer.Deserialize(k)).ToList();
        /// <inheritdoc />
        ICollection<TValue> IDictionary<TKey, TValue>.Values => _inner.Values.Select(v => _valueSerializer.Deserialize(v)).ToList();
        /// <inheritdoc />
        public void Add(TKey key, TValue value)
        {
            var serializedKey = _keySerializer.Serialize(key);
            _inner.TryAdd(serializedKey, _valueSerializer.Serialize(value));
        }
        /// <inheritdoc />
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            var serializedKey = _keySerializer.Serialize(item.Key);
            _inner.TryAdd(serializedKey, _valueSerializer.Serialize(item.Value));
        }
        /// <inheritdoc />
        public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
        {
            var serializedKey = _keySerializer.Serialize(key);
            if (_inner.TryGetValue(serializedKey, out var existingSerializedValue))
            {

                TValue newValue = updateValueFactory(key, _valueSerializer.Deserialize(existingSerializedValue));
                _inner.TryUpdate(serializedKey, _valueSerializer.Serialize(newValue), existingSerializedValue);
                return newValue;
            }
            else
            {
                TValue newValue = addValueFactory(key);
                _inner.TryAdd(serializedKey, _valueSerializer.Serialize(newValue));
                return newValue;
            }
        }
        /// <inheritdoc />
        public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
        {
            return AddOrUpdate(key, k => addValue, updateValueFactory);
        }
        /// <inheritdoc />
        public void Clear()
        {
            _inner.Clear();
        }
        /// <inheritdoc />
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _inner.Contains(SerializeKeyValuePair(item));
        }
        /// <inheritdoc />
        public bool ContainsKey(TKey key)
        {
            return _inner.ContainsKey(_keySerializer.Serialize(key));
        }
        /// <inheritdoc />
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc />
        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<TKey, TValue>>)this).GetEnumerator();
        }
        /// <inheritdoc />
        public bool Remove(TKey key)
        {
            return ((IDictionary<object, object>)_inner).Remove(_keySerializer.Serialize(key));
        }
        /// <inheritdoc />
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<object, object>>)_inner).Remove(SerializeKeyValuePair(item));
        }
        /// <inheritdoc />
        public bool TryAdd(TKey key, TValue value)
        {
            return _inner.TryAdd(_keySerializer.Serialize(key), _valueSerializer.Serialize(value));
        }
        /// <inheritdoc />
        public bool TryGetValue(TKey key, out TValue value)
        {
            bool ok = _inner.TryGetValue(_keySerializer.Serialize(key), out var serializedValue);
            if (ok)
            {
                value = _valueSerializer.Deserialize(serializedValue);
            }
            else
            {
                value = default;
            }
            return ok;
        }
        /// <inheritdoc />
        public bool TryRemove(TKey key, out TValue value)
        {
            bool ok = _inner.TryRemove(_keySerializer.Serialize(key), out var serializedValue);
            if (ok)
            {
                value = _valueSerializer.Deserialize(serializedValue);
            }
            else
            {
                value = default;
            }
            return ok;
        }
        /// <inheritdoc />
        public bool TryUpdate(TKey key, TValue newValue, TValue comparisonValue)
        {
            return _inner.TryUpdate(_keySerializer.Serialize(key), _valueSerializer.Serialize(newValue), _valueSerializer.Serialize(comparisonValue));
        }
        /// <inheritdoc />
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            foreach (var item in _inner)
            {
                yield return DeserializeKeyValuePair(item);
            }
        }

        private KeyValuePair<object, object> SerializeKeyValuePair(KeyValuePair<TKey, TValue> item)
        {
            return new KeyValuePair<object, object>(_keySerializer.Serialize(item.Key), _valueSerializer.Serialize(item.Value));
        }

        private KeyValuePair<TKey, TValue> DeserializeKeyValuePair(KeyValuePair<object, object> item)
        {
            return new KeyValuePair<TKey, TValue>(_keySerializer.Deserialize(item.Key), _valueSerializer.Deserialize(item.Value));
        }
    }

}
