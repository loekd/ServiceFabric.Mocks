using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace ServiceFabric.Mocks
{
    public static class ActorExtensions
    {
        /// <summary>
        /// Gets all registered timers for the provided ActorBase.
        /// </summary>
        /// <param name="actor"></param>
        /// <returns></returns>
        public static IEnumerable<IActorTimer> GetActorTimers(this ActorBase actor)
        {
            var field = typeof(ActorBase).GetField("timers", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field != null)
            {
                var timers = (List<IActorTimer>)(field.GetValue(actor));
                return timers;
            }
            throw new InvalidOperationException("'timers' field was not found (anymore) on ActorBase.");
        }
        /// <summary>
        /// Gets all registered reminders for the provided ActorBase.
        /// </summary>
        /// <param name="actor"></param>
        /// <returns></returns>
        public static IEnumerable<IActorReminder> GetActorReminders(this ActorBase actor)
        {
            var reminderCollection = actor.ActorService.StateProvider.LoadRemindersAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            return reminderCollection[actor.Id];
        }
    }
}
