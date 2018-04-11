using System.Fabric;

namespace ServiceFabric.Shift
{
    public class ReplicaIdleSecondaryState : ReplicaState
    {
        public override ReplicaRole ReplicaRole => ReplicaRole.IdleSecondary;
    }
}
