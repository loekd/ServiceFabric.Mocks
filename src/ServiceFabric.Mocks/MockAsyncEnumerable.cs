using System.Collections.Generic;
using Microsoft.ServiceFabric.Data;

namespace ServiceFabric.Mocks
{
    /// <summary>
    /// Exposes the <cref name="AsyncEnumerator" />, which supports an asynchronous iteration over a collection of a specified type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class MockAsyncEnumerable<T> : IAsyncEnumerable<T>
    {
        private readonly IEnumerable<T> _source;

        /// <inheritdoc />
        public MockAsyncEnumerable(IEnumerable<T> source)
        {
            _source = source;
        }

        /// <inheritdoc />
        public IAsyncEnumerator<T> GetAsyncEnumerator()
        {
            return new MockAsyncEnumerator<T>(_source.GetEnumerator());
        }
    }
}
