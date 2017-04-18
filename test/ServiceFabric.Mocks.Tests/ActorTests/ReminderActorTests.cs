using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceFabric.Mocks.Tests.Actors;

namespace ServiceFabric.Mocks.Tests.ActorTests
{
    [TestClass]
    public class ReminderTimerActorTests
    {
        [TestMethod]
        public async Task TestActorReminderRegistration()
        {
            var svc = MockActorServiceFactory.CreateActorServiceForActor<ReminderTimerActor>();
            var actor = svc.Activate(new ActorId(Guid.NewGuid()));
            string reminderName = "reminder";
            
            //setup
            await actor.RegisterReminderAsync(reminderName);
			
           
            //assert
            var reminderCollection = actor.GetActorReminders();
            bool hasReminder = reminderCollection.Any(r => string.Equals(r.Name, reminderName));
            Assert.IsTrue(hasReminder);

			bool isReminderRegistered = await actor.IsReminderRegisteredAsync(reminderName);
			Assert.IsTrue(isReminderRegistered);
		}

		[TestMethod]
		public async Task TestActorReminderUnregistration()
		{
			var svc = MockActorServiceFactory.CreateActorServiceForActor<ReminderTimerActor>();
			var actor = svc.Activate(new ActorId(Guid.NewGuid()));
			string reminderName = "reminder";

			//setup
			await actor.RegisterReminderAsync(reminderName);
			await actor.UnregisterReminderAsync(reminderName);

			//assert

			var reminderCollection = actor.GetActorReminders();
			bool hasReminder = reminderCollection.Any(r => string.Equals(r.Name, reminderName));
			Assert.IsFalse(hasReminder);

			bool isReminderRegistered = await actor.IsReminderRegisteredAsync(reminderName);
			Assert.IsFalse(isReminderRegistered);
		}


		[TestMethod]
        public async Task TestActorTimerRegistration()
        {
            var svc = MockActorServiceFactory.CreateActorServiceForActor<ReminderTimerActor>();
            var actor = svc.Activate(new ActorId(Guid.NewGuid()));

            //setup
            await actor.RegisterTimerAsync();

            //assert
            var timers = actor.GetActorTimers(); //extension method
            bool hasTimer = timers.Any();
            Assert.IsTrue(hasTimer);
        }
    }
}
