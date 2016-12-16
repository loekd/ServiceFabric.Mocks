using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Query;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Data;

namespace ServiceFabric.Mocks
{
    public class MockActorStateProvider : IActorStateProvider
    {
        private readonly Dictionary<ActorId, Dictionary<string, object>> _state = new Dictionary<ActorId, Dictionary<string, object>>();
        private readonly MockActorReminderCollection _reminders = new MockActorReminderCollection();


        public StatefulServiceInitializationParameters InitializationParameters { get; private set; }

        public ReplicaRole Role { get; private set; }

        public ActorTypeInformation ActorTypeInformation  { get; private set; }

        public void Initialize(StatefulServiceInitializationParameters initializationParameters)
        {
            InitializationParameters = initializationParameters;
        }

        public void Initialize(ActorTypeInformation actorTypeInformation)
        {
            ActorTypeInformation = actorTypeInformation;
        }

        public Task<IReplicator> OpenAsync(ReplicaOpenMode openMode, IStatefulServicePartition partition, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReplicator>(new MockReplicator());
        }

        public Task ChangeRoleAsync(ReplicaRole newRole, CancellationToken cancellationToken)
        {
            Role = newRole;
            return Task.FromResult(true);
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        public void Abort()
        {
        }

        public Task BackupAsync(Func<BackupInfo, CancellationToken, Task<bool>> backupCallback)
        {
            throw new NotImplementedException();
        }

        public Task BackupAsync(BackupOption option, TimeSpan timeout, CancellationToken cancellationToken, Func<BackupInfo, CancellationToken, Task<bool>> backupCallback)
        {
            throw new NotImplementedException();
        }

        public Task RestoreAsync(string backupFolderPath)
        {
            throw new NotImplementedException();
        }

        public Task RestoreAsync(string backupFolderPath, RestorePolicy restorePolicy, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Func<CancellationToken, Task<bool>> OnDataLossAsync { get; set; }

        

        public Task ActorActivatedAsync(ActorId actorId, CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult(true);
        }

        public Task ReminderCallbackCompletedAsync(ActorId actorId, IActorReminder reminder,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult(true);
        }

        public Task<T> LoadStateAsync<T>(ActorId actorId, string stateName, CancellationToken cancellationToken = new CancellationToken())
        {
            T result = default(T);
            Dictionary<string, object> actorState;
            if (!_state.TryGetValue(actorId, out actorState))
                return Task.FromResult(result);

            object stateEntry;
            if (actorState.TryGetValue(stateName, out stateEntry))
            {
                result = (T)stateEntry;
            }
            return Task.FromResult(result);
        }

        public Task SaveStateAsync(ActorId actorId, IReadOnlyCollection<ActorStateChange> stateChanges,
            CancellationToken cancellationToken = new CancellationToken())
        {
            Dictionary<string, object> actorState;
            if (!_state.TryGetValue(actorId, out actorState))
            {
                actorState = new Dictionary<string, object>();
                _state.Add(actorId, actorState);
            }

            foreach (var stateChange in stateChanges)
            {
                switch (stateChange.ChangeKind)
                {
                    case StateChangeKind.None:
                        break;
                    case StateChangeKind.Add:
                    case StateChangeKind.Update:
                        actorState[stateChange.StateName] = stateChange.Value;
                        break;
                    case StateChangeKind.Remove:
                        actorState.Remove(stateChange.StateName);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return Task.FromResult(true);
        }

        public Task<bool> ContainsStateAsync(ActorId actorId, string stateName, CancellationToken cancellationToken = new CancellationToken())
        {
            Dictionary<string, object> actorState;
            if (!_state.TryGetValue(actorId, out actorState))
                return Task.FromResult(false);

            object stateEntry;
            return Task.FromResult(actorState.TryGetValue(stateName, out stateEntry));
        }

        public Task RemoveActorAsync(ActorId actorId, CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult(_state.Remove(actorId));
        }

        public Task<IEnumerable<string>> EnumerateStateNamesAsync(ActorId actorId, CancellationToken cancellationToken = new CancellationToken())
        {
            Dictionary<string, object> actorState;
            if (!_state.TryGetValue(actorId, out actorState))
                return Task.FromResult(Enumerable.Empty<string>());

            IEnumerable<string> keys = actorState.Keys;
            return Task.FromResult(keys);
        }

        public Task<PagedResult<ActorId>> GetActorsAsync(int numItemsToReturn, ContinuationToken continuationToken,
            CancellationToken cancellationToken)
        {
            var skip = (int?)continuationToken?.Marker ?? 0;
            var actors = _state.Keys.Skip(skip).Take(numItemsToReturn);
            var result = new PagedResult<ActorId>
            {
                Items = actors,
                ContinuationToken = new ContinuationToken(numItemsToReturn)
            };

            return Task.FromResult(result);
        }

        public Task SaveReminderAsync(ActorId actorId, IActorReminder reminder,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var utcNow = DateTime.UtcNow;
            var timeOfDay = utcNow.TimeOfDay;

            var reminderState = new MockActorReminderState(new MockActorReminderData(actorId, reminder, timeOfDay), timeOfDay);
            _reminders.Add(actorId, reminderState);
            return Task.FromResult(reminderState);
        }

        public Task DeleteReminderAsync(ActorId actorId, string reminderName,
            CancellationToken cancellationToken = new CancellationToken())
        {
            _reminders.Delete(actorId, reminderName);
            return Task.FromResult(true);
        }

        public Task<IActorReminderCollection> LoadRemindersAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult<IActorReminderCollection>(_reminders);
        }
    }
}
