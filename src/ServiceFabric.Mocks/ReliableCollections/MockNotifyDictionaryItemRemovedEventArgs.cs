namespace ServiceFabric.Mocks.ReliableCollections
{
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Notifications;

    public sealed class MockNotifyDictionaryItemRemovedEventArgs<Tkey1, Tvalue1> : NotifyDictionaryItemRemovedEventArgs<Tkey1, Tvalue1>
    {
        internal MockNotifyDictionaryItemRemovedEventArgs(ITransaction tx, Tkey1 key, Tvalue1 value)
            : base(tx, key)
        {
            Value = value;
        }

        public Tvalue1 Value { get; private set; }
    }
}
