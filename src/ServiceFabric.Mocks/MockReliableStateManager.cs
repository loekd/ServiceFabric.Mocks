namespace ServiceFabric.Mocks
{
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Collections;
    using Microsoft.ServiceFabric.Data.Notifications;
    using ServiceFabric.Mocks.ReliableCollections;
    using System;
    using System.Fabric;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines replica of a reliable state provider.
    /// </summary>
    public class MockReliableStateManager : IReliableStateManagerReplica2
    {
        private int _totalTransactionInstanceCount = 0;
        private TransactedConcurrentDictionary<Uri, IReliableState> _store;

        public MockReliableStateManager()
        {
            // Initialze _store to a TransactedConcurrentDictionary that fires the StateManagerChanged event in the OnDictionaryChanged callback.
            _store = new TransactedConcurrentDictionary<Uri, IReliableState>(
                new Uri("fabric://state", UriKind.Absolute),
                (c) =>
                {
                    if (StateManagerChanged != null)
                    {
                        NotifyStateManagerSingleEntityChangedEventArgs changeEvent;
                        switch (c.ChangeType)
                        {
                            case ChangeType.Added:
                                changeEvent = new NotifyStateManagerSingleEntityChangedEventArgs(c.Transaction, c.Added, NotifyStateManagerChangedAction.Add);
                                break;

                            case ChangeType.Removed:
                                changeEvent = new NotifyStateManagerSingleEntityChangedEventArgs(c.Transaction, c.Removed, NotifyStateManagerChangedAction.Remove);
                                break;

                            default:
                                return false;
                        }

                        StateManagerChanged.Invoke(this, changeEvent);
                    }

                    return true;
                }
            );
        }

        #region TestHooks
        /// <summary>
        /// Test hook to verify transaction status. Raised for both commit and abort.
        /// </summary>
        public event EventHandler<MockTransaction> MockTransactionChanged;

        /// <summary>
        /// Returns last known <see cref="ReplicaRole"/>.
        /// </summary>
        public ReplicaRole? ReplicaRole { get; set; }

        public Task TriggerDataLoss()
        {
            return OnDataLossAsync(CancellationToken.None);
        }

        public Task TriggerRestoreCompleted()
        {
            return OnRestoreCompletedAsync(CancellationToken.None);
        }
        #endregion

        #region IReliableStateManager
        /// <summary>
        /// Occurs when State Manager's state changes.
        /// For example, creation or delete of reliable state or rebuild of the reliable state manager.
        /// </summary>
        public event EventHandler<NotifyStateManagerChangedEventArgs> StateManagerChanged;

        /// <summary>
        /// Fires when a transaction is committed since the Action enum only has a value for commit.
        /// </summary>
        public event EventHandler<NotifyTransactionChangedEventArgs> TransactionChanged;

        public ITransaction CreateTransaction()
        {
            return new MockTransaction(this, Interlocked.Increment(ref _totalTransactionInstanceCount));
        }

        #region GetOrAddAsync
        public Task<T> GetOrAddAsync<T>(string name) where T : IReliableState
        {
            return GetOrAddAsync<T>(name, default(TimeSpan));
        }

        public Task<T> GetOrAddAsync<T>(string name, TimeSpan timeout) where T : IReliableState
        {
            return GetOrAddAsync<T>(CreateUri(name), timeout);
        }

        public Task<T> GetOrAddAsync<T>(ITransaction tx, string name) where T : IReliableState
        {
            return GetOrAddAsync<T>(tx, name, default(TimeSpan));
        }

        public Task<T> GetOrAddAsync<T>(ITransaction tx, string name, TimeSpan timeout) where T : IReliableState
        {
            return GetOrAddAsync<T>(tx, CreateUri(name), timeout);
        }

        public Task<T> GetOrAddAsync<T>(Uri name) where T : IReliableState
        {
            return GetOrAddAsync<T>(name, default(TimeSpan));
        }

        public async Task<T> GetOrAddAsync<T>(Uri name, TimeSpan timeout) where T : IReliableState
        {
            using (var tx = CreateTransaction())
            {
                var result = await GetOrAddAsync<T>(tx, name, timeout);
                await tx.CommitAsync();

                return result;
            }
        }

        public Task<T> GetOrAddAsync<T>(ITransaction tx, Uri name) where T : IReliableState
        {
            return GetOrAddAsync<T>(tx, name, default(TimeSpan));
        }

        public async Task<T> GetOrAddAsync<T>(ITransaction tx, Uri name, TimeSpan timeout) where T : IReliableState
        {
            IReliableState constructed = null;

            var result = await _store.GetOrAddAsync(tx, name, collectionName =>
            {
                var typeArguments = typeof(T).GetGenericArguments();
                var typeDefinition = typeof(T).GetGenericTypeDefinition();

                if (typeof(IReliableDictionary<,>).IsAssignableFrom(typeDefinition)
                || typeof(IReliableDictionary2<,>).IsAssignableFrom(typeDefinition))
                {
                    constructed = ConstructMockCollection(collectionName, typeof(MockReliableDictionary<,>), typeArguments);
                }
                else if (typeof(IReliableConcurrentQueue<>).IsAssignableFrom(typeDefinition))
                {
                    constructed = ConstructMockCollection(collectionName, typeof(MockReliableConcurrentQueue<>), typeArguments);
                }
                else
                {
                    constructed = ConstructMockCollection(collectionName, typeof(MockReliableQueue<>), typeArguments);
                }
                return constructed;
            });

            return (T)result;
        }
        #endregion

        #region RemoveAsync
        public Task RemoveAsync(string name)
        {
            return RemoveAsync(name, default(TimeSpan));
        }

        public Task RemoveAsync(string name, TimeSpan timeout)
        {
            return RemoveAsync(CreateUri(name), timeout);
        }

        public Task RemoveAsync(ITransaction tx, string name)
        {
            return RemoveAsync(tx, name, default(TimeSpan));
        }

        public Task RemoveAsync(ITransaction tx, string name, TimeSpan timeout)
        {
            return RemoveAsync(tx, CreateUri(name), timeout);
        }

        public Task RemoveAsync(Uri name)
        {
            return RemoveAsync(name, default(TimeSpan));
        }

        public async Task RemoveAsync(Uri name, TimeSpan timeout)
        {
            using (var tx = CreateTransaction())
            {
                await RemoveAsync(tx, name, timeout);
                await tx.CommitAsync();
            }
        }

        public Task RemoveAsync(ITransaction tx, Uri name)
        {
            return RemoveAsync(tx, name, default(TimeSpan));
        }

        public async Task RemoveAsync(ITransaction tx, Uri name, TimeSpan timeout)
        {
            await _store.TryRemoveAsync(tx, name, timeout);
        }
        #endregion

        public bool TryAddStateSerializer<T>(IStateSerializer<T> stateSerializer)
        {
            return true;
        }

        #region TrygetAsync
        public Task<ConditionalValue<T>> TryGetAsync<T>(string name) where T : IReliableState
        {
            return TryGetAsync<T>(CreateUri(name));
        }

        public async Task<ConditionalValue<T>> TryGetAsync<T>(Uri name) where T : IReliableState
        {
            using (var tx = CreateTransaction())
            {
                var result = await _store.TryGetValueAsync(tx, name, LockMode.Default);
                return new ConditionalValue<T>(result.HasValue, (T)result.Value);
            }
        }
        #endregion

        public IAsyncEnumerator<IReliableState> GetAsyncEnumerator()
        {
            return new MockAsyncEnumerator<IReliableState>(_store.ValuesEnumerable);
        }
        #endregion

        #region IStateProviderReplica
        /// <summary>
        /// Called when <see cref="TriggerDataLoss"/> is called.
        /// </summary>
        public Func<CancellationToken, Task<bool>> OnDataLossAsync { set; get; }

        public void Abort()
        { }

        public Task BackupAsync(Func<BackupInfo, CancellationToken, Task<bool>> backupCallback)
        {
            return BackupAsync(BackupOption.Full, TimeSpan.MaxValue, CancellationToken.None, backupCallback);
        }

        public Task BackupAsync(BackupOption option, TimeSpan timeout, CancellationToken cancellationToken, Func<BackupInfo, CancellationToken, Task<bool>> backupCallback)
        {
            string stateBin = Path.Combine(Path.GetTempPath(), "state.bin");
            using (var fs = File.Create(stateBin))
            {
                _store.Serialize(fs);
            }
            var info = new BackupInfo(Path.GetDirectoryName(stateBin), option, new BackupInfo.BackupVersion());
            return backupCallback(info, CancellationToken.None);
        }

        public Task ChangeRoleAsync(ReplicaRole newRole, CancellationToken cancellationToken)
        {
            ReplicaRole = newRole;
            return Task.FromResult(true);
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        public void Initialize(StatefulServiceInitializationParameters initializationParameters)
        { }

        public Task<IReplicator> OpenAsync(ReplicaOpenMode openMode, IStatefulServicePartition partition, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReplicator>(null);
        }

        public Task RestoreAsync(string backupFolderPath)
        {
            string stateBin = Path.Combine(backupFolderPath, "state.bin");

            if (!File.Exists(stateBin))
            {
                throw new InvalidOperationException("No backed up state exists.");
            }

            using (var fs = File.OpenRead(stateBin))
            {
                _store.Deserialize(fs);
            }
            return Task.FromResult(true);
        }

        public Task RestoreAsync(string backupFolderPath, RestorePolicy restorePolicy, CancellationToken cancellationToken)
        {
            return RestoreAsync(backupFolderPath);
        }
        #endregion

        #region IStateProviderReplica2
        /// <summary>
        /// Called when <see cref="TriggerRestoreCompleted"/> is called.
        /// </summary>
        public Func<CancellationToken, Task> OnRestoreCompletedAsync { get; set; }
        #endregion

        public void OnTransactionChanged(ITransaction tx, bool isCommit)
        {
            if (isCommit)
            {
                TransactionChanged?.Invoke(this, new NotifyTransactionChangedEventArgs(tx, NotifyTransactionChangedAction.Commit));
            }

            MockTransactionChanged?.Invoke(this, (MockTransaction)tx);
        }

        private static IReliableState ConstructMockCollection(Uri name, Type genericType, Type[] typeArguments)
        {
            var type = genericType.MakeGenericType(typeArguments);
            var reliable = (IReliableState)Activator.CreateInstance(type, name);

            return reliable;
        }

        private static Uri CreateUri(string name)
        {
            return new Uri($"fabric://mocks/{name}", UriKind.Absolute);
        }
    }
}