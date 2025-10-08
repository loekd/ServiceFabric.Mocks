using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceFabric.Mocks.NetCoreTests.Actors;

namespace ServiceFabric.Mocks.NetCoreTests.ActorTests
{
    [TestClass]
    public class ActorBaseExtensionsTest
    {
        [TestMethod]
        public async Task InvokeOnActivateAsyncTest()
        {
            var svc = MockActorServiceFactory.CreateActorServiceForActor<InvokeOnActor>();
            var actor = svc.Activate(new ActorId(Guid.NewGuid()));

            await actor.InvokeOnActivateAsync();

            Assert.IsTrue(actor.OnActivateCalled);
        }

        [TestMethod]
        public async Task InvokeOnDeactivateAsyncTest()
        {
            var svc = MockActorServiceFactory.CreateActorServiceForActor<InvokeOnActor>();
            var actor = svc.Activate(new ActorId(Guid.NewGuid()));

            await actor.InvokeOnDeactivateAsync();

            Assert.IsTrue(actor.OnDeactivateCalled);
        }

        [TestMethod]
        public async Task InvokeOnPreActorMethodAsyncTest()
        {
            var svc = MockActorServiceFactory.CreateActorServiceForActor<InvokeOnActor>();
            var actor = svc.Activate(new ActorId(Guid.NewGuid()));

            var context = MockActorMethodContextFactory.CreateForActor(nameof(actor.ActorOperation));
            await actor.InvokeOnPreActorMethodAsync(context);

            Assert.IsTrue(actor.OnPreActorMethodCalled);
        }

        [TestMethod]
        public async Task InvokeOnPostActorMethodAsyncTest()
        {
            var svc = MockActorServiceFactory.CreateActorServiceForActor<InvokeOnActor>();
            var actor = svc.Activate(new ActorId(Guid.NewGuid()));

            var context = MockActorMethodContextFactory.CreateForTimer(nameof(actor.ActorOperation));
            await actor.InvokeOnPostActorMethodAsync(context);

            Assert.IsTrue(actor.OnPostActorMethodCalled);
        }

        [TestMethod]
        public void TestActorMethodContexts()
        {
            var svc = MockActorServiceFactory.CreateActorServiceForActor<InvokeOnActor>();
            var actor = svc.Activate(new ActorId(Guid.NewGuid()));

            var context = MockActorMethodContextFactory.CreateForActor(nameof(actor.ActorOperation));
            Assert.IsInstanceOfType<ActorMethodContext>(context);

            context = MockActorMethodContextFactory.CreateForTimer(nameof(actor.ActorOperation));
            Assert.IsInstanceOfType<ActorMethodContext>(context);

            context = MockActorMethodContextFactory.CreateForReminder(nameof(actor.ActorOperation));
            Assert.IsInstanceOfType<ActorMethodContext>(context);
        }

        [TestMethod]
        public async Task TestGetActorRemindersExtensionMethod()
        {
            var svc = MockActorServiceFactory.CreateActorServiceForActor<ReminderTimerActor>();
            var actor = svc.Activate(new ActorId(Guid.NewGuid()));
            string reminderName = "reminder";
            IEnumerable<IActorReminder> reminderCollection = null;

            // Test empty when no reminders
            reminderCollection = actor.GetActorReminders();
            Assert.IsFalse(reminderCollection.Any());

            // Test non-empty when reminder registered
            await actor.RegisterReminderAsync(reminderName);
            reminderCollection = actor.GetActorReminders();
            Assert.IsTrue(reminderCollection.Any(r => string.Equals(r.Name, reminderName)));

            // Test non-empty when reminder registered
            await actor.UnregisterReminderAsync(reminderName);
            reminderCollection = actor.GetActorReminders();
            Assert.IsFalse(reminderCollection.Any(r => string.Equals(r.Name, reminderName)));
        }
    }
}
