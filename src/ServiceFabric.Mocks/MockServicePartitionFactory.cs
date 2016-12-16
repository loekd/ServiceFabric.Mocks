using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Health;
using System.Fabric.Query;
using System.Globalization;
using System.Reflection;
using Microsoft.ServiceFabric.Services.Client;

namespace ServiceFabric.Mocks
{
	/// <summary>
	/// Creates mocks of Service Partitions.
	/// </summary>
	public static class MockQueryPartitionFactory
	{
		/// <summary>
		/// Creates an <see cref="Int64RangePartitionInformation"/> with the provided values. 
		/// Can be used as argument to call <see cref="CreateStatefulPartition"/>.
		/// Can be used as argument to call <see cref="CreateStatelessPartition"/>.
		/// </summary>
		/// <param name="lowKey"></param>
		/// <param name="highKey"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		public static Int64RangePartitionInformation CreateIntPartitonInfo(long lowKey = long.MinValue, long highKey = long.MaxValue, Guid id = default(Guid))
		{
			//new Int64RangePartitionInformation(),
			var type = typeof(Int64RangePartitionInformation);
			var partition = (Int64RangePartitionInformation)Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.NonPublic, null, null, CultureInfo.CurrentCulture);

			type
				.GetProperty(nameof(Int64RangePartitionInformation.LowKey))
				.SetValue(partition, lowKey, BindingFlags.Instance | BindingFlags.NonPublic, null, null, CultureInfo.CurrentCulture);

			type
				.GetProperty(nameof(Int64RangePartitionInformation.HighKey))
				.SetValue(partition, highKey, BindingFlags.Instance | BindingFlags.NonPublic, null, null, CultureInfo.CurrentCulture);

			typeof(ServicePartitionInformation)
				.GetProperty(nameof(ServicePartitionInformation.Id))
				.SetValue(partition, id, BindingFlags.Instance | BindingFlags.NonPublic, null, null, CultureInfo.CurrentCulture);

			return partition;
		}

		/// <summary>
		/// Creates an <see cref="NamedPartitionInformation"/> with the provided values. 
		/// Can be used as argument to call <see cref="CreateStatelessPartition"/>.
		/// Can be used as argument to call <see cref="CreateStatefulPartition"/>.
		/// </summary>
		/// <returns></returns>
		public static NamedPartitionInformation CreateNamedPartitonInfo(string name = "MockPartitionOne", Guid id = default(Guid))
		{
			var type = typeof(NamedPartitionInformation);
			var partition = (NamedPartitionInformation)Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.NonPublic, null, null, CultureInfo.CurrentCulture);

			type
				.GetProperty(nameof(NamedPartitionInformation.Name))
				.SetValue(partition, name, BindingFlags.Instance | BindingFlags.NonPublic, null, null, CultureInfo.CurrentCulture);

			typeof(ServicePartitionInformation)
				.GetProperty(nameof(ServicePartitionInformation.Id))
				.SetValue(partition, id, BindingFlags.Instance | BindingFlags.NonPublic, null, null, CultureInfo.CurrentCulture);

