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
            Type packageType = typeof(ConfigurationPackage);
            var configPackage = (ConfigurationPackage)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(packageType);

            packageType.GetProperty(nameof(configPackage.Path)).SetValue(configPackage, path);
            packageType.GetProperty(nameof(configPackage.Settings)).SetValue(configPackage, configSettings);

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
    }
}
