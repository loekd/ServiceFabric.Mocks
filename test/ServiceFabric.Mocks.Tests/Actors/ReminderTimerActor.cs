using System;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace ServiceFabric.Mocks.Tests.Actors
{
    public class ReminderTimerActor : Actor, IRemindable, IReminderTimerActor
    {
        public ReminderTimerActor(ActorService actorService, ActorId actorId) : base(actorService, actorId)
        {
        }

        /// <inheritdoc />
        public Task RegisterReminderAsync(string reminderName)
        {
            return RegisterReminderAsync(reminderName, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(-1));
        }

		/// <inheritdoc />
		public Task<bool> IsReminderRegisteredAsync(string reminderName)
		{
			IActorReminder reminder = null;
			try
			{
				reminder = GetReminder(reminderName);
			}
			catch (ReminderNotFoundException)
			{
				//not found...
			}
			return Task.FromResult(reminder != null);
		}

		/// <inheritdoc />
		public Task UnregisterReminderAsync(string reminderName)
		{
			var reminder = GetReminder(reminderName);
			return UnregisterReminderAsync(reminder);
		}

		/// <inheritdoc />
		public Task ReceiveReminderAsync(string reminderName, byte[] context, TimeSpan dueTime, TimeSpan period)
        {
            //will not be called automatically.
            return Task.FromResult(true);
        }


        /// <inheritdoc />
        public Task RegisterTimerAsync()
        {
            RegisterTimer(TimerCallbackAsync, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(-1));
            return Task.FromResult(true);
        }

        ///<summary>
        /// Callback for timer
        /// </summary>
        private Task TimerCallbackAsync(object state)
        {
            //will not be called automatically.
            return Task.FromResult(true);
        }

	   
    }
}
