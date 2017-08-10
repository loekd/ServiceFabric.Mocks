using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Fabric;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Fabric.Description;
using System.Collections.ObjectModel;
using System.Security;
using static ServiceFabric.Mocks.MockConfigurationPackage;

namespace ServiceFabric.Mocks.Tests.ServiceTests
{
    [TestClass]
    public class ConfigurationPackageTests
    {
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
