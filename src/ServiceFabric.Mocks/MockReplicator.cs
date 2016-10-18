using System.Fabric;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceFabric.Mocks
{
    public class MockReplicator : IReplicator
    {
        public Task<bool> OnDataLossAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        public void UpdateCatchUpReplicaSetConfiguration(ReplicaSetConfiguration currentConfiguration,
            ReplicaSetConfiguration previousConfiguration)
        {
        }

        public Task WaitForCatchUpQuorumAsync(ReplicaSetQuorumMode quorumMode, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        public void UpdateCurrentReplicaSetConfiguration(ReplicaSetConfiguration currentConfiguration)
        {
        }

        public Task BuildReplicaAsync(ReplicaInformation replicaInfo, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        public void RemoveReplica(long replicaId)
        {
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(nameof(OpenAsync));

        }

        public Task ChangeRoleAsync(Epoch epoch, ReplicaRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        public void Abort()
        {
        }

        public long GetCurrentProgress()
        {
            return 0L;
        }

        public long GetCatchUpCapability()
        {
            return 0L;
        }

        public Task UpdateEpochAsync(Epoch epoch, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }
    }
}