using System.Globalization;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace ServiceFabric.Mocks
{
	/// <summary>
	/// Extension methods for <see cref="ActorService"/>
	/// </summary>
	public static class ActorServiceExtensions
	{
		/// <summary>
		/// Creates a new <see cref="TActor"/> instance using the provided <paramref name="actorId"/>.
		/// </summary>
		/// <param name="actorService">The actor service to extend.</param>
		/// <param name="actorId">The <see cref="ActorId"/> to use when creating an Actor instance of type <typeparamref name="TActor"/></param>
		/// <returns></returns>
		public static TActor Activate<TActor>(this ActorService actorService, ActorId actorId)
			where TActor : ActorBase
		{
			var property = (typeof(ActorService).GetProperty("ActorActivator", Constants.InstanceNonPublic));
			var actorActivator = property.GetValue(actorService, Constants.InstanceNonPublic, null, null, CultureInfo.InvariantCulture);
			// IActorActivator: ActorBase Activate(ActorService actorService, ActorId actorId);
			var method = actorActivator.GetType().GetMethod("Microsoft.ServiceFabric.Actors.Runtime.IActorActivator.Activate", Constants.InstanceNonPublic);
			return (TActor)method.Invoke(actorActivator, new object[] {actorService, actorId});
		}
	}
}