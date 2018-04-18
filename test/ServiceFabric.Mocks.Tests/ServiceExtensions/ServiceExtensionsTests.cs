using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ServiceFabric.Mocks.Tests.ServiceExtensions
{
    [TestClass]
    public class ServiceExtensionsTests
    {
        [TestMethod]
        public async Task TestStatelessInvokeOnOpenAsync()
        {
            var serviceInstance = new NestedStatelessService(MockStatelessServiceContextFactory.Default);
            await serviceInstance.InvokeOnOpenAsync();
            Assert.IsTrue(serviceInstance.OnOpenCalled);
        }

        [TestMethod]
        public async Task TestStatelessInvokeOnCloseAsync()
        {
            var serviceInstance = new NestedStatelessService(MockStatelessServiceContextFactory.Default);
            await serviceInstance.InvokeOnCloseAsync();
            Assert.IsTrue(serviceInstance.OnCloseCalled);
        }

        [TestMethod]
        public async Task TestStatelessInvokeRunAsync()
        {
            var serviceInstance = new NestedStatelessService(MockStatelessServiceContextFactory.Default);
            await serviceInstance.InvokeRunAsync();
            Assert.IsTrue(serviceInstance.RunAsyncCalled);
        }

        [TestMethod]
        public void TestInvokeCreateServiceInstanceListeners()
        {
            var serviceInstance = new NestedStatelessService(MockStatelessServiceContextFactory.Default);
            var result = serviceInstance.InvokeCreateServiceInstanceListeners();
            Assert.IsInstanceOfType(result, typeof(ServiceInstanceListener[]));
        }


        [TestMethod]
        public async Task TestStatefulInvokeOnOpenAsync()
        {
            var serviceInstance = new NestedStatefulService(MockStatefulServiceContextFactory.Default);
            await serviceInstance.InvokeOnOpenAsync();
            Assert.IsTrue(serviceInstance.OnOpenCalled);
        }

        [TestMethod]
        public async Task TestStatefulInvokeOnCloseAsync()
        {
            var serviceInstance = new NestedStatefulService(MockStatefulServiceContextFactory.Default);
            await serviceInstance.InvokeOnCloseAsync();
            Assert.IsTrue(serviceInstance.OnCloseCalled);
        }

        [TestMethod]
        public async Task TestStatefulInvokeRunAsync()
        {
            var serviceInstance = new NestedStatefulService(MockStatefulServiceContextFactory.Default);
            await serviceInstance.InvokeRunAsync();
            Assert.IsTrue(serviceInstance.RunAsyncCalled);
        }

        [TestMethod]
        public async Task TestStatefulInvokeOnChangeRoleAsync()
        {
            var serviceInstance = new NestedStatefulService(MockStatefulServiceContextFactory.Default);
            await serviceInstance.InvokeOnChangeRoleAsync(ReplicaRole.Primary);
            Assert.IsTrue(serviceInstance.OnChangeRoleCalled);
        }

        [TestMethod]
        public void TestInvokeInvokeCreateServiceReplicaListeners()
        {
            var serviceInstance = new NestedStatefulService(MockStatefulServiceContextFactory.Default);
            var result = serviceInstance.InvokeCreateServiceReplicaListeners();
            Assert.IsInstanceOfType(result, typeof(ServiceReplicaListener[]));
        }


        private class NestedStatelessService : StatelessService
        {
            public bool OnOpenCalled { get; private set; }
            public bool OnCloseCalled { get; private set; }
            public bool RunAsyncCalled { get; private set; }

            public NestedStatelessService(StatelessServiceContext serviceContext) : base(serviceContext)
            {
            }

            protected override Task OnOpenAsync(CancellationToken cancellationToken)
            {
                OnOpenCalled = true;
                return Task.FromResult(true);
            }

            protected override Task OnCloseAsync(CancellationToken cancellationToken)
            {
                OnCloseCalled = true;
                return Task.FromResult(true);
            }

            protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
            {
                return new[] { new ServiceInstanceListener(_ => null) };
            }

            protected override Task RunAsync(CancellationToken cancellationToken)
            {
                RunAsyncCalled = true;
                return Task.FromResult(true);
            }
        }

        private class NestedStatefulService : StatefulService
        {
            public bool OnOpenCalled { get; private set; }
            public bool OnCloseCalled { get; private set; }
            public bool OnChangeRoleCalled { get; private set; }
            public bool RunAsyncCalled { get; private set; }

            public NestedStatefulService(StatefulServiceContext serviceContext) : base(serviceContext)
            {
            }

            protected override Task OnOpenAsync(ReplicaOpenMode replicaOpenMode, CancellationToken cancellationToken)
            {
                OnOpenCalled = true;
                return Task.FromResult(true);
            }

            protected override Task OnCloseAsync(CancellationToken cancellationToken)
            {
                OnCloseCalled = true;
                return Task.FromResult(true);
            }

            protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
            {
                return new[] {new ServiceReplicaListener(_ => null)};
            }

            protected override Task RunAsync(CancellationToken cancellationToken)
            {
                RunAsyncCalled = true;
                return Task.FromResult(true);
            }

            protected override Task OnChangeRoleAsync(ReplicaRole newRole, CancellationToken cancellationToken)
            {
                OnChangeRoleCalled = true;
                return Task.FromResult(true);
            }
        }
    }
}
