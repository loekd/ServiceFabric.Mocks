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

	}
}
