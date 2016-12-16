using System.Collections.Generic;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace ServiceFabric.Mocks
{
    public class MockActorReminderCollection : Dictionary<ActorId, IReadOnlyCollection<IActorReminderState>>, IActorReminderCollection
    {
        public void Add(ActorId actorId, IActorReminderState reminderState)
        {
            IReadOnlyCollection<IActorReminderState> states;
            if (!TryGetValue(actorId, out states))
            {
                states = new List<IActorReminderState>();
                Add(actorId, states);
            }
            ((List<IActorReminderState>)states).Add(reminderState);
        }

        public void Delete(ActorId actorId, string reminderName)
        {
            IReadOnlyCollection<IActorReminderState> states;
            if (!TryGetValue(actorId, out states))
            {
                return;
            }
            ((List<IActorReminderState>)states).RemoveAll(r => string.Equals(r.Name, reminderName));
        }
    }
}