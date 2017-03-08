# ServiceFabric.Mocks
ServiceFabric.Mocks contains Mock classes to enable unit testing of Actors and Services

## Nuget package (preview) here:
https://www.nuget.org/packages/ServiceFabric.Mocks/

## Release notes

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
