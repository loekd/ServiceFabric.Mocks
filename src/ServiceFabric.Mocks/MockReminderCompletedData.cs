using System;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace ServiceFabric.Mocks
{
    /// <summary>
    /// Describes a completed <see cref="IActorReminder"/>
    /// </summary>
    public class MockReminderCompletedData
    {
        public TimeSpan LogicalTime { get; set; }

        public DateTime UtcTime { get; set; }

        public MockReminderCompletedData(TimeSpan logicalTime, DateTime utcTime)
        {
            LogicalTime = logicalTime;
            UtcTime = utcTime;
        }

        public long EstimateDataLength()
        {
            return 16;
        }
    }
}