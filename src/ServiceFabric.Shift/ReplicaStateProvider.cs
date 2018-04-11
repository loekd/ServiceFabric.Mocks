using System.Collections.Generic;
using System.Fabric;

namespace ServiceFabric.Shift
{
    public class ReplicaStateProvider : IReplicaStateProvider
    {
        private readonly IDictionary<ReplicaRole, ReplicaState> _replicaStateMap;

        public ReplicaStateProvider(ReplicaState primary, ReplicaState activeSecondary, ReplicaState idleSecondary, ReplicaState unknown)
        {
            _replicaStateMap = new Dictionary<ReplicaRole, ReplicaState>
            {
                { ReplicaRole.Primary, primary },
                { ReplicaRole.ActiveSecondary, activeSecondary },
                { ReplicaRole.IdleSecondary, idleSecondary },
                { ReplicaRole.Unknown, unknown },
                { ReplicaRole.None, null },
            };
        }

        public ReplicaState Get(ReplicaRole newRole) => _replicaStateMap[newRole];
    }
}