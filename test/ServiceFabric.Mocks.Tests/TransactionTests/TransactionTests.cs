using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceFabric.Mocks.Tests.Services;
using System.Linq;
using Microsoft.ServiceFabric.Data;

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

            List<MockTransaction> changedTransactions = new List<MockTransaction>();
            stateManager.MockTransactionChanged += (s, t) => { changedTransactions.Add(t); };

            const string stateName = "test";
            var payload = new Payload(StatePayload);

            //create state-->Tran 1
            await service.InsertAsync(stateName, payload);

            Assert.AreEqual(1, changedTransactions.Count);
            foreach (var tx in changedTransactions)
            {
                Assert.IsTrue(tx.IsCommitted);
                Assert.IsFalse(tx.IsAborted);
            }
        }

        [TestMethod]
        public async Task TestServiceState_TwoTransactionsCommitted()
        {
            var context = MockStatefulServiceContextFactory.Default;
            var stateManager = new MockReliableStateManager();
            var service = new MyStatefulService(context, stateManager);

            List<MockTransaction> changedTransactions = new List<MockTransaction>();
            stateManager.MockTransactionChanged += (s, t) => { changedTransactions.Add(t); };

            const string stateName = "test";
            var payload = new Payload(StatePayload);

            //create state-->Tran 1
            await service.InsertAsync(stateName, payload);
            //create state-->Tran 2
            await service.InsertAsync(stateName, payload);

            Assert.AreEqual(2, changedTransactions.Count);
            foreach (var tx in changedTransactions)
            {
                Assert.IsTrue(tx.IsCommitted);
                Assert.IsFalse(tx.IsAborted);
            }
        }

        [TestMethod]
        public async Task TestServiceState_100TransactionsCommitted()
        {
            var context = MockStatefulServiceContextFactory.Default;
            var stateManager = new MockReliableStateManager();
            var service = new MyStatefulService(context, stateManager);

            List<MockTransaction> changedTransactions = new List<MockTransaction>();
            stateManager.MockTransactionChanged += (s, t) => { changedTransactions.Add(t); };

            const string stateName = "test";
            var payload = new Payload(StatePayload);

            var tasks = new List<Task>(100);
            for (int i = 0; i < 3; i++)
            {
                //create state
                tasks.Add(service.InsertAsync(stateName, payload));
            };
            await Task.WhenAll(tasks);

            Assert.AreEqual(3, changedTransactions.Count);
            foreach (var tx in changedTransactions)
            {
                Assert.IsTrue(tx.IsCommitted);
                Assert.IsFalse(tx.IsAborted);
            }
        }

        [TestMethod]
        public async Task TestServiceState_99TransactionsCommittedOneAborted()
        {
            var context = MockStatefulServiceContextFactory.Default;
            var stateManager = new MockReliableStateManager();
            var service = new MyStatefulService(context, stateManager);

            List<MockTransaction> changedTransactions = new List<MockTransaction>();
            stateManager.MockTransactionChanged += (s, t) => { changedTransactions.Add(t); };

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

            Assert.AreEqual(100, changedTransactions.Count);
            foreach (var tx in changedTransactions)
            {
                if (tx.TransactionId < 100)
                {
                    Assert.IsTrue(tx.IsCommitted);
                    Assert.IsFalse(tx.IsAborted);
                }
                else
                {
                    Assert.IsFalse(tx.IsCommitted);
                    Assert.IsTrue(tx.IsAborted);
                }
            }
            //check ordering
            Assert.IsTrue(changedTransactions.Select(t => (int)t.TransactionId).Last() == 100);
        }

        [TestMethod]
        public async Task TestServiceState_TransactionAborted()
        {
            var context = MockStatefulServiceContextFactory.Default;
            var stateManager = new MockReliableStateManager();
            var service = new MyStatefulService(context, stateManager);

            List<ITransaction> changedTransactions = new List<ITransaction>();
            stateManager.MockTransactionChanged += (s, t) => { changedTransactions.Add(t); };

            const string stateName = "test";
            var payload = new Payload(StatePayload);

            //create state-->Tran 1
            await service.InsertAndAbortAsync(stateName, payload);

            Assert.AreEqual(1, changedTransactions.Count);
            foreach (var tx in changedTransactions)
            {
                Assert.IsInstanceOfType(tx, typeof(MockTransaction));
                Assert.IsFalse(((MockTransaction)tx).IsCommitted);
                Assert.IsTrue(((MockTransaction)tx).IsAborted);
            }
        }

        [TestMethod]
        public async Task TestServiceState_TwoTransactionsOneCommitted()
        {
            var context = MockStatefulServiceContextFactory.Default;
            var stateManager = new MockReliableStateManager();
            var service = new MyStatefulService(context, stateManager);

            List<MockTransaction> changedTransactions = new List<MockTransaction>();
            stateManager.MockTransactionChanged += (s, t) => { changedTransactions.Add(t); };

            const string stateName = "test";
            var payload = new Payload(StatePayload);

            //create state-->Tran 1
            await service.InsertAsync(stateName, payload);
            //create state-->Tran 2
            await service.InsertAndAbortAsync(stateName, payload);

            Assert.AreEqual(2, changedTransactions.Count);
            MockTransaction tx;

            tx = changedTransactions[0];
            Assert.IsTrue(tx.IsCommitted);
            Assert.IsFalse(tx.IsAborted);

            tx = changedTransactions[1];
            Assert.IsFalse(tx.IsCommitted);
            Assert.IsTrue(tx.IsAborted);
        }
    }
}
