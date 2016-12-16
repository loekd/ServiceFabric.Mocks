using System;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ServiceFabric.Mocks.Tests.MocksTests
{
	[TestClass]
	public class MockActorServiceFactoryTests
	{
		[TestMethod]
		public void TestDefault()
		{
			var instance = MockActorServiceFactory.CreateActorServiceForActor<MockActor>();
			Assert.IsInstanceOfType(instance, typeof(MockActorService<MockActor>));
			Assert.AreEqual(MockStatefulServiceContextFactory.Default, instance.Context);
			Assert.AreEqual(ActorTypeInformation.Get(typeof(MockActor)).ImplementationType, instance.ActorTypeInformation.ImplementationType);
			Assert.IsNotNull(instance.StateProvider);
			Assert.IsNotNull(instance.Settings);
		}

		[TestMethod]
		public void TestCustomContext()
		{
			var newUri = new Uri("fabric:/MockApp/OtherMockStatefulService");
			var serviceTypeName = "OtherMockServiceType";
			var partitionId = Guid.NewGuid();
			var replicaId = long.MaxValue;
			var context = new MockCodePackageActivationContext("fabric:/MyApp", "MyAppType", "Code", "Ver", "Context", "Log", "Temp", "Work", "Man", "ManVer");

			var serviceContext = MockStatefulServiceContextFactory.Create(context, serviceTypeName, newUri, partitionId, replicaId);

			var instance = MockActorServiceFactory.CreateActorServiceForActor<MockActor>(context: serviceContext);

			Assert.IsInstanceOfType(instance, typeof(MockActorService<MockActor>));
			Assert.AreEqual(serviceContext, instance.Context);
		}

		// ReSharper disable once ClassNeverInstantiated.Local   //implicit
		private class MockActor : Actor, IMockActor
		{
			public MockActor(ActorService actorService, ActorId actorId) : base(actorService, actorId)
			{
			}
		}

		private interface IMockActor : IActor
		{
			//Task DoStuffAsync();
		}
	}
}