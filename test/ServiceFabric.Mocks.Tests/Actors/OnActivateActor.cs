using System;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace ServiceFabric.Mocks.Tests.Actors
{
	public class InvokeOnActor : Actor, IOnActivateActor
	{
		public bool OnActivateCalled { get; private set; }

		public bool OnDeactivateCalled { get; private set; }

		public bool OnPreActorMethodCalled { get; private set; }

		public bool OnPostActorMethodCalled { get; private set; }


		public InvokeOnActor(ActorService actorService, ActorId actorId) : base(actorService, actorId)
		{
		}

		public Task ActorOperation()
		{
			throw new NotImplementedException();
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

		protected override Task OnPreActorMethodAsync(ActorMethodContext actorMethodContext)
		{
			OnPreActorMethodCalled = true;
			return Task.FromResult(OnPreActorMethodCalled);
		}

		protected override Task OnPostActorMethodAsync(ActorMethodContext actorMethodContext)
		{
			OnPostActorMethodCalled = true;
			return Task.FromResult(OnPostActorMethodCalled);
		}
	}

	public interface IOnActivateActor: IActor
	{
	}
}