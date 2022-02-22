using System;
using System.Collections.Concurrent;

namespace ServiceFabric.Mocks.ReliableCollections
{
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
}
