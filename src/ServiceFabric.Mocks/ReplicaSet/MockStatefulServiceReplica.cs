using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceFabric.Mocks.ReplicaSet
{
    public class MockStatefulServiceReplica<TStatefulService>
        where TStatefulService : StatefulService
    {
        private readonly Func<StatefulServiceContext, TStatefulService> _serviceFactory;
        private readonly TStatefulService _serviceInstance;
        private readonly StatefulServiceContext _context;
        private IEnumerable<ICommunicationListener> _openListeners = new List<ICommunicationListener>();        

        public MockStatefulServiceReplica(Func<StatefulServiceContext, TStatefulService> serviceFactory, StatefulServiceContext context)
        {
            _context = context;
            _serviceFactory = serviceFactory;       
            _serviceInstance = _serviceFactory.Invoke(context);
        }

        public TStatefulService ServiceInstance => _serviceInstance;

        public Exception LastException { get; set; }

        public CancellationTokenSource RunCancellation { get; set; } = new CancellationTokenSource();

        public CancellationTokenSource OpenCancellation { get; set; } = new CancellationTokenSource();

        public CancellationTokenSource CloseCancellation { get; set; } = new CancellationTokenSource();

        public CancellationTokenSource ChangeRoleCancellation { get; set; } = new CancellationTokenSource();

        public ReplicaRole ReplicaRole { get; private set; } = ReplicaRole.Unknown;

        public long ReplicaId => _serviceInstance?.Context.ReplicaId ?? default(long);

        public async Task CreateAsync(ReplicaRole role)
        {
            await OpenAsync(ReplicaOpenMode.New);

            if (role == ReplicaRole.Primary)
            {
                await OpenServiceReplicaListeners();
                await ChangeRoleAsync(role);
                await RunAsync();
            }
            else
                await ChangeRoleAsync(role);
        }

        public async Task DeleteAsync()
        {
            await ChangeRoleAsync(ReplicaRole.None);            

            await Task.WhenAll(
                CloseServiceReplicaListeners(),
                Task.Run(() => RunCancellation.Cancel())
                );

            await CloseAsync();
        }        

        public async Task PromoteToPrimaryAsync()
        {
            if (RunCancellation.IsCancellationRequested)
                RunCancellation = new CancellationTokenSource();

            await OpenServiceReplicaListeners();
            await ChangeRoleAsync(ReplicaRole.Primary);
            await RunAsync();
        }

        public async Task PromoteToActiveSecondaryAsync()
        {
            await OpenServiceReplicaListeners();
            await ChangeRoleAsync(ReplicaRole.ActiveSecondary);
        }

        public async Task DemoteToActiveSecondaryAsync()
        {
            await ChangeRoleAsync(ReplicaRole.ActiveSecondary);

            await Task.WhenAll(
                CloseServiceReplicaListeners(),
                Task.Run(() => RunCancellation.Cancel())
                );
        }

        private async Task ChangeRoleAsync(ReplicaRole newRole)
        {
            ReplicaRole = newRole;
            await (Task)_serviceInstance
                .GetType()
                .GetMethod("OnChangeRoleAsync", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(_serviceInstance, new object[] { newRole, ChangeRoleCancellation.Token });

            if(_serviceInstance.StateManager is MockReliableStateManager)
            {
                await ((MockReliableStateManager)_serviceInstance.StateManager).ChangeRoleAsync(newRole, ChangeRoleCancellation.Token);
            }
        }

        private Task CloseAsync()
        {
            return (Task)_serviceInstance
                .GetType()
                .GetMethod("OnCloseAsync", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(_serviceInstance, new object[] { CloseCancellation.Token });
        }

        private Task OpenAsync(ReplicaOpenMode mode)
        {
            return (Task)_serviceInstance
                .GetType()
                .GetMethod("OnOpenAsync", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(_serviceInstance, new object[] { mode, OpenCancellation.Token });
        }

        private Task RunAsync()
        {
            return (Task)_serviceInstance
                .GetType()
                .GetMethod("RunAsync", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(_serviceInstance, new object[] { RunCancellation.Token });
        }

        private Task OpenServiceReplicaListeners()
        {
            var serviceReplicaListeners = (IEnumerable<ServiceReplicaListener>)_serviceInstance
                .GetType()
                .GetMethod("CreateServiceReplicaListeners", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(_serviceInstance, new object[] { });

            _openListeners = serviceReplicaListeners
                .Where(rl => (ReplicaRole == ReplicaRole.ActiveSecondary && rl.ListenOnSecondary == true) || ReplicaRole == ReplicaRole.Primary)
                .Select(rl => rl.CreateCommunicationListener(_context))
                .ToList();

            return Task.WhenAll(_openListeners.Select(l => l.OpenAsync(OpenCancellation.Token)));
        }

        private Task CloseServiceReplicaListeners() => Task.WhenAll(_openListeners.Select(l => l.CloseAsync(CloseCancellation.Token)));
    }
}