using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Health;

namespace ServiceFabric.Mocks
{
    /// <inheritdoc />
    public class MockStatefulServicePartition : IStatefulServicePartition
    {
        /// <inheritdoc />
        public PartitionAccessStatus ReadStatus { get; set; }

        /// <inheritdoc />
        public PartitionAccessStatus WriteStatus { get; set; }

        /// <inheritdoc />
        public ServicePartitionInformation PartitionInfo { get; set; }
        
        /// <inheritdoc />
        public void ReportLoad(IEnumerable<LoadMetric> metrics)
        {
        }

        /// <inheritdoc />
        public void ReportFault(FaultType faultType)
        {
        }

        /// <inheritdoc />
        public void ReportMoveCost(MoveCost moveCost)
        {
        }

        /// <inheritdoc />
        public void ReportPartitionHealth(HealthInformation healthInfo)
        {
        }

        /// <inheritdoc />
        public void ReportPartitionHealth(HealthInformation healthInfo, HealthReportSendOptions sendOptions)
        {
        }
        
        /// <inheritdoc />
        public FabricReplicator CreateReplicator(IStateProvider stateProvider, ReplicatorSettings replicatorSettings)
        {
            return null;
        }

        /// <inheritdoc />
        public void ReportReplicaHealth(HealthInformation healthInfo)
        {
        }

        /// <inheritdoc />
        public void ReportReplicaHealth(HealthInformation healthInfo, HealthReportSendOptions sendOptions)
        {
        }

    }
}
