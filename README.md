# ServiceFabric.Mocks
ServiceFabric.Mocks contains Mock classes to enable unit testing of Actors and Services

## Nuget package (preview) here:
https://www.nuget.org/packages/ServiceFabric.Mocks/

## Release notes

- 0.9.0-preview First implementation.  

## Unit Testing Actors

``` csharp

//Actor under test
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
```

using the MockActorStateManager:

``` chsarp
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
    var payload = new Payload("content");
    await actor.InsertAsync(stateName, payload);
    var actual = await stateManager.GetStateAsync<Payload>(stateName);
    
    Assert.AreEqual(payload.Content, actual.Content);
}
```

using the MockCodePackageActivationContext:

``` chsarp
private static readonly ICodePackageActivationContext CodePackageContext = new MockCodePackageActivationContext(
   "fabric:/MockApp",
   "MockAppType",
   "Code",
   "1.0.0.0",
   Guid.NewGuid().ToString(),
   @"C:\logDirectory",
   @"C:\tempDirectory",
   @"C:\workDirectory",
   "ServiceManifestName",
   "1.0.0.0"
   );

private static readonly StatefulServiceContext StatefulServiceContext = new StatefulServiceContext(
   new NodeContext("Node0", new NodeId(0, 1), 0, "NodeType1", "MOCK.MACHINE"),
   CodePackageContext, "MockServiceType",
   new Uri("fabric:/MockApp/MockService"),
   null,
   Guid.NewGuid(),
   long.MaxValue
   );
   
```

And mock immutable Payload
``` csharp
internal class Payload
{
    [DataMember]
    public readonly string Content;

    public Payload(string content)
    {
        Content = content;
    }
}
```
