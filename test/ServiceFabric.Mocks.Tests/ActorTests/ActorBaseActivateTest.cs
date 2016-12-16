using System;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceFabric.Mocks.Tests.Actors;

namespace ServiceFabric.Mocks.Tests.ActorTests
{
	[TestClass]
    public class ActorBaseActivateTest
    {
		[TestMethod]
	    public async Task InvokeOnActivateAsyncTest()
	    {
			var svc = MockActorServiceFactory.CreateActorServiceForActor<OnActivateActor>();
			var actor = svc.Activate(new ActorId(Guid.NewGuid()));

		    await actor.InvokeOnActivateAsync();

			Assert.IsTrue(actor.OnActivateCalled);
	    }

		[TestMethod]
		public async Task InvokeOnDeactivateAsyncTest()
		{
			var svc = MockActorServiceFactory.CreateActorServiceForActor<OnActivateActor>();
			var actor = svc.Activate(new ActorId(Guid.NewGuid()));

			await actor.InvokeOnDeactivateAsync();

			Assert.IsTrue(actor.OnDeactivateCalled);
		}
	}
}
