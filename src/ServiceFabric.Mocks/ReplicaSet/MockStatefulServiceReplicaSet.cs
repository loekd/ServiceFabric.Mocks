using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceFabric.Mocks.ReplicaSet
{
    public class MockStatefulServiceReplicaSet<TStatefulService>
        where TStatefulService : StatefulService
    {
        private readonly List<MockStatefulServiceReplica<TStatefulService>> _replicas = new List<MockStatefulServiceReplica<TStatefulService>>();
        private readonly Func<StatefulServiceContext, IReliableStateManagerReplica2, TStatefulService> _serviceFactory;
        private readonly IReliableStateManagerReplica2 _stateManager;
        private readonly Random _random;

        public MockStatefulServiceReplicaSet(
            Func<StatefulServiceContext, IReliableStateManagerReplica2, TStatefulService> serviceFactory,
            IReliableStateManagerReplica2 stateManager,
            string serviceTypeName = MockStatefulServiceContextFactory.ServiceTypeName, 
            string serviceName = MockStatefulServiceContextFactory.ServiceName, 
            ICodePackageActivationContext codePackageActivationContext = null)
        {
            _serviceFactory = serviceFactory;
            _stateManager = stateManager;
            _random = new Random();
            CodePackageActivationContext = codePackageActivationContext ?? MockCodePackageActivationContext.Default;
            ServiceTypeName = serviceTypeName;
            ServiceName = serviceName;            
        }

        public string ServiceTypeName { get; }

        public Uri ServiceUri { get { return new Uri(ServiceName); } }

        public string ServiceName { get; }

        public ICodePackageActivationContext CodePackageActivationContext { get; }

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
            var replica = new MockStatefulServiceReplica<TStatefulService>(_serviceFactory, serviceContext, _stateManager);
            await replica.CreateAsync(role);
            _replicas.Add(replica);
        }

        public Task PromoteIdleSecondaryToActiveSecondaryAsync(long? replicaId = null)
        {
            return GetIdleSecondary(replicaId).PromoteToActiveSecondaryAsync();
        }

        public async Task PromoteActiveSecondaryToPrimaryAsync(long? replicaId = null)
        {
            var primary = Primary;
            var activeSecondary = GetActiveSecondary(replicaId);
            if (primary != null)
                await primary.DemoteToActiveSecondaryAsync();

            await activeSecondary.PromoteToPrimaryAsync();
        }

        public async Task PromoteNewReplicaToPrimaryAsync(long? newReplicaId = null)
        {
            newReplicaId = newReplicaId ?? _random.Next();
            long? primaryReplicaId = Primary?.ReplicaId;

            await AddReplicaAsync(ReplicaRole.IdleSecondary, newReplicaId);
            await PromoteIdleSecondaryToActiveSecondaryAsync(newReplicaId);
            await PromoteActiveSecondaryToPrimaryAsync(newReplicaId);

            if (primaryReplicaId.HasValue)
                await this[primaryReplicaId.Value].DeleteAsync();
        }
    }
}
