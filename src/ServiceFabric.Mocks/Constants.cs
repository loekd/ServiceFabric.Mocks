using System;
using System.Reflection;

namespace ServiceFabric.Mocks
{
    internal class Constants
    {
	    public const BindingFlags InstanceNonPublic = BindingFlags.Instance | BindingFlags.NonPublic;

	    public const BindingFlags InstancePublicNonPublic = InstanceNonPublic | BindingFlags.Public;

        public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(4);
    }
}
