using Microsoft.ServiceFabric.Services.Runtime;
using ServiceFabric.Shift;
using System;
using System.Fabric;
using System.Threading;

namespace ServiceFabric.Mocks.ReplicaSet
{
    public class MockStatefulServiceReplica<TStatefulService>
        where TStatefulService : StatefulService, IReplica
    {
        private readonly TStatefulService _serviceInstance;

        public MockStatefulServiceReplica(TStatefulService serviceInstance) => _serviceInstance = serviceInstance;

        public TStatefulService ServiceInstance => _serviceInstance;

        public Exception LastException { get; set; }

        public CancellationTokenSource RunCancellation { get; set; } = new CancellationTokenSource();

        public CancellationTokenSource OpenCancellation { get; set; } = new CancellationTokenSource();

        public CancellationTokenSource CloseCancellation { get; set; } = new CancellationTokenSource();

        public CancellationTokenSource ChangeRoleCancellation { get; set; } = new CancellationTokenSource();

        public ReplicaRole ReplicaRole => _serviceInstance?.ReplicaContext.ReplicaRole ?? ReplicaRole.None;

        public long ReplicaId => _serviceInstance?.Context.ReplicaId ?? default(long);
    }
}