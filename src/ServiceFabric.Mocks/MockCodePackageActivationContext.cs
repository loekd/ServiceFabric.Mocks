using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Fabric;
using System.Fabric.Description;
using System.Fabric.Health;

namespace ServiceFabric.Mocks
{
    /// <summary>
    /// Represents activation context for the Service Fabric activated service.
    /// </summary>
    /// <remarks>Includes information from the service manifest as well as information
    /// about the currently activated code package like work directory, context id etc.</remarks>
    public class MockCodePackageActivationContext : ICodePackageActivationContext
    {
        private bool _isDisposed;

        /// <summary>
        /// Returns a default instance, using mock values.
        /// </summary>
        public static ICodePackageActivationContext Default { get; } = new MockCodePackageActivationContext(
            "fabric:/MockApp",
            "MockAppType",
            "Code",
            "1.0.0.0",
            Guid.NewGuid().ToString(),
            @"C:\logDirectory",
            @"C:\tempDirectory",
            @"C:\workDirectory",
            "ServiceManifestName",
            "1.0.0.0"
        );

       

        public MockCodePackageActivationContext(
           string applicationName,
           string applicationTypeName,
           string codePackageName,
           string codePackageVersion,
           string context,
           string logDirectory,
           string tempDirectory,
           string workDirectory,
           string serviceManifestName,
           string serviceManifestVersion)
        {

            ApplicationName = applicationName;
            ApplicationTypeName = applicationTypeName;
            CodePackageName = codePackageName;
            CodePackageVersion = codePackageVersion;
            ContextId = context;
            LogDirectory = logDirectory;
            TempDirectory = tempDirectory;
            WorkDirectory = workDirectory;
            ServiceManifestName = serviceManifestName;
            ServiceManifestVersion = serviceManifestVersion;
        }

        public event EventHandler<PackageAddedEventArgs<CodePackage>> CodePackageAddedEvent;

        public event EventHandler<PackageModifiedEventArgs<CodePackage>> CodePackageModifiedEvent;

        public event EventHandler<PackageRemovedEventArgs<CodePackage>> CodePackageRemovedEvent;

        public event EventHandler<PackageAddedEventArgs<ConfigurationPackage>> ConfigurationPackageAddedEvent;

        public event EventHandler<PackageModifiedEventArgs<ConfigurationPackage>> ConfigurationPackageModifiedEvent;

        public event EventHandler<PackageRemovedEventArgs<ConfigurationPackage>> ConfigurationPackageRemovedEvent;

        public event EventHandler<PackageAddedEventArgs<DataPackage>> DataPackageAddedEvent;

        public event EventHandler<PackageModifiedEventArgs<DataPackage>> DataPackageModifiedEvent;

        public event EventHandler<PackageRemovedEventArgs<DataPackage>> DataPackageRemovedEvent;

        public string ApplicationName { get; set; }

        public ApplicationPrincipalsDescription ApplicationPrincipalsDescription { get; set; }
        public string ApplicationTypeName { get; set; }

        public CodePackage CodePackage { get; set; }
        public string CodePackageName { get; set; }

        public string CodePackageVersion { get; set; }

        public ConfigurationPackage ConfigurationPackage { get; set; }
        public List<string> ConfigurationPackageNames { get; set; }
        public string ContextId { get; set; }

        public DataPackage DataPackage { get; set; }
        public List<string> DataPackageNames { get; set; }
        public KeyedCollection<string, EndpointResourceDescription> EndpointResourceDescriptions { get; set; }
        public List<HealthInformation> HealthInformations { get; set; } = new List<HealthInformation>();
        public string LogDirectory { get; set; }

        public KeyedCollection<string, ServiceGroupTypeDescription> ServiceGroupTypes { get; set; }
        public string ServiceManifestName { get; set; }
        public string ServiceManifestVersion { get; set; }
        public KeyedCollection<string, ServiceTypeDescription> ServiceTypes { get; set; }
        public string TempDirectory { get; set; }

