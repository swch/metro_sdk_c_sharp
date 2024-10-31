using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;

using Switch.CardSavr.Exceptions;
using Switch.CardSavr.Http;
using Xunit;
using Xunit.Priority;

namespace cardsavr_e2e
{

    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    [Collection("CardsavrSession collection")]
    public class AccountTests
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        CardsavrSession session;

        public AccountTests(CardsavrSession session)
        {
            this.session = session;
        }

        [Fact, Priority(1)]
        public async void TestCreateAccounts() {

            List<Cardholder> cardholders = new List<Cardholder>();

            // create cardholders.
            PropertyBag bag = new PropertyBag();
            int count = 10;

            string[] safeKeys = new string[count];

            for (int n = 0; n < count; ++n)
            {
                // generate using an easily reproducible safe-key.
                bag["cuid"] = $"{CardsavrSession.e2e_identifier}_account_tests_{CardsavrSession.random.Next(10000)}_{n}";
                bag["first_name"] = $"Otto_{n}_cardholder";
                bag["last_name"] = $"Matic_{n}_cardholder";
                bag["email"] = $"cardsavr_e2e_{CardsavrSession.random.Next(10000)}@gmail.com";

                safeKeys[n] = n % 2 == 0 ? null : CardsavrSession.GenerateBogus32BitPassword(bag.GetString("cuid"));
                CardSavrResponse<Cardholder> result = await this.session.http.CreateCardholderAsync(bag, safeKeys[n]);
                cardholders.Add(result.Body);
            }
            
            Assert.Equal(cardholders.Count, count);

            //MerchantSite site = ctx.GetSyntheticSite();
            CardSavrResponse<List<MerchantSite>> sites = await this.session.http.GetMerchantSitesAsync(new NameValueCollection() {
                    { "host", "synthetic-sites-server.vercel.app/index.html" }
                });

            for (int n = 0; n < count; ++n) {
                // create an account.
                bag.Clear();
                bag["cardholder_id"] = cardholders[n].id;
                bag["merchant_site_id"] = sites.Body[0].id;
                bag["username"] = "good_email";
                bag["password"] = "asdf";

                CardSavrResponse<Account> result = await this.session.http.CreateAccountAsync(bag, safeKeys[n]);
                Assert.Equal(HttpStatusCode.Created, result.StatusCode);
                log.Info($"created account {result.Body.id} for cardholder-id={cardholders[n].id} ({cardholders[n].cuid})");

                // update it.
                bag.Clear();
                bag["password"] = $"{CardsavrSession.e2e_identifier}-{CardsavrSession.e2e_identifier}";
                result = await this.session.http.UpdateAccountAsync(result.Body.id, bag, null, safeKeys[n]);
                Assert.Equal(HttpStatusCode.Created, result.StatusCode);
                log.Info($"updated account-id={result.Body.id}");

                if (safeKeys[n] != null) {
                    Exception ex = await Assert.ThrowsAsync<RequestException>(async () => {
                        result = await this.session.http.UpdateAccountAsync(result.Body.id, bag, null, null);
                    });
                    Assert.True(ex.Message.IndexOf("No safe_key for this cardholder_id") >= 0);
                }

            }

            CardSavrResponse<List<Cardholder>> cardholderResponse = await this.session.http.GetCardholdersAsync(null);
            foreach (Cardholder c in cardholderResponse.Body) {
                if (c.cuid.StartsWith($"{CardsavrSession.e2e_identifier}_account_tests")) {
                    await this.session.http.DeleteCardholderAsync(c.id);
                    count--;
                }
            }
            Assert.Equal(0, count);
        }
    }
}