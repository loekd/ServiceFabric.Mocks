using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;

namespace ServiceFabric.Mocks
{  
    /// <summary>
    /// A sequence of operations performed as a single logical unit of work.
    /// </summary>
    public class MockTransaction : ITransaction
    {
        public int InstanceCount { get; }

        public long CommitSequenceNumber => 0L;

        public bool IsCommitted { get; private set; }

        public bool IsAborted { get; private set; }

        public bool IsCompleted => IsCommitted || IsAborted;

        public long TransactionId => 0L;

        public MockTransaction(int instanceCount)
        {
            InstanceCount = instanceCount;
        }

        public void Abort()
        {
            if (!IsCommitted)
            {
                IsAborted = true;
            }
        }

        public Task CommitAsync()
        {
            if (!IsAborted)
            {
                IsCommitted = true;
            }
            return Task.FromResult(true);
        }

        public void Dispose()
        {
            if (!IsCommitted)
            {
                IsAborted = true;
            }
        }

        public Task<long> GetVisibilitySequenceNumberAsync()
        {
            return Task.FromResult(0L);
        }
    }
}