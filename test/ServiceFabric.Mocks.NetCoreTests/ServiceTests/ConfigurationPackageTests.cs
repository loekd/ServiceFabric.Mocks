using System;
using System.Fabric;
using System.Fabric.Description;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static ServiceFabric.Mocks.MockConfigurationPackage;

namespace ServiceFabric.Mocks.NetCoreTests.ServiceTests
{
    [TestClass]
    public class ConfigurationPackageTests
    {
        [TestMethod]
        public void DataPackageAtMockCodePackageActivationContextTest()
        {
            //arrange
            const string path = "some://path";
            var context = MockCodePackageActivationContext.Default;
            const string name = "name";
            const string version = "version";
            const string serviceManifestName = "manifestName";
            const string serviceManifestVersion = "manifestVersion";
            var dataPackageDescription = MockDataPackage.CreateDataPackageDescription(name, version, serviceManifestName, serviceManifestVersion, path);
            var dataPackage = MockDataPackage.CreateDataPackage(path, dataPackageDescription);
            //act
            context.SetDataPackage(dataPackage);

            //assert
            DataPackage actual = context.GetDataPackageObject("<<anything>>");
            Assert.AreEqual(dataPackage, actual);
            Assert.AreEqual(path, actual.Path);
            Assert.AreEqual(name, actual.Description.Name);
            Assert.AreEqual(version, actual.Description.Version);
            Assert.AreEqual(serviceManifestName, actual.Description.ServiceManifestName);
            Assert.AreEqual(serviceManifestVersion, actual.Description.ServiceManifestVersion);


#pragma warning disable CS0618 // Type or member is obsolete
            Assert.AreEqual(path, actual.Description.Path);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        [TestMethod]
        public void ConfigurationPackageAtMockCodePackageActivationContextTest()
        {
            //build ConfigurationSectionCollection
            var configSections = new ConfigurationSectionCollection();

            //Build ConfigurationSettings
            var configSettings = CreateConfigurationSettings(configSections);

            //add one ConfigurationSection
            ConfigurationSection configSection = CreateConfigurationSection(nameof(configSection.Name));
            configSections.Add(configSection);

            //add one Parameters entry
            ConfigurationProperty parameter = CreateConfigurationSectionParameters(nameof(parameter.Name), nameof(parameter.Value));
            configSection.Parameters.Add(parameter);

            //Build ConfigurationPackage
            ConfigurationPackage configPackage = CreateConfigurationPackage(configSettings, nameof(configPackage.Path));

            var context = new MockCodePackageActivationContext(
                "fabric:/MockApp",
                "MockAppType",
                "Code",
                "1.0.0.0",
                Guid.NewGuid().ToString(),
                @"C:\logDirectory",
                @"C:\tempDirectory",
                @"C:\workDirectory",
                "ServiceManifestName",
                "1.0.0.0")
            {
                ConfigurationPackage = configPackage
            };

            Assert.AreEqual(configPackage, context.ConfigurationPackage);
            Assert.AreEqual(configSettings, context.ConfigurationPackage.Settings);
            Assert.AreEqual(nameof(configPackage.Path), context.ConfigurationPackage.Path);

            Assert.AreEqual(configSettings, configPackage.Settings);

            Assert.AreEqual(configSection, configPackage.Settings.Sections[0]);
            Assert.AreEqual(nameof(configSection.Name), configPackage.Settings.Sections[0].Name);

            Assert.AreEqual(parameter, configPackage.Settings.Sections[0].Parameters[0]);
            Assert.AreEqual(nameof(parameter.Name), configPackage.Settings.Sections[0].Parameters[0].Name);
            Assert.AreEqual(nameof(parameter.Value), configPackage.Settings.Sections[0].Parameters[0].Value);



        }
    }
}
