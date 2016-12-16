using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace ServiceFabric.Mocks.Tests.Actors
{
	public class OnActivateActor : Actor, IOnActivateActor
	{
		public bool OnActivateCalled { get; private set; }

		public bool OnDeactivateCalled { get; private set; }


		public OnActivateActor(ActorService actorService, ActorId actorId) : base(actorService, actorId)
		{
		}

		protected override Task OnActivateAsync()
		{
			OnActivateCalled = true;
			return Task.FromResult(OnActivateCalled);
		}

		protected override Task OnDeactivateAsync()
		{
			OnDeactivateCalled = true;
			return Task.FromResult(OnDeactivateCalled);
		}
	}

	public interface IOnActivateActor: IActor
	{
	}
}