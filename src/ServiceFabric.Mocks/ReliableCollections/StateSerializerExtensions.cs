using System;
using System.IO;

namespace ServiceFabric.Mocks.ReliableCollections
{
    /// <summary>
    /// Helpers for collections that serialize their values.
    /// </summary>
    public static class StateSerializerExtensions
    {
        /// <summary>
        /// If the provided value is a stream, a serializer will be used to deserialize the stream into an object.
        /// If the value is anything else, it will be returned unmodified.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="encoding">Optional stream encoding. Defaults to UTF8.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static T Deserialize<T>(this Microsoft.ServiceFabric.Data.IStateSerializer<T> serializer, object value, System.Text.Encoding encoding = null)
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
                    throw new InvalidOperationException($"State value is of type Stream, but no serializer was found. Call 'TryAddSerializer<T>' for type '{typeof(T).FullName}'");
                }
                using (var reader = new BinaryReader(stream, encoding ?? System.Text.Encoding.UTF8, true))
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
        /// <param name="encoding">Optional stream encoding. Defaults to UTF.</param>
        /// <returns></returns>
        public static object Serialize<T>(this Microsoft.ServiceFabric.Data.IStateSerializer<T> serializer, T value, System.Text.Encoding encoding = null)
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
            using (var writer = new BinaryWriter(stream, encoding ?? System.Text.Encoding.UTF8, true))
            {
                serializer.Write(value, writer);
                stream.Seek(0, SeekOrigin.Begin);
            }
            return stream;
        }
    }

}
