# ServiceFabric.Mocks
ServiceFabric.Mocks contains Mock classes to enable unit testing of Actors and Services

## Nuget package here:
https://www.nuget.org/packages/ServiceFabric.Mocks/

## Release notes

- 0.9.0-preview First implementation.  

## Unit Testing Actors

``` csharp
[StatePersistence(StatePersistence.Persisted)]
public class TestStatefulActor : Actor
{
    public TestStatefulActor(ActorService actorSerice, ActorId actorId)
        : base(actorSerice, actorId)
    {     }

    public async Task InsertAsync(string stateName, Payload value)
    {
        await StateManager.AddStateAsync(stateName, value);
    }
}

[TestMethod]
public async Task TestActorState()
{
    var actorGuid = Guid.NewGuid();
    var id = new ActorId(actorGuid);
    var stateManager = new MockActorStateManager();


    Func<ActorService, ActorId, ActorBase> actorFactory = (service, actorId) => new TestStatefulActor(service, id);
    Func<ActorBase, IActorStateProvider, IActorStateManager> stateManagerFactory = (actr, stateProvider) => stateManager;
    var svc = new ActorService(StatefulServiceContext, ActorTypeInformation.Get(typeof(SalesOrderActor)), actorFactory, stateManagerFactory);
    var actor = new TestStatefulActor(svc, id);

    string stateName = "test";
    var payload = new Payload();
    await actor.InsertAsync(stateName, payload);
    Assert.AreEqual(payload, await stateManager.GetStateAsync<Payload>(stateName));
}
```
