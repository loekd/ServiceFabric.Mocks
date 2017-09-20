using System;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceFabric.Mocks.Tests.Actors;
using ServiceFabric.Mocks.Tests.ActorServices;

namespace ServiceFabric.Mocks.Tests.ActorTests
{
	[TestClass]
	public class CustomActorServiceTests
	{
		[TestMethod]
		public void TestCustomActorServiceActivate()
		{
			//an ActorService with a standard constructor can be created by the MockActorServiceFactory
			var customActorService = MockActorServiceFactory.CreateCustomActorServiceForActor<CustomActorService, InvokeOnActor>();
			var actor = customActorService.Activate<InvokeOnActor>(new ActorId(123L));

			Assert.IsInstanceOfType(customActorService, typeof(CustomActorService));
			Assert.IsInstanceOfType(actor, typeof(InvokeOnActor));
			Assert.AreEqual(123L, actor.Id.GetLongId());
		}

		[TestMethod]
		public void TestAnotherCustomActorService_CreateFails()
		{
			//an ActorService with a NON standard constructor can be created by the MockActorServiceFactory
			Assert.ThrowsException<InvalidOperationException>(() =>
			{
				// ReSharper disable once UnusedVariable
				var customActorService =
					MockActorServiceFactory.CreateCustomActorServiceForActor<AnotherCustomActorService, InvokeOnActor>();
			});
		}

		[TestMethod]
		public void TestAnotherCustomActorService()
		{
			//an ActorService with a NON standard constructor can be created by passing Mock arguments:

			IActorStateProvider actorStateProvider = new MockActorStateProvider();
			actorStateProvider.Initialize(ActorTypeInformation.Get(typeof(InvokeOnActor)));
			var context = MockStatefulServiceContextFactory.Default;
			var dummy = new object(); //this argument causes the 'non standard' ctor.
			var customActorService = new AnotherCustomActorService(dummy, context, ActorTypeInformation.Get(typeof(InvokeOnActor)));

			var actor = customActorService.Activate<InvokeOnActor>(new ActorId(123L));

			Assert.IsInstanceOfType(actor, typeof(InvokeOnActor));
			Assert.AreEqual(123L, actor.Id.GetLongId());
		}
	}
}
