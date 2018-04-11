using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceFabric.Shift
{
    public abstract class StatefulServiceReplica : StatefulService, IReplica
    {
        protected StatefulServiceReplica(ReplicaContext replicaContext, IReliableStateManagerReplica stateManagerReplica)
            : base(replicaContext.ServiceContext, stateManagerReplica)
        {
            ReplicaContext = replicaContext;
        }

        public ReplicaContext ReplicaContext { get; }

        protected override Task OnChangeRoleAsync(ReplicaRole newRole, CancellationToken cancellationToken) =>
            ReplicaContext.ChangeRoleAsync(newRole, cancellationToken);

        protected override Task RunAsync(CancellationToken cancellationToken) =>
            ReplicaContext.RunAsync(cancellationToken);
    }
}