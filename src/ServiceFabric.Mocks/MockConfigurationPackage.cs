using System;
using System.Collections.ObjectModel;
using System.Fabric;
using System.Fabric.Description;

namespace ServiceFabric.Mocks
{
    public class MockConfigurationPackage
    {
        /// <summary>
        /// Returns a new instance of <see cref="ConfigurationSection"/>
        /// </summary>
        /// <returns></returns>
        public static ConfigurationSection CreateConfigurationSection(string name)
        {
            Type configSectionType = typeof(ConfigurationSection);
            var configurationParameters = new ConfigurationPropertyCollection();
            var configSection = (ConfigurationSection)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(configSectionType);

            configSectionType.GetProperty(nameof(configSection.Name)).SetValue(configSection, name);
            configSectionType.GetProperty(nameof(configSection.Parameters)).SetValue(configSection, configurationParameters);

            return configSection;
        }

        /// <summary>
        /// Returns a new instance of <see cref="ConfigurationProperty"/>
        /// Does not support encrypted values.
        /// </summary>
        /// <returns></returns>
        public static ConfigurationProperty CreateConfigurationSectionParameters(string name, string value)
        {
            Type configPropertyType = typeof(ConfigurationProperty);
            var parameter = (ConfigurationProperty)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(configPropertyType);
            bool isEncrypted = false;
            bool mustOverride = false;

            configPropertyType.GetProperty(nameof(parameter.IsEncrypted)).SetValue(parameter, isEncrypted);
            configPropertyType.GetProperty(nameof(parameter.MustOverride)).SetValue(parameter, mustOverride);
            configPropertyType.GetProperty(nameof(parameter.Name)).SetValue(parameter, name);
            configPropertyType.GetProperty(nameof(parameter.Value)).SetValue(parameter, value);

            return parameter;
        }

        /// <summary>
        /// Returns a new instance of <see cref="ConfigurationSettings"/> and setting the provided <see cref="ConfigurationSection"/> as its Sections property.
        /// </summary>
        /// <param name="configSections"></param>
        /// <returns></returns>
        public static ConfigurationSettings CreateConfigurationSettings(ConfigurationSectionCollection configSections)
        {
            Type settingsType = typeof(ConfigurationSettings);
            var configSettings = (ConfigurationSettings)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(settingsType);

            settingsType.GetProperty(nameof(configSettings.Sections)).SetValue(configSettings, configSections);

            return configSettings;
        }

        /// <summary>
        /// Returns a new instance of <see cref="ConfigurationPackage"/> and setting the provided <see cref="ConfigurationSection"/> as its Settings property.
        /// </summary>
        /// <param name="configSettings"></param>
        /// <returns></returns>
        public static ConfigurationPackage CreateConfigurationPackage(ConfigurationSettings configSettings, string path = null)
        {
            Type packageType = typeof(ConfigurationPackageDescription);
            var description = ReflectionHelpers.CreateInstance<ConfigurationPackageDescription>();
            packageType.GetProperty(nameof(ConfigurationPackageDescription.Settings)).SetValue(description, configSettings);
            packageType.GetProperty(nameof(ConfigurationPackageDescription.Path)).SetValue(description, path);
            var configPackage = ReflectionHelpers.CreateInstance<ConfigurationPackage>(new []{description});
            return configPackage;
        }

        public static implicit operator ConfigurationPackage(MockConfigurationPackage mockConfigurationPackage)
        {
            return null;
        }


        public class ConfigurationPropertyCollection : KeyedCollection<string, ConfigurationProperty>
        {
            protected override string GetKeyForItem(ConfigurationProperty item)
            {
                return item.Name;
            }
        }

        public class ConfigurationSectionCollection : KeyedCollection<string, ConfigurationSection>
        {
            protected override string GetKeyForItem(ConfigurationSection item)
            {
                return item.Name;
            }
        }

        public class EndpointResourceDescriptionsKeyedCollection : KeyedCollection<string, EndpointResourceDescription>
        {
            protected override string GetKeyForItem(EndpointResourceDescription item)
            {
                return item.Name;
            }
        }

        public class ServiceGroupTypeDescriptionKeyedCollection : KeyedCollection<string, ServiceGroupTypeDescription>
        {
            protected override string GetKeyForItem(ServiceGroupTypeDescription item)
            {
                return item.ServiceTypeDescription.ServiceTypeName;
            }
        }
        public class ServiceTypeDescriptionKeyedCollection : KeyedCollection<string, ServiceTypeDescription>
        {
            protected override string GetKeyForItem(ServiceTypeDescription item)
            {
                return item.ServiceTypeName;
            }
        }
        

    }
}
