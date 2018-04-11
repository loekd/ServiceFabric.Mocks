using System.Fabric;

namespace ServiceFabric.Shift
{
    public class ReplicaActiveSecondaryState : ReplicaState
    {
        public override ReplicaRole ReplicaRole => ReplicaRole.ActiveSecondary;
    }
}
