#if RELEASE

using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ServiceFabric.Mocks.Tests
{

    [TestClass]
    public class AssemblyStrongNameTests
    {

        [TestMethod]
        public void TestAssemblyIdentity()
        {
            Assembly main = Assembly.Load("ServiceFabric.Mocks");
            Assert.IsTrue(main.FullName.Contains("PublicKeyToken=c8a3b3cecf8974ee"));
        }
    }
}

#endif
