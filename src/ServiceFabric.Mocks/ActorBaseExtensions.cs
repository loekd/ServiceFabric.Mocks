using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace ServiceFabric.Mocks
{
	/// <summary>
	/// Contains extension methods to invoke non public members of <see cref="ActorBase"/>
	/// </summary>
	public static class ActorBaseExtensions
	{
		/// <summary>
		/// Invokes <see cref="ActorBase.OnActivateAsync"/> and returns the resulting task.
		/// </summary>
		/// <param name="actor"></param>
		/// <returns></returns>
		public static Task InvokeOnActivateAsync(this ActorBase actor)
		{
			return (Task)typeof(ActorBase)
				.GetMethod("OnActivateAsync", BindingFlags.Instance | BindingFlags.NonPublic)
				.Invoke(actor, null);
		}

		/// <summary>
		/// Invokes <see cref="ActorBase.OnDeactivateAsync"/> and returns the resulting task.
		/// </summary>
		/// <param name="actor"></param>
		/// <returns></returns>
		public static Task InvokeOnDeactivateAsync(this ActorBase actor)
		{
			return (Task)typeof(ActorBase)
				.GetMethod("OnDeactivateAsync", BindingFlags.Instance | BindingFlags.NonPublic)
				.Invoke(actor, null);
		}

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