using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Runtime;
using ServiceFabric.Shift;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceFabric.Mocks.ReplicaSet
{
    public class MockStatefulServiceReplicaSet<TStatefulService>
        where TStatefulService : StatefulService, IReplica, IMockStatefulService
    {
        private readonly List<MockStatefulServiceReplica<TStatefulService>> _replicas = new List<MockStatefulServiceReplica<TStatefulService>>();
        private readonly IReliableStateManagerReplica _stateManager;
        private readonly Func<StatefulServiceContext, TStatefulService> _serviceFactory;
        private readonly Random _random;

        public MockStatefulServiceReplicaSet(
            IReliableStateManagerReplica stateManager,
            Func<StatefulServiceContext, TStatefulService> serviceFactory,
            ICodePackageActivationContext codePackageActivationContext = null
            )
        {
            _stateManager = stateManager;
            _serviceFactory = serviceFactory;
            _random = new Random();
        }

        public string ServiceTypeName { get; set; } = MockStatefulServiceContextFactory.ServiceTypeName;

        public Uri ServiceUri { get { return new Uri(ServiceName); } }

        public string ServiceName { get; set; } = MockStatefulServiceContextFactory.ServiceName;

        public ICodePackageActivationContext CodePackageActivationContext { get; set; } = MockCodePackageActivationContext.Default;

        public IEnumerable<MockStatefulServiceReplica<TStatefulService>> Replicas => _replicas;

        public MockStatefulServiceReplica<TStatefulService> Primary => _replicas.SingleOrDefault(r => r.ReplicaRole == ReplicaRole.Primary);

        public MockStatefulServiceReplica<TStatefulService> FirstActiveSecondary => ActiveSecondaryReplicas.FirstOrDefault();

        public IEnumerable<MockStatefulServiceReplica<TStatefulService>> ActiveSecondaryReplicas => _replicas.Where(r => r.ReplicaRole == ReplicaRole.ActiveSecondary);

        public MockStatefulServiceReplica<TStatefulService> FirstIdleSecondary => IdleSecondaryReplicas.FirstOrDefault();

        public IEnumerable<MockStatefulServiceReplica<TStatefulService>> IdleSecondaryReplicas => _replicas.Where(r => r.ReplicaRole == ReplicaRole.IdleSecondary);

        public MockStatefulServiceReplica<TStatefulService> FirstUnknown => UnknownReplicas.FirstOrDefault();

        public IEnumerable<MockStatefulServiceReplica<TStatefulService>> UnknownReplicas => _replicas.Where(r => r.ReplicaRole == ReplicaRole.Unknown);

        public MockStatefulServiceReplica<TStatefulService> FirstDeleted => _replicas.FirstOrDefault();

        public IEnumerable<MockStatefulServiceReplica<TStatefulService>> DeletedReplicas => _replicas.Where(r => r.ReplicaRole == ReplicaRole.None);

        public IEnumerable<MockStatefulServiceReplica<TStatefulService>> SecondaryReplicas => ActiveSecondaryReplicas.Union(IdleSecondaryReplicas);

        public MockStatefulServiceReplica<TStatefulService> this[long replicaId] => _replicas.FirstOrDefault(r => r.ReplicaId == replicaId);

        public MockStatefulServiceReplica<TStatefulService> GetActiveSecondary(long? replicaId = null) => replicaId.HasValue ? ActiveSecondaryReplicas.SingleOrDefault(r => r.ReplicaId == replicaId) : FirstActiveSecondary;

        public MockStatefulServiceReplica<TStatefulService> GetIdleSecondary(long? replicaId = null) => replicaId.HasValue ? IdleSecondaryReplicas.SingleOrDefault(r => r.ReplicaId == replicaId) : FirstIdleSecondary;

        public MockStatefulServiceReplica<TStatefulService> GetUnknown(long? replicaId = null) => replicaId.HasValue ? UnknownReplicas.SingleOrDefault(r => r.ReplicaId == replicaId) : FirstUnknown;

        public MockStatefulServiceReplica<TStatefulService> GetDeleted(long? replicaId = null) => replicaId.HasValue ? DeletedReplicas.SingleOrDefault(r => r.ReplicaId == replicaId) : FirstDeleted;

        public async Task AddReplicaAsync(ReplicaRole role, long? replicaId = null, int activationDelayMs = 0)
        {
            var serviceContext = MockStatefulServiceContextFactory.Create(CodePackageActivationContext, ServiceTypeName, ServiceUri, Guid.NewGuid(), replicaId ?? _random.Next());
            var serviceInstance = _serviceFactory.Invoke(serviceContext);
            var replica = new MockStatefulServiceReplica<TStatefulService>(serviceInstance);
            _replicas.Add(replica);

            await serviceInstance.OpenAsync(ReplicaOpenMode.New, replica.OpenCancellation.Token);

            if (role == ReplicaRole.Primary)
            {
                await serviceInstance.OpenServiceReplicaListeners(replica.OpenCancellation.Token);
                await serviceInstance.ChangeRoleAsync(role, replica.ChangeRoleCancellation.Token);
                await serviceInstance.RunAsync(replica.RunCancellation.Token);
            }
            else
                await NotifyReplicaRoleChange(replica, role);
        }

        public async Task PromoteIdleSecondaryToActiveSecondaryAsync(long? replicaId = null)
        {
            var replica = GetIdleSecondary(replicaId);
            await replica.ServiceInstance.OpenServiceReplicaListeners(replica.OpenCancellation.Token);
            await replica.ServiceInstance.ChangeRoleAsync(System.Fabric.ReplicaRole.ActiveSecondary, replica.ChangeRoleCancellation.Token);
        }

        public async Task DeleteReplicaAsync(int replicaId)
        {
            var replica = Replicas.Single(r => r.ReplicaId == replicaId);
            await NotifyReplicaRoleChange(replica, ReplicaRole.None);

            await Task.WhenAll(
                replica.ServiceInstance.CloseServiceReplicaListeners(replica.CloseCancellation.Token),
                Task.Run(() => replica.RunCancellation.Cancel())
                );

            await replica.ServiceInstance.CloseAsync(replica.CloseCancellation.Token);
        }

        public async Task PromoteActiveSecondaryToPrimaryAsync(long? replicaId = null)
        {
            var primary = Primary;
            var activeSecondary = GetActiveSecondary(replicaId);
            if (primary != null)
                await DemotePrimaryToActiveSecondaryAsync();

            if (activeSecondary.RunCancellation.IsCancellationRequested)
                activeSecondary.RunCancellation = new CancellationTokenSource();

            await activeSecondary.ServiceInstance.OpenServiceReplicaListeners(activeSecondary.OpenCancellation.Token);
            await NotifyReplicaRoleChange(activeSecondary, ReplicaRole.Primary);
            await activeSecondary.ServiceInstance.RunAsync(activeSecondary.RunCancellation.Token);
        }

        public async Task DemotePrimaryToActiveSecondaryAsync()
        {
            var primary = Primary;
            await NotifyReplicaRoleChange(primary, ReplicaRole.ActiveSecondary);

            await Task.WhenAll(
                primary.ServiceInstance.CloseServiceReplicaListeners(primary.CloseCancellation.Token),
                Task.Run(() => primary.RunCancellation.Cancel())
                );
        }

        public async Task DeletePrimaryAsync()
        {
            var primary = Primary;
            await NotifyReplicaRoleChange(primary, ReplicaRole.None);

            await Task.WhenAll(
                primary.ServiceInstance.CloseServiceReplicaListeners(primary.CloseCancellation.Token),
                Task.Run(() => primary.RunCancellation.Cancel())
                );

            await primary.ServiceInstance.CloseAsync(primary.CloseCancellation.Token);
        }

        private Task NotifyReplicaRoleChange(MockStatefulServiceReplica<TStatefulService> replica, ReplicaRole newRole) =>
            replica.ServiceInstance.ChangeRoleAsync(newRole, replica.ChangeRoleCancellation.Token)
                .ContinueWith(t => { if (t.IsCompleted) NotifyStateManagerRoleChange(newRole, replica.ChangeRoleCancellation.Token); });

        private Task NotifyStateManagerRoleChange(ReplicaRole newRole, CancellationToken cancellationToken) =>
            _stateManager is MockReliableStateManager ? ((MockReliableStateManager)_stateManager).ChangeRoleAsync(newRole, cancellationToken) : Task.Delay(0);
    }
}
