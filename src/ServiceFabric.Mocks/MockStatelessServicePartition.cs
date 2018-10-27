using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Health;

namespace ServiceFabric.Mocks
{
    /// <inheritdoc />
    public class MockStatelessServicePartition : IStatelessServicePartition
    {
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
        public ServicePartitionInformation PartitionInfo { get; set; }

        /// <inheritdoc />
        public void ReportInstanceHealth(HealthInformation healthInfo)
        {
        }

        /// <inheritdoc />
        public void ReportInstanceHealth(HealthInformation healthInfo, HealthReportSendOptions sendOptions)
        {
        }
    }
}