        public string WorkDirectory { get; set; }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
            }
        }

        public ApplicationPrincipalsDescription GetApplicationPrincipals()
        {
            return ApplicationPrincipalsDescription;
        }

        public IList<string> GetCodePackageNames()
        {
            return new List<string>() { CodePackageName };
        }

        public CodePackage GetCodePackageObject(string packageName)
        {
            return CodePackage;
        }

        public IList<string> GetConfigurationPackageNames()
        {
            return ConfigurationPackageNames;
        }

        public ConfigurationPackage GetConfigurationPackageObject(string packageName)
        {
            return ConfigurationPackage;
        }

        public IList<string> GetDataPackageNames()
        {
            return DataPackageNames;
        }

        public DataPackage GetDataPackageObject(string packageName)
        {
            return DataPackage;
        }

        public EndpointResourceDescription GetEndpoint(string endpointName)
        {
            return EndpointResourceDescriptions[endpointName];
        }

        public KeyedCollection<string, EndpointResourceDescription> GetEndpoints()
        {
            return EndpointResourceDescriptions;
        }

        public KeyedCollection<string, ServiceGroupTypeDescription> GetServiceGroupTypes()
        {
            return ServiceGroupTypes;
        }

        public string GetServiceManifestName()
        {
            return ServiceManifestName;
        }

        public string GetServiceManifestVersion()
        {
            return ServiceManifestVersion;
        }

        public KeyedCollection<string, ServiceTypeDescription> GetServiceTypes()
        {
            return ServiceTypes;
        }

        public void OnCodePackageAddedEvent(PackageAddedEventArgs<CodePackage> e)
        {
            CodePackageAddedEvent?.Invoke(this, e);
        }

        public void OnCodePackageModifiedEvent(PackageModifiedEventArgs<CodePackage> e)
        {
            CodePackageModifiedEvent?.Invoke(this, e);
        }

        public void OnCodePackageRemovedEvent(PackageRemovedEventArgs<CodePackage> e)
        {
            CodePackageRemovedEvent?.Invoke(this, e);
        }

        public void OnConfigurationPackageAddedEvent(PackageAddedEventArgs<ConfigurationPackage> e)
        {
            ConfigurationPackageAddedEvent?.Invoke(this, e);
        }

        public void OnConfigurationPackageModifiedEvent(PackageModifiedEventArgs<ConfigurationPackage> e)
        {
            ConfigurationPackageModifiedEvent?.Invoke(this, e);
        }

        public void OnConfigurationPackageRemovedEvent(PackageRemovedEventArgs<ConfigurationPackage> e)
        {
            ConfigurationPackageRemovedEvent?.Invoke(this, e);
        }

        public void OnDataPackageAddedEvent(PackageAddedEventArgs<DataPackage> e)
        {
            DataPackageAddedEvent?.Invoke(this, e);
        }

        public void OnDataPackageModifiedEvent(PackageModifiedEventArgs<DataPackage> e)
        {
            DataPackageModifiedEvent?.Invoke(this, e);
        }

        public void OnDataPackageRemovedEvent(PackageRemovedEventArgs<DataPackage> e)
        {
            DataPackageRemovedEvent?.Invoke(this, e);
        }

        public void ReportApplicationHealth(HealthInformation healthInformation)
        {
            HealthInformations?.Add(healthInformation);
        }

        public void ReportDeployedApplicationHealth(HealthInformation healthInformation)
        {
            HealthInformations?.Add(healthInformation);
        }

        public void ReportDeployedServicePackageHealth(HealthInformation healthInformation)
        {
            HealthInformations?.Add(healthInformation);
        }

        public void ReportApplicationHealth(HealthInformation healthInformation, HealthReportSendOptions sendOptions)
        {
            HealthInformations?.Add(healthInformation);
        }

        public void ReportDeployedApplicationHealth(HealthInformation healthInformation, HealthReportSendOptions sendOptions)
        {
            HealthInformations?.Add(healthInformation);
        }

        public void ReportDeployedServicePackageHealth(HealthInformation healthInformation, HealthReportSendOptions sendOptions)
        {
            HealthInformations?.Add(healthInformation);
        }
    }
}
