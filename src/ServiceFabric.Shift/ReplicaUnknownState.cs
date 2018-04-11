using System.Fabric;

namespace ServiceFabric.Shift
{
    public class ReplicaUnknownState : ReplicaState
    {
        public override ReplicaRole ReplicaRole => ReplicaRole.Unknown;
    }
}
