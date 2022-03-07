using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceFabric.Mocks.ReplicaSet
{
    public class MockStatefulServiceReplica<TStatefulService>
        where TStatefulService : StatefulService
    {
        private readonly TStatefulService _serviceInstance;
        private readonly StatefulServiceContext _context;
        private readonly IReliableStateManagerReplica2 _stateManager;
        private Task _runAsyncTask;

        public MockStatefulServiceReplica(Func<StatefulServiceContext, IReliableStateManagerReplica2, TStatefulService> serviceFactory, StatefulServiceContext context, IReliableStateManagerReplica2 stateManager)
        {
            _context = context;
            _stateManager = stateManager;
            _serviceInstance = serviceFactory.Invoke(context, _stateManager);
        }

        public IEnumerable<ICommunicationListener> OpenListeners { get; private set; } = new List<ICommunicationListener>();

        public TStatefulService ServiceInstance => _serviceInstance;

        public Exception LastException {
            get {
                if (_runAsyncTask != null && _runAsyncTask.IsFaulted)
                {
                    return _runAsyncTask.Exception;
                }
                return null;
            }
        }

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
                await ChangeRoleAsync(role);
                await OpenServiceReplicaListeners();
                RunAsync();
            }
            else
                await ChangeRoleAsync(role);
        }

        public async Task DeleteAsync()
        {
            RunCancellation.CancelAfter(0);
            if (_runAsyncTask != null)
            {
                await _runAsyncTask;
            }
            _runAsyncTask = null;
            
            await CloseServiceReplicaListeners();
            await ChangeRoleAsync(ReplicaRole.None);
            await CloseAsync();
        }

        public async Task PromoteToPrimaryAsync()
        {
            if (RunCancellation.IsCancellationRequested)
                RunCancellation = new CancellationTokenSource();

            await ChangeRoleAsync(ReplicaRole.Primary);
            await OpenServiceReplicaListeners();
            RunAsync();
        }

        public async Task PromoteToActiveSecondaryAsync()
        {
            await ChangeRoleAsync(ReplicaRole.ActiveSecondary);
            await OpenServiceReplicaListeners();
        }

        public async Task DemoteToActiveSecondaryAsync()
        {
            RunCancellation.CancelAfter(0);
            if (_runAsyncTask != null)
            {
                await _runAsyncTask;
            }
            _runAsyncTask = null;

            await CloseServiceReplicaListeners();
            await ChangeRoleAsync(ReplicaRole.ActiveSecondary);
        }

        private async Task ChangeRoleAsync(ReplicaRole newRole)
        {
            ReplicaRole = newRole;
            await _serviceInstance.InvokeOnChangeRoleAsync(newRole, ChangeRoleCancellation.Token);
            await _stateManager.ChangeRoleAsync(newRole, ChangeRoleCancellation.Token);
        }

        private Task CloseAsync()
        {
            return _serviceInstance.InvokeOnCloseAsync(CloseCancellation.Token);
        }

        private Task OpenAsync(ReplicaOpenMode mode)
        {
            return _serviceInstance.InvokeOnOpenAsync(mode, OpenCancellation.Token);
        }

        private void RunAsync()
        {
            _runAsyncTask = _serviceInstance.InvokeRunAsync(RunCancellation.Token);
        }   

        private Task OpenServiceReplicaListeners()
        {
            var serviceReplicaListeners = _serviceInstance.InvokeCreateServiceReplicaListeners();

            OpenListeners = serviceReplicaListeners
                .Where(rl => ReplicaRole == ReplicaRole.ActiveSecondary && rl.ListenOnSecondary
                    || ReplicaRole == ReplicaRole.Primary)
                .Select(rl => rl.CreateCommunicationListener(_context))
                .ToList();

            return Task.WhenAll(OpenListeners.Select(l => l.OpenAsync(OpenCancellation.Token)));
        }

        private Task CloseServiceReplicaListeners() => Task.WhenAll(OpenListeners.Select(l => l.CloseAsync(CloseCancellation.Token)));
    }
}