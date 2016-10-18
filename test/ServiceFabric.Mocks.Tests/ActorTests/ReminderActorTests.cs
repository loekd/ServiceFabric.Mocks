using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
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
