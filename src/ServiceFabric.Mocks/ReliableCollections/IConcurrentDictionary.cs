using ServiceFabric.Mocks.ReliableCollections;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ServiceFabric.Mocks.ReliableCollections
{
    public interface IConcurrentDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        bool TryAdd(TKey key, TValue value);

        bool TryRemove(TKey key, out TValue value);

        bool TryUpdate(TKey key, TValue newValue, TValue comparisonValue);

        TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory);

        TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory);
    }

    ///// <summary>
    ///// Concurrent dictionary (unmodified).
    ///// </summary>
    ///// <typeparam name="TKey"></typeparam>
    ///// <typeparam name="TValue"></typeparam>
    //public class MockConcurrentDictionary<TKey, TValue> : ConcurrentDictionary<TKey, TValue>, IConcurrentDictionary<TKey, TValue>
    //{ }


    /// <summary>
    /// Concurrent collection for state serializers.
    /// </summary>
    public class SerializerCollection
    {
        /// <summary>
        /// Gets the registered collection of <see cref="Microsoft.ServiceFabric.Data.IStateSerializer{T}"/>
        /// </summary>
        private ConcurrentDictionary<Type, object> Serializers { get; }

        /// <summary>
        /// Creates a new instance using the provided collection of <see cref="Microsoft.ServiceFabric.Data.IStateSerializer{T}"/>
        /// </summary>
        /// <param name="serializers"></param>
        internal SerializerCollection(ConcurrentDictionary<Type, object> serializers = null)
        {
            Serializers = serializers ?? new ConcurrentDictionary<Type, object>();
        }
        /// <summary>
        /// Creates a new default instance.
        /// </summary>
        public SerializerCollection() : this(null){}

        /// <summary>
        /// Registers a serializer for type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializer"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void AddSerializer<T>(Microsoft.ServiceFabric.Data.IStateSerializer<T> serializer)
        {
            if (serializer is null)
            {
                throw new ArgumentNullException(nameof(serializer));
            }

            Serializers.TryAdd(typeof(T), serializer);
        }

        /// <summary>
        /// Unregisters a serializer for type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializer"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void RemoveSerializer<T>(Microsoft.ServiceFabric.Data.IStateSerializer<T> serializer)
        {
            if (serializer is null)
            {
                throw new ArgumentNullException(nameof(serializer));
            }

            Serializers.TryRemove(typeof(T), out var _);
        }

        /// <summary>
        /// Gets a serializer based on the type of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public bool TryGetSerializer<T>(out Microsoft.ServiceFabric.Data.IStateSerializer<T> serializer)
        {
            bool ok = Serializers.TryGetValue(typeof(T), out var value);
            serializer = (Microsoft.ServiceFabric.Data.IStateSerializer<T>)value;
            return ok;
        }
    }

    /// <summary>
    /// Base type for collections that serialize their values.
    /// </summary>
    public abstract class SerializedCollection
    {
        /// <summary>
        /// If the provided value is a stream, a serializer will be used to deserialize the stream into an object.
        /// If the value is anything else, it will be returned unmodified.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        protected T Deserialize<T>(Microsoft.ServiceFabric.Data.IStateSerializer<T> serializer, object value)
        {
            T result = default;

            if (value is null)
            {
                return result;
            }
            if (value is Stream stream)
            {
                //reset stream and deserialize
                stream.Seek(0, SeekOrigin.Begin);
                if (serializer is null)
                {
                    throw new InvalidOperationException($"State value is of type Stream, but no serializer was found. Call 'AddSerializer<T>' for type '{value.GetType().Name}'");
                }
                using (var reader = new BinaryReader(stream, System.Text.Encoding.Unicode, true))
                {
                    result = serializer.Read(reader);
                }

                return result;
            }

            return (T)value;
        }

        /// <summary>
        /// If there is a registered serializer for the provided value, it will be used to create a stream from the value.
        /// If there isn't, the value will be returned unmodified.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        protected object Serialize<T>(Microsoft.ServiceFabric.Data.IStateSerializer<T> serializer, T value)
        {
            if (value == null)
                return null;

            if (serializer is null)
            {
                //return regular value if there is no serializer for this type
                return value;
            }

            //create stream and serialize value
            var stream = new MemoryStream();
            using (var writer = new BinaryWriter(stream, System.Text.Encoding.Unicode, true))
            {
                serializer.Write(value, writer);
                stream.Seek(0, SeekOrigin.Begin);
            }
            return stream;
        }
    }

    /// <summary>
    /// Concurrent dictionary with serializer support.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class SerializedDictionary<TKey, TValue> : SerializedCollection, IConcurrentDictionary<TKey, TValue>
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
            get => Deserialize(_valueSerializer, _inner[Serialize(_keySerializer, key)]);
            set => _inner[Serialize(_keySerializer, key)] = Serialize(_valueSerializer, value);
        }
        /// <inheritdoc />
        public bool IsReadOnly => false;
        /// <inheritdoc />
        public int Count => _inner.Count;
        /// <inheritdoc />
        ICollection<TKey> IDictionary<TKey, TValue>.Keys => _inner.Keys.Select(k => Deserialize(_keySerializer, k)).ToList();
        /// <inheritdoc />
        ICollection<TValue> IDictionary<TKey, TValue>.Values => _inner.Values.Select(v => Deserialize(_valueSerializer, v)).ToList();
        /// <inheritdoc />
        public void Add(TKey key, TValue value)
        {
            var serializedKey = Serialize(_keySerializer, key);
            _inner.TryAdd(serializedKey, Serialize(_valueSerializer, value));
        }
        /// <inheritdoc />
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            var serializedKey = Serialize(_keySerializer, item.Key);
            _inner.TryAdd(serializedKey, Serialize(_valueSerializer, item.Value));
        }
        /// <inheritdoc />
        public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
        {
            var serializedKey = Serialize(_keySerializer, key);
            if (_inner.TryGetValue(serializedKey, out var existingSerializedValue))
            {

                TValue newValue = updateValueFactory(key, Deserialize(_valueSerializer, existingSerializedValue));
                _inner.TryUpdate(serializedKey, Serialize(_valueSerializer, newValue), existingSerializedValue);
                return newValue;
            }
            else
            {
                TValue newValue = addValueFactory(key);
                _inner.TryAdd(serializedKey, Serialize(_valueSerializer, newValue));
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
            return _inner.ContainsKey(Serialize(_keySerializer, key));
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
            return ((IDictionary<object, object>)_inner).Remove(Serialize(_keySerializer, key));
        }
        /// <inheritdoc />
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<object, object>>)_inner).Remove(SerializeKeyValuePair(item));
        }
        /// <inheritdoc />
        public bool TryAdd(TKey key, TValue value)
        {
            return _inner.TryAdd(Serialize(_keySerializer, key), Serialize(_valueSerializer, value));
        }
        /// <inheritdoc />
        public bool TryGetValue(TKey key, out TValue value)
        {
            bool ok = _inner.TryGetValue(Serialize(_keySerializer, key), out var serializedValue);
            if (ok)
            {
                value = Deserialize(_valueSerializer, serializedValue);
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
            bool ok = _inner.TryRemove(Serialize(_keySerializer, key), out var serializedValue);
            if (ok)
            {
                value = Deserialize(_valueSerializer, serializedValue);
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
            return _inner.TryUpdate(Serialize(_keySerializer, key), Serialize(_valueSerializer, newValue), Serialize(_valueSerializer, comparisonValue));
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
            return new KeyValuePair<object, object>(Serialize(_keySerializer, item.Key), Serialize(_valueSerializer, item.Value));
        }

        private KeyValuePair<TKey, TValue> DeserializeKeyValuePair(KeyValuePair<object, object> item)
        {
            return new KeyValuePair<TKey, TValue>(Deserialize(_keySerializer, item.Key), Deserialize(_valueSerializer, item.Value));
        }
    }

    /// <summary>
    /// Queue with serializer support.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class SerializedQueue<TValue> : SerializedCollection, IEnumerable<TValue>
    {
        private readonly ArrayList _inner = new ArrayList();
        private readonly Microsoft.ServiceFabric.Data.IStateSerializer<TValue> _valueSerializer;

        /// <inheritdoc />
        public long Count => _inner.Count;

        /// <summary>
        /// Creates a new instance using the provided collection of <see cref="Microsoft.ServiceFabric.Data.IStateSerializer{T}"/>.
        /// </summary>
        /// <param name="serializers"></param>
        public SerializedQueue(SerializerCollection serializers = null)
        {
            serializers?.TryGetSerializer(out _valueSerializer);
        }

        /// <summary>
        /// Take from front of queue
        /// </summary>
        /// <returns></returns>
        public TValue Dequeue()
        {
            if (Count == 0)
            {
                return default;
            }
            var serializedValue = _inner[0];
            _inner.RemoveAt(0);
            return Deserialize(_valueSerializer, serializedValue);
        }

        /// <summary>
        /// Add to back of queue
        /// </summary>
        /// <param name="value"></param>
        public void Enqueue(TValue value)
        {
            _inner.Add(Serialize(_valueSerializer, value));
        }

        /// <summary>
        /// Add to front of queue
        /// </summary>
        /// <param name="value"></param>
        public void Push(TValue value)
        {
            _inner.Insert(0, Serialize(_valueSerializer, value));
        }

        /// <inheritdoc />
        public IEnumerator<TValue> GetEnumerator()
        {
            foreach (var item in _inner)
            {
                yield return Deserialize<TValue>(_valueSerializer, item);
            }
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Returns item from front of queueu without dequeueing
        /// </summary>
        /// <returns></returns>
        public TValue Peek()
        {
            if (_inner.Count == 0)
            {
                return default;
            }
            var serializedValue = _inner[0];
            return Deserialize(_valueSerializer, serializedValue);
        }

        /// <summary>
        /// Removes any item from the queue
        /// </summary>
        /// <param name="item"></param>
        public void Remove(TValue item)
        {
            _inner.Remove(Serialize(_valueSerializer, item));
        }

        /// <summary>
        /// Clears all items
        /// </summary>
        public void Clear()
        {
            _inner.Clear();
        }
    }

}
