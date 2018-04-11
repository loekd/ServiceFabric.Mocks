using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceFabric.Mocks.ReplicaSet
{
    public interface IMockStatefulService
    {
        Task ChangeRoleAsync(ReplicaRole newRole, CancellationToken cancellationToken);
        Task OpenAsync(ReplicaOpenMode mode, CancellationToken cancellationToken);
        Task CloseAsync(CancellationToken cancellationToken);
        Task RunAsync(CancellationToken cancellationToken);
        Task OpenServiceReplicaListeners(CancellationToken cancellationToken);
        Task CloseServiceReplicaListeners(CancellationToken cancellationToken);
    }
}
