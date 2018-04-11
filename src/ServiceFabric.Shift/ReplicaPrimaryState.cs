using System.Fabric;

namespace ServiceFabric.Shift
{
    public class ReplicaPrimaryState : ReplicaState
    {
        public override ReplicaRole ReplicaRole => ReplicaRole.Primary;
    }
}
