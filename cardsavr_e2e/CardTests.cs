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
    public class CardTests
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        CardsavrSession session;

        public CardTests(CardsavrSession session)
        {
            this.session = session;
        }

        [Fact, Priority(1)]
        public async void TestCreateCards() {

            List<Cardholder> cardholders = new List<Cardholder>();

            // create cardholders.
            PropertyBag bag = new PropertyBag();
            int count = 2;

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

            for (int n = 0; n < count; ++n) {
                bag.Clear();
                // create an address.
                bag["cardholder_id"] = cardholders[n].id;
                bag["is_primary"] = true;
                bag["address1"] = $"{CardsavrSession.random.Next(1000, 9000)} SDK Ave NE";
                bag["address2"] = CardsavrSession.e2e_identifier;
                bag["city"] = "Seattle";
                bag["subnational"] = "Washington";
                bag["country"] = "USA";
                bag["postal_code"] = "98119";

                Address addr = (await this.session.http.CreateAddressAsync(bag)).Body;
                Assert.Equal(addr.address1, bag["address1"]);
                log.Info($"created primary address {addr.id} for cardholder: {cardholders[n].first_name} {cardholders[n].last_name} ({cardholders[n].id})");

                // update it.
                bag.Clear();
                bag["address1"] = $"{CardsavrSession.random.Next(1000, 9000)} CSharp-SDK Ave NE";
                log.Info(JsonConvert.SerializeObject(bag));
                addr = (await this.session.http.UpdateAddressAsync(addr.id, bag)).Body;
                log.Info(JsonConvert.SerializeObject(addr));
                Assert.Equal(addr.address1, bag["address1"]);
                log.Info($"updated primary address {addr.id} for cardholder: {cardholders[n].first_name} {cardholders[n].last_name} ({cardholders[n].id})");


                // the card we create uses our (possibly truncated) special identifier as the color
                // so we can identify it later if needed.
                DateTime expire = DateTime.Now.AddYears(1);
                string expYear =  (expire.Year % 2000).ToString();
                string expMonth = expire.Month.ToString();
                string pan = "4111111111111111";

                PropertyBag body = new PropertyBag()
                {
                    { "cardholder_id", cardholders[n].id },
                    { "address_id", addr.id },
                    { "pan", pan },
                    { "cvv", "345" },
                    { "par", ApiUtil.GenerateRandomPAR(pan, expMonth, expYear, cardholders[n].cuid) },
                    { "first_name", cardholders[n].first_name },
                    { "last_name", cardholders[n].last_name },
                    { "name_on_card", "BOGUS CARD" },
                    { "expiration_month", expMonth },
                    { "expiration_year", expYear}
                };
                // our test cardholders sometimes have a known safe-key.
                // NOT BACKWARD COMPATIBLE - client is now always the agent
                CardSavrResponse<Card> card = await this.session.http.CreateCardAsync(body, safeKeys[n]);
                log.Info($"created card-id={card.Body.id}");
                // update it: just change the address.
                body.Clear();
                body.Add("id", card.Body.id);
                body.Add("name_on_card", "REALLY BOGUS CARD");
                CardSavrResponse<Card> upd = await this.session.http.UpdateCardAsync(null, body);
                Assert.Equal(upd.Body.name_on_card, body["name_on_card"]);

                log.Info($"update card for cardholder \"{cardholders[n].cuid}\"");

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
