using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;

namespace ServiceFabric.Mocks.Tests
{
    public interface IReminderTimerActor : IActor
    {
        /// <summary>
        /// Explicitly registers a reminder.
        /// </summary>
        /// <returns></returns>
        Task RegisterReminderAsync(string reminderName);

        /// <summary>
        /// Explicitly registers a timer.
        /// </summary>
        /// <returns></returns>
        Task RegisterTimerAsync();
    }
}