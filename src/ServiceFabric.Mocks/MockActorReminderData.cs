using System;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace ServiceFabric.Mocks
{
    /// <summary>
    /// Describes an <see cref="IActorReminder"/>
    /// </summary>
    public class MockActorReminderData
    {
        public ActorId ActorId { get; set; }

        public string Name { get; set; }

        public TimeSpan DueTime { get; set; }

        public TimeSpan Period { get; set; }

        public byte[] State { get; set; }

        public TimeSpan LogicalCreationTime { get; set; }

        public bool IsReadOnly { get; set; }

        public MockActorReminderData(ActorId actorId, IActorReminder reminder, TimeSpan logicalCreationTime)
        {
            ActorId = actorId;
            Name = reminder.Name;
            DueTime = reminder.DueTime;
            Period = reminder.Period;
            State = reminder.State;
            LogicalCreationTime = logicalCreationTime;
        }
    }
}