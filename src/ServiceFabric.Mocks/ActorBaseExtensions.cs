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
		/// Invokes <see cref="ActorBase.OnPreActorMethodAsync"/> and returns the resulting task.
		/// Call <see cref="MockActorMethodContextFactory.ActorMethodContextCreateForActor"/> 
		/// /<see cref="MockActorMethodContextFactory.ActorMethodContextCreateForReminder"/> 
		/// /<see cref="MockActorMethodContextFactory.ActorMethodContextCreateForTimer"/> 
		/// to create an <see cref="ActorMethodContext"/>.
		/// </summary>
		/// <param name="actor"></param>
		/// <param name="actorMethodContext"> An <see cref="ActorMethodContext" /> describing the method that will be invoked by actor runtime after this method finishes.</param>
		/// <returns></returns>
		public static Task InvokeOnPreActorMethodAsync(this ActorBase actor, ActorMethodContext actorMethodContext)
		{
			return (Task)typeof(ActorBase)
				.GetMethod("OnPreActorMethodAsync", BindingFlags.Instance | BindingFlags.NonPublic)
				.Invoke(actor, new object[]{actorMethodContext});
		}

		/// <summary>
		/// Invokes <see cref="ActorBase.InvokeOnPostActorMethodAsync"/> and returns the resulting task.
		/// Call <see cref="MockActorMethodContextFactory.ActorMethodContextCreateForActor"/> 
		/// /<see cref="MockActorMethodContextFactory.ActorMethodContextCreateForReminder"/> 
		/// /<see cref="MockActorMethodContextFactory.ActorMethodContextCreateForTimer"/> 
		/// to create an <see cref="ActorMethodContext"/>./// </summary>
		/// <param name="actor"></param>
		/// <param name="actorMethodContext"> An <see cref="ActorMethodContext" /> describing the method that will be invoked by actor runtime after this method finishes.</param>
		/// <returns></returns>
		public static Task InvokeOnPostActorMethodAsync(this ActorBase actor, ActorMethodContext actorMethodContext)
		{
			return (Task)typeof(ActorBase)
				.GetMethod("OnPostActorMethodAsync", BindingFlags.Instance | BindingFlags.NonPublic)
				.Invoke(actor, new object[] { actorMethodContext });
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

	public static class MockActorMethodContextFactory
	{
		/// <summary>
		/// Creates an <see cref="ActorMethodContext"/> for an Actor interface method, to use when invoking <see cref="InvokeOnPreActorMethodAsync"/>.
		/// </summary>
		/// <param name="methodName"></param>
		/// <returns></returns>
		public static ActorMethodContext CreateForActor(string methodName)
		{
			return (ActorMethodContext)typeof(ActorMethodContext)
				.GetMethod("CreateForActor", BindingFlags.Static | BindingFlags.NonPublic)
				.Invoke(null, new object[] { methodName });
		}

		/// <summary>
		/// Creates an <see cref="ActorMethodContext"/> for an Actor timer method, to use when invoking <see cref="InvokeOnPreActorMethodAsync"/>.
		/// </summary>
		/// <param name="methodName"></param>
		/// <returns></returns>
		public static ActorMethodContext CreateForTimer(string methodName)
		{
			return (ActorMethodContext)typeof(ActorMethodContext)
				.GetMethod("CreateForTimer", BindingFlags.Static | BindingFlags.NonPublic)
				.Invoke(null, new object[] { methodName });
		}

		/// <summary>
		/// Creates an <see cref="ActorMethodContext"/> for an Actor reminder method, to use when invoking <see cref="InvokeOnPreActorMethodAsync"/>.
		/// </summary>
		/// <param name="actor">ignored</param>
		/// <param name="methodName"></param>
		/// <returns></returns>
		public static ActorMethodContext CreateForReminder(string methodName)
		{
			return (ActorMethodContext)typeof(ActorMethodContext)
				.GetMethod("CreateForReminder", BindingFlags.Static | BindingFlags.NonPublic)
				.Invoke(null, new object[] { methodName });
		}
	}
}