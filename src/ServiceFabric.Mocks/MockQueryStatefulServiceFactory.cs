using System;
using System.Fabric;
using System.Fabric.Health;
using System.Fabric.Query;
using System.Globalization;
using System.Reflection;

namespace ServiceFabric.Mocks
{
	/// <summary>
	/// Creates mocks of <see cref="System.Fabric.Query.StatefulService"/>
	/// </summary>
	public static class MockQueryServiceFactory
	{
		/// <summary>
		/// Creates a <see cref="StatefulService"/> with the provided values as would be returned as list element 
		/// from query <see cref="FabricClient.QueryClient.GetServiceListAsync(System.Uri)"/>.
		/// </summary>
		/// <returns></returns>
		public static StatefulService CreateStatefulServiceInstance(Uri serviceName, string serviceTypeName, string serviceManifestVersion, bool hasPersistedState, HealthState healthState, ServiceStatus serviceStatus, bool isServiceGroup = false)
		{
			object[] param =
			{
				serviceName, serviceTypeName, serviceManifestVersion, hasPersistedState, healthState, serviceStatus, isServiceGroup
			};
			//new StatefulService(serviceName, "MockServiceType", "manifest", true, HealthState.Ok, ServiceStatus.Active, false)
			return (StatefulService)Activator.CreateInstance(typeof(StatefulService), BindingFlags.Instance | BindingFlags.NonPublic, null, param, CultureInfo.CurrentCulture);
		}

		/// <summary>
		/// Creates a <see cref="StatelessService"/> with the provided values as would be returned as list element
		/// from query <see cref="FabricClient.QueryClient.GetServiceListAsync(System.Uri)"/>.
		/// </summary>
		/// <returns></returns>
		public static StatelessService CreateStatelessServiceInstance(Uri serviceName, string serviceTypeName, string serviceManifestVersion, bool hasPersistedState, HealthState healthState, ServiceStatus serviceStatus, bool isServiceGroup = false)
		{
			object[] param =
			{
				serviceName, serviceTypeName, serviceManifestVersion, hasPersistedState, healthState, serviceStatus, isServiceGroup
			};
			//new StatelessService(serviceName, "MockServiceType", "manifest", true, HealthState.Ok, ServiceStatus.Active, false)
			return (StatelessService)Activator.CreateInstance(typeof(StatelessService), BindingFlags.Instance | BindingFlags.NonPublic, null, param, CultureInfo.CurrentCulture);
		}
	}
}