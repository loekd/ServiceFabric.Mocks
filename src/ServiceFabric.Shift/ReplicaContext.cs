using System.Fabric;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceFabric.Shift
{
    public class ReplicaContext
    {
        protected readonly IReplicaStateProvider _replicaStateProvider;
        protected ReplicaState _currentState;

        public ReplicaContext(StatefulServiceContext serviceContext, IReplicaStateProvider replicaStateProvider)
        {
            ServiceContext = serviceContext;
            _replicaStateProvider = replicaStateProvider;
            _currentState = _replicaStateProvider.Get(ReplicaRole.Unknown);
        }

        public ReplicaRole ReplicaRole => _currentState?.ReplicaRole ?? ReplicaRole.None;

        public CancellationToken CancellationToken { get; internal set; }

        public StatefulServiceContext ServiceContext { get; }

        public Task RunAsync(CancellationToken cancellationToken)
        {
            CancellationToken = cancellationToken;
            return _currentState.RunAsync(this);
        }

        public virtual async Task ChangeRoleAsync(ReplicaRole newRole, CancellationToken cancellationToken)
        {
            if (_currentState.ReplicaRole != newRole)
            {
                await _currentState.ChangeRoleAsync(this, newRole, cancellationToken).ConfigureAwait(false);
                _currentState = _replicaStateProvider.Get(newRole);
            }
        }

        public Task RequestAsync() => _currentState.RequestAsync(this);
    }
}