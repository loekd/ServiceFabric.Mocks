using System;
using System.Diagnostics;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.V1.Client;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceFabric.Mocks.Tests.Actors;
using ServiceFabric.Mocks.Tests.ActorServices;

namespace ServiceFabric.Mocks.Tests.ServiceRemoting
{
	[TestClass]
	public class ServiceRemotingTests
	{
		[TestMethod]
		public async Task TestRemotingFactoryAsync()
		{
			var service = new CustomActorService(MockStatefulServiceContextFactory.Default, ActorTypeInformation.Get(typeof(MyStatefulActor)));
			var factory = new MockActorServiceRemotingClientFactory(service);
			var client = await factory.GetClientAsync(new Uri("fabric:/App/Service"), ServicePartitionKey.Singleton,
				TargetReplicaSelector.Default, "Listener", new OperationRetrySettings(), CancellationToken.None);

			Assert.IsInstanceOfType(factory, typeof(IServiceRemotingClientFactory));
			Assert.IsInstanceOfType(client, typeof(IServiceRemotingClient));
			Assert.IsInstanceOfType(client, typeof(MockActorServiceRemotingClient));
			Assert.AreEqual("Listener", client.ListenerName);
		}


		[TestMethod]
		public async Task TestActorRemotingAsync()
		{
			var service = new CustomActorService(MockStatefulServiceContextFactory.Default, ActorTypeInformation.Get(typeof(MyStatefulActor)));
			var factory = new MockActorServiceRemotingClientFactory(service);
			var proxyFactory = new ActorProxyFactory(callbackClient => factory);
			var proxy = proxyFactory.CreateActorProxy<IMyStatefulActor>(ActorId.CreateRandom(), "App", "Service", "Listener");
			await proxy.InsertAsync("state", new Payload("content"));

			Assert.IsInstanceOfType(proxy, typeof(IMyStatefulActor));
		}
	}

	[TestClass]
	public class ActorEventTests
	{
		protected static bool IsSuccess = false;

		public interface IExampleEvents : IActorEvents
		{
			void OnSuccess(string msg);
		}

		public interface IExampleActor : IActor, IActorEventPublisher<IExampleEvents>
		{
			Task ActorSomething(string msg);
		}

		public class ExampleActorMock : Actor, IExampleActor
		{
			public ExampleActorMock(ActorService actorService, ActorId actorId) : base(actorService, actorId)
			{
			}

			public Task ActorSomething(string msg)
			{
				Debug.WriteLine("Actor:" + msg);
				var ev = GetEvent<IExampleEvents>();
				ev.OnSuccess(msg);
				return Task.FromResult(true);
			}
		}

		public interface IExampleService : IService
		{
			Task DoSomething(Guid id, string msg);
		}


		public class ExampleClient : StatefulService, IExampleService, IExampleEvents
		{
			private readonly IActorEventSubscriptionHelper _subscriptionHelper;
			private readonly IActorProxyFactory _actorProxyFactory;

			public ExampleClient(StatefulServiceContext serviceContext, IActorEventSubscriptionHelper subscriptionHelper)
				: base(serviceContext)
			{
				_subscriptionHelper = subscriptionHelper ?? new ActorEventSubscriptionHelper();
			}

			public ExampleClient(StatefulServiceContext serviceContext, IReliableStateManagerReplica2 reliableStateManagerReplica,
				IActorEventSubscriptionHelper subscriptionHelper, IActorProxyFactory actorProxyFactory)
				: base(serviceContext, reliableStateManagerReplica)
			{
				if (actorProxyFactory == null) throw new ArgumentNullException(nameof(actorProxyFactory));
				_subscriptionHelper = subscriptionHelper ?? new ActorEventSubscriptionHelper();
				_actorProxyFactory = actorProxyFactory;
			}

			public async Task DoSomething(Guid id, string msg)
			{
				var proxy = _actorProxyFactory.CreateActorProxy<IExampleActor>(new ActorId(id), "App", "Service", "Listener");
				await _subscriptionHelper.SubscribeAsync<IExampleEvents>(proxy, this);
				//await proxy.SubscribeAsync<IExampleEvents>(this);  //crashes if the caller is not of type ActorProxy, which is not the case when mocked.
				await proxy.ActorSomething(msg);
			}

			public void OnSuccess(string msg)
			{
				Debug.WriteLine("Service: " + msg);
				IsSuccess = true;
			}
		}


		[TestMethod]
		public async Task TestSubscribe_Doesnt_CrashAsync()
		{
			//var service = new CustomActorService(MockStatefulServiceContextFactory.Default, ActorTypeInformation.Get(typeof(MyStatefulActor)));
			//var factory = new MockActorServiceRemotingClientFactory(service);
			//var proxyFactory = new ActorProxyFactory(callbackClient => factory)

			var guid = Guid.NewGuid();
			var id = new ActorId(guid);
			Func<ActorService, ActorId, ActorBase> factory = (service, actorId) => new ExampleActorMock(service, actorId);
			var svc = MockActorServiceFactory.CreateActorServiceForActor<ExampleActorMock>(factory);
			var actor = svc.Activate(id);

			var mockProxyFactory = new MockActorProxyFactory();
			mockProxyFactory.RegisterActor(actor);

			var eventSubscriptionHelper = new MockActorEventSubscriptionHelper();
			var exampleService = new ExampleClient(MockStatefulServiceContextFactory.Default, new MockReliableStateManager(),
				eventSubscriptionHelper, mockProxyFactory);
			await exampleService.DoSomething(guid, "message text");

			Assert.IsTrue(eventSubscriptionHelper.IsSubscribed<IExampleEvents>(exampleService));
			Assert.IsFalse(IsSuccess);
				//Subscribe doesn't crash the test, but the Event is not really fired and processed at this time
		}

		[TestMethod]
		public async Task Alternative_TestSubscribe_Doesnt_CrashAsync()
		{
			var guid = Guid.NewGuid();
			var service = new CustomActorService(MockStatefulServiceContextFactory.Default, ActorTypeInformation.Get(typeof(MyStatefulActor)));
			var factory = new MockActorServiceRemotingClientFactory(service);

			var mockProxyFactory = new ActorProxyFactory(callbackClient => factory);
			var exampleService = new ExampleClient(MockStatefulServiceContextFactory.Default, new MockReliableStateManager(),
				null, mockProxyFactory);
			await exampleService.DoSomething(guid, "message text");

			Assert.IsFalse(IsSuccess); //Subscribe doesn't crash the test, but the Event is not really fired and processed at this time
		}
	}

}
