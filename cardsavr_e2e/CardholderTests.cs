using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Net.Http;

using Switch.CardSavr.Http;
using Xunit;
using Xunit.Priority;
using Newtonsoft.Json;

namespace cardsavr_e2e
{
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    [Collection("CardsavrSession collection")]
    public class CardholderTests
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        CardsavrSession session;

        public CardholderTests(CardsavrSession session)
        {
            this.session = session;
        }

        [Fact, Priority(1)]
        public async void TestCreateCardholders() {

            List<Cardholder> cardholders = new List<Cardholder>();

            // create cardholders.
            PropertyBag bag = new PropertyBag();
            int count = 10;
            for (int n = 0; n < count; ++n)
            {
                // generate using an easily reproducible safe-key.
                bag["cuid"] = $"{CardsavrSession.e2e_identifier}_cardholder_tests_{CardsavrSession.random.Next(10000)}_{n}";
                bag["first_name"] = $"Otto_{n}_cardholder";
                bag["last_name"] = $"Matic_{n}_cardholder";
                bag["email"] = $"cardsavr_e2e_{CardsavrSession.random.Next(10000)}@gmail.com";

                string safeKey = count % 2 == 0 ? null : CardsavrSession.GenerateBogus32BitPassword(bag.GetString("cuid"));
                CardSavrResponse<Cardholder> result = await this.session.http.CreateCardholderAsync(bag, safeKey);
                cardholders.Add(result.Body);
            }
            
            Assert.Equal(cardholders.Count, count);

            CardSavrResponse<List<Cardholder>> cardholderResponse = await this.session.http.GetCardholdersAsync(null);

            foreach (Cardholder c in cardholderResponse.Body) {
                if (c.cuid.StartsWith($"{CardsavrSession.e2e_identifier}_cardholder_tests")) {
                    await this.session.http.DeleteCardholderAsync(c.id);
                    count--;
                }
            }
            Assert.Equal(0, count);
        }

    }

}