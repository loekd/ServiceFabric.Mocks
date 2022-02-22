using System.Collections.Concurrent;

namespace ServiceFabric.Mocks.ReliableCollections
{
    /// <summary>
    /// Concurrent dictionary (unmodified).
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class MockConcurrentDictionary<TKey, TValue> : ConcurrentDictionary<TKey, TValue>, IConcurrentDictionary<TKey, TValue>
    { }

}
