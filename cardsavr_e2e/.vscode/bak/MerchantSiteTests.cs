using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Net;

using Switch.CardSavr.Http;
using Xunit;
using Xunit.Priority;

namespace cardsavr_e2e
{
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    [Collection("CardsavrSession collection")]
    public class MerchantSiteTests
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        CardsavrSession session;
        
        public MerchantSiteTests(CardsavrSession session)
        {
            this.session = session;
        }

        [Fact, Priority(1)]
        public async void TestSelectMerchants() {
            Paging paging = new Paging() { PageLength = 10, Sort = "name" };
            CardSavrResponse<List<MerchantSite>> merchants = await this.session.http.GetMerchantSitesAsync(null, paging);
            Assert.Equal(10, merchants.Body.Count);
            foreach (MerchantSite merch in merchants.Body) {
                log.Info($"Loaded merchant site: {merch.name}: {merch.host}");
            }

            merchants = await this.session.http.GetMerchantSitesAsync(
                new NameValueCollection() {
                    { "top_hosts", "amazon.com,apple.com"}, {"exclude_hosts", "petco.com,safeway.com" }
                }
            );
            Assert.Equal("amazon.com", merchants.Body[0].host);          
        }
    }
}
