namespace ServiceFabric.Mocks.ReliableCollections
{
    using Microsoft.ServiceFabric.Data;
    using System;

    public enum ChangeType
    {
        Added,
        Removed,
        Updated,
    }

    /// <summary>
    /// Describe the change made to the collection. ChangeType:
    ///   Added: Added is set to the value that was added.
    ///   Removed: Removed is set to the value that was removed.
    ///   Updated: Added is set to the updated value, Removed is set to the origional value.
    /// </summary>
    public sealed class DictionaryChangedEvent<TKey, TValue> : EventArgs
    {
        public DictionaryChangedEvent(ITransaction tx, ChangeType changeType, TKey key, TValue added = default(TValue), TValue removed = default(TValue))
        {
            Transaction = tx;
            ChangeType = changeType;
            Key = key;
            Added = added;
            Removed = removed;
        }

        public ITransaction Transaction { get; private set; }

        public ChangeType ChangeType { get; private set; }

        public TKey Key { get; private set; }

        public TValue Added { get; private set; }

        public TValue Removed { get; private set; }
    }
}