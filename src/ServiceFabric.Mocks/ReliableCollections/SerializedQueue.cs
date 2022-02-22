using System.Collections;
using System.Collections.Generic;

namespace ServiceFabric.Mocks.ReliableCollections
{
    /// <summary>
    /// Queue with serializer support.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class SerializedQueue<TValue> : IEnumerable<TValue>
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
            return _valueSerializer.Deserialize(serializedValue);
        }

        /// <summary>
        /// Add to back of queue
        /// </summary>
        /// <param name="value"></param>
        public void Enqueue(TValue value)
        {
            _inner.Add(_valueSerializer.Serialize(value));
        }

        /// <summary>
        /// Add to front of queue
        /// </summary>
        /// <param name="value"></param>
        public void Push(TValue value)
        {
            _inner.Insert(0, _valueSerializer.Serialize(value));
        }

        /// <inheritdoc />
        public IEnumerator<TValue> GetEnumerator()
        {
            foreach (var item in _inner)
            {
                yield return _valueSerializer.Deserialize(item);
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
            return _valueSerializer.Deserialize(serializedValue);
        }

        /// <summary>
        /// Removes any item from the queue
        /// </summary>
        /// <param name="item"></param>
        public void Remove(TValue item)
        {
            _inner.Remove(_valueSerializer.Serialize(item));
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
