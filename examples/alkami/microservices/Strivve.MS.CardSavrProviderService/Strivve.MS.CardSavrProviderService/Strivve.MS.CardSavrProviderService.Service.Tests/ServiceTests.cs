using Alkami.Contracts;
using NUnit.Framework;
using Strivve.MS.CardSavrProviderService.Contracts.Requests;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Strivve.MS.CardSavrProviderService.Service.Tests
{
    [TestFixture]
    public class ServiceTests
    {
        /// <summary>
        /// This will hold the instance of the service implementation we're going to test
        /// </summary>
        private readonly ServiceImp _testClass;

        public ServiceTests()
        {
            var providerName = "Strivve.MS.CardSavrProviderService";
            var providerType = "Strivve";
            _testClass = new ServiceImp(providerType, providerName);
        }
        private void AugmentBaseRequest(BaseRequest request)
        {
            request.BankIdentifier = Guid.Parse("78554577-9DE6-43CD-9085-5868977156D1"); //developer dynamic
            request.BankUri = "developer.dev.alkamitech.com";
            request.UserIdentifier = Guid.Parse("3061E6D7-87F9-4EEB-A9BB-E42DB629D4CD"); //mike.brady
        }

        [SetUp]
        public void Setup()
        {
            /*
			 ...
			 do other injections here
			 ...
			 */
        }

        [Test]
        public async Task CanGetSettings()
        {
            var request = new GetSettingsRequest();
            AugmentBaseRequest(request);

            // Result should have two default settings
            var result = await _testClass.GetSettingsAsync(request);

            // Each should have a name containing these partial strings
            var hasFirstDefaultSetting = result.ItemList.Any(x => x.Name.Contains("First"));
            var hasSecondDefaultSetting = result.ItemList.Any(x => x.Name.Contains("Second"));

            Assert.IsTrue(result.ItemList.Count == 2);
            Assert.IsTrue(hasFirstDefaultSetting);
            Assert.IsTrue(hasSecondDefaultSetting);
        }
    }
}