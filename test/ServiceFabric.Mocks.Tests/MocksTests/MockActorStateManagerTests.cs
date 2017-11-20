using System;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ServiceFabric.Mocks.Tests.MocksTests
{
	[TestClass]
    public class MockActorStateManagerTests
    {
		[TestMethod]
	    public async Task Nullable_Int_Empty_TryGetStateAsyncTest()
	    {
			var instance = new MockActorStateManager();
		    var result = await instance.TryGetStateAsync<int?>("not existing");

			Assert.IsInstanceOfType(result, typeof(ConditionalValue<int?>));
			Assert.IsNull(result.Value);
			Assert.IsFalse(result.HasValue);
	    }


		[TestMethod]
		public async Task Nullable_Int_TryGetStateAsyncTest()
		{
			var instance = new MockActorStateManager();
			await instance.TryAddStateAsync("existing", (int?) 6);

			var result = await instance.TryGetStateAsync<int?>("existing");

			Assert.IsInstanceOfType(result, typeof(ConditionalValue<int?>));
			Assert.IsNotNull(result.Value);
			Assert.IsTrue(result.HasValue);
			Assert.AreEqual(6, result.Value);
		}

		[TestMethod]
		public async Task Int_Empty_TryGetStateAsyncTest()
		{
			var instance = new MockActorStateManager();
			var result = await instance.TryGetStateAsync<int>("not existing");

			Assert.IsInstanceOfType(result, typeof(ConditionalValue<int>));
			Assert.AreEqual(default(int), result.Value);
			Assert.IsFalse(result.HasValue);
		}


		[TestMethod]
		public async Task Int_TryGetStateAsyncTest()
		{
			var instance = new MockActorStateManager();
			await instance.TryAddStateAsync("existing", 6);

			var result = await instance.TryGetStateAsync<int>("existing");

			Assert.IsInstanceOfType(result, typeof(ConditionalValue<int>));
			Assert.IsNotNull(result.Value);
			Assert.IsTrue(result.HasValue);
			Assert.AreEqual(6, result.Value);
		}

		[TestMethod]
		public async Task Int_TryRemoveStateAsyncTest()
		{
			var instance = new MockActorStateManager();
			await instance.TryAddStateAsync("existing", 6);
			await instance.TryRemoveStateAsync("existing");
			var result = await instance.TryGetStateAsync<int>("existing");

			Assert.IsInstanceOfType(result, typeof(ConditionalValue<int>));
			Assert.AreEqual(default(int), result.Value);
			Assert.IsFalse(result.HasValue);
		}

		[TestMethod]
		public async Task Int_AddStateAsyncTest()
		{
			var instance = new MockActorStateManager();
			await instance.AddStateAsync("existing", 6);

			var result = await instance.TryGetStateAsync<int>("existing");

			Assert.IsInstanceOfType(result, typeof(ConditionalValue<int>));
			Assert.IsNotNull(result.Value);
			Assert.IsTrue(result.HasValue);
			Assert.AreEqual(6, result.Value);
		}

		[TestMethod]
		public async Task Int_DuplicateAddStateAsyncTest()
		{
			var instance = new MockActorStateManager();
			await instance.AddStateAsync("existing", 6);

			Assert.ThrowsException<InvalidOperationException>(() => {
				instance.AddStateAsync("existing", 6).ConfigureAwait(false).GetAwaiter().GetResult();
			});
		}

        [TestMethod]
        public async Task Int_DuplicateTryAddStateAsyncTest()
        {
            var instance = new MockActorStateManager();
            await instance.TryAddStateAsync("existing", 6);
            var result = await instance.TryAddStateAsync("existing", 6);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task MultiType_SetStateAsyncTest()
        {
            var instance = new MockActorStateManager();
            string stateName = "stateName";
            await instance.SetStateAsync(stateName, string.Empty);
            await instance.SetStateAsync(stateName, 5);
            DateTimeOffset utcNow = DateTimeOffset.UtcNow;
            await instance.SetStateAsync(stateName, utcNow);

            var result = await instance.TryGetStateAsync<DateTimeOffset>(stateName);
            Assert.IsInstanceOfType(result, typeof(ConditionalValue<DateTimeOffset>));
            Assert.IsNotNull(result.Value);
            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(utcNow, result.Value);
        }
	}
}