			return partition;
		}

		/// <summary>
		/// Creates an <see cref="SingletonPartitionInformation"/> with the provided values. 
		/// Can be used as argument to call <see cref="CreateStatelessPartition"/>.
		/// Can be used as argument to call <see cref="CreateStatefulPartition"/>.
		/// </summary>
		/// <returns></returns>
		public static SingletonPartitionInformation CreateSingletonPartitonInfo(Guid id = default(Guid))
		{
			var partition = new SingletonPartitionInformation();
			
			typeof(ServicePartitionInformation)
				.GetProperty(nameof(ServicePartitionInformation.Id))
				.SetValue(partition, id, BindingFlags.Instance | BindingFlags.NonPublic, null, null, CultureInfo.CurrentCulture);

			return partition;
		}

		/// <summary>
		/// Creates a Stateful <see cref="Partition"/> with the provided values as would be returned as part of <see cref="ServicePartitionList"/> by
		/// from query <see cref="FabricClient.QueryClient.GetPartitionListAsync(System.Uri)"/>.
		/// </summary>
		/// <returns></returns>
		public static Partition CreateStatefulPartition(ServicePartitionInformation partitionInformation, long targetReplicaSetSize, long minReplicaSetSize,
			HealthState healthState, ServicePartitionStatus partitionStatus, TimeSpan lastQuorumLossDuration, Epoch primaryEpoch)
		{
			object[] param =
			{
				partitionInformation, targetReplicaSetSize, minReplicaSetSize, healthState, partitionStatus, lastQuorumLossDuration, primaryEpoch
			};
			//new StatefulServicePartition(new SingletonPartitionInformation(), 3, 3, HealthState.Ok, ServicePartitionStatus.Ready, TimeSpan.Zero, new Epoch())
			return (Partition)Activator.CreateInstance(typeof(StatefulServicePartition), BindingFlags.Instance | BindingFlags.NonPublic, null, param, CultureInfo.CurrentCulture);
		}


		/// <summary>
		/// Creates a Stateless <see cref="Partition"/> with the provided values as would be returned as part of <see cref="ServicePartitionList"/> by
		/// from query <see cref="FabricClient.QueryClient.GetPartitionListAsync(System.Uri)"/>.
		/// </summary>
		/// <returns></returns>
		public static Partition CreateStatelessPartition(ServicePartitionInformation partitionInformation, long instanceCount,
			HealthState healthState, ServicePartitionStatus partitionStatus)
		{
			object[] param =
			{
				partitionInformation, instanceCount, healthState, partitionStatus
			};
			//new StatelessServicePartition(new SingletonPartitionInformation(), 3, 3, HealthState.Ok, ServicePartitionStatus.Ready, TimeSpan.Zero, new Epoch())
			return (Partition)Activator.CreateInstance(typeof(StatelessServicePartition), BindingFlags.Instance | BindingFlags.NonPublic, null, param, CultureInfo.CurrentCulture);
		}

		/// <summary>
		/// Creates a <see cref="ResolvedServicePartition"/> with the provided values as would be returned by query <see cref="ServicePartitionResolver.ResolveAsync(System.Uri,Microsoft.ServiceFabric.Services.Client.ServicePartitionKey,System.Threading.CancellationToken)"/>
		/// </summary>
		/// <returns></returns>
		public static ResolvedServicePartition CreateResolvedServicePartition(Uri serviceName, List<ResolvedServiceEndpoint> endpoints)
		{
			//new ResolvedServicePartition();
			var type = typeof(ResolvedServicePartition);

			var resolvedServicePartition = (ResolvedServicePartition)Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.NonPublic, null, null, CultureInfo.CurrentCulture);

			type
				.GetProperty(nameof(ResolvedServicePartition.Endpoints))
				.SetValue(resolvedServicePartition, endpoints, BindingFlags.Instance | BindingFlags.NonPublic, null, null, CultureInfo.CurrentCulture);

			type
				.GetProperty(nameof(ResolvedServicePartition.ServiceName))
				.SetValue(resolvedServicePartition, serviceName, BindingFlags.Instance | BindingFlags.NonPublic, null, null, CultureInfo.CurrentCulture);

			return resolvedServicePartition;
		}

		/// <summary>
		/// Creates a <see cref="ResolvedServiceEndpoint"/> with the provided address 
		/// Can be used to call <see cref="CreateResolvedServicePartition"/>.
		/// </summary>
		/// <returns></returns>
		public static ResolvedServiceEndpoint CreateResolvedServiceEndpoint(string address)
		{
			//return new ResolvedServiceEndpoint();
			var type = typeof(ResolvedServiceEndpoint);
			var resolvedServiceEndpoint = (ResolvedServiceEndpoint)Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.NonPublic, null, null, CultureInfo.CurrentCulture);
			type
				.GetProperty(nameof(ResolvedServiceEndpoint.Address))
				.SetValue(resolvedServiceEndpoint, address, BindingFlags.Instance | BindingFlags.NonPublic, null, null, CultureInfo.CurrentCulture);

			return resolvedServiceEndpoint;
		}
	}
}
