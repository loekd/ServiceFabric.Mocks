# ServiceFabric.Mocks
ServiceFabric.Mocks contains many Mock and helper classes to facilitate and simplify unit testing of Service Fabric Actors and Services.

**Make sure to have a look at the [test](https://github.com/loekd/ServiceFabric.Mocks/tree/master/test) code within this repo, to see how everything works.**

## Nuget packages here:
- https://www.nuget.org/packages/ServiceFabric.Mocks/
- https://github.com/loekd?tab=packages&repo_name=ServiceFabric.Mocks

## Contribute!
Contributions are most welcome:

- Please use [conventional commits](https://www.conventionalcommits.org/en/v1.0.0/#summary) markup within pull requests.

Doing so will allow me to simply accept the PR, which will automatically trigger the release of a new package.
Please also make sure all feature additions have a corresponding unit test.

## Financial contributions
Sponsor me, or [donate](https://paypal.me/lduys/5) a cup of coffee.

Thank you for contributing!

## Release notes

- 7.1.0
    - First version that uses SF 7.0.1816
- 7.0.1
    - Deprecated, don't use this.
- 6.0.11
    - Added [releases](https://github.com/loekd/ServiceFabric.Mocks/releases)
    - Added package to [GH Nuget Feed](https://github.com/loekd?tab=packages&repo_name=ServiceFabric.Mocks) (also using regular NuGet)
    - Update pipeline to use Github Actions
    - Use Dependabot for automatic version upgrades
    - Added code owners (for auto reviewer assignment)
    - Added branch protection to `master`
    - Dependency upgrades will not require release notes anymore, so they can be automated.
    - Versioning is now based on [SemVer](https://semver.org/) and [conventional commits](https://www.conventionalcommits.org/en/v1.0.0/#summary) can be used to make changes other than simple patches.

- 6.0.5
    - Update Service Fabric SDK to version 6.0.1121
- 6.0.4
    - Update Service Fabric SDK to version 6.0.1107
	
- 6.0.3
    - Update Service Fabric SDK to version 6.0.1048

- 6.0.2
    - Update Service Fabric SDK to version 6.0.1028

- 6.0.1
    - Update Service Fabric SDK to version 6.0.1017
    - Implemented new GetRemindersAsync defined in IActorStateProvider 
- 6.0.0
    - Breaking change: deprecated support for .NET 4.5.2
    - Update Service Fabric SDK to version 5.2.1572
    - Implement LastException to include the exception thrown from RunAsync

- 5.0.6
    - Update Service Fabric SDK to version 5.2.1363
    - Fix issue #120 reported by gtcooke94. Notification for reliable dictionary clearing.

- 5.0.5
    - Update Service Fabric SDK to version 5.2.1235

- 5.0.4
    - Update Service Fabric SDK to version 5.1.329

- 5.0.3
    - Update Service Fabric SDK to version 5.1.316

- 5.0.2
    - Update Service Fabric SDK to version 5.0.521

- 5.0.1
    - Update Service Fabric SDK to version 5.0.516

- 5.0.0
    - Update Service Fabric SDK to version 5.0.514 (SF v8)
    - add `IStatelessServicePartition.ReportCompletion` to `MockStatelessServicePartition`. Thanks @JelmerBaas for the contribution!

- 4.2.9
    - Cleaned up indirect NuGet-References
    - Changed NuGet-References to a version range to avoid updating each version.

- 4.2.8
    - Add support for mocking `MockCodePackageActivationContext.DataPackage` using `MockDataPackage`. See test named 'DataPackageAtMockCodePackageActivationContextTest' .

- 4.2.7
    - Update Service Fabric SDK to version 4.2.445

- 4.2.6
    - Call `CancellationTokenSource.CancelAfter(0)` instead of `CancellationTokenSource.Cancel` to prevent blocking.

- 4.2.5
    - Update Service Fabric SDK to version 4.2.434.

- 4.2.4
    - Update Service Fabric SDK to version 4.1.458.

- 4.2.3
    - Fix null reference issue reported by kazepreaux. Inside `Lock.Wait`, the reference to `token` is now assigned within the lock.

- 4.2.2
    - Update Service Fabric SDK to version 4.1.456.

- 4.2.1
    - Update Service Fabric SDK to version 4.1.428.

- 4.2.0
    - Allow Services to implement RunAsync and throw OperationCancelledException to exit. Reported by jonblankenship. `ServiceExtensions.InvokeRunAsync` now runs a background task, instead of blocking.

- 4.1.5
    - Update Service Fabric SDK to version 4.1.417.

- 4.1.4
    - Update Service Fabric SDK to version 4.1.409.

- 4.1.3
    - MockStatefulServiceReplicaSet is now thread safe

- 4.1.2
    - upgraded to new SDK (SF 4.0.470)

- 4.1.1
    - upgraded to new SDK (SF 4.0.466)

- 4.1.0
    - fixed `CreateCustomActorServiceForActor` to provide unique state managers per actor id

- 4.0.0
    - upgraded to new SDK (SF 4.0.457)

- 3.4.20
	- minor change; throw KeyNotFound in MockActorStateManager.GetStateAsync when key does not exist

- 3.4.19
	- minor change; added InitializationData being passed to service context via context factory/replica set

- 3.4.18
    - upgraded to new SDK (SF 3.4.664)

- 3.4.17
    - upgraded to new SDK (SF 3.4.658)

- 3.4.16
    - upgraded to new SDK (SF 3.4.641)

- 3.4.15
    - upgraded to new SDK (SF 3.4.639)

- 3.4.14
    - upgraded to new SDK (SF 3.3.658)

- 3.4.13
    - minor change; allow setting of `RebuildNotificationAsyncCallback` on MockReliableDictionary. Reported by shanberg1.
    
- 3.4.12
    - minor fix; Open communication listeners after changing role instead of before. Reported by VigoC.

- 3.4.11
    - minor fix; parameter set mismatch for stateless service instance
    - upgraded to new SDK (SF 3.3.654)

- 3.4.10
    - Fix for issue 82 'Create Replica never returns if RunAsync is endless.' Reported by sportfort. 
    - RunAsync is now not awaited until the replica is closed or deleted.

- 3.4.9
    - Upgraded to new SDK (SF 3.3.644).

- 3.4.8
    - Fix ordering issue in TX rollback in `MockReliableQueue`. Reported by ayonalan.

- 3.4.7
    - Upgraded to new SDK (SF 3.3.638).

- 3.4.6
    - Downgraded to old SDK (SF 3.3.622).

- 3.4.5
    - Add support for `IServiceProxyFactory.CreateNonIServiceProxy`.

- 3.4.4
    - Upgraded to new SDK (SF 3.3.624).

- 3.4.3 
    - Minor fix; allow zero wait time for `ReliableConcurrentQueue` dequeue. Reported by CloudStu

- 3.4.2 
    - Minor fix; add additional lock attempt. By likevi-MSFT
    
- 3.4.1 
    - Add `ServiceExtensions.GetPartition` and `SetPartition` for alexmarshall132.

- 3.4.0
    - Fix issue 70. Racing for a lock could result in incorrect timeout. Thanks shushengli & likevi-MSFT.

- 3.3.11
    - Upgraded to new SDK (SF 3.2.187).

- 3.3.10
    - Upgraded to new SDK (SF 3.2.167).

- 3.3.9
    - Added default instances of keyed collections on `MockCodePackageActivationContext`. Requested by tharris29

- 3.3.8
    - Upgraded to new SDK (SF 3.2.162).

- 3.3.7
    - Upgraded to new SDK (SF 3.1.301).

- 3.3.6
    - Upgraded to new SDK (SF 3.1.283).

- 3.3.5
    - Update required to test DLL signing.

- 3.3.4
    - Merged PR by Scythen. Fix for issue in MissingActor event using multiple Actor types with the same id.

- 3.3.3
    - Upgraded to new SDK (SF 3.1.274).

- 3.3.2
    - Merged PR by charleszipp. Adds additional stateful service role change test options.

- 3.3.1
    - Upgraded to new SDK (SF 3.1.262).

- 3.3.0
    - Merged PR by charleszipp. Adds stateful service role change test options.

- 3.2.3
    - Upgraded to new SDK (SF 3.0.480).

- 3.2.2
    - Upgraded to new SDK (SF 3.0.472).

- 3.2.1
    - Upgraded to new SDK (SF 3.0.467). Bumped .NET Fx min version to 4.5.2

- 3.2.0
  - Add support for netstandard 2.0. Thanks @Scythen and @Eli Pulsifer.

- 3.1.7
  - Add `ServiceExtensions` to invoke non-public operations on Services.

- 3.1.6
  - Upgraded to new SDK (SF 3.0.456)

- 3.1.5
  - Fixed bug in SaveReminderAsync which did not update existing reminder if called multiple times with same reminder name. Now updates existing reminder, if any.
- Fixed bug in GetActorReminders which threw exception if called when no reminders had been registered at all. Now returns empty collection.

- 3.1.4
  - Fixed bug in TransactedConcurrentDictionary.SetAsync which threw ArgumentException when key was not already present. Updated to follow MS documentation:
    Adds a key/value pair to the Reliable Dictionary if the key does not already exist, or updates a key/value pair in the Reliable Dictionary if the key already exists.
    Fixes issue #44.

- 3.1.3
  - Merged PR by Jaah. Fix creation of `IReliableDictionary2` collections

- 3.1.2
  - Merged PR by Jaah. Add support for `IReliableDictionary2`

- 3.1.1
    - Sign assembly

- 3.1.0
    - Upgraded nuget packages (SF 2.8.232)

- 3.0.1
    - Merged PR by esbenbach, fix `ActorStateManager` `TryAddStateAsync` behavior not same as actual implementation. Added unit test.

- 3.0.0
    - Merged PR by ralphcu
    - Breaking change: list of past Transactions replaced by events (`MockTransactionChanged`)
    - Created Lock and LockManager classes to try to simulate the locking and blocking of SF transactions. Added unit tests.
    - Created TransactedConcurrentDictionary class which is now used by MockReliableDictionary and MockReliableStateManager. Added unit tests.
      o OnStateManagerChanged is now fired on CommitAsync() of transaction used to add or remove.
      o OndictionaryChanged is now fired on CommitAsync() of transaction used for add, remove, update.
    - Add transaction support to MockReliableQueue and MockReliableConcurrentQueue.
    - Reworked MockreliableDictionary changes to work with above changes.


- 2.4.0
    - Upgraded nuget packages (SF 2.8.219)
    
- 2.3.2
    - Merged PR by huesie, fix exception type thrown for duplicate key in `MockReliableDictionary`. Added unit tests.

- 2.3.1
    - Merged PR by Izzmo, adding a list of past Transactions to the `MockReliableStateManager`. Added unit tests

- 2.3.0
    - Merged PR by dotnetgator, upgrading to SF 2.8.211

- 2.2.4
    - Added `InvokeOnPreActorMethodAsync` & `InvokeOnPostActorMethodAsync` on `ActorBaseExtensions`, requested by JefSchraag.

- 2.2.4
    - Fixed `MockActorStateManager` issue, when calling `SetStateAsync` with different types for T. Found by samneirinck.

- 2.2.3
    - Added `MockConfigurationPackage` to mock service configuration for CharlesZhong

- 2.2.2
    - Upgraded nuget packages (SF 2.7.198)

- 2.2.1
    - Allow MockActorServiceFactory to create several actors

- 2.2.0
    - Upgraded nuget packages (SF 2.6.220, MSTest 1.1.18)

- 2.1.0
    - Upgraded nuget packages (SF 2.6.210)

- 2.0.0
    - Upgraded sln to VS2017

- 1.9.0
    - Upgraded nuget packages (SF 2.6.204)
    - ReliableConcurrentQueue no longer preview

- 1.8.1
    - Fix issue in MockActorStateManager.AddStateAsync not throwing on duplicate keys, found by mackgyver2k.

- 1.8.0
    - Added MockActorServiceRemotingClientFactory and MockActorServiceRemotingClient to mock the IServiceRemotingCallbackClient of ActorProxyFactory.
      (still looking for a way to mock remoting though)
    - Added example use of MockActorEventSubscriptionHelper for VDBorovikov

- 1.7.0
    - Upgraded nuget packages (SF 2.5.216)

- 1.6.3
    - Merged PR by kotvisbj. Added 'MissingActor' event to MockActorProxyFactory, to dynamically resolve Actor instances if needed. 
    - Fixed some code quality issues.

- 1.6.2
    - MockReliableConcurrentQueue Name property now has public get and set.

- 1.6.1
    - merged PR by Scythen that sets the Name of a ReliableCollection when it's created.

- 1.6.0
    - upgraded to new SDK (2.4.164)
    - added MockActorEventSubscriptionHelper to assist with testing Actor Events, as suggested by ThiemeNL.

- 1.5.0 
    - Added support for custom ActorServices. (For andrejohansson)
      (See 'CustomActorServiceTests' for details on using custom ActorServices.)
    - Added ActorServiceExtensions class to enable Actor activation from any ActorService implementation.

- 1.4.1
    - Fixed issue in ActorStateManager TryGetStateAsync using valuetype, found by massimobonanni 

- 1.4.0 
    - upgraded to new SDK (2.4.145)

- 1.3.2
    - Add MockServicePartitionFactory to create mock service partitions.
    - Add option to invoke OnActivateAsync while creating an Actor.

- 1.3.1 
    - Merged pull request from moswald to allow multiple instances of different Actor types share the same ActorId

- 1.3.0
    - upgraded to new SDK (2.3.311)

- 1.2.0
    - Merged pull request from WonderPanda
    - add customizable service context factories
    - add some unit test 

- 1.1.0 
    - add MockActorStateProvider to unit test reminder registration **(Note: Reminders are not automatically triggered by the mock.)**
    - add MockActorService with a method that creates Actor instances
    - changed MockActorServiceFactory to support MockActorStateProvider and MockActorService
    - add ReminderTimerActor to demo unit tests for timers and reminders (registration)

- 1.0.0 
    - upgraded to new SDK and packages (2.3.301) 

- 0.9.4 
    - add MockActorProxyFactory, MockActorServiceFactory, MockStatelessServiceContextFactory
    - added samples to Mock Service and Actor proxies.

- 0.9.3 
    - add support for the preview collection type 'IReliableConcurrentQueue<T>' using MockReliableConcurrentQueue<T>

- 0.9.2 
    - no longer preview

- 0.9.1-preview 
    - Fixed issue in MockReliableStateManager. 
    - Added unit tests.

- 0.9.0-preview 
    - First implementation.  

## Unit Testing Actors

### Define Actor under test

``` csharp
[StatePersistence(StatePersistence.Persisted)]
public class MyStatefulActor : Actor, IMyStatefulActor
{

    public MyStatefulActor(ActorService actorSerice, ActorId actorId)
        : base(actorSerice, actorId)
    {
    }

    public async Task InsertAsync(string stateName, Payload value)
    {
        await StateManager.AddStateAsync(stateName, value);
    }
}

[DataContract]
public class Payload
{
    [DataMember]
    public readonly string Content;

    public Payload(string content)
    {
        Content = content;
    }
}
```

### Create an instance using an ActorService with the MockActorStateManager and MockStatefulServiceContextFactory.Default

``` csharp
private const string StatePayload = "some value";

[TestMethod]
public async Task TestActorState()
{
    var actorGuid = Guid.NewGuid();
    var id = new ActorId(actorGuid);

    var actor = CreateActor(id);
    var stateManager = (MockActorStateManager)actor.StateManager;


    const string stateName = "test";
    var payload = new Payload(StatePayload);

    //create state
    await actor.InsertAsync(stateName, payload);

    //get state
    var actual = await stateManager.GetStateAsync<Payload>(stateName);
    Assert.AreEqual(StatePayload, actual.Content);

}

internal static MyStatefulActor CreateActor(ActorId id)
{
    Func<ActorService, ActorId, ActorBase> actorFactory = (service, actorId) => new MyStatefulActor(service, id);
    var svc = MockActorServiceFactory.CreateActorServiceForActor<MyStatefulActor>(actorFactory);
    var actor = svc.Activate(id);
    return actor;
}
```


## Unit Testing Stateful Services

### Define Stateful Service under test

``` csharp
public class MyStatefulService : StatefulService, IMyStatefulService
{
    public const string StateManagerDictionaryKey = "dictionaryname";
    public const string StateManagerQueueKey = "queuename";
    public const string StateManagerConcurrentQueueKey = "concurrentqueuename";


    public MyStatefulService(StatefulServiceContext serviceContext) : base(serviceContext)
    {
    }

    public MyStatefulService(StatefulServiceContext serviceContext, IReliableStateManagerReplica reliableStateManagerReplica)
        : base(serviceContext, reliableStateManagerReplica)
    {
    }

    public async Task InsertAsync(string stateName, Payload value)
    {
        var dictionary = await StateManager.GetOrAddAsync<IReliableDictionary<string, Payload>>(StateManagerDictionaryKey);

        using (var tx = StateManager.CreateTransaction())
        {
            await dictionary.TryAddAsync(tx, stateName, value);
            await tx.CommitAsync();
        }
    }


    public async Task EnqueueAsync(Payload value)
    {
        var queue = await StateManager.GetOrAddAsync<IReliableQueue<Payload>>(StateManagerQueueKey);

        using (var tx = StateManager.CreateTransaction())
        {
            await queue.EnqueueAsync(tx, value);
            await tx.CommitAsync();
        }
    }

    public async Task ConcurrentEnqueueAsync(Payload value)
    {
        var concurrentQueue = await StateManager.GetOrAddAsync<IReliableConcurrentQueue<Payload>>(StateManagerConcurrentQueueKey);

        using (var tx = StateManager.CreateTransaction())
        {
            await concurrentQueue.EnqueueAsync(tx, value);
            await tx.CommitAsync();
        }
    }
}
```

### Create an instance using with the MockReliableStateManager and MockStatefulServiceContextFactory.Default

#### Test ReliableDictionary:

``` csharp
 [TestMethod]
public async Task TestServiceState_Dictionary()
{
    var context = MockStatefulServiceContextFactory.Default;
    var stateManager = new MockReliableStateManager();
    var service = new MyStatefulService(context, stateManager);

    const string stateName = "test";
    var payload = new Payload(StatePayload);

    //create state
    await service.InsertAsync(stateName, payload);

    //get state
    var dictionary = await stateManager.TryGetAsync<IReliableDictionary<string, Payload>>(MyStatefulService.StateManagerDictionaryKey);
    var actual = (await dictionary.Value.TryGetValueAsync(null, stateName)).Value;
    Assert.AreEqual(StatePayload, actual.Content);
}
```

#### Test ReliableQueue:

``` csharp
[TestMethod]
public async Task TestServiceState_Queue()
{
    var context = MockStatefulServiceContextFactory.Default;
    var stateManager = new MockReliableStateManager();
    var service = new MyStatefulService(context, stateManager);

    var payload = new Payload(StatePayload);

    //create state
    await service.EnqueueAsync(payload);

    //get state
    var queue = await stateManager.TryGetAsync<IReliableQueue<Payload>>(MyStatefulService.StateManagerQueueKey);
    var actual = (await queue.Value.TryPeekAsync(null)).Value;
    Assert.AreEqual(StatePayload, actual.Content);
}
```

#### Test ReliableConcurrentQueue

``` csharp
[TestMethod]
public async Task TestServiceState_ConcurrentQueue()
{
    var context = MockStatefulServiceContextFactory.Default;
    var stateManager = new MockReliableStateManager();
    var service = new MyStatefulService(context, stateManager);

    var payload = new Payload(StatePayload);

    //create state
    await service.ConcurrentEnqueueAsync(payload);

    //get state
    var queue = await stateManager.TryGetAsync<IReliableConcurrentQueue<Payload>>(MyStatefulService.StateManagerConcurrentQueueKey);
    var actual = (await queue.Value.DequeueAsync(null));
    Assert.AreEqual(StatePayload, actual.Content);
}
```

## Communication between Actors and Services
Works by injecting IServiceProxyFactory and/or IActorProxyFactory Mocks into Actors and Services. The factories will create Mock Proxies.

### Mocking out called Actors

#### Create Service Under Test

``` csharp
public class ActorCallerService : StatelessService
{
    public static readonly Guid CalledActorId = Guid.Parse("{1F263E8C-78D4-4D91-AAE6-C4B9CE03D6EB}");

    public IActorProxyFactory ProxyFactory { get; }

    public ActorCallerService(StatelessServiceContext serviceContext, IActorProxyFactory proxyFactory = null) 
        : base(serviceContext)
    {
        ProxyFactory = proxyFactory ?? new ActorProxyFactory();
    }

    public async Task CallActorAsync()
    {
        var proxy = ProxyFactory.CreateActorProxy<IMyStatefulActor>(new ActorId(CalledActorId));
        var value = new Payload("some other value");
        await proxy.InsertAsync("test", value);
    }
}
```

#### Create Service Test

``` csharp
[TestMethod]
public async Task TestActorProxyFactory()
{
    //mock out the called actor
    var id = new ActorId(ActorCallerService.CalledActorId);
    Func<ActorService, ActorId, ActorBase> actorFactory = (service, actorId) => new MockTestStatefulActor(service, id);
    var svc = MockActorServiceFactory.CreateActorServiceForActor<MockTestStatefulActor>(actorFactory);
    var actor = svc.Activate(id);

    //prepare the service:
    var mockProxyFactory = new MockActorProxyFactory();
    mockProxyFactory.RegisterActor(actor);
    var serviceInstance = new ActorCallerService(MockStatelessServiceContextFactory.Default, mockProxyFactory);

    //act:
    await serviceInstance.CallActorAsync();

    //assert:
    Assert.IsTrue(actor.InsertAsyncCalled);
}


private class MockTestStatefulActor : Actor, IMyStatefulActor
{
    public bool InsertAsyncCalled { get; private set; }

    public MockTestStatefulActor(ActorService actorService, ActorId actorId) : base(actorService, actorId)
    {
    }

    public Task InsertAsync(string stateName, Payload value)
    {
        InsertAsyncCalled = true;
        return Task.FromResult(true);
    }
}
```

### Mocking out called Services

#### Create Actor Under Test

``` csharp
public class ServiceCallerActor : Actor, IMyStatefulActor
{
    public static readonly Uri CalledServiceName = new Uri("fabric:/MockApp/MockStatefulService");

    public IServiceProxyFactory ServiceProxyFactory { get; }

    public ServiceCallerActor(ActorService actorService, ActorId actorId, IServiceProxyFactory serviceProxyFactory) 
    : base(actorService, actorId)
    {
        ServiceProxyFactory = serviceProxyFactory ?? new ServiceProxyFactory();
    }

    public Task InsertAsync(string stateName, Payload value)
    {
        var serviceProxy = ServiceProxyFactory.CreateServiceProxy<IMyStatefulService>(CalledServiceName, new ServicePartitionKey(0L));
        return serviceProxy.InsertAsync(stateName, value);
    }
}
```

#### Create Actor Test

``` csharp
[TestMethod]
public async Task TestServiceProxyFactory()
{
    //mock out the called service
    var mockProxyFactory = new MockServiceProxyFactory();
    var mockService = new MockTestStatefulService();
    mockProxyFactory.RegisterService(ServiceCallerActor.CalledServiceName, mockService);

    //prepare the actor:
    Func<ActorService, ActorId, ActorBase> actorFactory = (service, actorId) => new ServiceCallerActor(service, actorId, mockProxyFactory);
    var svc = MockActorServiceFactory.CreateActorServiceForActor<ServiceCallerActor>(actorFactory);
    var actor = svc.Activate(ActorId.CreateRandom());

    //act:
    await actor.InsertAsync("test", new Payload("some other value"));

    //assert:
    Assert.IsTrue(mockService.InsertAsyncCalled);
}
        
private class MockTestStatefulService : IMyStatefulService
{
    public bool InsertAsyncCalled { get; private set; }

    public Task ConcurrentEnqueueAsync(Payload value)
    {
        throw new NotImplementedException();
    }

    public Task EnqueueAsync(Payload value)
    {
        throw new NotImplementedException();
    }

    public Task InsertAsync(string stateName, Payload value)
    {
        InsertAsyncCalled = true;
        return Task.FromResult(true);
    }
}
```

### Mocking out called Actors

#### Create Actor Dynamically Within Another Actor Test

An actor is created from the ActorCallerActor and we need to test that it is there and that its state was set.

``` csharp
public class ActorCallerActor : Actor, IMyStatefulActor
{
    public static readonly Uri CalledServiceName = new Uri("fabric:/MockApp/MyStatefulActor");
    public const string ChildActorIdKeyName = "ChildActorIdKeyName";

    public IActorProxyFactory ActorProxyFactory { get; }

    public ActorCallerActor(ActorService actorService, ActorId actorId, IActorProxyFactory actorProxyFactory) 
    : base(actorService, actorId)
    {
        ActorProxyFactory = actorProxyFactory ?? new ActorProxyFactory();
    }

    public Task InsertAsync(string stateName, Payload value)
    {
        var actorProxy = ActorProxyFactory.CreateActorProxy<IMyStatefulActor>(CalledServiceName, new ActorId(Guid.NewGuid()));

        this.StateManager.SetStateAsync(ChildActorIdKeyName, actorProxy.GetActorId());

        return actorProxy.InsertAsync(stateName, value);

    }
}
```

#### Create Actor Test
Here a callback is used when the actor is requested and not found that will allow you to create it with the identifier defined in the original actor.  So you can test it.

``` csharp
[TestMethod]
public async Task TestServiceProxyFactory()
{
    //mock out the called service

    var mockProxyFactory = new MockActorProxyFactory();
    mockProxyFactory.MissingActor += MockProxyFactory_MissingActorId;
    

    //prepare the actor:
    Func<ActorService, ActorId, ActorBase> actorFactory = (service, actorId) => new ActorCallerActor(service, actorId, mockProxyFactory);
    var svc = MockActorServiceFactory.CreateActorServiceForActor<ActorCallerActor>(actorFactory);
    var actor = svc.Activate(ActorId.CreateRandom());

    //act:
    await actor.InsertAsync("test", new Payload("some other value"));

    //check if the other actor was called
    var statefulActorId = await actor.StateManager.GetStateAsync<ActorId>(ActorCallerActor.ChildActorIdKeyName);

    Func<ActorService, ActorId, ActorBase> statefulActorFactory = (service, actorId) => new MyStatefulActor(service, actorId);
    var statefulActor = mockProxyFactory.CreateActorProxy<IMyStatefulActor>(ActorCallerActor.CalledServiceName, statefulActorId);
    
    var payload = await ((MyStatefulActor)statefulActor).StateManager.GetStateAsync<Payload>("test");

    //assert:
    Assert.AreEqual("some other value", payload.Content);
}

private void MockProxyFactory_MissingActorId(object sender, MissingActorEventArgs args)
{
    var registrar = (MockActorProxyFactory)sender;

    Func<ActorService, ActorId, ActorBase> actorFactory = (service, actorId) => new MyStatefulActor(service, actorId);
    var svc = MockActorServiceFactory.CreateActorServiceForActor<MyStatefulActor>(actorFactory);
    var actor = svc.Activate(args.Id);
    registrar.RegisterActor(actor);
}
```

### Test Actor Reminders and timers

#### Actor under test:

``` csharp
public class ReminderTimerActor : Actor, IRemindable, IReminderTimerActor
{
    public ReminderTimerActor(ActorService actorService, ActorId actorId) : base(actorService, actorId)
    {
    }

    public Task RegisterReminderAsync(string reminderName)
    {
        return RegisterReminderAsync(reminderName, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(-1));
    }

    public Task ReceiveReminderAsync(string reminderName, byte[] context, TimeSpan dueTime, TimeSpan period)
    {
        //will not be called automatically.
        return Task.FromResult(true);
    }


    public Task RegisterTimerAsync()
    {
        RegisterTimer(TimerCallbackAsync, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(-1));
        return Task.FromResult(true);
    }
        
    private Task TimerCallbackAsync(object state)
    {
        //will not be called automatically.
        return Task.FromResult(true);
    }
}
```

#### Test code:

``` csharp
[TestMethod]
public async Task TestActorReminderRegistration()
{
    var svc = MockActorServiceFactory.CreateActorServiceForActor<ReminderTimerActor>();
    var actor = svc.Activate(new ActorId(Guid.NewGuid()));
    string reminderName = "reminder";
            
    //setup
    await actor.RegisterReminderAsync(reminderName);
           
    //assert
    var reminderCollection = actor.GetActorReminders(); //extension method
    bool hasReminder = reminderCollection.Any();
    Assert.IsTrue(hasReminder);
}


[TestMethod]
public async Task TestActorTimerRegistration()
{
    var svc = MockActorServiceFactory.CreateActorServiceForActor<ReminderTimerActor>();
    var actor = svc.Activate(new ActorId(Guid.NewGuid()));

    //setup
    await actor.RegisterTimerAsync();

    //assert
    var timers = actor.GetActorTimers(); //extension method
    bool hasTimer = timers.Any();
    Assert.IsTrue(hasTimer);
}
```


## Using your custom ActorService implementations

You can use a custom ActorService implementations to create your Actor instances.

### Custom ActorService

``` csharp
public class CustomActorService : ActorService
{
    //no additional constructor parameters
    public CustomActorService(StatefulServiceContext context, ActorTypeInformation actorTypeInfo, Func<ActorService, ActorId, 
        ActorBase> actorFactory = null, Func<ActorBase, IActorStateProvider, IActorStateManager> stateManagerFactory = null, 
        IActorStateProvider stateProvider = null, ActorServiceSettings settings = null) : base(context, actorTypeInfo, 
        actorFactory, stateManagerFactory, stateProvider, settings)
    {
    }
}
``` 

#### Test code

``` csharp
//an ActorService with a standard constructor can be created by the MockActorServiceFactory
var customActorService = MockActorServiceFactory.CreateCustomActorServiceForActor<CustomActorService, OnActivateActor>();
var actor = customActorService.Activate<OnActivateActor>(new ActorId(123L));

Assert.IsInstanceOfType(customActorService, typeof(CustomActorService));
Assert.IsInstanceOfType(actor, typeof(OnActivateActor));
Assert.AreEqual(123L, actor.Id.GetLongId());
```

### ActorService with non standard constructor
In this situation you can't use the MockActorServiceFactory, so you'll need to create an instance directly.


//an ActorService with a NON standard constructor can be created by passing Mock arguments:

``` csharp
var stateManager = new MockActorStateManager();
Func<ActorBase, IActorStateProvider, IActorStateManager> stateManagerFactory = (actr, stateProvider) => stateManager;

IActorStateProvider actorStateProvider = new MockActorStateProvider();
actorStateProvider.Initialize(ActorTypeInformation.Get(typeof(OnActivateActor)));
var context = MockStatefulServiceContextFactory.Default;
var dummy = new object(); //this argument causes the 'non standard' ctor.
var customActorService = new AnotherCustomActorService(dummy, context, ActorTypeInformation.Get(typeof(OnActivateActor)));

var actor = customActorService.Activate<OnActivateActor>(new ActorId(123L));
Assert.IsInstanceOfType(actor, typeof(OnActivateActor));
Assert.AreEqual(123L, actor.Id.GetLongId());
```

#### Testing service configuration

To inject a configuration section into the MockCodePackageActivationContext, you can use this code:

```
[TestClass]
    public class ConfigurationPackageTests
    {
        [TestMethod]
        public void ConfigurationPackageAtMockCodePackageActivationContextTest()
        {
            //build ConfigurationSectionCollection
            var configSections = new ConfigurationSectionCollection();

            //Build ConfigurationSettings
            var configSettings = CreateConfigurationSettings(configSections);

            //add one ConfigurationSection
            ConfigurationSection configSection = CreateConfigurationSection(nameof(configSection.Name));
            configSections.Add(configSection);

            //add one Parameters entry
            ConfigurationProperty parameter = CreateConfigurationSectionParameters(nameof(parameter.Name), nameof(parameter.Value));
            configSection.Parameters.Add(parameter);
            
            //Build ConfigurationPackage
            ConfigurationPackage configPackage = CreateConfigurationPackage(configSettings, nameof(configPackage.Path));
        
        [..]
        }
     }
}
     
```
