using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceFabric.Mocks.ReliableCollections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceFabric.Mocks.NetCoreTests.SerializationTests
{
    [TestClass]
    public class StateSerializerExtensionsTests
    {
        const string originalContentValue = "original value";
        const string modifiedContentValue = "modified value";

        [TestMethod]
        public void Serialize_WithExistingSerializerTest()
        {
            ModifyablePayload value = new ModifyablePayload { Content = originalContentValue };
            var actual = StateSerializerExtensions.Serialize(new ModifyablePayloadSerializer(), value);
            Assert.IsInstanceOfType(actual, typeof(Stream));
        }

        [TestMethod]
        public void Serialize_WithNullSerializerTest()
        {
            ModifyablePayload value = new ModifyablePayload { Content = originalContentValue };
            var actual = StateSerializerExtensions.Serialize(null, value);
            Assert.IsInstanceOfType(actual, typeof(ModifyablePayload));
            Assert.AreSame(value, actual);
        }

        [TestMethod]
        public void RoundTrip_WithExistingSerializerTest()
        {
            ModifyablePayload value = new ModifyablePayload { Content = originalContentValue };
            var stream = StateSerializerExtensions.Serialize(new ModifyablePayloadSerializer(), value);
            var actual = StateSerializerExtensions.Deserialize(new ModifyablePayloadSerializer(), stream);
            Assert.IsInstanceOfType(actual, typeof(ModifyablePayload));
            Assert.AreNotSame(value, actual);
            Assert.AreEqual(originalContentValue, actual.Content);
        }

        [TestMethod]
        public void RoundTrip_WithNullSerializerTest()
        {
            ModifyablePayload value = new ModifyablePayload { Content = originalContentValue };
            var same = StateSerializerExtensions.Serialize(null, value);
            var actual = StateSerializerExtensions.Deserialize<ModifyablePayload>(null, same);
            Assert.IsInstanceOfType(actual, typeof(ModifyablePayload));
            Assert.AreSame(value, actual);
        }
    }
}
