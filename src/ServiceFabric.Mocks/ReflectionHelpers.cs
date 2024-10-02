using System;
using System.Globalization;
using System.Reflection;

using static ServiceFabric.Mocks.Constants;

namespace ServiceFabric.Mocks
{
    internal static class ReflectionHelpers
    {
        public static T CreateInstance<T>()
        {
            return (T)Activator.CreateInstance(typeof(T), BindingFlags.Instance | BindingFlags.NonPublic, null, null, CultureInfo.CurrentCulture);
        }

        public static T CreateInstance<T>(object[] args)
        {
            return (T)Activator.CreateInstance(typeof(T), BindingFlags.Instance | BindingFlags.NonPublic, null, args, CultureInfo.CurrentCulture);
        }

        public static T GetPropertyValue<T>(this object input, string propertyName)
        {
            return (T)GetProperty(input, propertyName).GetValue(input);
        }

        public static void SetPropertyValue(this object input, string propertyName, object value)
        {
           GetProperty(input, propertyName).SetValue(input, value);
        }

        private static PropertyInfo GetProperty(object input, string propertyName)
        {
            return input?.GetType().GetProperty(propertyName, InstancePublicNonPublic);
        }
    }
}
