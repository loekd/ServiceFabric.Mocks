using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceFabric.Mocks.Tests.Services;
using System.Linq;

namespace ServiceFabric.Mocks.Tests.TransactionTests
{
    [TestClass]
    public class TransactionTests
    {
        private const string StatePayload = "some value";

        [TestMethod]
        public async Task TestServiceState_TransactionCommitted()
        {
            var context = MockStatefulServiceContextFactory.Default;
            var stateManager = new MockReliableStateManager();
            var service = new MyStatefulService(context, stateManager);

            const string stateName = "test";
            var payload = new Payload(StatePayload);

            //create state-->Tran 1
            await service.InsertAsync(stateName, payload);

            Assert.IsInstanceOfType(stateManager.Transaction, typeof(MockTransaction));
            Assert.AreEqual(1, stateManager.AllTransactions.Count());
            Assert.IsTrue(stateManager.Transaction.IsCommitted);
            Assert.IsFalse(stateManager.Transaction.IsAborted);
        }

        [TestMethod]
        public async Task TestServiceState_TwoTransactionsCommitted()
        {
            var context = MockStatefulServiceContextFactory.Default;
            var stateManager = new MockReliableStateManager();
            var service = new MyStatefulService(context, stateManager);

            const string stateName = "test";
            var payload = new Payload(StatePayload);

            //create state-->Tran 1
            await service.InsertAsync(stateName, payload);
            //create state-->Tran 2
            await service.InsertAsync(stateName, payload);

            Assert.IsInstanceOfType(stateManager.Transaction, typeof(MockTransaction));
            Assert.AreEqual(2, stateManager.AllTransactions.Count());
            Assert.IsTrue(stateManager.Transaction.IsCommitted);
            Assert.IsFalse(stateManager.Transaction.IsAborted);
        }

        [TestMethod]
        public async Task TestServiceState_100TransactionsCommitted()
        {
            var context = MockStatefulServiceContextFactory.Default;
            var stateManager = new MockReliableStateManager();
            var service = new MyStatefulService(context, stateManager);

            const string stateName = "test";
            var payload = new Payload(StatePayload);

            var tasks = new List<Task>(100);
            for (int i = 0; i < 100; i++)
            {
                //create state
                tasks.Add(service.InsertAsync(stateName, payload));
            };
            await Task.WhenAll(tasks);
            Assert.IsInstanceOfType(stateManager.Transaction, typeof(MockTransaction));
            Assert.AreEqual(100, stateManager.AllTransactions.Count());
            Assert.IsTrue(stateManager.Transaction.IsCommitted);
            Assert.IsFalse(stateManager.Transaction.IsAborted);
        }

        [TestMethod]
        public async Task TestServiceState_99TransactionsCommittedOneAborted()
        {
            var context = MockStatefulServiceContextFactory.Default;
            var stateManager = new MockReliableStateManager();
            var service = new MyStatefulService(context, stateManager);

            const string stateName = "test";
            var payload = new Payload(StatePayload);

            var tasks = new List<Task>(99);
            for (int i = 0; i < 99; i++)
            {
                //create state
                tasks.Add(service.InsertAsync(stateName, payload));
            };
            await Task.WhenAll(tasks);
            await service.InsertAndAbortAsync(stateName, payload);
            Assert.IsInstanceOfType(stateManager.Transaction, typeof(MockTransaction));
            Assert.AreEqual(100, stateManager.AllTransactions.Count());
            Assert.IsFalse(stateManager.Transaction.IsCommitted);
            Assert.IsTrue(stateManager.Transaction.IsAborted);
            //check ordering
            Assert.IsTrue(stateManager.AllTransactions.Select(t=>t.InstanceCount).SequenceEqual(Enumerable.Range(1, 100)));
        }

        [TestMethod]
        public async Task TestServiceState_TransactionAborted()
        {
            var context = MockStatefulServiceContextFactory.Default;
            var stateManager = new MockReliableStateManager();
            var service = new MyStatefulService(context, stateManager);

            const string stateName = "test";
            var payload = new Payload(StatePayload);

            //create state-->Tran 1
            await service.InsertAndAbortAsync(stateName, payload);

            Assert.IsInstanceOfType(stateManager.Transaction, typeof(MockTransaction));
            Assert.AreEqual(1, stateManager.AllTransactions.Count());
            Assert.IsFalse(stateManager.Transaction.IsCommitted);
            Assert.IsTrue(stateManager.Transaction.IsAborted);
        }

        [TestMethod]
        public async Task TestServiceState_TwoTransactionsOneCommitted()
        {
            var context = MockStatefulServiceContextFactory.Default;
            var stateManager = new MockReliableStateManager();
            var service = new MyStatefulService(context, stateManager);

            const string stateName = "test";
            var payload = new Payload(StatePayload);

            //create state-->Tran 1
            await service.InsertAsync(stateName, payload);
            //create state-->Tran 2
            await service.InsertAndAbortAsync(stateName, payload);

            Assert.IsInstanceOfType(stateManager.Transaction, typeof(MockTransaction));
            Assert.AreEqual(2, stateManager.AllTransactions.Count());
            Assert.IsFalse(stateManager.Transaction.IsCommitted);
            Assert.IsTrue(stateManager.Transaction.IsAborted);
        }
    }
}
