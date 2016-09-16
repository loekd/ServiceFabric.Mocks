# ServiceFabric.Mocks
ServiceFabric.Mocks contains Mock classes to enable unit testing of Actors and Services

## Nuget package (preview) here:
https://www.nuget.org/packages/ServiceFabric.Mocks/

## Release notes

- 0.9.1-preview Fixed issue in MockReliableStateManager. 
				Added unit tests.
- 0.9.0-preview First implementation.  

## Unit Testing Actors

``` csharp

### Define Actor under test
[StatePersistence(StatePersistence.Persisted)]
public class TestStatefulActor : Actor, ITestStatefulActor
{
    public TestStatefulActor(ActorService actorSerice, ActorId actorId)
        : base(actorSerice, actorId)
    { }

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

``` chsharp

private const string StatePayload = "some value";

[TestMethod]
public async Task TestActorState()
{
    var actorGuid = Guid.NewGuid();
    var id = new ActorId(actorGuid);
    var stateManager = new MockActorStateManager();

    var actor = CreateActor(id, stateManager);

    const string stateName = "test";
    var payload = new Payload(StatePayload);

    //create state
    await actor.InsertAsync(stateName, payload);

    //get state
    var actual = await stateManager.GetStateAsync<Payload>(stateName);
    Assert.AreEqual(payload.Content, actual.Content);
}

private static TestStatefulActor CreateActor(ActorId id, MockActorStateManager stateManager)
{
    Func<ActorService, ActorId, ActorBase> actorFactory = (service, actorId) => new TestStatefulActor(service, id);
    Func<ActorBase, IActorStateProvider, IActorStateManager> stateManagerFactory = (actr, stateProvider) => stateManager;
    var svc = new ActorService(MockStatefulServiceContextFactory.Default, ActorTypeInformation.Get(typeof(TestStatefulActor)), actorFactory, stateManagerFactory);
    var actor = new TestStatefulActor(svc, id);
    return actor;
}
```


## Unit Testing Stateful Services

### Define Stateful Service under test

``` chsharp
public class TestStatefulService : StatefulService
{
    public const string StateManagerDictionaryKey = "dictionaryname";
    public const string StateManagerQueueKey = "queuename";

    public TestStatefulService(StatefulServiceContext serviceContext) : base(serviceContext)
    {    }

    public TestStatefulService(StatefulServiceContext serviceContext, IReliableStateManagerReplica reliableStateManagerReplica)
        : base(serviceContext, reliableStateManagerReplica)
    {    }

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
}
```

### Create an instance using with the MockReliableStateManager and MockStatefulServiceContextFactory.Default

#### Test ReliableDictionary:

``` chsharp
 [TestMethod]
public async Task TestServiceState_Dictionary()
{
    var context = MockStatefulServiceContextFactory.Default;
    var stateManager = new MockReliableStateManager();
    var service = new TestStatefulService(context, stateManager);

    const string stateName = "test";
    var payload = new Payload(StatePayload);

    //create state
    await service.InsertAsync(stateName, payload);

    //get state
    var dictionary = await stateManager.TryGetAsync<IReliableDictionary<string, Payload>>(TestStatefulService.StateManagerDictionaryKey);
    var actual = (await dictionary.Value.TryGetValueAsync(null, stateName)).Value;
    Assert.AreEqual(payload.Content, actual.Content);
}
```

#### Test ReliableQueue:

``` chsharp
[TestMethod]
public async Task TestServiceState_Queue()
{
    var context = MockStatefulServiceContextFactory.Default;
    var stateManager = new MockReliableStateManager();
    var service = new TestStatefulService(context, stateManager);

    var payload = new Payload(StatePayload);

    //create state
    await service.EnqueueAsync(payload);

    //get state
    var queue = await stateManager.TryGetAsync<IReliableQueue<Payload>>(TestStatefulService.StateManagerQueueKey);
    var actual = (await queue.Value.TryPeekAsync(null)).Value;
    Assert.AreEqual(payload.Content, actual.Content);
}

```