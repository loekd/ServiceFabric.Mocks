using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;

namespace ServiceFabric.Mocks
{  
    /// <summary>
   /// A sequence of operations performed as a single logical unit of work.
   /// </summary>
    public class MockTransaction : ITransaction
    {
        public long CommitSequenceNumber => 0L;

        public long TransactionId => 0L;

        public void Abort()
        {
        }

        public Task CommitAsync()
        {
            return Task.FromResult(true);
        }

        public void Dispose()
        {
        }

        public Task<long> GetVisibilitySequenceNumberAsync()
        {
            return Task.FromResult(0L);
        }
    }
}