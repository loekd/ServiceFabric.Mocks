using System;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceFabric.Shift
{
    public abstract class ReplicaState
    {
        public abstract ReplicaRole ReplicaRole { get; }

        public virtual Task RunAsync(ReplicaContext context) => Task.Delay(0);

        public virtual Task ChangeRoleAsync(ReplicaContext context, ReplicaRole newRole, CancellationToken changeRoleCancellation) => Task.Delay(0);

        public virtual Task RequestAsync(ReplicaContext context) =>
            ReplicaRole == ReplicaRole.Primary ?
                Task.Delay(0) :
                throw new InvalidOperationException($"Request Made to Non Primary Replica. CurrentState:{ReplicaRole}");
    }
}
