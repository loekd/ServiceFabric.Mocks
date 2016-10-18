using System;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace ServiceFabric.Mocks
{
    /// <summary>
    /// Contains state info about <see cref="IActorReminder"/>.
    /// </summary>
    public class MockActorReminderState : IActorReminderState
    {
        private readonly MockActorReminderData _reminderData;

        public TimeSpan RemainingDueTime { get; set; }

        public string Name => _reminderData.Name;

        public TimeSpan DueTime => _reminderData.DueTime;

        public TimeSpan Period => _reminderData.Period;

        public byte[] State => _reminderData.State;

        public MockActorReminderState(MockActorReminderData reminder, TimeSpan currentLogicalTime, MockReminderCompletedData reminderCompletedData = null)
        {
            _reminderData = reminder;
            if (reminderCompletedData != null)
                RemainingDueTime = ComputeRemainingTime(currentLogicalTime, reminderCompletedData.LogicalTime, reminder.Period);
            else
                RemainingDueTime = ComputeRemainingTime(currentLogicalTime, reminder.LogicalCreationTime, reminder.DueTime);
        }

        public void Complete(MockActorReminderData reminder, TimeSpan currentLogicalTime, MockReminderCompletedData reminderCompletedData)
        {
            RemainingDueTime = ComputeRemainingTime(currentLogicalTime, reminderCompletedData.LogicalTime, reminder.Period);
        }

        private static TimeSpan ComputeRemainingTime(TimeSpan currentLogicalTime, TimeSpan createdOrLastCompletedTime, TimeSpan dueTimeOrPeriod)
        {
            TimeSpan timeSpan1 = TimeSpan.Zero;
            if (currentLogicalTime > createdOrLastCompletedTime)
                timeSpan1 = currentLogicalTime - createdOrLastCompletedTime;
            TimeSpan timeSpan2 = TimeSpan.Zero;
            if (dueTimeOrPeriod > timeSpan1)
                timeSpan2 = dueTimeOrPeriod - timeSpan1;
            return timeSpan2;
        }
    }
}