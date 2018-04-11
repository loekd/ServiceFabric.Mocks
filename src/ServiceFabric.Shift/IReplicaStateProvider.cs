using System.Fabric;

namespace ServiceFabric.Shift
{
    public interface IReplicaStateProvider
    {
        ReplicaState Get(ReplicaRole newRole);
    }
}