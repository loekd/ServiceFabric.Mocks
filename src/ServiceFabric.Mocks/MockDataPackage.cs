using System;
using System.Fabric;
using System.Fabric.Description;

namespace ServiceFabric.Mocks
{
    public class MockDataPackage
    {
        public static DataPackageDescription CreateDataPackageDescription(string name, string version, string serviceManifestName, string serviceManifestVersion, string path)
        {
            Type dataPackageDescriptionType = typeof(DataPackageDescription);
            var dataPackageDescription = ReflectionHelpers.CreateInstance<DataPackageDescription>();
            dataPackageDescriptionType.GetProperty(nameof(dataPackageDescription.Name)).SetValue(dataPackageDescription, name);
            dataPackageDescriptionType.GetProperty(nameof(dataPackageDescription.Version)).SetValue(dataPackageDescription, version);
            dataPackageDescriptionType.GetProperty(nameof(dataPackageDescription.ServiceManifestName)).SetValue(dataPackageDescription, serviceManifestName);
            dataPackageDescriptionType.GetProperty(nameof(dataPackageDescription.ServiceManifestVersion)).SetValue(dataPackageDescription, serviceManifestVersion);
#pragma warning disable CS0618 // Type or member is obsolete
            dataPackageDescriptionType.GetProperty(nameof(dataPackageDescription.Path)).SetValue(dataPackageDescription, path);
#pragma warning restore CS0618 // Type or member is obsolete

            return dataPackageDescription;
        }

        public static DataPackage CreateDataPackage(string path, DataPackageDescription description)
        {
            var dataPackage = ReflectionHelpers.CreateInstance<DataPackage>(new []{description});
            typeof(DataPackageDescription).GetProperty(nameof(PackageDescription.Path)).SetValue(description, path);
            return dataPackage;
        }

        public static DataPackage CreateDataPackage(string path)
        {
            Type dataPackageType = typeof(DataPackage);
            var dataPackage = ReflectionHelpers.CreateInstance<DataPackage>();
#pragma warning disable CS0618 // Type or member is obsolete
            DataPackageDescription description = CreateDataPackageDescription(nameof(DataPackageDescription.Name)
                , nameof(DataPackageDescription.Version), nameof(DataPackageDescription.ServiceManifestName)
                , nameof(DataPackageDescription.ServiceManifestVersion), path);
#pragma warning restore CS0618 // Type or member is obsolete

            dataPackageType.GetProperty(nameof(dataPackage.Description)).SetValue(dataPackage, description);
            return dataPackage;
        }
    }
}
