using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceFabric.Mocks.ReplicaSet;

namespace ServiceFabric.Mocks.Tests.ServiceTests
{
    [TestClass]
    public class MockStatefulServiceReplicaTests
    {
        [TestMethod]
        public async Task TestPrimaryReplicaShouldHaveOpenListenersAsync()
        {
            Func<StatefulServiceContext, IReliableStateManagerReplica2, StatefulServiceWithReplicaListener> serviceFactory = (StatefulServiceContext context, IReliableStateManagerReplica2 stateManager) => new StatefulServiceWithReplicaListener(context);
            var replicaSet = new MockStatefulServiceReplicaSet<StatefulServiceWithReplicaListener>(serviceFactory);
            await replicaSet.AddReplicaAsync(ReplicaRole.Primary, 1);
            var openListeners = replicaSet.Primary.OpenListeners;
            Assert.AreEqual(1, openListeners.Count());
        }

    }
    public class StatefulServiceWithReplicaListener : StatefulService, IService
    {
        public StatefulServiceWithReplicaListener(StatefulServiceContext serviceContext) : base(serviceContext)
        {
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new List<ServiceReplicaListener>() {
                new ServiceReplicaListener((context) => new Listener()),
            };
        }
    }
    public class Listener : ICommunicationListener
    {
        public void Abort()
        {
            throw new NotImplementedException();
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            return await Task.FromResult(string.Empty);
        }
    }
}
