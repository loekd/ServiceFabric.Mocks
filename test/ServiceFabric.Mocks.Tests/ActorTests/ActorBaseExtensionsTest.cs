using System;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceFabric.Mocks.Tests.Actors;

namespace ServiceFabric.Mocks.Tests.ActorTests
{
	[TestClass]
    public class ActorBaseExtensionsTest
    {
		[TestMethod]
	    public async Task InvokeOnActivateAsyncTest()
	    {
			var svc = MockActorServiceFactory.CreateActorServiceForActor<InvokeOnActor>();
			var actor = svc.Activate(new ActorId(Guid.NewGuid()));

		    await actor.InvokeOnActivateAsync();

			Assert.IsTrue(actor.OnActivateCalled);
	    }

		[TestMethod]
		public async Task InvokeOnDeactivateAsyncTest()
		{
			var svc = MockActorServiceFactory.CreateActorServiceForActor<InvokeOnActor>();
			var actor = svc.Activate(new ActorId(Guid.NewGuid()));

			await actor.InvokeOnDeactivateAsync();

			Assert.IsTrue(actor.OnDeactivateCalled);
		}

		[TestMethod]
		public async Task InvokeOnPreActorMethodAsyncTest()
		{
			var svc = MockActorServiceFactory.CreateActorServiceForActor<InvokeOnActor>();
			var actor = svc.Activate(new ActorId(Guid.NewGuid()));

			var context = MockActorMethodContextFactory.CreateForActor(nameof(actor.ActorOperation));
			await actor.InvokeOnPreActorMethodAsync(context);

			Assert.IsTrue(actor.OnPreActorMethodCalled);
		}

		[TestMethod]
		public async Task InvokeOnPostActorMethodAsyncTest()
		{
			var svc = MockActorServiceFactory.CreateActorServiceForActor<InvokeOnActor>();
			var actor = svc.Activate(new ActorId(Guid.NewGuid()));

			var context = MockActorMethodContextFactory.CreateForTimer(nameof(actor.ActorOperation));
			await actor.InvokeOnPostActorMethodAsync(context);

			Assert.IsTrue(actor.OnPostActorMethodCalled);
		}

		[TestMethod]
	    public void TestActorMethodContexts()
	    {
		    var svc = MockActorServiceFactory.CreateActorServiceForActor<InvokeOnActor>();
		    var actor = svc.Activate(new ActorId(Guid.NewGuid()));

			var context = MockActorMethodContextFactory.CreateForActor(nameof(actor.ActorOperation));
			Assert.IsInstanceOfType(context, typeof(ActorMethodContext));
			context = MockActorMethodContextFactory.CreateForTimer(nameof(actor.ActorOperation));
		    Assert.IsInstanceOfType(context, typeof(ActorMethodContext));
		    context = MockActorMethodContextFactory.CreateForReminder(nameof(actor.ActorOperation));
		    Assert.IsInstanceOfType(context, typeof(ActorMethodContext));
		}
	}
}
